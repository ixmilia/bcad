using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using BCad.UI;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for LayerManager.xaml
    /// </summary>
    public partial class LayerManager : BCadControl
    {
        public IWorkspace Workspace { get; private set; }

        // TODO: how to make designer not suck?
        //public LayerManager()
        //{
        //    InitializeComponent();
        //}

        public LayerManager(IWorkspace workspace)
        {
            InitializeComponent();

            this.Workspace = workspace;
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
            //throw new NotImplementedException();
        }

        public override void Cancel()
        {
            //throw new NotImplementedException();
        }
    }
}
