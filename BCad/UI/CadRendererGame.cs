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
        private Color backgroundColor;
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
                UpdateTransform();
            }
        }

        public void Resize()
        {
            UpdateTransform();
        }

        private void UpdateTransform()
        {
            transform = Matrix4.CreateScale(1, 1, 0)
                    * workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.BackgroundColorString:
                    backgroundColor = workspace.SettingsManager.BackgroundColor.ToColor();
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
            effect.VertexColorEnabled = true;

            workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            Workspace_WorkspaceChanged(this, new WorkspaceChangeEventArgs(true, true, true, true, true));
            foreach (var prop in new[] { Constants.BackgroundColorString })
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
            // TODO: transform is incorrect on first launch
            GraphicsDevice.Clear(backgroundColor);
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
            var generator = inputService.PrimitiveGenerator;
            if (inputService.IsDrawing && generator != null)
            {
                var cursor = viewControl.GetCursorPoint();
                var rubber = generator(cursor);
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
                    if (endAngle < startAngle)
                        endAngle += MathHelper.TwoPI;
                    var angleDelta = 1.0 * MathHelper.DegreesToRadians;
                    var trans = transform * el.FromUnitCircleProjection();
                    var last = trans.Transform(new Point(Math.Cos(startAngle), Math.Sin(startAngle), 0.0));
                    double angle;
                    for (angle = startAngle; angle <= endAngle; angle += angleDelta)
                    {
                        var next = trans.Transform(new Point(Math.Cos(angle), Math.Sin(angle), 0.0));
                        batch.DrawLine(new VertexPositionColor(last.ToVector3(), color), new VertexPositionColor(next.ToVector3(), color));
                        last = next;
                    }

                    if (angle != endAngle)
                    {
                        // draw final line if needed.  this could occur if either startAngle or endAngle aren't whole numbers
                        var next = trans.Transform(new Point(Math.Cos(endAngle), Math.Sin(endAngle), 0.0));
                        batch.DrawLine(new VertexPositionColor(last.ToVector3(), color), new VertexPositionColor(next.ToVector3(), color));
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
