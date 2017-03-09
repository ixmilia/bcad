// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BCad.SnapPoints;

namespace BCad.Ribbons
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
            workspace.SettingsManager.PropertyChanged += (_, __) => UpdateProperty(string.Empty);
        }

        public ISettingsManager SettingsManager { get { return workspace.SettingsManager; } }

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
            get { return SettingsManager.SnapPointSize; }
            set
            {
                if (SettingsManager.SnapPointSize == value)
                    return;
                SettingsManager.SnapPointSize = value;
                OnPropertyChanged();
            }
        }

        public double SnapPointDistance
        {
            get { return SettingsManager.SnapPointDistance; }
            set
            {
                if (SettingsManager.SnapPointDistance == value)
                    return;
                SettingsManager.SnapPointDistance = value;
                OnPropertyChanged();
            }
        }

        public double EntitySelectionRadius
        {
            get { return SettingsManager.EntitySelectionRadius; }
            set
            {
                if (SettingsManager.EntitySelectionRadius == value)
                    return;
                SettingsManager.EntitySelectionRadius = value;
                OnPropertyChanged();
            }
        }

        public int CursorSize
        {
            get { return SettingsManager.CursorSize; }
            set
            {
                if (SettingsManager.CursorSize == value)
                    return;
                SettingsManager.CursorSize = value;
                OnPropertyChanged();
            }
        }

        public string[] AvailableRenderers
        {
            get { return new[] { "Software", "Skia" }; }
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
            get { return SettingsManager.BackgroundColor; }
            set
            {
                var newColor = value ?? CadColor.Black;
                if (SettingsManager.BackgroundColor == newColor)
                    return;
                SettingsManager.BackgroundColor = newColor;
                OnPropertyChanged();
            }
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
            get { return SettingsManager.HotPointColor; }
            set
            {
                var newColor = value ?? CadColor.Black;
                if (SettingsManager.HotPointColor == newColor)
                    return;
                SettingsManager.HotPointColor = newColor;
                OnPropertyChanged();
            }
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
            get { return SettingsManager.SnapPointColor; }
            set
            {
                var newColor = value ?? CadColor.Black;
                if (SettingsManager.SnapPointColor == newColor)
                    return;
                SettingsManager.SnapPointColor = newColor;
                OnPropertyChanged();
            }
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
            return (SettingsManager.AllowedSnapPoints & kind) == kind;
        }

        private void SetSnapPointFlag(SnapPointKind kind)
        {
            SettingsManager.AllowedSnapPoints |= kind;
        }

        private void ClearSnapPointFlag(SnapPointKind kind)
        {
            SettingsManager.AllowedSnapPoints &= ~kind;
        }

        #region Snap angles

        private double[] ninetyDegreeAngles = new[] { 0.0, 90.0, 180.0, 270.0 };
        private double[] fortyFiveDegreeAngles = new[] { 0.0, 45.0, 90.0, 135.0, 180.0, 215.0, 270.0, 315.0 };
        private double[] isoAngles = new[] { 30.0, 90.0, 150.0, 210.0, 270.0, 330.0 };

        public bool IsNinetyDegree
        {
            get { return AreEqual(ninetyDegreeAngles, SettingsManager.SnapAngles); }
            set
            {
                if (value && !AreEqual(ninetyDegreeAngles, SettingsManager.SnapAngles))
                {
                    SettingsManager.SnapAngles = ninetyDegreeAngles;
                }
            }
        }

        public bool IsFortyFiveDegree
        {
            get { return AreEqual(fortyFiveDegreeAngles, SettingsManager.SnapAngles); }
            set
            {
                if (value && !AreEqual(fortyFiveDegreeAngles, SettingsManager.SnapAngles))
                {
                    SettingsManager.SnapAngles = fortyFiveDegreeAngles;
                }
            }
        }

        public bool IsIsometric
        {
            get { return AreEqual(isoAngles, SettingsManager.SnapAngles); }
            set
            {
                if (value && !AreEqual(isoAngles, SettingsManager.SnapAngles))
                {
                    SettingsManager.SnapAngles = isoAngles;
                }
            }
        }

        #endregion

        public void UpdateProperty(string propertyName = "")
        {
            OnPropertyChangedDirect(string.Empty);
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
