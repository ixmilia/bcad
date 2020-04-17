namespace IxMilia.BCad.Core.Test
{
    public static class TestExtensions
    {
        public static Layer GetLayer(this IWorkspace workspace, string layerName)
        {
            return workspace.Drawing.Layers.GetValue(layerName);
        }

        public static void AddLayer(this IWorkspace workspace, string layerName)
        {
            workspace.Add(new Layer(layerName));
        }
    }
}
