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
using BCad.EventArguments;

namespace BCad.UI.Views
{
    /// <summary>
    /// Interaction logic for Direct3DViewControl.xaml
    /// </summary>
    [ExportViewControl("Direct3D")]
    public partial class Direct3DViewControl : ViewControl, IPartImportsSatisfiedNotification
    {
        public Direct3DViewControl()
        {
            InitializeComponent();
        }

        //[Import]
        //private IView View = null;

        //[Import]
        //private IInputService InputService = null;

        //[Import]
        //private IWorkspace Workspace = null;

        public void OnImportsSatisfied()
        {
            //InputService.RubberBandGeneratorChanged += UserConsole_RubberBandGeneratorChanged;
            //View.ViewPortChanged += TransformationMatrixChanged;
            //Workspace.CommandExecuted += Workspace_CommandExecuted;
            //Workspace.DocumentChanging += DocumentChanging;
            //Workspace.DocumentChanged += DocumentChanged;
        }

        public override Point GetCursorPoint()
        {
            throw new NotImplementedException();
        }
    }
}
