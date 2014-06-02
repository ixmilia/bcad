using System.Collections.Generic;
using System.Linq;
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
            //https://github.com/sharpdx/SharpDX-Samples/blob/master/Toolkit/Common/MiniCube/MiniCubeGame.cs
            private IWorkspace workspace;
            private GraphicsDeviceManager graphicsDeviceManager;
            //private Color4 backgroundColor;
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
                var lines = workspace.Drawing.GetEntities().SelectMany(entity => entity.GetPrimitives().OfType<PrimitiveLine>());
                var verts = new List<VertexPositionColor>();
                foreach (var line in lines)
                {
                    verts.Add(new VertexPositionColor(new Vector3((float)line.P1.X, (float)line.P1.Y, (float)line.P1.Z), SharpDX.Color.White));
                    verts.Add(new VertexPositionColor(new Vector3((float)line.P2.X, (float)line.P2.Y, (float)line.P2.Z), SharpDX.Color.White));
                }
                if (verts.Count == 0)
                {
                    verts.Add(new VertexPositionColor());
                    verts.Add(new VertexPositionColor());
                }
                vertices = ToDisposeContent(Buffer<VertexPositionColor>.New(GraphicsDevice, verts.ToArray(), BufferFlags.VertexBuffer));
                inputLayout = VertexInputLayout.FromBuffer(0, vertices);
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
                //GraphicsDevice.Clear(backgroundColor);
                GraphicsDevice.Clear(SharpDX.Color.Black);

                GraphicsDevice.SetVertexBuffer(vertices);
                GraphicsDevice.SetVertexInputLayout(inputLayout);

                basicEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.Draw(PrimitiveType.LineList, vertices.ElementCount);

                base.Draw(gameTime);
            }
        }
    }
}
