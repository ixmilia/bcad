using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Core.Services;
using IxMilia.BCad.Display;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Services;
using IxMilia.BCad.Settings;

namespace IxMilia.BCad
{
    public abstract class WorkspaceBase : IWorkspace
    {
        private RubberBandGenerator rubberBandGenerator;
        private readonly Regex settingsPattern = new Regex(@"^/p:([a-zA-Z\.]+)=(.*)$");

        private List<CadCommandInfo> _commands = new List<CadCommandInfo>();
        private List<IWorkspaceService> _services = new List<IWorkspaceService>();

        public IReadOnlyCollection<CadCommandInfo> Commands => _commands;

        public WorkspaceBase()
        {
            Drawing = new Drawing();
            DrawingPlane = new Plane(Point.Origin, Vector.ZAxis);
            ActiveViewPort = ViewPort.CreateDefaultViewPort();
            SelectedEntities = new ObservableHashSet<Entity>();
            ViewControl = null;
            RubberBandGenerator = null;

            RegisterDefaultCommands();
            RegisterDefaultServices();
            RegisterDefaultSettings();
        }

        private void RegisterDefaultCommands()
        {
            RegisterCommand(new CadCommandInfo("Edit.Copy", "COPY", new CopyCommand(), ModifierKeys.Control, Key.C, "copy", "co"));
            RegisterCommand(new CadCommandInfo("Debug.Dump", "DUMP", new DebugDumpCommand(), "dump"));
            RegisterCommand(new CadCommandInfo("Debug.Attach", "ATTACH", new DebuggerAttachCommand(), "attach"));
            RegisterCommand(new CadCommandInfo("Edit.Delete", "DELETE", new DeleteCommand(), ModifierKeys.None, Key.Delete, "delete", "d", "del"));
            RegisterCommand(new CadCommandInfo("View.Distance", "DIST", new DistanceCommand(), "distance", "di", "dist"));
            RegisterCommand(new CadCommandInfo("Draw.Arc", "ARC", new DrawArcCommand(), "arc", "a"));
            RegisterCommand(new CadCommandInfo("Draw.Circle", "CIRCLE", new DrawCircleCommand(), "circle", "c", "cir"));
            RegisterCommand(new CadCommandInfo("Draw.Ellipse", "ELLIPSE", new DrawEllipseCommand(), "ellipse", "el"));
            RegisterCommand(new CadCommandInfo("Draw.Image", "IMAGE", new DrawImageCommand(), "image", "i"));
            RegisterCommand(new CadCommandInfo("Draw.Line", "LINE", new DrawLineCommand(), "line", "l"));
            RegisterCommand(new CadCommandInfo("Draw.Point", "POINT", new DrawPointCommand(), "point", "p"));
            RegisterCommand(new CadCommandInfo("Draw.Polygon", "POLYGON", new DrawPolygonCommand(), "polygon", "pg"));
            RegisterCommand(new CadCommandInfo("Draw.PolyLine", "POLYLINE", new DrawPolyLineCommand(), "polyline", "pl"));
            RegisterCommand(new CadCommandInfo("Draw.Rectangle", "RECTANGLE", new DrawRectangleCommand(), "rectangle", "rect"));
            RegisterCommand(new CadCommandInfo("Draw.Text", "TEXT", new DrawTextCommand(), "text", "t"));
            RegisterCommand(new CadCommandInfo("Edit.Extend", "EXTEND", new ExtendCommand(), "extend", "ex"));
            RegisterCommand(new CadCommandInfo("Edit.Intersection", "INTERSECTION", new IntersectionCommand(), "intersection", "int"));
            RegisterCommand(new CadCommandInfo("Edit.JoinPolyline", "PJOIN", new JoinPolylineCommand(), "pjoin"));
            RegisterCommand(new CadCommandInfo("Edit.Layers", "LAYERS", new LayersCommand(), ModifierKeys.Control, Key.L, "layers", "layer", "la"));
            RegisterCommand(new CadCommandInfo("Edit.Move", "MOVE", new MoveCommand(), "move", "mov", "m"));
            RegisterCommand(new CadCommandInfo("File.New", "NEW", new NewCommand(), ModifierKeys.Control, Key.N, "new", "n"));
            RegisterCommand(new CadCommandInfo("Edit.Offset", "OFFSET", new OffsetCommand(), "offset", "off", "of"));
            RegisterCommand(new CadCommandInfo("Edit.Scale", "SCALE", new ScaleCommand(), "scale", "sc"));
            RegisterCommand(new CadCommandInfo("File.Open", "OPEN", new OpenCommand(), ModifierKeys.Control, Key.O, "open", "o"));
            RegisterCommand(new CadCommandInfo("View.Pan", "PAN", new PanCommand(), "pan", "p"));
            RegisterCommand(new CadCommandInfo("File.Plot", "PLOT", new PlotCommand(), ModifierKeys.Control | ModifierKeys.Alt, Key.P, "plot"));
            RegisterCommand(new CadCommandInfo("Edit.Quantize", "QUANTIZE", new QuantizeCommand(), "quant"));
            RegisterCommand(new CadCommandInfo("Edit.Redo", "REDO", new RedoCommand(), ModifierKeys.Control, Key.Y, "redo", "re", "r"));
            RegisterCommand(new CadCommandInfo("Edit.Rotate", "ROTATE", new RotateCommand(), "rotate", "rot", "ro"));
            RegisterCommand(new CadCommandInfo("File.Save", "SAVE", new SaveCommand(), ModifierKeys.Control, Key.S, "save", "s"));
            RegisterCommand(new CadCommandInfo("File.SaveAs", "SAVEAS", new SaveAsCommand(), ModifierKeys.Control | ModifierKeys.Shift, Key.S, "saveas", "sa"));
            RegisterCommand(new CadCommandInfo("Edit.Subtract", "SUBTRACT", new SubtractCommand(), "subtract", "sub"));
            RegisterCommand(new CadCommandInfo("Edit.Trim", "TRIM", new TrimCommand(), "trim", "tr"));
            RegisterCommand(new CadCommandInfo("Edit.Undo", "UNDO", new UndoCommand(), ModifierKeys.Control, Key.Z, "undo", "u"));
            RegisterCommand(new CadCommandInfo("Edit.Union", "UNION", new UnionCommand(), "union", "un"));
            RegisterCommand(new CadCommandInfo("Zoom.Extents", "ZOOMEXTENTS", new ZoomExtentsCommand(), "zoomextents", "ze"));
            RegisterCommand(new CadCommandInfo("Zoom.Window", "ZOOMWINDOW", new ZoomWindowCommand(), "zoomwindow", "zw"));
        }

        private void RegisterDefaultServices()
        {
            RegisterService<IDebugService>(new DebugService());
            RegisterService<IReaderWriterService>(new ReaderWriterService(this));
            RegisterService<IInputService>(new InputService(this));
            RegisterService<IOutputService>(new OutputService());
            RegisterService<ISettingsService>(new SettingsService(this));
            RegisterService<IUndoRedoService>(new UndoRedoService(this));
        }

        private void RegisterDefaultSettings()
        {
            SettingsService.RegisterSetting(DefaultSettingsNames.Debug, typeof(bool), false);
            SettingsService.RegisterSetting(DefaultSettingsNames.DrawingPrecision, typeof(int), 16);
            SettingsService.RegisterSetting(DefaultSettingsNames.DrawingUnits, typeof(UnitFormat), UnitFormat.Architectural);
            SettingsService.RegisterSetting(DisplaySettingsNames.AngleSnap, typeof(bool), true);
            SettingsService.RegisterSetting(DisplaySettingsNames.BackgroundColor, typeof(CadColor), "#FF2F2F2F");
            SettingsService.RegisterSetting(DisplaySettingsNames.CursorSize, typeof(int), 60);
            SettingsService.RegisterSetting(DisplaySettingsNames.EntitySelectionRadius, typeof(double), 3.0);
            SettingsService.RegisterSetting(DisplaySettingsNames.HotPointColor, typeof(CadColor), "#FF0000FF");
            SettingsService.RegisterSetting(DisplaySettingsNames.HotPointSize, typeof(double), 10.0);
            SettingsService.RegisterSetting(DisplaySettingsNames.Ortho, typeof(bool), false);
            SettingsService.RegisterSetting(DisplaySettingsNames.PointSnap, typeof(bool), true);
            SettingsService.RegisterSetting(DisplaySettingsNames.SnapAngleDistance, typeof(double), 30.0);
            SettingsService.RegisterSetting(DisplaySettingsNames.SnapAngles, typeof(double[]), new[] { 0.0, 90.0, 180.0, 270.0 });
            SettingsService.RegisterSetting(DisplaySettingsNames.SnapPointColor, typeof(CadColor), "#FFFFFF00");
            SettingsService.RegisterSetting(DisplaySettingsNames.SnapPointDistance, typeof(double), 15.0);
            SettingsService.RegisterSetting(DisplaySettingsNames.SnapPointSize, typeof(double), 15.0);
            SettingsService.RegisterSetting(DisplaySettingsNames.TextCursorSize, typeof(int), 18);
            SettingsService.RegisterSetting(DisplaySettingsNames.PointDisplaySize, typeof(double), 48.0);

            SettingsService.SettingChanged += (o, e) =>
            {
                DrawingSettings newSettings = null;
                switch (e.SettingName)
                {
                    case DefaultSettingsNames.DrawingPrecision:
                        newSettings = Drawing.Settings.Update(unitPrecision: SettingsService.GetValue<int>(e.SettingName));
                        break;
                    case DefaultSettingsNames.DrawingUnits:
                        newSettings = Drawing.Settings.Update(unitFormat: SettingsService.GetValue<UnitFormat>(e.SettingName));
                        break;
                }

                if (newSettings is object)
                {
                    var newDrawing = Drawing.Update(settings: newSettings);
                    Update(drawing: newDrawing);
                }
            };
        }

        #region Events

        public event CommandExecutingEventHandler CommandExecuting;

        protected virtual void OnCommandExecuting(CadCommandExecutingEventArgs e)
        {
            var executing = CommandExecuting;
            if (executing != null)
                executing(this, e);
        }

        public event CommandExecutedEventHandler CommandExecuted;

        protected virtual void OnCommandExecuted(CadCommandExecutedEventArgs e)
        {
            var executed = CommandExecuted;
            if (executed != null)
                executed(this, e);
        }

        public event EventHandler RubberBandGeneratorChanged;

        protected virtual void OnRubberBandGeneratorChanged(EventArgs e)
        {
            var changed = RubberBandGeneratorChanged;
            if (changed != null)
                changed(this, e);
        }

        #endregion

        #region Properties

        public bool IsDirty { get; private set; }

        public Drawing Drawing { get; private set; }

        public Plane DrawingPlane { get; private set; }

        public ViewPort ActiveViewPort { get; private set; }

        public IViewControl ViewControl { get; private set; }

        public ObservableHashSet<Entity> SelectedEntities { get; private set; }

        public RubberBandGenerator RubberBandGenerator
        {
            get { return rubberBandGenerator; }
            set
            {
                if (rubberBandGenerator == value)
                    return;
                rubberBandGenerator = value;
                OnRubberBandGeneratorChanged(new EventArgs());
            }
        }

        public bool IsDrawing { get { return RubberBandGenerator != null; } }

        public bool IsCommandExecuting { get; private set; }

        #endregion

        #region IWorkspace implementation

        public void RegisterService<TService>(TService service) where TService : class, IWorkspaceService
        {
            _services.Add(service);
        }

        public TService GetService<TService>() where TService : class, IWorkspaceService
        {
            return _services.OfType<TService>().SingleOrDefault();
        }

        // well-known services
        private IDebugService _debugServiceCache;
        public IDebugService DebugService => CacheService<IDebugService>(ref _debugServiceCache);

        private IDialogService _dialogServiceCache;
        public IDialogService DialogService => CacheService<IDialogService>(ref _dialogServiceCache);

        private IFileSystemService _fileSystemServiceCache;
        public IFileSystemService FileSystemService => CacheService<IFileSystemService>(ref _fileSystemServiceCache);

        private IInputService _inputServiceCache;
        public IInputService InputService => CacheService<IInputService>(ref _inputServiceCache);

        private IOutputService _outputServiceCache;
        public IOutputService OutputService => CacheService<IOutputService>(ref _outputServiceCache);

        private IReaderWriterService _readerWriterServiceCache;
        public IReaderWriterService ReaderWriterService => CacheService<IReaderWriterService>(ref _readerWriterServiceCache);

        private ISettingsService _settingsService;
        public ISettingsService SettingsService => CacheService<ISettingsService>(ref _settingsService);

        private IUndoRedoService _undoRedoServiceCache;
        public IUndoRedoService UndoRedoService => CacheService<IUndoRedoService>(ref _undoRedoServiceCache);

        private TService CacheService<TService>(ref TService backingStore) where TService : class, IWorkspaceService
        {
            if (backingStore == null)
            {
                backingStore = GetService<TService>();
            }

            return backingStore;
        }

        public async Task Initialize(params string[] args)
        {
            string fileName = null;
            foreach (var arg in args)
            {
                var match = settingsPattern.Match(arg);
                if (match.Success)
                {
                    var settingName = match.Groups[1].Value;
                    var settingValue = match.Groups[2].Value;
                    SettingsService.SetValue(settingName, settingValue);
                }
                else
                {
                    // try to match a file to open
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        throw new NotSupportedException("More than one file specified on the command line.");
                    }
                    else
                    {
                        fileName = arg;
                    }
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                Update(isDirty: false); // don't let the following command prompt for unsaved changes
                await ExecuteCommand("File.Open", fileName);
            }
        }

        public virtual void Update(
            Optional<Drawing> drawing = default(Optional<Drawing>),
            Optional<Plane> drawingPlane = default(Optional<Plane>),
            Optional<ViewPort> activeViewPort = default(Optional<ViewPort>),
            Optional<IViewControl> viewControl = default(Optional<IViewControl>),
            bool isDirty = true)
        {
            var e = new WorkspaceChangeEventArgs(
                drawing.HasValue,
                drawingPlane.HasValue,
                activeViewPort.HasValue,
                viewControl.HasValue,
                this.IsDirty != isDirty);

            OnWorkspaceChanging(e);
            if (drawing.HasValue)
                this.Drawing = drawing.Value;
            if (drawingPlane.HasValue)
                this.DrawingPlane = drawingPlane.Value;
            if (activeViewPort.HasValue)
                this.ActiveViewPort = activeViewPort.Value;
            if (viewControl.HasValue)
                this.ViewControl = viewControl.Value;
            this.IsDirty = isDirty;
            OnWorkspaceChanged(e);

            var selectedEntityIds = SelectedEntities.Select(ent => ent.Id).ToList();
        }

        public event WorkspaceChangingEventHandler WorkspaceChanging;

        protected void OnWorkspaceChanging(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanging;
            if (handler != null)
                handler(this, e);
        }

        public event WorkspaceChangedEventHandler WorkspaceChanged;

        protected void OnWorkspaceChanged(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanged;
            if (handler != null)
                handler(this, e);
        }

        private async Task<bool> Execute(Tuple<ICadCommand, string> commandPair, object arg)
        {
            var command = commandPair.Item1;
            var display = commandPair.Item2;
            var outputService = GetService<IOutputService>();
            OnCommandExecuting(new CadCommandExecutingEventArgs(command));
            outputService.WriteLine(display);
            bool result;
            try
            {
                result = await command.Execute(this, arg);
            }
            catch (Exception ex)
            {
                outputService.WriteLine("Error: {0} - {1}", ex.GetType().ToString(), ex.Message);
                result = false;
            }

            RubberBandGenerator = null;
            OnCommandExecuted(new CadCommandExecutedEventArgs(command));
            return result;
        }

        public void RegisterCommand(CadCommandInfo commandInfo)
        {
            _commands.Add(commandInfo);
        }

        public async Task<bool> ExecuteCommand(string commandName, object arg)
        {
            if (commandName == null && lastCommand == null)
            {
                return false;
            }

            lock (executeGate)
            {
                if (IsCommandExecuting)
                    return false;
                IsCommandExecuting = true;
            }

            commandName = commandName ?? lastCommand;
            var debugService = GetService<IDebugService>();
            var outputService = GetService<IOutputService>();
            debugService.Add(new WorkspaceLogEntry(string.Format("execute {0}", commandName)));
            var commandPair = GetCommand(commandName);
            if (commandPair == null)
            {
                outputService.WriteLine("Command {0} not found", commandName);
                IsCommandExecuting = false;
                return false;
            }

            var selectedStart = SelectedEntities;
            var result = await Execute(commandPair, arg);
            lastCommand = commandName;
            lock (executeGate)
            {
                IsCommandExecuting = false;
                SelectedEntities = selectedStart;
            }

            return result;
        }

        public bool CommandExists(string commandName)
        {
            return GetCommand(commandName) != null;
        }

        public bool CanExecute()
        {
            return !this.IsCommandExecuting;
        }

        public abstract Task<UnsavedChangesResult> PromptForUnsavedChanges();

        #endregion

        #region Privates

        private string lastCommand = null;
        private object executeGate = new object();

        public virtual Tuple<ICadCommand, string> GetCommand(string commandName)
        {
            var candidateCommands =
                from commandInfo in _commands
                where string.Compare(commandInfo.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                   || commandInfo.Aliases.Any(alias => string.Compare(alias, commandName, StringComparison.OrdinalIgnoreCase) == 0)
                select commandInfo;
            var command = candidateCommands.FirstOrDefault();
            if (candidateCommands.Count() > 1)
            {
                throw new InvalidOperationException($"Ambiguous command name '{commandName}'.  Possibilities: {string.Join(", ", candidateCommands.Select(c => c.Name))}");
            }

            return command == null ? null : Tuple.Create(command.Command, command.DisplayName);
        }

        #endregion
    }
}
