using System;
using System.ComponentModel;
using System.Linq;
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
    public class SharpDXRenderer : Game
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

        public SharpDXRenderer(IWorkspace workspace, IInputService inputService, IViewControl viewControl)
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
            //var compiled = SharpDX.D3DCompiler.ShaderBytecode.Compile(@"
            //struct PS_IN
            //{
            //    float4 pos : SV_POSITION;
            //    float4 col : COLOR;
            //};

            //float4 PS(PS_IN input) : SV_Target
            //{
            //    float4 res;
            //    int p = input.pos.x + input.pos.y;
            //    if (p % 2 == 0)
            //        res = input.col;
            //    else
            //        res = float4(0.0f, 0.0f, 0.0f, 0.0f);

            //    return res;
            //}

            //technique11 Render
            //{
            //    pass P0
            //    {
            //        SetGeometryShader(0);
            //        SetPixelShader(CompileShader(ps_4_0, PS()));
            //        SetVertexShader(0);
            //    }
            //}
            //", "fx_5_0", SharpDX.D3DCompiler.ShaderFlags.None, SharpDX.D3DCompiler.EffectFlags.None);

            batch = new PrimitiveBatch<VertexPositionColor>(GraphicsDevice);
            effect = new BasicEffect(GraphicsDevice);
            effect.VertexColorEnabled = true;

            workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            Workspace_WorkspaceChanged(this, WorkspaceChangeEventArgs.Reset());
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
            if (viewControl.DisplayWidth != GraphicsDevice.BackBuffer.Width || viewControl.DisplayHeight != GraphicsDevice.BackBuffer.Height)
            {
                UpdateTransform();
            }

            GraphicsDevice.Clear(backgroundColor);
            effect.CurrentTechnique.Passes[0].Apply();
            batch.Begin();

            // draw entities
            foreach (var layer in workspace.Drawing.GetLayers())
            {
                foreach (var ent in layer.GetEntities())
                {
                    foreach (var prim in ent.GetPrimitives())
                    {
                        DrawPrimitive(prim, layer.Color, true);
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
                    DrawPrimitive(prim, IndexedColor.Auto, false);
                }
            }

            batch.End();
            base.Draw(gameTime);
        }

        private void DrawPrimitive(IPrimitive primitive, IndexedColor layerColor, bool highQuality)
        {
            var color = GetColor(layerColor, primitive.Color);
            VertexPositionColor[] verticies;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    var p1 = transform.Transform(line.P1);
                    var p2 = transform.Transform(line.P2);
                    verticies = new[]
                    {
                        new VertexPositionColor(p1.ToVector3(), color),
                        new VertexPositionColor(p2.ToVector3(), color)
                    };
                    break;
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    var verts = el.GetProjectedVerticies(transform, highQuality ? 180 : 72).ToArray();
                    verticies = new VertexPositionColor[verts.Length];
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verticies[i] = new VertexPositionColor(verts[i].ToVector3(), color);
                    }
                    break;
                case PrimitiveKind.Text:
                    var text = (PrimitiveText)primitive;
                    var rad = text.Rotation * MathHelper.DegreesToRadians;
                    var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * text.Width;
                    var up = text.Normal.Cross(right).Normalize() * text.Height;
                    verticies = new[]
                    {
                        new VertexPositionColor(transform.Transform(text.Location).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location + right).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location + right + up).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location + up).ToVector3(), color),
                        new VertexPositionColor(transform.Transform(text.Location).ToVector3(), color)
                    };
                    break;
                default:
                    // TODO: draw points
                    verticies = new VertexPositionColor[0];
                    break;
            }

            batch.Draw(PrimitiveType.LineStrip, verticies);
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
