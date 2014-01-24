using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BCad.Entities;
using BCad.Services;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for XamlRenderer.xaml
    /// </summary>
    public partial class XamlRenderer : UserControl, IRenderer
    {
        private IViewControl ViewControl;
        private IInputService InputService;
        private DoubleCollection solidLine = new DoubleCollection();
        private DoubleCollection dashedLine = new DoubleCollection(new[] { 4.0, 4.0 });

        public XamlRenderer()
        {
            InitializeComponent();
        }

        public XamlRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
            : this()
        {
            Initialize(workspace);
            ViewControl = viewControl;
            InputService = inputService;

            Workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            var selectedEntities = Workspace.SelectedEntities;
            var children = PrimitiveCanvas.Children;
            foreach (FrameworkElement child in children)
            {
                var containingEntity = child.Tag as Entity;
                if (containingEntity != null)
                {
                    if (selectedEntities.Contains(containingEntity))
                        SetSelected(child);
                    else
                        SetUnselected(child);
                }
            }
        }

        private void SetSelected(FrameworkElement element)
        {
            if (element is Shape)
            {
                ((Shape)element).StrokeDashArray = dashedLine;
            }
            else if (element is TextBlock)
            {
                element.Opacity = 0.5;
            }
            else if (element is Grid)
            {
                var grid = (Grid)element;
                if (grid.Children.Count == 1 && grid.Children[0] is Path)
                {
                    ((Path)grid.Children[0]).StrokeDashArray = dashedLine;
                }
                else
                {
                    Debug.Fail("unexpected grid child");
                }
            }
            else
            {
                Debug.Fail("unexpected canvas child");
            }
        }

        private void SetUnselected(FrameworkElement element)
        {
            if (element is Shape)
            {
                ((Shape)element).StrokeDashArray = solidLine;
            }
            else if (element is TextBlock)
            {
                element.Opacity = 1.0;
            }
            else if (element is Grid)
            {
                var grid = (Grid)element;
                if (grid.Children.Count == 1 && grid.Children[0] is Path)
                {
                    ((Path)grid.Children[0]).StrokeDashArray = solidLine;
                }
                else
                {
                    Debug.Fail("unexpected grid child");
                }
            }
            else
            {
                Debug.Fail("unexpected canvas child");
            }
        }
    }
}
