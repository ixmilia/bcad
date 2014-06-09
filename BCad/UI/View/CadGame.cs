using System.Collections.Generic;
using BCad.Extensions;
using BCad.Primitives;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace BCad.UI.View
{
    public class CadGame : Game
    {
        private IWorkspace workspace;
        private GraphicsDeviceManager graphicsDeviceManager;
        private Color4 backgroundColor;
        private Color autoColor;
        private BasicEffect basicEffect;
        private Buffer<VertexPositionColor> lineVertices;
        private VertexInputLayout lineInputLayout;

        public CadGame(IWorkspace workspace)
        {
            this.workspace = workspace;
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            workspace.WorkspaceChanged += (sender, e) =>
            {
                if (e.IsDrawingChange)
                    UpdateVericies();
                if (e.IsActiveViewPortChange)
                    UpdateMatrices();
            };
            workspace.SettingsManager.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case Constants.BackgroundColorString:
                        SetColors();
                        UpdateVericies();
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
            UpdateMatrices();
            base.LoadContent();
        }

        private void UpdateVericies()
        {
            var lineVerts = new List<VertexPositionColor>();
            var drawing = workspace.Drawing;
            foreach (var layer in drawing.GetLayers())
            {
                var layerColor = MapColor(layer.Color, autoColor);
                foreach (var entity in layer.GetEntities())
                {
                    var entityColor = MapColor(entity.Color, layerColor);
                    foreach (var prim in entity.GetPrimitives())
                    {
                        var primColor = MapColor(prim.Color, entityColor);
                        switch (prim.Kind)
                        {
                            case PrimitiveKind.Line:
                                var line = (PrimitiveLine)prim;
                                lineVerts.Add(new VertexPositionColor(line.P1.ToVector3(), primColor));
                                lineVerts.Add(new VertexPositionColor(line.P2.ToVector3(), primColor));
                                break;
                            case PrimitiveKind.Ellipse:
                                var el = (PrimitiveEllipse)prim;
                                var delta = 1.0;
                                var last = new VertexPositionColor(el.GetPoint(el.StartAngle).ToVector3(), primColor);
                                for (var angle = el.StartAngle + delta; angle <= el.EndAngle; angle += delta)
                                {
                                    var p = el.GetPoint(angle);
                                    var next = new VertexPositionColor(p.ToVector3(), primColor);
                                    lineVerts.Add(last);
                                    lineVerts.Add(next);
                                    last = next;
                                }

                                // add final line
                                lineVerts.Add(last);
                                lineVerts.Add(new VertexPositionColor(el.GetPoint(el.EndAngle).ToVector3(), primColor));
                                break;
                        }
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

            GraphicsDevice.SetVertexBuffer(lineVertices);
            GraphicsDevice.SetVertexInputLayout(lineInputLayout);

            basicEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.Draw(PrimitiveType.LineList, lineVertices.ElementCount);

            base.Draw(gameTime);
        }
    }
}
