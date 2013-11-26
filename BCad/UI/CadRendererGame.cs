using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Extensions;
using SharpDX.Toolkit;

namespace BCad.UI
{
    public class CadRendererGame : Game
    {
        private GraphicsDeviceManager deviceManager;
        private IWorkspace workspace;

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
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(workspace.SettingsManager.BackgroundColor.ToColor4());

            base.Draw(gameTime);
        }
    }
}
