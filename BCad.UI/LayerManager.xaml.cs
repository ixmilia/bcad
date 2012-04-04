using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for LayerManager.xaml
    /// </summary>
    public partial class LayerManager : BCadControl
    {
        public IWorkspace Workspace { get; private set; }

        private Document updatedDocument;

        [Obsolete("Default constructor is for WPF designer only")]
        public LayerManager()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                InitializeComponent();
            }
            else
            {
                throw new Exception("Default constructor is for WPF designer only");
            }
        }

        public LayerManager(IWorkspace workspace)
        {
            InitializeComponent();
            this.Workspace = workspace;
            this.updatedDocument = this.Workspace.Document;
        }

        public override void Initialize()
        {
            this.layerList.Items.Clear();
            foreach (var layer in this.Workspace.Document.Layers.Values.OrderBy(l => l.Name))
            {
                this.layerList.Items.Add(layer);
            }
        }

        public override void Commit()
        {
            this.Workspace.Document = this.updatedDocument;
        }

        public override void Cancel()
        {
            // do nothing (changes are uncommitted)
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
