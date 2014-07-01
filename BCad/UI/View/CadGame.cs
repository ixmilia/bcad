using System.Collections.Generic;
using System.Linq;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace BCad.UI.View
{
    public class CadGame : Game
    {
        private IWorkspace workspace;
        private IViewControl viewControl;
        private GraphicsDeviceManager graphicsDeviceManager;
        private Color4 backgroundColor;
        private Color autoColor;
        private BasicEffect basicEffect;
        private Buffer<VertexPositionColor> lineVertices;
        private Buffer<VertexPositionColor> rubberBandVertices;
        private VertexInputLayout lineInputLayout;
        private VertexInputLayout rubberBandInputLayout;
        private bool drawingRubberBandLines;

        public CadGame(IWorkspace workspace, IViewControl viewControl)
        {
            this.workspace = workspace;
            this.viewControl = viewControl;
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            workspace.WorkspaceChanged += (sender, e) =>
            {
                if (e.IsDrawingChange)
                    UpdateVericies();
                if (e.IsActiveViewPortChange)
                    UpdateMatrices();
            };
            workspace.RubberBandGeneratorChanged += (sender, e) =>
            {
                UpdateRubberBandLines();
            };
            workspace.SettingsManager.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case Constants.BackgroundColorString:
                        SetColors();
                        UpdateVericies();
                        UpdateRubberBandLines();
                        break;
                }
            };
            SetColors();
        }

        private void SetColors()
        {
            var bg = workspace.SettingsManager.BackgroundColor;
            var auto = bg.GetAutoContrastingColor();
            backgroundColor = new Color4(bg.R / 255f, bg.G / 255f, bg.B / 255f, bg.A / 255f);
            autoColor = new Color(auto.R, auto.G, auto.B, auto.A);
        }

        protected override void LoadContent()
        {
            basicEffect = ToDisposeContent(new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = SharpDX.Matrix.Scaling(1.0f, 1.0f, 0.0f), // flatten everything to show on the screen
                Projection = SharpDX.Matrix.Identity, // this gets updated in UpdateMatrices()
                World = SharpDX.Matrix.Identity // unused since all objects have absolute world coordinates
            });
            UpdateVericies();
            UpdateRubberBandLines();
            UpdateMatrices();
            base.LoadContent();
        }

        public void Resize(int width, int height)
        {
            GraphicsDevice.Presenter.Resize(width, height, GraphicsDevice.BackBuffer.Format, null);
            UpdateMatrices();
        }

        private void UpdateVericies()
        {
            var lineVerts = new List<VertexPositionColor>();
            var drawing = workspace.Drawing;
            foreach (var layer in drawing.GetLayers().Where(l => l.IsVisible))
            {
                var layerColor = MapColor(layer.Color, autoColor);
                foreach (var entity in layer.GetEntities())
                {
                    var entityColor = MapColor(entity.Color, layerColor);
                    foreach (var prim in entity.GetPrimitives())
                    {
                        var primColor = MapColor(prim.Color, entityColor);
                        AddVerticesToList(prim, primColor, lineVerts);
                    }
                }
            }

            if (lineVerts.Count == 0)
            {
                // we have to display something
                lineVerts.Add(new VertexPositionColor());
                lineVerts.Add(new VertexPositionColor());
            }

            lineVertices = ToDisposeContent(Buffer<VertexPositionColor>.New(GraphicsDevice, lineVerts.ToArray(), BufferFlags.VertexBuffer));
            lineInputLayout = VertexInputLayout.FromBuffer(0, lineVertices);
        }

        public void UpdateRubberBandLines()
        {
            var generator = workspace.RubberBandGenerator;
            if (generator == null || viewControl == null)
            {
                drawingRubberBandLines = false;
                return;
            }

            var lineVerts = new List<VertexPositionColor>();
            if (generator != null && viewControl != null)
            {
                var primitives = generator(viewControl.GetCursorPoint());
                foreach (var prim in primitives)
                {
                    var primColor = MapColor(prim.Color, autoColor);
                    AddVerticesToList(prim, primColor, lineVerts);
                }
            }

            if (lineVerts.Count == 0)
            {
                // we have to display something
                lineVerts.Add(new VertexPositionColor());
                lineVerts.Add(new VertexPositionColor());
            }

            rubberBandVertices = ToDisposeContent(Buffer<VertexPositionColor>.New(GraphicsDevice, lineVerts.ToArray(), BufferFlags.VertexBuffer));
            rubberBandInputLayout = VertexInputLayout.FromBuffer(0, lineVertices);
            drawingRubberBandLines = true;
        }

        private void AddVerticesToList(IPrimitive primitive, Color color, List<VertexPositionColor> list)
        {
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    list.Add(new VertexPositionColor(line.P1.ToVector3(), color));
                    list.Add(new VertexPositionColor(line.P2.ToVector3(), color));
                    break;
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    var delta = 1.0;
                    var start = el.StartAngle;
                    var end = el.EndAngle;
                    if (start > end)
                        start -= MathHelper.ThreeSixty;
                    var last = new VertexPositionColor(el.GetPoint(start).ToVector3(), color);
                    for (var angle = start + delta; angle < end; angle += delta)
                    {
                        var p = el.GetPoint(angle);
                        var next = new VertexPositionColor(p.ToVector3(), color);
                        list.Add(last);
                        list.Add(next);
                        last = next;
                    }

                    // add final line
                    list.Add(last);
                    list.Add(new VertexPositionColor(el.GetPoint(el.EndAngle).ToVector3(), color));
                    break;
            }
        }

        private Color MapColor(IndexedColor color, Color fallback)
        {
            if (color.IsAuto)
                return fallback;
            var real = workspace.SettingsManager.ColorMap[color];
            return new Color(real.R, real.G, real.B, real.A);
        }

        private void UpdateMatrices()
        {
            var m = workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
            basicEffect.Projection = new SharpDX.Matrix(
                (float)m.M11,
                (float)m.M21,
                (float)m.M31,
                (float)m.M41,
                (float)m.M12,
                (float)m.M22,
                (float)m.M32,
                (float)m.M42,
                (float)m.M13,
                (float)m.M23,
                (float)m.M33,
                (float)m.M43,
                (float)m.M14,
                (float)m.M24,
                (float)m.M34,
                (float)m.M44);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor);

            basicEffect.CurrentTechnique.Passes[0].Apply();
            
            GraphicsDevice.SetVertexBuffer(lineVertices);
            GraphicsDevice.SetVertexInputLayout(lineInputLayout);
            GraphicsDevice.Draw(PrimitiveType.LineList, lineVertices.ElementCount);

            if (drawingRubberBandLines)
            {
                GraphicsDevice.SetVertexBuffer(rubberBandVertices);
                GraphicsDevice.SetVertexInputLayout(rubberBandInputLayout);
                GraphicsDevice.Draw(PrimitiveType.LineList, rubberBandVertices.ElementCount);
            }

            base.Draw(gameTime);
        }
    }
}
