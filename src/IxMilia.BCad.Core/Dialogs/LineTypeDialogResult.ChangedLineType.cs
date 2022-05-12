namespace IxMilia.BCad.Dialogs
{
    public partial class LineTypeDialogResult
    {
        public class LineTypeResult
        {
            public string OldLineTypeName { get; set; }
            public string NewLineTypeName { get; set; }
            public double[] Pattern { get; set; }
            public string Description { get; set; }
        }
    }
}
