using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BCad.EventArguments;

namespace BCad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            App.Container.SatisfyImportsOnce(this);
        }

        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IUserConsole UserConsole { get; set; }

        [Import]
        public IUserConsoleFactory UserConsoleFactory { get; set; }

        [Import]
        public IViewFactory ViewFactory { get; set; }

        [Import]
        public IView View { get; set; }

        [Import]
        public ICommandManager CommandManager { get; set; }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Workspace.Document = new Document();
            var console = UserConsoleFactory.Generate();
            this.inputPanel.Content = console;
            FocusHelper.Focus(this.inputPanel.Content as UserControl);
            UserConsole.Reset();

            var view = ViewFactory.Generate();
            this.viewPanel.Content = view;
            View.RegisteredControl = view;

            View.UpdateView(viewPoint: new Point(0, 0, 1),
                sight: Vector.ZAxis,
                up: Vector.YAxis,
                viewWidth: 300.0,
                bottomLeft: new Point(-10, -10, 0));

            Workspace.DocumentChanging += Workspace_DocumentChanging;
            Workspace.DocumentChanged += Workspace_DocumentChanged;
            Workspace.Document.DocumentDetailsChanged += Document_DocumentDetailsChanged;

            SetTitle(Workspace.Document);
        }

        private void Workspace_DocumentChanging(object sender, DocumentChangingEventArgs e)
        {
            e.OldDocument.DocumentDetailsChanged -= Document_DocumentDetailsChanged;
        }

        private void Workspace_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            e.Document.DocumentDetailsChanged += Document_DocumentDetailsChanged;
            e.Document.CurrentLayerChanged += Document_CurrentLayerChanged;
        }

        void Document_CurrentLayerChanged(object sender, CurrentLayerChangedEventArgs e)
        {
            // TODO: set current layer
        }

        private void Document_DocumentDetailsChanged(object sender, DocumentDetailsChangedEventArgs e)
        {
            SetTitle(e.Document);
        }

        private void SetTitle(Document document)
        {
            string filename = document.FileName == null ? "Untitled" : Path.GetFileName(document.FileName);
            Dispatcher.BeginInvoke((Action)(() =>
                this.Title = string.Format("BCad [{0}]{1}", filename, document.Dirty ? " *" : "")));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
                e.Cancel = true;
        }
    }

    static class FocusHelper
    {
        private delegate void MethodInvoker();

        public static void Focus(UIElement element)
        {
            //Focus in a callback to run on another thread, ensuring the main UI thread is initialized by the
            //time focus is set
            ThreadPool.QueueUserWorkItem(delegate(Object foo)
            {
                UIElement elem = (UIElement)foo;
                elem.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (MethodInvoker)delegate()
                    {
                        elem.Focus();
                        Keyboard.Focus(elem);
                    });
            }, element);
        }
    }
}
