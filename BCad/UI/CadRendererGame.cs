using System;
using System.ComponentModel;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using BCad.Services;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace BCad.UI
{
    public class CadRendererGame : Game
    {
        private GraphicsDeviceManager deviceManager;
        private IWorkspace workspace;
        private IInputService inputService;
        private IViewControl viewControl;
        private BasicEffect effect;
        private PrimitiveBatch<VertexPositionColor> batch;
        private Color autoColor;
        private Matrix4 transform;

        public CadRendererGame(IWorkspace workspace, IInputService inputService, IViewControl viewControl)
        {
            deviceManager = new GraphicsDeviceManager(this);
            this.workspace = workspace;
            this.inputService = inputService;
            this.viewControl = viewControl;
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsActiveViewPortChange)
            {
                transform = Matrix4.CreateScale(1, 1, 0)
                    * workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
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

            workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            Workspace_WorkspaceChanged(this, new WorkspaceChangeEventArgs(true, true, true, true, true));
            foreach (var prop in new[] { "BackgroundColor" })
            {
                SettingsManager_PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(workspace.SettingsManager.BackgroundColor.ToColor4());
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
                        DrawPrimitive(prim, layer.Color);
                    }
                }
            }

            // draw rubber band primitives
            if (inputService.IsDrawing && inputService.PrimitiveGenerator != null)
            {
                var cursor = viewControl.GetCursorPoint();
                var rubber = inputService.PrimitiveGenerator(cursor);
                foreach (var prim in rubber)
                {
                    DrawPrimitive(prim, IndexedColor.Auto);
                }
            }

            batch.End();
            base.Draw(gameTime);
        }

        private void DrawPrimitive(IPrimitive primitive, IndexedColor layerColor)
        {
            var color = GetColor(layerColor, primitive.Color);
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    var p1 = transform.Transform(line.P1);
                    var p2 = transform.Transform(line.P2);
                    batch.DrawLine(new VertexPositionColor(p1.ToVector3(), color), new VertexPositionColor(p2.ToVector3(), color));
                    break;
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    var startAngle = el.StartAngle * MathHelper.DegreesToRadians;
                    var endAngle = el.EndAngle * MathHelper.DegreesToRadians;
                    var angleDelta = 1.0 * MathHelper.DegreesToRadians;
                    var trans = transform * el.FromUnitCircleProjection();
                    var last = trans.Transform(new Point(Math.Cos(startAngle), Math.Sin(startAngle), 0.0));
                    for (var angle = startAngle; angle <= endAngle; angle += angleDelta)
                    {
                        var next = trans.Transform(new Point(Math.Cos(angle), Math.Sin(angle), 0.0));
                        batch.DrawLine(new VertexPositionColor(last.ToVector3(), color), new VertexPositionColor(next.ToVector3(), color));
                        last = next;
                    }
                    break;
            }
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
