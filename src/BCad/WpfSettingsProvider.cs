// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Settings;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad
{
    [ExportSetting(AllowedSnapPoints, typeof(SnapPointKind), SnapPointKind.All)]
    [ExportSetting(AngleSnapShortcut, typeof(KeyboardShortcut), "None+F7")]
    [ExportSetting(DebugShortcut, typeof(KeyboardShortcut), "None+F12")]
    [ExportSetting(LayerDialogId, typeof(string), "Default")]
    [ExportSetting(OrthoShortcut, typeof(KeyboardShortcut), "None+F8")]
    [ExportSetting(PlotDialogId, typeof(string), "Default")]
    [ExportSetting(PointSize, typeof(double), 15.0)]
    [ExportSetting(PointSnapShortcut, typeof(KeyboardShortcut), "None+F3")]
    [ExportSetting(RendererId, typeof(string), "Skia")]
    [ExportSetting(RibbonOrder, typeof(string[]), new[] { "home", "view", "settings", "debug" })]
    public class WpfSettingsProvider
    {
        public const string Prefix = "UI.";
        public const string AllowedSnapPoints = Prefix + nameof(AllowedSnapPoints);
        public const string AngleSnapShortcut = Prefix + nameof(AngleSnapShortcut);
        public const string DebugShortcut = Prefix + nameof(DebugShortcut);
        public const string LayerDialogId = Prefix + nameof(LayerDialogId);
        public const string OrthoShortcut = Prefix + nameof(OrthoShortcut);
        public const string PlotDialogId = Prefix + nameof(PlotDialogId);
        public const string PointSize = Prefix + nameof(PointSize);
        public const string PointSnapShortcut = Prefix + nameof(PointSnapShortcut);
        public const string RendererId = Prefix + nameof(RendererId);
        public const string RibbonOrder = Prefix + nameof(RibbonOrder);
    }
}
