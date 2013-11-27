using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Extensions;
using BCad.Primitives;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace BCad.UI
{
    public class CadRendererGame : Game
    {
        private GraphicsDeviceManager deviceManager;
        private IWorkspace workspace;
        private BasicEffect effect;
        private PrimitiveBatch<VertexPositionColor> batch;

        public CadRendererGame(IWorkspace workspace)
        {
            deviceManager = new GraphicsDeviceManager(this);
            this.workspace = workspace;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Initialize()
        {
            batch = new PrimitiveBatch<VertexPositionColor>(GraphicsDevice);
            effect = new BasicEffect(GraphicsDevice);

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(workspace.SettingsManager.BackgroundColor.ToColor4());
            var transform = workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);

            effect.Projection = Matrix.Identity;
            effect.CurrentTechnique.Passes[0].Apply();
            batch.Begin();
            batch.DrawLine(
                new VertexPositionColor(new Vector3(0, 0, 0), Color.White),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, 0), Color.White));
            foreach (var layer in workspace.Drawing.GetLayers())
            {
                foreach (var ent in layer.GetEntities())
                {
                    foreach (var prim in ent.GetPrimitives())
                    {
                        switch (prim.Kind)
                        {
                            case PrimitiveKind.Line:
                                var line = (PrimitiveLine)prim;
                                var p1 = transform.Transform(line.P1);
                                var p2 = transform.Transform(line.P2);
                                //renderTarget.DrawLine(new Vector2((float)p1.X, (float)p1.Y), new Vector2((float)p2.X, (float)p2.Y), brush);
                                batch.DrawLine(new VertexPositionColor(p1.ToVector3(), Color.White), new VertexPositionColor(p2.ToVector3(), Color.White));
                                break;
                        }
                    }
                }
            }

            batch.End();
            base.Draw(gameTime);
        }
    }
}
