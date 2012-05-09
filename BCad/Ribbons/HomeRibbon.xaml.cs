using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for HomeRibbon.xaml
    /// </summary>
    [Export(typeof(RibbonTab))]
    public partial class HomeRibbon : RibbonTab
    {
        private IWorkspace workspace = null;
        private bool listenToChangeEvent = true;

        public HomeRibbon()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public HomeRibbon(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;

            // subscribe to events
            this.workspace.PropertyChanged += SettingsChanged;

            // populate the layers
            PopulateDropDown();
        }

        void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentLayer":
                    if (listenToChangeEvent)
                        this.Dispatcher.BeginInvoke((Action)(() => this.currentLayer.SelectedItem = workspace.CurrentLayer));
                    break;
                case "Document":
                    this.Dispatcher.BeginInvoke((Action)(() => PopulateDropDown(true)));
                    break;
                default:
                    break;
            }
        }

        private void PopulateDropDown(bool preserveCurrent = false)
        {
            listenToChangeEvent = false;
            string currentLayerName = workspace.CurrentLayer.Name;
            this.currentLayer.Items.Clear();
            foreach (var layer in workspace.Document.Layers.Values.OrderBy(l => l.Name))
                this.currentLayer.Items.Add(layer);
            if (preserveCurrent && workspace.Document.Layers.ContainsKey(currentLayerName))
                this.currentLayer.SelectedItem = workspace.Document.Layers[currentLayerName];
            else
                this.currentLayer.SelectedItem = workspace.CurrentLayer;
            listenToChangeEvent = true;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = workspace == null ? false : workspace.CanExecute();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Assert(workspace != null, "Workspace should not have been null");
            var command = e.Parameter as string;
            Debug.Assert(command != null, "Command string should not have been null");
            workspace.ExecuteCommand(command);
        }

        private void CurrentLayerSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (listenToChangeEvent)
                workspace.CurrentLayer = (Layer)this.currentLayer.SelectedItem;
        }
    }
}
