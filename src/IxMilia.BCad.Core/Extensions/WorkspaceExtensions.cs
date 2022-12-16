using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Services;

namespace IxMilia.BCad
{
    public static class WorkspaceExtensions
    {
        /// <summary>
        /// Add a layer to the drawing contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer to add to the drawing.</param>
        public static void Add(this IWorkspace workspace, Layer layer)
        {
            var updatedDrawing = workspace.Drawing.Add(layer);
            workspace.Update(drawing: updatedDrawing);
        }

        /// <summary>
        /// Add an entity to the specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer to which to add the entity.</param>
        /// <param name="entity">The entity to add.</param>
        public static void Add(this IWorkspace workspace, Layer layer, Entity entity)
        {
            var updatedDrawing = workspace.Drawing.Add(layer, entity);
            workspace.Update(drawing: updatedDrawing);
        }

        /// <summary>
        /// Add an entity to the current drawing layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="entity">The entity to add.</param>
        public static void AddToCurrentLayer(this IWorkspace workspace, Entity entity)
        {
            workspace.Update(drawing: workspace.Drawing.AddToCurrentLayer(entity));
        }

        /// <summary>
        /// Replace a layer in the drawing contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="oldLayer">The layer to be replaced.</param>
        /// <param name="newLayer">The replacement layer.</param>
        public static void Replace(this IWorkspace workspace, Layer oldLayer, Layer newLayer)
        {
            var updatedDrawing = workspace.Drawing.Replace(oldLayer, newLayer);
            workspace.Update(drawing: updatedDrawing);
        }

        /// <summary>
        /// Replace the entity in a specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer containing the entities.</param>
        /// <param name="oldEntity">The entity to be replaced.</param>
        /// <param name="newEntity">The replacement entity.</param>
        public static void Replace(this IWorkspace workspace, Layer layer, Entity oldEntity, Entity newEntity)
        {
            var updatedDrawing = workspace.Drawing.Replace(layer, oldEntity, newEntity);
            workspace.Update(drawing: updatedDrawing);
        }

        /// <summary>
        /// Remove a layer from the drawing contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer to remove from the drawing.</param>
        public static void Remove(this IWorkspace workspace, Layer layer)
        {
            var updatedDrawing = workspace.Drawing.Remove(layer);
            workspace.Update(drawing: updatedDrawing);
        }

        /// <summary>
        /// Remove an entity from the specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer containing the entity.</param>
        /// <param name="entity">The entity to be removed.</param>
        public static void Remove(this IWorkspace workspace, Layer layer, Entity entity)
        {
            var updatedDrawing = workspace.Drawing.Remove(layer, entity);
            workspace.Update(drawing: updatedDrawing);
        }

        /// <summary>
        /// Sets the current layer by name.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layerName">The name of the desired current layer.</param>
        public static void SetCurrentLayer(this IWorkspace workspace, string layerName)
        {
            if (workspace.Drawing.Settings.CurrentLayerName != layerName)
                workspace.Update(drawing: workspace.Drawing.Update(settings: workspace.Drawing.Settings.Update(currentLayerName: layerName)));
        }

        /// <summary>
        /// Update the current drawing settings.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="settings">The drawing settings to set.</param>
        public static void UpdateDrawingSettings(this IWorkspace workspace, DrawingSettings settings)
        {
            workspace.Update(drawing: workspace.Drawing.Update(settings: settings));
        }

        /// <summary>
        /// Format the specified value as per the current drawing settings.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="value">The value to format.</param>
        /// <returns>The formatted value.</returns>
        public static string Format(this IWorkspace workspace, double value)
        {
            return DrawingSettings.FormatUnits(value, workspace.Drawing.Settings.UnitFormat, workspace.Drawing.Settings.UnitPrecision);
        }

        /// <summary>
        /// Format the specified point as per the current drawing settings.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="point">The point to format.</param>
        /// <returns>The formatted point.</returns>
        public static string Format(this IWorkspace workspace, Point point)
        {
            return string.Format("({0}, {1}, {2})", workspace.Format(point.X), workspace.Format(point.Y), workspace.Format(point.Z));
        }

        private static IEnumerable<FileSpecification> DrawingFileSpecifications = new[]
        {
            new FileSpecification("DXF File", new[] { ".dxf" }), // keep this first so it's the default
            new FileSpecification("DWG File", new[] { ".dwg" }),
            new FileSpecification("IGES File", new[] { ".iges", ".igs" }),
        };

        private static IEnumerable<FileSpecification> AllDrawingFileSpecifications = new[]
        {
            new FileSpecification("All CAD Drawings", DrawingFileSpecifications.SelectMany(spec => spec.FileExtensions)),
        }.Concat(DrawingFileSpecifications);

        private static IEnumerable<FileSpecification> PlotFileSpecifications = new[]
        {
            new FileSpecification("PDF File", new[] { ".pdf" }),
            new FileSpecification("SVG File", new[] { ".svg" }),
        };

        public static Task<string> GetDrawingFilenameFromUserForOpen(this IWorkspace workspace)
        {
            return workspace.FileSystemService.GetFileNameFromUserForOpen(AllDrawingFileSpecifications);
        }

        public static Task<string> GetDrawingFilenameFromUserForSave(this IWorkspace workspace)
        {
            return workspace.FileSystemService.GetFileNameFromUserForSave(AllDrawingFileSpecifications);
        }

        public static FileSpecification GetFileSpecificationFromExtension(this IWorkspace _workspace, string extension)
        {
            return DrawingFileSpecifications.Concat(PlotFileSpecifications).FirstOrDefault(spec => spec.FileExtensions.Contains(extension));
        }

        public static async Task<bool> ExecuteCommandLine(this IWorkspace workspace, string commandLine)
        {
            if (!CommandLineSplitter.TrySplitCommandLine(commandLine, out var parts))
            {
                return false;
            }

            if (parts.Length == 0)
            {
                return false;
            }

            var command = parts[0];
            var arguments = parts.Skip(1).ToArray();
            var argumentIndex = 0;
            var pushedNone = false;
            void ValueRequestedHandler(object sender, ValueRequestedEventArgs e)
            {
                if (argumentIndex >= arguments.Length)
                {
                    if (pushedNone)
                    {
                        throw new Exception("Command already finished");
                    }
                    else
                    {
                        pushedNone = true;
                        workspace.InputService.PushNone();
                    }
                }
                else
                {
                    var argValue = arguments[argumentIndex];
                    var result = workspace.InputService.TrySubmitValue(argValue);
                    argumentIndex++;
                }
            };
            void ExecutionFinished(object sender, CadCommandExecutedEventArgs e)
            {
                if (argumentIndex < arguments.Length)
                {
                    throw new ArgumentException($"Unconsumed arguments: {string.Join(", ", arguments.Skip(argumentIndex))}");
                }
            }

            workspace.CommandExecuted += ExecutionFinished;
            workspace.InputService.ValueRequested += ValueRequestedHandler;
            var result = await workspace.ExecuteCommand(command);
            workspace.InputService.ValueRequested -= ValueRequestedHandler;
            workspace.CommandExecuted -= ExecutionFinished;

            return result;
        }
    }
}
