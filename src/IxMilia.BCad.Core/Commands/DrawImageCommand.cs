using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
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

            var imageData = await workspace.FileSystemService.GetContentResolverRelativeToPath(workspace.Drawing.Settings.FileName)(imagePath);
            var (rawImageWidth, rawImageHeight) = ImageHelpers.GetImageDimensions(imagePath, imageData);

            var imageWidthInput = await workspace.InputService.GetDistance(new UserDirective($"Width [{rawImageWidth}]"), defaultDistance: rawImageWidth);
            if (imageWidthInput.Cancel) return false;
            if (!imageWidthInput.HasValue) return false;
            var imageWidth = imageWidthInput.Value;

            var scaledRawHeight = (double)rawImageHeight / rawImageWidth * imageWidth;
            var imageHeightInput = await workspace.InputService.GetDistance(new UserDirective($"Height [{scaledRawHeight}]"), defaultDistance: scaledRawHeight);
            if (imageHeightInput.Cancel) return false;
            if (!imageHeightInput.HasValue) return false;
            var imageHeight = imageHeightInput.Value;

            var defaultRotation = 0;
            var imageRotationInput = await workspace.InputService.GetText($"Rotation [{defaultRotation}]");
            if (imageRotationInput.Cancel) return false;
            var rawRotationInput = imageRotationInput.HasValue && !string.IsNullOrWhiteSpace(imageRotationInput.Value) ? imageRotationInput.Value : defaultRotation.ToString();
            if (!double.TryParse(rawRotationInput, out var imageRotation)) return false;

            var image = new Image(location, imagePath, imageData, imageWidth, imageHeight, imageRotation, lineTypeSpecification: workspace.Drawing.Settings.CurrentLineTypeSpecification);
            workspace.AddToCurrentLayer(image);

            return true;
        }
    }
}
