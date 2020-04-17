using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.FileHandlers.Extensions;
using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;

namespace IxMilia.BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, true, FileExtension)]
    public class DxfFileHandler : IFileHandler
    {
        public const string DisplayName = "DXF Files (" + FileExtension + ")";
        public const string FileExtension = ".dxf";

        public INotifyPropertyChanged GetFileSettingsFromDrawing(Drawing drawing)
        {
            var settings = new DxfFileSettings();
            var dxfFile = drawing.Tag as DxfFile;
            if (dxfFile != null)
            {
                settings.FileVersion = dxfFile.Header.Version.ToFileVersion();
            }

            return settings;
        }

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
        {
            var file = DxfFile.Load(fileStream);
            var layers = new ReadOnlyTree<string, Layer>();
            foreach (var layer in file.Layers)
            {
                layers = layers.Insert(layer.Name, new Layer(layer.Name, color: layer.Color.ToColor(), isVisible: layer.IsLayerOn));
            }

            foreach (var item in file.Entities)
            {
                var layer = GetOrCreateLayer(ref layers, item.Layer);

                // create the entity
                var entity = item.ToEntity();

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
                    var tempEnt = item.ToEntity();
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

            drawing = new Drawing(
                settings: new DrawingSettings(fileName, file.Header.UnitFormat.ToUnitFormat(), file.Header.UnitPrecision),
                layers: layers,
                currentLayerName: file.Header.CurrentLayer ?? layers.GetKeys().OrderBy(x => x).First(),
                author: null);
            drawing.Tag = file;

            // prefer `*ACTIVE` view port first
            // TODO: use `DxfFile.ActiveViewPort` when available
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

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, INotifyPropertyChanged fileSettings)
        {
            var file = new DxfFile();
            if (drawing.Tag is DxfFile oldFile)
            {
                // preserve settings from the original file
                file.Header.CreationDate = oldFile.Header.CreationDate;
                file.Header.CreationDateUniversal = oldFile.Header.CreationDateUniversal;
                file.Header.Version = oldFile.Header.Version;
            }

            if (fileSettings is DxfFileSettings dxfSettings)
            {
                file.Header.Version = dxfSettings.FileVersion.ToDxfFileVersion();
            }

            // save layers and entities
            file.Header.CurrentLayer = drawing.CurrentLayer.Name;
            file.Header.UnitFormat = drawing.Settings.UnitFormat.ToDxfUnitFormat();
            file.Header.UnitPrecision = (short)drawing.Settings.UnitPrecision;
            foreach (var layer in drawing.GetLayers().OrderBy(x => x.Name))
            {
                file.Layers.Add(new DxfLayer(layer.Name, layer.Color.ToDxfColor()) { IsLayerOn = layer.IsVisible });
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
                    }
                    else
                    {
                        var entity = item.ToDxfEntity(layer);
                        if (entity != null)
                            file.Entities.Add(entity);
                    }
                }
            }

            // save viewport
            // TODO: use `DxfFile.ActiveViewPort` when available
            file.ViewPorts.Clear();
            file.ViewPorts.Add(new DxfViewPort()
            {
                Name = DxfViewPort.ActiveViewPortName,
                LowerLeft = viewPort.BottomLeft.ToDxfPoint(),
                ViewDirection = viewPort.Sight.ToDxfVector(),
                ViewHeight = viewPort.ViewHeight
            });

            file.Save(fileStream);
            return true;
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
