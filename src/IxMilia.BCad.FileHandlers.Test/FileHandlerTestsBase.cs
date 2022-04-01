using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Core.Test;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    public abstract class FileHandlerTestsBase : TestBase
    {
        public abstract IFileHandler FileHandler { get; }

        protected virtual async Task<Entity> RoundTripEntity(Entity entity)
        {
            var drawing = new Drawing().Add(new Layer("layer").Add(entity));
            var result = await RoundTripDrawing(drawing);
            var roundTrippedEntity = result.GetLayers().Single(l => l.Name == "layer").GetEntities().Single();
            return roundTrippedEntity;
        }

        protected async Task VerifyRoundTrip(Entity entity)
        {
            var afterRoundTrip = await RoundTripEntity(entity);
            Assert.True(entity.EquivalentTo(afterRoundTrip));
        }

        public async Task VerifyRoundTrip(Layer layer)
        {
            var afterRoundTrip = await RoundTripLayer(layer);
            Assert.Equal(layer.Name, afterRoundTrip.Name);
            Assert.Equal(layer.Color, afterRoundTrip.Color);
            Assert.Equal(layer.IsVisible, afterRoundTrip.IsVisible);
            Assert.Equal(layer.EntityCount, afterRoundTrip.EntityCount);
        }

        public async Task<Drawing> RoundTripDrawing(Drawing drawing)
        {
            using (var stream = await WriteToStream(drawing))
            {
                return await ReadFromStream(stream);
            }
        }

        public async Task<Stream> WriteToStream(Drawing drawing)
        {
            var ms = new MemoryStream();
            var fileSettings = FileHandler.GetFileSettingsFromDrawing(drawing);
            Assert.True(await FileHandler.WriteDrawing("filename", ms, drawing, ViewPort.CreateDefaultViewPort(), fileSettings));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public async Task<Drawing> ReadFromStream(Stream stream)
        {
            var result = await FileHandler.ReadDrawing("filename", stream, Workspace.FileSystemService.ReadAllBytesAsync);
            Assert.True(result.Success);
            return result.Drawing;
        }

        public async Task<Layer> RoundTripLayer(Layer layer)
        {
            var drawing = new Drawing().Add(layer);
            var result = await RoundTripDrawing(drawing);
            return result.GetLayers().Single(l => l.Name == layer.Name);
        }
    }
}
