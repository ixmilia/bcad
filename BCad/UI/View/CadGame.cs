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
        private Buffer<VertexPositionColor> lineVertexBuffer;
        private Buffer<VertexPositionColor> rubberBandVertexBuffer;
        private Buffer<VertexPositionColor> pointVertexBuffer;
        private VertexInputLayout lineInputLayout;
        private VertexInputLayout rubberBandInputLayout;
        private VertexInputLayout pointInputLayout;
        private List<Matrix> pointMatrices;
        private List<Matrix> rubberBandPointMatrices;
        private List<Color> pointColors;
        private List<Color> rubberBandPointColors;
        private bool drawingRubberBandLines;
        private VertexPositionColor[] pointVertexArray;

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
                View = Matrix.Scaling(1.0f, 1.0f, 0.0f), // flatten everything to show on the screen
                Projection = Matrix.Identity, // this gets updated in UpdateMatrices()
                World = Matrix.Identity // unused since all objects have absolute world coordinates (except for points)
            });

            // pre-load point drawing
            var size = 0.5f;
            pointVertexArray = new[]
            {
                new VertexPositionColor(new Vector3(-size, 0.0f, 0.0f), Color.White),
                new VertexPositionColor(new Vector3(size, 0.0f, 0.0f), Color.White),
                new VertexPositionColor(new Vector3(0.0f, -size, 0.0f), Color.White),
                new VertexPositionColor(new Vector3(0.0f, size, 0.0f), Color.White),
            };
            pointVertexBuffer = ToDisposeContent(Buffer<VertexPositionColor>.New(GraphicsDevice, pointVertexArray, BufferFlags.VertexBuffer));
            pointInputLayout = VertexInputLayout.FromBuffer(0, pointVertexBuffer);

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
            var pointMats = new List<Matrix>();
            var pointCols = new List<Color>();
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
                        AddVerticesToList(prim, primColor, lineVerts, pointMats, pointCols);
                    }
                }
            }

            // we have to prepare something
            if (lineVerts.Count == 0)
            {
                lineVerts.Add(new VertexPositionColor());
                lineVerts.Add(new VertexPositionColor());
            }

            lineVertexBuffer = ToDisposeContent(Buffer<VertexPositionColor>.New(GraphicsDevice, lineVerts.ToArray(), BufferFlags.VertexBuffer));
            lineInputLayout = VertexInputLayout.FromBuffer(0, lineVertexBuffer);

            pointMatrices = pointMats;
            pointColors = pointCols;
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
            var pointMats = new List<Matrix>();
            var pointCols = new List<Color>();
            if (generator != null && viewControl != null)
            {
                var primitives = generator(viewControl.GetCursorPoint());
                foreach (var prim in primitives)
                {
                    var primColor = MapColor(prim.Color, autoColor);
                    AddVerticesToList(prim, primColor, lineVerts, pointMats, pointCols);
                }
            }

            // we have to prepare something
            if (lineVerts.Count == 0)
            {
                lineVerts.Add(new VertexPositionColor());
                lineVerts.Add(new VertexPositionColor());
            }

            rubberBandVertexBuffer = ToDisposeContent(Buffer<VertexPositionColor>.New(GraphicsDevice, lineVerts.ToArray(), BufferFlags.VertexBuffer));
            rubberBandInputLayout = VertexInputLayout.FromBuffer(0, rubberBandVertexBuffer);

            rubberBandPointMatrices = pointMats;
            rubberBandPointColors = pointCols;

            drawingRubberBandLines = true;
        }

        private void AddVerticesToList(IPrimitive primitive, Color color, List<VertexPositionColor> lineVerts, List<Matrix> pointMats, List<Color> pointColors)
        {
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    lineVerts.Add(new VertexPositionColor(line.P1.ToVector3(), color));
                    lineVerts.Add(new VertexPositionColor(line.P2.ToVector3(), color));
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
                        lineVerts.Add(last);
                        lineVerts.Add(next);
                        last = next;
                    }

                    // add final line
                    lineVerts.Add(last);
                    lineVerts.Add(new VertexPositionColor(el.GetPoint(el.EndAngle).ToVector3(), color));
                    break;
                case PrimitiveKind.Point:
                    var point = (PrimitivePoint)primitive;
                    pointMats.Add(Matrix.Translation((float)point.Location.X, (float)point.Location.Y, (float)point.Location.Z));
                    pointColors.Add(color);
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
            basicEffect.Projection = new Matrix(
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

            DrawEntities(lineVertexBuffer, lineInputLayout);
            if (pointMatrices.Count > 0)
            {
                DrawPoints(pointMatrices, pointColors);
            }

            if (drawingRubberBandLines && rubberBandVertexBuffer.ElementCount > 0)
            {
                DrawEntities(rubberBandVertexBuffer, rubberBandInputLayout);
                if (rubberBandPointMatrices.Count > 0)
                {
                    DrawPoints(rubberBandPointMatrices, rubberBandPointColors);
                }
            }

            base.Draw(gameTime);
        }

        private void DrawEntities(Buffer<VertexPositionColor> vertices, VertexInputLayout layout)
        {
            basicEffect.World = Matrix.Identity;
            basicEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SetVertexBuffer(vertices);
            GraphicsDevice.SetVertexInputLayout(layout);
            GraphicsDevice.Draw(PrimitiveType.LineList, vertices.ElementCount);
        }

        private void DrawPoints(List<Matrix> worldMatrices, List<Color> colors)
        {
            // TODO: get point size from settings
            var pointSize = 15;
            var scale = workspace.ActiveViewPort.ViewHeight / GraphicsDevice.BackBuffer.Height * pointSize;
            var scaling = Matrix.Scaling((float)scale);
            GraphicsDevice.SetVertexBuffer(pointVertexBuffer);
            GraphicsDevice.SetVertexInputLayout(pointInputLayout);
            for (int i = 0; i < worldMatrices.Count; i++)
            {
                for (int j = 0; j < pointVertexArray.Length; j++)
                {
                    pointVertexArray[j].Color = colors[i];
                }

                pointVertexBuffer.SetData(pointVertexArray);
                basicEffect.World = scaling * worldMatrices[i];
                basicEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.Draw(PrimitiveType.LineList, pointVertexBuffer.ElementCount);
            }
        }
    }
}
