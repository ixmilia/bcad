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
        private IViewHost viewHost;
        private BasicEffect effect;
        private PrimitiveBatch<VertexPositionColor> batch;
        private Color autoColor;
        private Color backgroundColor;
        private Matrix4 transform;

        public CadRendererGame(IWorkspace workspace, IInputService inputService, IViewHost viewControl)
        {
            deviceManager = new GraphicsDeviceManager(this);
            this.workspace = workspace;
            this.inputService = inputService;
            this.viewHost = viewControl;
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsActiveViewPortChange)
            {
                UpdateTransform();
            }
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
            // transform is incorrect on first launch
            if (viewHost.DisplayWidth != GraphicsDevice.BackBuffer.Width || viewHost.DisplayHeight != GraphicsDevice.BackBuffer.Height)
            {
                UpdateTransform();
            }

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
                var cursor = viewHost.GetCursorPoint();
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
                    var verticies = el.GetProjectedVerticies(transform);
                    for (int i = 0; i < verticies.Length - 1; i++)
                    {
                        batch.DrawLine(
                            new VertexPositionColor(verticies[i].ToVector3(), color),
                            new VertexPositionColor(verticies[i + 1].ToVector3(), color));
                    }

                    break;
                case PrimitiveKind.Text:
                    var text = (PrimitiveText)primitive;
                    var rad = text.Rotation * MathHelper.DegreesToRadians;
                    var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * text.Width;
                    var up = text.Normal.Cross(right).Normalize() * text.Height;
                    batch.DrawLine(
                        new VertexPositionColor(transform.Transform(text.Location).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location + right).ToVector3(), color));
                    batch.DrawLine(
                        new VertexPositionColor(transform.Transform(text.Location + right).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location + right + up).ToVector3(), color));
                    batch.DrawLine(
                        new VertexPositionColor(transform.Transform(text.Location + right + up).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location + up).ToVector3(), color));
                    batch.DrawLine(
                        new VertexPositionColor(transform.Transform(text.Location + up).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location).ToVector3(), color));
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
