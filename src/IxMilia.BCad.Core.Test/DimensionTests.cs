using System.Linq;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.Entities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class DimensionTests
    {
        private static string[] GetDimensionStyleNames(Drawing drawing)
        {
            return drawing.Settings.DimensionStyles.Select(ds => ds.Name).OrderBy(name => name).ToArray();
        }

        [Fact]
        public void DimensionStyleCanBeAdded()
        {
            var drawing = new Drawing();
            Assert.Equal(new[] { "STANDARD" }, GetDimensionStyleNames(drawing));
            var styleChanges = new DimensionStylesDialogParameters(
                drawing.Settings.CurrentDimensionStyleName,
                new[]
                {
                    DimensionStylesDialogEntry.FromDimensionStyle(drawing.Settings.CurrentDimensionStyle),
                    new DimensionStylesDialogEntry(
                        isDeleted: false,
                        originalName: "",
                        name: "new-dim-style",
                        arrowSize: 1.0,
                        extensionLineOffset: 2.0,
                        extensionLineExtension: 3.0,
                        textHeight: 4.0,
                        lineGap: 5.0,
                        lineColor: CadColor.Black,
                        textColor: CadColor.Blue),
                });
            var updatedDrawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, styleChanges);
            Assert.Equal(new[] { "new-dim-style", "STANDARD" }, GetDimensionStyleNames(updatedDrawing));
            var dimStyle = updatedDrawing.Settings.DimensionStyles["new-dim-style"];
            Assert.Equal(1.0, dimStyle.ArrowSize);
            Assert.Equal(2.0, dimStyle.ExtensionLineOffset);
            Assert.Equal(3.0, dimStyle.ExtensionLineExtension);
            Assert.Equal(4.0, dimStyle.TextHeight);
            Assert.Equal(5.0, dimStyle.LineGap);
            Assert.Equal(CadColor.Black, dimStyle.LineColor);
            Assert.Equal(CadColor.Blue, dimStyle.TextColor);
        }

        [Fact]
        public void DimensionStyleCanBeDeleted()
        {
            var drawing = new Drawing();
            drawing = drawing.Update(settings: drawing.Settings.Update(dimStyles: drawing.Settings.DimensionStyles.Add(
                new DimensionStyle("my-dim-style", arrowSize: 1.0, extensionLineOffset: 2.0, extensionLineExtension: 3.0, textHeight: 4.0, lineGap: 5.0, lineColor: CadColor.Black, textColor: CadColor.Blue))));
            Assert.Equal(new[] { "my-dim-style", "STANDARD" }, GetDimensionStyleNames(drawing));
            var styleChanges = new DimensionStylesDialogParameters(
                drawing.Settings.CurrentDimensionStyleName,
                new[]
                {
                    DimensionStylesDialogEntry.FromDimensionStyle(drawing.Settings.CurrentDimensionStyle),
                    new DimensionStylesDialogEntry(
                        isDeleted: true,
                        originalName: "my-dim-style",
                        name: "my-dim-style",
                        arrowSize: 1.0,
                        extensionLineOffset: 2.0,
                        extensionLineExtension: 3.0,
                        textHeight: 4.0,
                        lineGap: 5.0,
                        lineColor: CadColor.Black,
                        textColor: CadColor.Blue),
                });
            var updatedDrawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, styleChanges);
            Assert.Equal(new[] { "STANDARD" }, updatedDrawing.Settings.DimensionStyles.Select(ds => ds.Name).OrderBy(name => name).ToArray());
        }

        [Fact]
        public void RenamedAndUpdatedDimensionStyleUpdatesAffectedEntities()
        {
            var drawing = new Drawing();
            drawing = drawing.Update(settings: drawing.Settings.Update(dimStyles: drawing.Settings.DimensionStyles.Add(
                new DimensionStyle("my-dim-style", arrowSize: 1.0, extensionLineOffset: 2.0, extensionLineExtension: 3.0, textHeight: 4.0, lineGap: 5.0, lineColor: CadColor.Black, textColor: CadColor.Blue))));
            drawing = drawing.AddToCurrentLayer(new LinearDimension(
                new Point(),
                new Point(),
                new Point(),
                true,
                new Point(),
                "my-dim-style"));
            var styleChanges = new DimensionStylesDialogParameters(
                drawing.Settings.CurrentDimensionStyleName,
                new[]
                {
                    DimensionStylesDialogEntry.FromDimensionStyle(drawing.Settings.CurrentDimensionStyle),
                    new DimensionStylesDialogEntry(
                        isDeleted: false,
                        originalName: "my-dim-style",
                        name: "renamed-dim-style",
                        arrowSize: 11.0,
                        extensionLineOffset: 22.0,
                        extensionLineExtension: 33.0,
                        textHeight: 44.0,
                        lineGap: 55.0,
                        lineColor: CadColor.Yellow,
                        textColor: CadColor.Red),
                });
            var updatedDrawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, styleChanges);
            Assert.Equal(new[] { "renamed-dim-style", "STANDARD" }, updatedDrawing.Settings.DimensionStyles.Select(ds => ds.Name).OrderBy(name => name).ToArray());
            var dimStyle = updatedDrawing.Settings.DimensionStyles["renamed-dim-style"];
            Assert.Equal(11.0, dimStyle.ArrowSize);
            Assert.Equal(22.0, dimStyle.ExtensionLineOffset);
            Assert.Equal(33.0, dimStyle.ExtensionLineExtension);
            Assert.Equal(44.0, dimStyle.TextHeight);
            Assert.Equal(55.0, dimStyle.LineGap);
            Assert.Equal(CadColor.Yellow, dimStyle.LineColor);
            Assert.Equal(CadColor.Red, dimStyle.TextColor);
            var dimension = Assert.IsType<LinearDimension>(updatedDrawing.GetEntities().Single());
            Assert.Equal("renamed-dim-style", dimension.DimensionStyleName);
        }

        [Fact]
        public void DimensionStyleCanBeDeletedAndAffectedEntitiesAreAssignedADifferentDimensionStyle()
        {
            var drawing = new Drawing();
            drawing = drawing.Update(settings: drawing.Settings.Update(dimStyles: drawing.Settings.DimensionStyles.Add(
                new DimensionStyle("my-dim-style", arrowSize: 1.0, extensionLineOffset: 2.0, extensionLineExtension: 3.0, textHeight: 4.0, lineGap: 5.0, lineColor: CadColor.Black, textColor: CadColor.Blue))));
            drawing = drawing.AddToCurrentLayer(new LinearDimension(
                new Point(),
                new Point(),
                new Point(),
                true,
                new Point(),
                "my-dim-style"));
            var styleChanges = new DimensionStylesDialogParameters(
                drawing.Settings.CurrentDimensionStyleName,
                new[]
                {
                    DimensionStylesDialogEntry.FromDimensionStyle(drawing.Settings.CurrentDimensionStyle),
                    new DimensionStylesDialogEntry(
                        isDeleted: true,
                        originalName: "my-dim-style",
                        name: "my-dim-style",
                        arrowSize: 11.0,
                        extensionLineOffset: 22.0,
                        extensionLineExtension: 33.0,
                        textHeight: 44.0,
                        lineGap: 55.0,
                        lineColor: CadColor.Yellow,
                        textColor: CadColor.Red),
                });
            var updatedDrawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, styleChanges);
            Assert.Equal(new[] { "STANDARD" }, updatedDrawing.Settings.DimensionStyles.Select(ds => ds.Name).OrderBy(name => name).ToArray());
            var dimension = Assert.IsType<LinearDimension>(updatedDrawing.GetEntities().Single());
            Assert.Equal("STANDARD", dimension.DimensionStyleName);
        }

        [Fact]
        public void OnlyDimensionStyleCanBeDeletedAndNewDefaultIsAdded()
        {
            var drawing = new Drawing();
            var styleChanges = new DimensionStylesDialogParameters(
                "RENAMED-STANDARD",
                new[]
                {
                    new DimensionStylesDialogEntry(
                        isDeleted: false,
                        originalName: "STANDARD",
                        name: "RENAMED-STANDARD",
                        arrowSize: 1.0,
                        extensionLineOffset: 2.0,
                        extensionLineExtension: 3.0,
                        textHeight: 4.0,
                        lineGap: 5.0,
                        lineColor: CadColor.Black,
                        textColor: CadColor.Blue),
                });
            drawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, styleChanges);
            Assert.Equal(new[] { "RENAMED-STANDARD" }, GetDimensionStyleNames(drawing));
            var newStyleChanges = new DimensionStylesDialogParameters(
                drawing.Settings.CurrentDimensionStyleName,
                new[]
                {
                    new DimensionStylesDialogEntry(
                        isDeleted: true,
                        originalName: "RENAMED-STANDARD",
                        name: "RENAMED-STANDARD",
                        arrowSize: 1.0,
                        extensionLineOffset: 2.0,
                        extensionLineExtension: 3.0,
                        textHeight: 4.0,
                        lineGap: 5.0,
                        lineColor: CadColor.Black,
                        textColor: CadColor.Blue),
                });
            var updatedDrawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, newStyleChanges);
            Assert.Equal(new[] { "STANDARD" }, GetDimensionStyleNames(updatedDrawing));
        }

        [Fact]
        public void OnlyDimensionStyleCanBeDeletedAndAffectedEntitiesAreUpdated()
        {
            var drawing = new Drawing();
            Assert.Equal(new[] { "STANDARD" }, GetDimensionStyleNames(drawing));
            var styleChanges = new DimensionStylesDialogParameters(
                "RENAMED-STANDARD",
                new[]
                {
                    new DimensionStylesDialogEntry(
                        isDeleted: false,
                        originalName: "STANDARD",
                        name: "RENAMED-STANDARD",
                        arrowSize: 1.0,
                        extensionLineOffset: 2.0,
                        extensionLineExtension: 3.0,
                        textHeight: 4.0,
                        lineGap: 5.0,
                        lineColor: CadColor.Black,
                        textColor: CadColor.Blue),
                });
            drawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, styleChanges);
            Assert.Equal(new[] { "RENAMED-STANDARD" }, GetDimensionStyleNames(drawing));
            drawing = drawing.AddToCurrentLayer(new LinearDimension(
                new Point(),
                new Point(),
                new Point(),
                true,
                new Point(),
                "RENAMED-STANDARD"));
            var newStyleChanges = new DimensionStylesDialogParameters(
                "RENAMED-STANDARD",
                new[]
                {
                    new DimensionStylesDialogEntry(
                        isDeleted: true,
                        originalName: "RENAMED-STANDARD",
                        name: "RENAMED-STANDARD",
                        arrowSize: 1.0,
                        extensionLineOffset: 2.0,
                        extensionLineExtension: 3.0,
                        textHeight: 4.0,
                        lineGap: 5.0,
                        lineColor: CadColor.Black,
                        textColor: CadColor.Blue),
                });
            var updatedDrawing = DimensionStylesCommand.ApplyDimensionStyleChanges(drawing, newStyleChanges);
            var dimension = Assert.IsType<LinearDimension>(updatedDrawing.GetEntities().Single());
            Assert.Equal("STANDARD", dimension.DimensionStyleName);
        }
    }
}
