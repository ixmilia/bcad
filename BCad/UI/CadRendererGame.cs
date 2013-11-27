using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;
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
        private IInputService inputService;
        private BasicEffect effect;
        private PrimitiveBatch<VertexPositionColor> batch;
        private Color autoColor;

        public CadRendererGame(IWorkspace workspace, IInputService inputService)
        {
            deviceManager = new GraphicsDeviceManager(this);
            this.workspace = workspace;
            this.inputService = inputService;

            this.workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            foreach (var prop in new[] { "BackgroundColor" })
            {
                SettingsManager_PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BackgroundColor":
                    autoColor = workspace.SettingsManager.BackgroundColor.GetAutoContrastingColor().ToColor();
                    break;
            }
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

            // draw entities
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
                                var color = GetColor(layer.Color, line.Color);
                                batch.DrawLine(new VertexPositionColor(p1.ToVector3(), Color.Red), new VertexPositionColor(p2.ToVector3(), Color.Red));
                                break;
                        }
                    }
                }
            }

            // draw rubber band primitives
            if (inputService.IsDrawing && inputService.PrimitiveGenerator != null)
            {
                //inputService.PrimitiveGenerator(GetCur)
            }

            batch.End();
            base.Draw(gameTime);
        }

        private Color GetColor(IndexedColor layerColor, IndexedColor entityColor)
        {
            if (entityColor.IsAuto)
            {
                if (layerColor.IsAuto)
                {
                    return autoColor;
                }
                else
                {
                    return layerColor.RealColor.ToColor();
                }
            }
            else
            {
                return entityColor.RealColor.ToColor();
            }
        }
    }
}
