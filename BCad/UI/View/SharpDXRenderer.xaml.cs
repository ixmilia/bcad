using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using BCad.Primitives;
using BCad.Services;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for SharpDXRenderer.xaml
    /// </summary>
    public partial class SharpDXRenderer : UserControl, IRenderer
    {
        public SharpDXRenderer()
        {
            InitializeComponent();
        }

        public SharpDXRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
            : this()
        {
            var game = new CadGame(workspace);
            game.Run(surface);
        }

        private class CadGame : Game
        {
            private IWorkspace workspace;
            private GraphicsDeviceManager graphicsDeviceManager;
            private Color4 backgroundColor;
            private Color autoColor;
            private BasicEffect basicEffect;
            private Buffer<VertexPositionColor> vertices;
            private VertexInputLayout inputLayout;

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
                        View = SharpDX.Matrix.Identity,
                        Projection = SharpDX.Matrix.Identity,
                        World = SharpDX.Matrix.Identity
                    });
                UpdateVericies();
                UpdateMatrices();
                base.LoadContent();
            }

            private void UpdateVericies()
            {
                var verts = new List<VertexPositionColor>();
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
                                    verts.Add(new VertexPositionColor(new Vector3((float)line.P1.X, (float)line.P1.Y, (float)line.P1.Z), primColor));
                                    verts.Add(new VertexPositionColor(new Vector3((float)line.P2.X, (float)line.P2.Y, (float)line.P2.Z), primColor));
                                    break;
                            }
                        }
                    }
                }

                if (verts.Count == 0)
                {
                    // we have to display something
                    verts.Add(new VertexPositionColor());
                    verts.Add(new VertexPositionColor());
                }

                vertices = ToDisposeContent(Buffer<VertexPositionColor>.New(GraphicsDevice, verts.ToArray(), BufferFlags.VertexBuffer));
                inputLayout = VertexInputLayout.FromBuffer(0, vertices);
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
                
                GraphicsDevice.SetVertexBuffer(vertices);
                GraphicsDevice.SetVertexInputLayout(inputLayout);

                basicEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.Draw(PrimitiveType.LineList, vertices.ElementCount);

                base.Draw(gameTime);
            }
        }
    }
}
