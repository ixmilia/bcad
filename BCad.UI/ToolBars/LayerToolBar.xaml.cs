using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using System.ComponentModel;

namespace BCad.UI.ToolBars
{
    /// <summary>
    /// Interaction logic for LayerToolBar.xaml
    /// </summary>
    [ExportToolbar]
    public partial class LayerToolBar : ToolBar, IPartImportsSatisfiedNotification
    {
        public LayerToolBar()
        {
            InitializeComponent();
        }

        [Import]
        public IWorkspace Workspace { get; set; }

        private bool listenToChangeEvent = true;

        public void OnImportsSatisfied()
        {
            // subscribe to events
            Workspace.PropertyChanged += Workspace_PropertyChanged;

            // populate the list
            PopulateDropDown();
        }

        void Workspace_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentLayer":
                    if (listenToChangeEvent)
                        this.Dispatcher.BeginInvoke((Action)(() => this.currentLayer.SelectedItem = Workspace.CurrentLayer));
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
            string currentLayerName = Workspace.CurrentLayer.Name;
            this.currentLayer.Items.Clear();
            foreach (var layer in Workspace.Document.Layers.Values.OrderBy(l => l.Name))
                this.currentLayer.Items.Add(layer);
            if (preserveCurrent && Workspace.Document.Layers.ContainsKey(currentLayerName))
                this.currentLayer.SelectedItem = Workspace.Document.Layers[currentLayerName];
            else
                this.currentLayer.SelectedItem = Workspace.CurrentLayer;
            listenToChangeEvent = true;
        }

        private void currentLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listenToChangeEvent)
                Workspace.CurrentLayer = (Layer)this.currentLayer.SelectedItem;
        }

        private void Layers_Click(object sender, RoutedEventArgs e)
        {
            Workspace.ExecuteCommand("Edit.Layers");
        }
    }
}
