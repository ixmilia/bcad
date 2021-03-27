using System.Threading.Tasks;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.FileHandlers;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Server
{
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
                default:
                    var settingsResult = await Agent.ShowDialog(id, parameter);
                    if (settingsResult != null)
                    {
                        result = settingsResult.ToObject<DxfFileSettings>();
                    }
                    break;
            }

            return result;
        }
    }
}
