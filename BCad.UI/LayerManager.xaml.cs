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
using BCad.UI;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for LayerManager.xaml
    /// </summary>
    public partial class LayerManager : BCadControl<Document>
    {
        public LayerManager()
        {
            InitializeComponent();
        }
    }
}
