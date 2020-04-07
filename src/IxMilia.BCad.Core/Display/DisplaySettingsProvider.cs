// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Settings;

namespace IxMilia.BCad.Display
{
    [ExportSetting(AngleSnap, typeof(bool), true)]
    [ExportSetting(BackgroundColor, typeof(CadColor), "#FF2F2F2F")]
    [ExportSetting(CursorSize, typeof(int), 60)]
    [ExportSetting(EntitySelectionRadius, typeof(double), 3.0)]
    [ExportSetting(HotPointColor, typeof(CadColor), "#FF0000FF")]
    [ExportSetting(HotPointSize, typeof(double), 10.0)]
    [ExportSetting(Ortho, typeof(bool), false)]
    [ExportSetting(PointSnap, typeof(bool), true)]
    [ExportSetting(SnapAngleDistance, typeof(double), 30.0)]
    [ExportSetting(SnapAngles, typeof(double[]), new[] { 0.0, 90.0, 180.0, 270.0 })]
    [ExportSetting(SnapPointColor, typeof(CadColor), "#FFFFFF00")]
    [ExportSetting(SnapPointDistance, typeof(double), 15.0)]
    [ExportSetting(SnapPointSize, typeof(double), 15.0)]
    [ExportSetting(TextCursorSize, typeof(int), 18)]
    [ExportSetting(PointDisplaySize, typeof(double), 48.0)]
    public class DisplaySettingsProvider
    {
        public const string Prefix = "Display.";
        public const string AngleSnap = Prefix + nameof(AngleSnap);
        public const string BackgroundColor = Prefix + nameof(BackgroundColor);
        public const string CursorSize = Prefix + nameof(CursorSize);
        public const string EntitySelectionRadius = Prefix + nameof(EntitySelectionRadius);
        public const string HotPointColor = Prefix + nameof(HotPointColor);
        public const string HotPointSize = Prefix + nameof(HotPointSize);
        public const string Ortho = Prefix + nameof(Ortho);
        public const string PointSnap = Prefix + nameof(PointSnap);
        public const string SnapAngleDistance = Prefix + nameof(SnapAngleDistance);
        public const string SnapAngles = Prefix + nameof(SnapAngles);
        public const string SnapPointColor = Prefix + nameof(SnapPointColor);
        public const string SnapPointDistance = Prefix + nameof(SnapPointDistance);
        public const string SnapPointSize = Prefix + nameof(SnapPointSize);
        public const string TextCursorSize = Prefix + nameof(TextCursorSize);
        public const string PointDisplaySize = Prefix + nameof(PointDisplaySize);
    }
}
