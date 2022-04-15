using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Commands
{
    public class DrawImageCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var locationInput = await workspace.InputService.GetPoint(new UserDirective("Location"));
            if (locationInput.Cancel) return false;
            if (!locationInput.HasValue) return false;
            var location = locationInput.Value;

            var imagePath = await workspace.FileSystemService.GetFileNameFromUserForOpen(new[] { new FileSpecification("JPEG Image", new[] { ".jpg", ".jpeg" }) });
            if (imagePath == null) return false;

            var imageWidthInput = await workspace.InputService.GetDistance(new UserDirective("Width"));
            if (imageWidthInput.Cancel) return false;
            if (!imageWidthInput.HasValue) return false;
            var imageWidth = imageWidthInput.Value;

            var imageHeightInput = await workspace.InputService.GetDistance(new UserDirective("Height"));
            if (imageHeightInput.Cancel) return false;
            if (!imageHeightInput.HasValue) return false;
            var imageHeight = imageHeightInput.Value;

            var imageRotationInput = await workspace.InputService.GetText("Rotation");
            if (imageRotationInput.Cancel) return false;
            if (string.IsNullOrEmpty(imageRotationInput.Value)) return false;
            if (!double.TryParse(imageRotationInput.Value, out var imageRotation)) return false;

            var imageData = await workspace.FileSystemService.GetContentResolverRelativeToPath(workspace.Drawing.Settings.FileName)(imagePath);
            var image = new Image(location, imagePath, imageData, imageWidth, imageHeight, imageRotation);
            workspace.AddToCurrentLayer(image);

            return true;
        }
    }
}
