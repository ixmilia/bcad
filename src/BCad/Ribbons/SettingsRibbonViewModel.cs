// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using IxMilia.BCad.SnapPoints;
using IxMilia.BCad.UI.View;

namespace IxMilia.BCad.Ribbons
{
    public class SettingsRibbonViewModel : INotifyPropertyChanged
    {
        private IWorkspace workspace = null;
        internal static List<Tuple<string, int>> ArchitecturalPrecisionValues;
        internal static List<Tuple<string, int>> DecimalPrecisionValues;

        static SettingsRibbonViewModel()
        {
            ArchitecturalPrecisionValues = new List<Tuple<string, int>>()
            {
                Tuple.Create("1\"", 0),
                Tuple.Create("1/2\"", 2),
                Tuple.Create("1/4\"", 4),
                Tuple.Create("1/8\"", 8),
                Tuple.Create("1/16\"", 16),
                Tuple.Create("1/32\"", 32),
            };
            DecimalPrecisionValues = new List<Tuple<string, int>>();
            for (int i = 0; i <= 16; i++)
            {
                DecimalPrecisionValues.Add(Tuple.Create(i.ToString(), i));
            }
        }

        public SettingsRibbonViewModel(IWorkspace workspace)
        {
            this.workspace = workspace;
            workspace.SettingsService.SettingChanged += (_, args) => UpdateProperty(args.SettingName);
        }

        private T GetValue<T>(string settingName)
        {
            return workspace.SettingsService.GetValue<T>(settingName);
        }

        private void SetValue<T>(string settingName, T value, [CallerMemberName] string propertyName = null)
        {
            workspace.SettingsService.SetValue(settingName, value);
            OnPropertyChangedDirect(propertyName);
        }

        public UnitFormat[] DrawingUnitValues
        {
            get
            {
                return new[]
                {
                    UnitFormat.Architectural,
                    UnitFormat.Metric
                };
            }
        }

        public UnitFormat UnitFormat
        {
            get { return workspace.Drawing.Settings.UnitFormat; }
            set
            {
                if (value == workspace.Drawing.Settings.UnitFormat)
                    return;
                workspace.UpdateDrawingSettings(workspace.Drawing.Settings.Update(unitFormat: value));
                OnPropertyChanged();
                OnPropertyChangedDirect("UnitPrecisionDisplay");
                OnPropertyChangedDirect("UnitPrecision");
            }
        }

        public IEnumerable<string> UnitPrecisionDisplay
        {
            get { return GetUnitPrecisions().Select(p => p.Item1); }
        }

        public string UnitPrecision
        {
            get
            {
                var precisions = GetUnitPrecisions();
                var prec = precisions.FirstOrDefault(p => p.Item2 == workspace.Drawing.Settings.UnitPrecision);
                if (prec == null)
                    return null;
                return prec.Item1;
            }
            set
            {
                if (value == null)
                    return;
                var precisions = GetUnitPrecisions();
                var prec = precisions.FirstOrDefault(p => p.Item1 == value);
                if (prec == null)
                    throw new InvalidOperationException("Unsupported unit precision");
                if (prec.Item2 == workspace.Drawing.Settings.UnitPrecision)
                    return;
                workspace.UpdateDrawingSettings(workspace.Drawing.Settings.Update(unitPrecision: prec.Item2));
                OnPropertyChanged();
            }
        }

        public double SnapPointSize
        {
            get => GetValue<double>(WpfSettingsProvider.SnapPointSize);
            set => SetValue(WpfSettingsProvider.SnapPointSize, value);
        }

        public double SnapPointDistance
        {
            get => GetValue<double>(WpfSettingsProvider.SnapPointDistance);
            set => SetValue(WpfSettingsProvider.SnapPointDistance, value);
        }

        public double EntitySelectionRadius
        {
            get => GetValue<double>(WpfSettingsProvider.EntitySelectionRadius);
            set => SetValue(WpfSettingsProvider.EntitySelectionRadius, value);
        }

        public int CursorSize
        {
            get => GetValue<int>(WpfSettingsProvider.CursorSize);
            set => SetValue(WpfSettingsProvider.CursorSize, value);
        }

        public string[] AvailableRenderers
        {
            get { return new[] { "Software", "Skia" }; }
        }

        public string SelectedRenderer
        {
            get => GetValue<string>(WpfSettingsProvider.RendererId);
            set => SetValue(WpfSettingsProvider.RendererId, value);
        }

        public SelectedEntityDrawStyle[] AvailableSelectedEntityDrawStyles
        {
            get { return (SelectedEntityDrawStyle[])Enum.GetValues(typeof(SelectedEntityDrawStyle)); }
        }

        public SelectedEntityDrawStyle SelectedEntityDrawStyle
        {
            get => GetValue<SelectedEntityDrawStyle>(SkiaSharpSettings.SelectedEntityDrawStyle);
            set => SetValue(SkiaSharpSettings.SelectedEntityDrawStyle, value);
        }

        public CadColor[] BackgroundColors
        {
            get
            {
                return new[]
                {
                    CadColor.Black,
                    CadColor.DarkSlateGray,
                    CadColor.CornflowerBlue,
                    CadColor.White
                };
            }
        }

        public CadColor? BackgroundColor
        {
            get => GetValue<CadColor>(WpfSettingsProvider.BackgroundColor);
            set => SetValue(WpfSettingsProvider.BackgroundColor, value ?? CadColor.Black);
        }

        public CadColor[] HotPointColors
        {
            get
            {
                return new[]
                {
                    CadColor.Blue,
                    CadColor.Green,
                    CadColor.Red
                };
            }
        }

        public CadColor? HotPointColor
        {
            get => GetValue<CadColor>(WpfSettingsProvider.HotPointColor);
            set => SetValue(WpfSettingsProvider.HotPointColor, value ?? CadColor.Black);
        }

        public CadColor[] SnapPointColors
        {
            get
            {
                return new[]
                {
                    CadColor.Yellow,
                    CadColor.White,
                    CadColor.Cyan,
                    CadColor.Red
                };
            }
        }

        public CadColor? SnapPointColor
        {
            get => GetValue<CadColor>(WpfSettingsProvider.SnapPointColor);
            set => SetValue(WpfSettingsProvider.SnapPointColor, value ?? CadColor.Black);
        }

        public bool IsEndPoint
        {
            get { return HasSnapPointFlag(SnapPointKind.EndPoint); }
            set
            {
                if (value)
                    SetSnapPointFlag(SnapPointKind.EndPoint);
                else
                    ClearSnapPointFlag(SnapPointKind.EndPoint);
                OnPropertyChanged();
            }
        }

        public bool IsMidPoint
        {
            get { return HasSnapPointFlag(SnapPointKind.MidPoint); }
            set
            {
                if (value)
                    SetSnapPointFlag(SnapPointKind.MidPoint);
                else
                    ClearSnapPointFlag(SnapPointKind.MidPoint);
                OnPropertyChanged();
            }
        }

        public bool IsCenter
        {
            get { return HasSnapPointFlag(SnapPointKind.Center); }
            set
            {
                if (value)
                    SetSnapPointFlag(SnapPointKind.Center);
                else
                    ClearSnapPointFlag(SnapPointKind.Center);
                OnPropertyChanged();
            }
        }

        public bool IsQuadrant
        {
            get { return HasSnapPointFlag(SnapPointKind.Quadrant); }
            set
            {
                if (value)
                    SetSnapPointFlag(SnapPointKind.Quadrant);
                else
                    ClearSnapPointFlag(SnapPointKind.Quadrant);
                OnPropertyChanged();
            }
        }

        public bool IsFocus
        {
            get { return HasSnapPointFlag(SnapPointKind.Focus); }
            set
            {
                if (value)
                    SetSnapPointFlag(SnapPointKind.Focus);
                else
                    ClearSnapPointFlag(SnapPointKind.Focus);
                OnPropertyChanged();
            }
        }

        private bool HasSnapPointFlag(SnapPointKind kind)
        {
            return (GetValue<SnapPointKind>(WpfSettingsProvider.AllowedSnapPoints) & kind) == kind;
        }

        private void SetSnapPointFlag(SnapPointKind kind)
        {
            SetValue(WpfSettingsProvider.AllowedSnapPoints, GetValue<SnapPointKind>(WpfSettingsProvider.AllowedSnapPoints) | kind);
        }

        private void ClearSnapPointFlag(SnapPointKind kind)
        {
            SetValue(WpfSettingsProvider.AllowedSnapPoints, GetValue<SnapPointKind>(WpfSettingsProvider.AllowedSnapPoints) & ~kind);
        }

        #region Snap angles

        private double[] ninetyDegreeAngles = new[] { 0.0, 90.0, 180.0, 270.0 };
        private double[] fortyFiveDegreeAngles = new[] { 0.0, 45.0, 90.0, 135.0, 180.0, 215.0, 270.0, 315.0 };
        private double[] isoAngles = new[] { 30.0, 90.0, 150.0, 210.0, 270.0, 330.0 };

        public bool IsNinetyDegree
        {
            get { return AreEqual(ninetyDegreeAngles, GetValue<double[]>(WpfSettingsProvider.SnapAngles)); }
            set
            {
                if (value && !AreEqual(ninetyDegreeAngles, GetValue<double[]>(WpfSettingsProvider.SnapAngles)))
                {
                    SetValue(WpfSettingsProvider.SnapAngles, ninetyDegreeAngles);
                }
            }
        }

        public bool IsFortyFiveDegree
        {
            get { return AreEqual(fortyFiveDegreeAngles, GetValue<double[]>(WpfSettingsProvider.SnapAngles)); }
            set
            {
                if (value && !AreEqual(fortyFiveDegreeAngles, GetValue<double[]>(WpfSettingsProvider.SnapAngles)))
                {
                    SetValue(WpfSettingsProvider.SnapAngles, fortyFiveDegreeAngles);
                }
            }
        }

        public bool IsIsometric
        {
            get { return AreEqual(isoAngles, GetValue<double[]>(WpfSettingsProvider.SnapAngles)); }
            set
            {
                if (value && !AreEqual(isoAngles, GetValue<double[]>(WpfSettingsProvider.SnapAngles)))
                {
                    SetValue(WpfSettingsProvider.SnapAngles, isoAngles);
                }
            }
        }

        #endregion

        public void UpdateProperty(string propertyName = "")
        {
            OnPropertyChangedDirect(propertyName);
        }

        private IEnumerable<Tuple<string, int>> GetUnitPrecisions()
        {
            switch (workspace.Drawing.Settings.UnitFormat)
            {
                case UnitFormat.Architectural:
                    return ArchitecturalPrecisionValues;
                case UnitFormat.Metric:
                    return DecimalPrecisionValues;
                default:
                    throw new InvalidOperationException("Invalid unit format");
            }
        }

        private bool AreEqual(double[] expected, double[] actual)
        {
            if (expected.Length != actual.Length)
                return false;
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                    return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChangedDirect(propertyName);
        }

        protected void OnPropertyChangedDirect(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
