namespace IxMilia.BCad.Dialogs
{
    public partial class LayerDialogResult
    {
        public class LayerResult
        {
            public string OldLayerName { get; set; }
            public string NewLayerName { get; set; }
            public CadColor? Color { get; set; }
            public bool IsVisible { get; set; }
        }
    }
}
