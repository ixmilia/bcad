using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.FileHandlers.Extensions;
using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;

namespace IxMilia.BCad.FileHandlers
{
    public class DxfFileHandler : IFileHandler
    {
        public object GetFileSettingsFromDrawing(Drawing drawing)
        {
            var settings = new DxfFileSettings();
            var dxfFile = drawing.Tag as DxfFile;
            if (dxfFile != null)
            {
                settings.FileVersion = dxfFile.Header.Version.ToFileVersion();
            }

            return settings;
        }

        public Task<ReadDrawingResult> ReadDrawing(string fileName, Stream fileStream, Func<string, Task<byte[]>> contentResolver)
        {
            var file = DxfFile.Load(fileStream);
            return FromDxfFile(fileName, file, contentResolver);
        }

        public Task<bool> WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings)
        {
            var file = ToDxfFile(drawing, viewPort, fileSettings as DxfFileSettings);
            file.Save(fileStream);
            return Task.FromResult(true);
        }

        public static async Task<ReadDrawingResult> FromDxfFile(string fileName, DxfFile file, Func<string, Task<byte[]>> contentResolver)
        {
            var layers = new ReadOnlyTree<string, Layer>();
            foreach (var layer in file.Layers)
            {
                layers = layers.Insert(layer.Name, new Layer(layer.Name, color: layer.Color.ToColor(), isVisible: layer.IsLayerOn));
            }

            foreach (var item in file.Entities)
            {
                var layer = GetOrCreateLayer(ref layers, item.Layer);

                // create the entity
                var entity = await item.ToEntity(contentResolver);

                // add the entity to the appropriate layer
                if (entity != null)
                {
                    layer = layer.Add(entity);
                    layers = layers.Insert(layer.Name, layer);
                }
            }

            foreach (var block in file.Blocks)
            {
                var layer = GetOrCreateLayer(ref layers, block.Layer);

                // create the aggregate entity
                var children = ReadOnlyList<Entity>.Empty();
                foreach (var item in block.Entities)
                {
                    var tempEnt = await item.ToEntity(contentResolver);
                    if (tempEnt != null)
                    {
                        children = children.Add(tempEnt);
                    }
                }

                // add the entity to the appropriate layer
                if (children.Count != 0)
                {
                    layer = layer.Add(new AggregateEntity(block.BasePoint.ToPoint(), children));
                    layers = layers.Insert(layer.Name, layer);
                }
            }

            var drawing = new Drawing(
                settings: new DrawingSettings(fileName, file.Header.UnitFormat.ToUnitFormat(), file.Header.UnitPrecision),
                layers: layers,
                currentLayerName: file.Header.CurrentLayer ?? layers.GetKeys().OrderBy(x => x).First(),
                author: null);
            drawing.Tag = file;

            // prefer `*ACTIVE` view port first
            // TODO: use `DxfFile.ActiveViewPort` when available
            ViewPort viewPort;
            var vp = file.ViewPorts.FirstOrDefault(v => string.Compare(v.Name, DxfViewPort.ActiveViewPortName, StringComparison.OrdinalIgnoreCase) == 0)
                ?? file.ViewPorts.FirstOrDefault();
            if (vp != null)
            {
                viewPort = new ViewPort(
                    vp.LowerLeft.ToPoint(),
                    vp.ViewDirection.ToVector(),
                    Vector.YAxis,
                    vp.ViewHeight);
            }
            else
            {
                viewPort = null; // auto-set it later
            }

            return ReadDrawingResult.Succeeded(drawing, viewPort);
        }

        public static DxfFile ToDxfFile(Drawing drawing, ViewPort viewPort, DxfFileSettings settings)
        {
            var file = new DxfFile();
            file.Layers.Clear();
            if (drawing.Tag is DxfFile oldFile)
            {
                // preserve settings from the original file
                file.Header.CreationDate = oldFile.Header.CreationDate;
                file.Header.CreationDateUniversal = oldFile.Header.CreationDateUniversal;
                file.Header.Version = oldFile.Header.Version;
            }

            if (settings is object)
            {
                file.Header.Version = settings.FileVersion.ToDxfFileVersion();
            }

            // save layers and entities
            file.Header.CurrentLayer = drawing.CurrentLayer.Name;
            file.Header.UnitFormat = drawing.Settings.UnitFormat.ToDxfUnitFormat();
            file.Header.UnitPrecision = (short)drawing.Settings.UnitPrecision;
            foreach (var layer in drawing.GetLayers().OrderBy(x => x.Name))
            {
                if (!file.Layers.Any(l => l.Name == layer.Name))
                {
                    file.Layers.Add(new DxfLayer(layer.Name, layer.Color.ToDxfColor()) { IsLayerOn = layer.IsVisible });
                }

                foreach (var item in layer.GetEntities().OrderBy(e => e.Id))
                {
                    if (item.Kind == EntityKind.Aggregate)
                    {
                        // dxf files treat aggregate entities as separate items
                        var agg = (AggregateEntity)item;
                        var block = new DxfBlock();
                        block.Layer = layer.Name;
                        foreach (var child in agg.Children.Select(c => c.ToDxfEntity(layer)))
                        {
                            block.Entities.Add(child);
                        }

                        file.Blocks.Add(block);
                    }
                    else
                    {
                        var entity = item.ToDxfEntity(layer);
                        if (entity != null)
                        {
                            file.Entities.Add(entity);
                        }
                    }
                }
            }

            // save viewport
            // TODO: use `DxfFile.ActiveViewPort` when available
            file.ViewPorts.Clear();
            file.ViewPorts.Add(new DxfViewPort(DxfViewPort.ActiveViewPortName)
            {
                LowerLeft = viewPort.BottomLeft.ToDxfPoint(),
                ViewDirection = viewPort.Sight.ToDxfVector(),
                ViewHeight = viewPort.ViewHeight
            });

            return file;
        }

        private static Layer GetOrCreateLayer(ref ReadOnlyTree<string, Layer> layers, string layerName)
        {
            Layer layer = null;

            // entities without a layer go to '0'
            layerName = layerName ?? "0";
            if (layers.KeyExists(layerName))
                layer = layers.GetValue(layerName);
            else
            {
                // add the layer if previously undefined
                layer = new Layer(layerName);
                layers = layers.Insert(layer.Name, layer);
            }

            return layer;
        }
    }
}
