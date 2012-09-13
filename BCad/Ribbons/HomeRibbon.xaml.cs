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
                case Constants.DrawingString:
                    this.Dispatcher.BeginInvoke((Action)(() => PopulateDropDown()));
                    break;
                default:
                    break;
            }
        }

        private void PopulateDropDown()
        {
            listenToChangeEvent = false;
            this.currentLayer.Items.Clear();
            foreach (var layer in workspace.Drawing.Layers.Values.OrderBy(l => l.Name))
                this.currentLayer.Items.Add(layer);
            this.currentLayer.SelectedItem = workspace.Drawing.CurrentLayer;
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
            {
                var layer = (Layer)this.currentLayer.SelectedItem;
                workspace.Drawing = workspace.Drawing.Update(currentLayerName: layer.Name);
            }
        }
    }
}
