using System.Composition;
using System.Threading.Tasks;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Server
{
    [ExportWorkspaceService, Shared]
    public class HtmlDialogService : IDialogService
    {
        internal ServerAgent Agent { get; set; }

        public async Task<object> ShowDialog(string id, object parameter)
        {
            object result = null;
            switch (id)
            {
                case "layer":
                    var layerParameters = (LayerDialogParameters)parameter;
                    var clientLayerParameters = new ClientLayerParameters(layerParameters);
                    var resultObject = await Agent.ShowDialog(id, clientLayerParameters);
                    if (resultObject != null)
                    {
                        var clientLayerResult = resultObject.ToObject<ClientLayerResult>();
                        var layerDialogResult = clientLayerResult.ToDialogResult();
                        result = layerDialogResult;
                    }
                    break;
            }

            return result;
        }
    }
}
