using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IxMilia.BCad.Display;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Lisp;
using IxMilia.BCad.Plotting.Svg;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Services;
using IxMilia.BCad.Settings;
using IxMilia.Lisp;
using IxMilia.Pdf;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace IxMilia.BCad.Rpc
{
    public class ServerAgent
    {
        private DisplayInteractionManager _dim;
        private JsonRpc _jsonRpc { get; }

        public LispWorkspace Workspace { get; }
        public bool IsRunning { get; private set; }
        public double Width => _dim.Width;
        public double Height => _dim.Height;

        private bool _readyEventFired;
        public event EventHandler IsReady;
        private string _versionHtml = null;

        public ServerAgent(LispWorkspace workspace, JsonRpc rpc)
        {
            Workspace = workspace;
            _jsonRpc = rpc;
            IsRunning = true;
            _dim = new DisplayInteractionManager(workspace, ProjectionStyle.OriginTopLeft);
        }

        public void StartListening()
        {
            _dim.CurrentSnapPointUpdated += _dim_CurrentSnapPointUpdated;
            _dim.CursorStateUpdated += _dim_CursorStateUpdated;
            _dim.HotPointsUpdated += _dim_HotPointsUpdated;
            _dim.RubberBandPrimitivesChanged += _dim_RubberBandPrimitivesChanged;
            _dim.SelectionRectangleUpdated += _dim_SelectionRectangleUpdated;

            Workspace.InputService.PromptChanged += InputService_PromptChanged;
            Workspace.OutputService.LineWritten += OutputService_LineWritten;
            Workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            Workspace.SettingsService.SettingChanged += SettingsService_SettingChanged;
            Workspace.WorkspaceChanged += _workspace_WorkspaceChanged;

            _jsonRpc.StartListening();
        }

        private void _dim_CurrentSnapPointUpdated(object sender, TransformedSnapPoint? e)
        {
            if (e.HasValue)
            {
                var clientUpdate = new ClientUpdate();
                clientUpdate.TransformedSnapPoint = e;
                PushUpdate(clientUpdate);
            }
        }

        private void SettingsService_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.Settings = GetSettings();
            PushUpdate(clientUpdate);
        }

        private void InputService_PromptChanged(object sender, PromptChangedEventArgs e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.Prompt = e.Prompt;
            PushUpdate(clientUpdate);
        }

        private void OutputService_LineWritten(object sender, WriteLineEventArgs e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.OutputLines = new[] { e.Line };
            PushUpdate(clientUpdate);
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            var selectedEntities = Workspace.SelectedEntities.ToList();
            var clientUpdate = new ClientUpdate();

            // prepare selected drawing
            clientUpdate.SelectedEntitiesDrawing = new ClientDrawing(null);
            var fallBackColor = Workspace.SettingsService.GetValue<CadColor>(DisplaySettingsNames.BackgroundColor).GetAutoContrastingColor();
            foreach (var entity in selectedEntities)
            {
                foreach (var primitive in entity.GetPrimitives(Workspace.Drawing.Settings))
                {
                    AddPrimitiveToDrawing(clientUpdate.SelectedEntitiesDrawing, primitive, null, fallBackColor);
                }
            }

            // update property pane
            ClientPropertyPane propertyPane;
            if (selectedEntities.Count == 0)
            {
                // nothing selected; send emtpy set to clear ui
                propertyPane = new ClientPropertyPane();
            }
            else
            {
                // only get properties common to all entities
                propertyPane = new ClientPropertyPane(Workspace.GetPropertyPaneValues());
            }

            clientUpdate.PropertyPane = propertyPane;

            PushUpdate(clientUpdate);
        }

        private void _dim_CursorStateUpdated(object sender, CursorState e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.CursorState = e;
            PushUpdate(clientUpdate);
        }

        private void _dim_HotPointsUpdated(object sender, IEnumerable<Point> e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.HotPoints = e.Select(p => new ClientPoint(p)).ToArray();
            PushUpdate(clientUpdate);
        }

        private void _dim_RubberBandPrimitivesChanged(object sender, IEnumerable<IPrimitive> primitives)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.RubberBandDrawing = new ClientDrawing(null);
            var fallBackColor = Workspace.SettingsService.GetValue<CadColor>(DisplaySettingsNames.BackgroundColor).GetAutoContrastingColor();
            foreach (var primitive in primitives)
            {
                AddPrimitiveToDrawing(clientUpdate.RubberBandDrawing, primitive, Array.Empty<double>(), fallBackColor);
            }

            PushUpdate(clientUpdate);
        }

        private void _dim_SelectionRectangleUpdated(object sender, SelectionState? e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.SelectionState = e;
            PushUpdate(clientUpdate);
        }

        private void _workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            var doUpdate = false;
            var clientUpdate = new ClientUpdate();

            if (e.IsDirtyChange)
            {
                doUpdate = true;
                clientUpdate.IsDirty = true;
            }

            if (e.IsActiveViewPortChange)
            {
                doUpdate = true;
                clientUpdate.Transform = GetDisplayTransform();
            }

            if (e.IsDrawingChange)
            {
                doUpdate = true;
                clientUpdate.Drawing = GetDrawing();
            }

            if (doUpdate)
            {
                PushUpdate(clientUpdate);
            }
        }

        private void PushUpdate(ClientUpdate clientUpdate)
        {
            var _ = _jsonRpc.NotifyAsync("ClientUpdate", clientUpdate);
        }

        public void Ready(double width, double height)
        {
            _dim.Resize(width, height);
            var clientUpdate = new ClientUpdate()
            {
                Drawing = GetDrawing(),
                Settings = GetSettings(),
                Transform = GetDisplayTransform(),
            };
            PushUpdate(clientUpdate);
            if (!_readyEventFired)
            {
                _readyEventFired = true;
                IsReady?.Invoke(this, new EventArgs());
            }
        }

        public void Resize(double width, double height)
        {
            _dim.Resize(width, height);
            var clientUpdate = new ClientUpdate()
            {
                Transform = GetDisplayTransform(),
            };
            PushUpdate(clientUpdate);
        }

        private ClientSettings GetSettings()
        {
            return new ClientSettings(Workspace);
        }

        private ClientTransform GetDisplayTransform()
        {
            var transform = Workspace.ActiveViewPort.GetDisplayTransformDirect3DStyle(_dim.Width, _dim.Height);
            var transformArray = transform.Transform.ToTransposeArray();
            if (transformArray.Any(double.IsNaN))
            {
                return null;
            }

            var canvasTransform = Workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(_dim.Width, _dim.Height);
            var canvasTransformArray = canvasTransform.ToTransposeArray();
            if (canvasTransformArray.Any(double.IsNaN))
            {
                return null;
            }

            return new ClientTransform(transformArray, canvasTransformArray, transform.DisplayXScale, transform.DisplayYScale);
        }

        public void WriteOutputLine(string line)
        {
            Workspace.OutputService.WriteLine(line);
        }

        public void ChangeCurrentDimensionStyle(string currentDimensionStyle)
        {
            Workspace.SetCurrentDimensionStyle(currentDimensionStyle);
        }

        public void ChangeCurrentLayer(string currentLayer)
        {
            Workspace.SetCurrentLayer(currentLayer);
        }

        public void ChangeCurrentLineType(string currentLineType)
        {
            if (currentLineType == "(Auto)")
            {
                currentLineType = null;
            }

            var currentLineTypeSpecification = Workspace.Drawing.Settings.CurrentLineTypeSpecification?.Update(name: currentLineType) ?? new LineTypeSpecification(currentLineType, 1.0);
            Workspace.Update(drawing: Workspace.Drawing.Update(settings: Workspace.Drawing.Settings.Update(currentLineTypeSpecification: currentLineTypeSpecification)));
        }

        public void Cancel()
        {
            Workspace.InputService.Cancel();
        }

        public void MouseDown(MouseButton button, double cursorX, double cursorY)
        {
            var _ = _dim.MouseDown(new Point(cursorX, cursorY, 0.0), button);
        }

        public void MouseUp(MouseButton button, double cursorX, double cursorY)
        {
            _dim.MouseUp(new Point(cursorX, cursorY, 0.0), button);
        }

        public void MouseMove(double cursorX, double cursorY)
        {
            _dim.MouseMove(new Point(cursorX, cursorY, 0.0));
        }

        public Task<bool> ExecuteCommand(string command)
        {
            return Workspace.ExecuteCommand(command);
        }

        public async Task<bool> ExecuteScript(string extension, string script)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".lisp":
                    var result = await Workspace.EvaluateAsync(script);
                    if (!result.IsNil())
                    {
                        Workspace.OutputService.WriteLine(result.ToString());
                    }
                    return result is not LispError;
                case ".scr":
                    return await Workspace.ExecuteTokensFromScriptAsync(script);
                default:
                    Workspace.OutputService.WriteLine($"Unsupported script type {extension}");
                    return false;
            }
        }

        public async Task ParseFile(string filePath, string data)
        {
            var bytes = Convert.FromBase64String(data);
            using var stream = new MemoryStream(bytes);

            var result = await Workspace.ReaderWriterService.ReadDrawing(filePath, stream, Workspace.FileSystemService.GetContentResolverRelativeToPath(filePath));
            if (result.Success)
            {
                Workspace.Update(drawing: result.Drawing, activeViewPort: result.ViewPort);
            }
        }

        public async Task<string> GetDrawingContents(string filePath, bool preserveSettings)
        {
            using var stream = new MemoryStream();
            var success = await Workspace.ReaderWriterService.TryWriteDrawing(filePath, Workspace.Drawing, Workspace.ActiveViewPort, stream, preserveSettings);
            if (success)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var bytes = stream.ToArray();
                var contents = Convert.ToBase64String(bytes);
                return contents;
            }

            return null;
        }

        public async Task<string> GetPlotPreview(ClientPlotSettings settings)
        {
            var htmlDialogService = (HtmlDialogService)Workspace.DialogService;
            var viewModel = (SvgPlotterViewModel)htmlDialogService.CreateAndPopulateViewModel(settings, Workspace.Drawing.Settings, plotTypeOverride: "svg"); // force to svg for preview
            viewModel.PlotAsDocument = false;
            viewModel.OutputWidth = settings.PreviewMaxSize;
            viewModel.OutputHeight = settings.PreviewMaxSize;
            if (settings.Width > settings.Height)
            {
                // keep width, reset height
                viewModel.OutputHeight = settings.Height / settings.Width * settings.PreviewMaxSize;
            }
            else
            {
                // keep height, reset width
                viewModel.OutputWidth = settings.Width / settings.Height * settings.PreviewMaxSize;
            }

            // plot previews are generated as svg, so pdf margins need to be manually adjusted
            if (settings.PlotType == "pdf")
            {
                var marginUnit = settings.MarginUnit == "in" ? PdfMeasurementType.Inch : PdfMeasurementType.Mm;
                var marginMeasurement = new PdfMeasurement(viewModel.Margin, marginUnit);
                var marginScale = viewModel.OutputHeight / viewModel.DisplayHeight;
                var scaledRawMargin = marginMeasurement.AsPoints() * marginScale;
                viewModel.Margin = scaledRawMargin;
            }

            var drawing = Workspace.Drawing.UpdateColors(settings.ColorType);
            using (var stream = await htmlDialogService.PlotToStream(viewModel, drawing, Workspace.ActiveViewPort))
            using (var reader = new StreamReader(stream))
            {
                var contents = await reader.ReadToEndAsync();
                return contents;
            }
        }

        public async Task<ClientRectangle?> GetSelectionRectangle()
        {
            var rect = await _dim.GetSelectionRectangle();
            if (rect.HasValue)
            {
                return new ClientRectangle(rect.GetValueOrDefault().TopLeftScreen, rect.GetValueOrDefault().BottomRightScreen);
            }

            return null;
        }

        public async Task InputChanged(string value)
        {
            if ((Workspace.InputService.AllowedInputTypes & InputType.Text) != InputType.Text)
            {
                // if not inputting text (because it allows spaces)
                if (value.StartsWith("("))
                {
                    // lisp-like input, don't do anything until the user manually submits it
                    return;
                }
                else if (value.EndsWith(" "))
                {
                    // space is a completion character
                    await SubmitInput(value.Substring(0, value.Length - 1));
                    if (_jsonRpc is not null)
                    {
                        // null check allows for unit tests to be happy
                        await _jsonRpc.InvokeAsync<JObject>("clearInput");
                    }
                }
            }
        }

        public async Task SubmitInput(string value)
        {
            if ((Workspace.InputService.AllowedInputTypes & InputType.Text) != InputType.Text &&
                value.StartsWith("("))
            {
                // if not inputting text and it looks like a lisp expression, evaluate it then submit that
                var result = await Workspace.EvaluateAsync(value);
                var submitValue = result is LispString s
                    ? s.Value
                    : result.ToString();
                await _dim.SubmitInputAsync(submitValue);
            }
            else
            {
                // otherwise just submit it directly
                await _dim.SubmitInputAsync(value);
            }
        }

        public void SetPropertyPaneValue(ClientPropertyPaneValue propertyPaneValue)
        {
            var availablePropertyPaneValues = Workspace.GetPropertyPaneValues().ToList();
            var matchingPropertyPaneValue = availablePropertyPaneValues.Single(x => x.Name == propertyPaneValue.Name);
            var didUpdate = false;
            var drawing = Workspace.Drawing;
            var newSelectedEntities = new HashSet<Entity>();
            foreach (var selectedEntity in Workspace.SelectedEntities)
            {
                if (matchingPropertyPaneValue.TryDoUpdate(drawing, selectedEntity, propertyPaneValue.Value, out var updatedDrawingAndEntity))
                {
                    drawing = updatedDrawingAndEntity.Item1;
                    didUpdate = true;
                    newSelectedEntities.Add(updatedDrawingAndEntity.Item2 ?? selectedEntity);
                }
                else
                {
                    newSelectedEntities.Add(selectedEntity);
                }
            }

            if (didUpdate)
            {
                Workspace.Update(drawing: drawing);
                Workspace.SelectedEntities.Set(newSelectedEntities);
            }
        }

        private ClientDrawing GetDrawing()
        {
            var drawing = Workspace.Drawing;
            var clientDrawing = new ClientDrawing(drawing.Settings.FileName);
            clientDrawing.CurrentLayer = drawing.Settings.CurrentLayerName;
            foreach (var layer in drawing.GetLayers())
            {
                var layerLineType = drawing.GetLineTypeFromLayer(layer);
                var layerLineTypePattern = layerLineType is not null
                    ? layerLineType.Pattern.Select(p => p * layer.LineTypeSpecification.Scale).ToArray()
                    : Array.Empty<double>();
                clientDrawing.Layers.Add(layer.Name);
                if (layer.IsVisible)
                {
                    foreach (var entity in layer.GetEntities())
                    {
                        var entityColor = entity.Color ?? layer.Color;
                        var entityLineType = drawing.GetLineTypeFromEntity(entity);
                        var entityLineTypePattern = entityLineType is not null
                            ? entityLineType.Pattern.Select(p => p * entity.LineTypeSpecification.Scale).ToArray()
                            : layerLineTypePattern;
                        foreach (var primitive in entity.GetPrimitives(drawing.Settings))
                        {
                            AddPrimitiveToDrawing(clientDrawing, primitive, entityLineTypePattern, fallBackColor: entityColor);
                        }
                    }
                }
            }

            clientDrawing.CurrentLineType = drawing.Settings.CurrentLineTypeSpecification?.Name;
            clientDrawing.LineTypes.AddRange(drawing.GetLineTypes().Select(lt => lt.Name));

            clientDrawing.CurrentDimensionStyle = drawing.Settings.CurrentDimensionStyleName;
            clientDrawing.DimensionStyles.AddRange(drawing.Settings.DimensionStyles.Select(d => d.Name));

            clientDrawing.Layers.Sort();
            return clientDrawing;
        }

        private void AddPrimitiveToDrawing(ClientDrawing clientDrawing, IPrimitive primitive, double[] linePattern, CadColor? fallBackColor)
        {
            var primitiveColor = primitive.Color ?? fallBackColor;
            primitive.DoPrimitive(
                ellipse =>
                {
                    var startAngle = ellipse.StartAngle.CorrectAngleDegrees();
                    var endAngle = ellipse.EndAngle.CorrectAngleDegrees();
                    if (endAngle <= startAngle)
                    {
                        endAngle += 360.0;
                    }
                    var transform = Matrix4.CreateScale(1.0, 1.0, 0.0) * ellipse.FromUnitCircle; // flatten display in z-plane
                    clientDrawing.Ellipses.Add(new ClientEllipse(startAngle, endAngle, transform.ToTransposeArray(), primitiveColor, linePattern));
                },
                line => clientDrawing.Lines.Add(new ClientLine(line.P1, line.P2, primitiveColor, linePattern)),
                point => clientDrawing.Points.Add(new ClientPointLocation(point.Location, primitiveColor)),
                text => clientDrawing.Text.Add(new ClientText(text.Value, text.Location, text.Height, text.Rotation, primitiveColor)),
                bezier => clientDrawing.Beziers.Add(new ClientBezier(bezier.P1, bezier.P2, bezier.P3, bezier.P4, primitiveColor, linePattern)),
                image => clientDrawing.Images.Add(new ClientImage(image.Location, Convert.ToBase64String(image.ImageData), image.Path, image.Width, image.Height, image.Rotation, primitiveColor)),
                triangle => clientDrawing.Triangles.Add(new ClientTriangle(triangle.P1, triangle.P2, triangle.P3, primitiveColor))
            );
        }

        public void Pan(double dx, double dy)
        {
            _dim.Pan(dx, dy);
        }

        public void Zoom(int cursorX, int cursorY, double delta)
        {
            var direction = delta < 0.0 ? ZoomDirection.Out : ZoomDirection.In;
            _dim.Zoom(direction, new Point(cursorX, cursorY, 0.0));
        }

        public async Task<JObject> ShowDialog(string id, object parameter)
        {
            var result = await _jsonRpc.InvokeAsync<JObject>("ShowDialog", new { id, parameter });
            return result;
        }

        public void SetSetting(string name, string value)
        {
            Workspace.SettingsService.SetValue(name, value);
        }

        public string GetVersionInformation()
        {
            if (_versionHtml is null)
            {
                // published app names
                var candidateAssemblyNames = new[]
                {
                    "bcad.exe",
                    "bcad.dll",
                    "bcad"
                };
                var assemblyPath = candidateAssemblyNames
                    .Select(name => Path.Combine(AppContext.BaseDirectory, name))
                    .FirstOrDefault(File.Exists)
                    ?? Assembly.GetExecutingAssembly().Location; // fall back to using reflection
                var versionString = assemblyPath is { }
                    ? FileVersionInfo.GetVersionInfo(assemblyPath).ProductVersion
                    : "<unknown>";
                _versionHtml = $"""
                    Version: {versionString}<br />
                    """;
            }

            return _versionHtml;
        }
    }
}
