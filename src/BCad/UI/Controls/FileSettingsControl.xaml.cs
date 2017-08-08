// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace IxMilia.BCad.UI.Controls
{
    /// <summary>
    /// Interaction logic for FileSettingsControl.xaml
    /// </summary>
    [ExportControl("FileSettings", "Default", "File Settings")]
    public partial class FileSettingsControl : BCadControl
    {
        public FileSettingsControl()
        {
            InitializeComponent();
        }

        public override void OnShowing()
        {
            if (DataContext == null)
            {
                return;
            }

            var currentRow = 0;
            foreach (var property in DataContext.GetType().GetProperties())
            {
                var element = CreateElement(property);
                if (element != null)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    var text = new TextBlock()
                    {
                        Text = property.Name
                    };
                    text.SetValue(Grid.RowProperty, currentRow);
                    text.SetValue(Grid.ColumnProperty, 0);
                    grid.Children.Add(text);
                    element.SetValue(Grid.RowProperty, currentRow);
                    element.SetValue(Grid.ColumnProperty, 1);
                    grid.Children.Add(element);
                    currentRow++;
                }
            }
        }

        private UIElement CreateElement(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsEnum)
            {
                return CreateEnumComboBox(propertyInfo);
            }

            return null;
        }

        private UIElement CreateEnumComboBox(PropertyInfo propertyInfo)
        {
            var cb = new ComboBox()
            {
                ItemsSource = Enum.GetValues(propertyInfo.PropertyType)
            };
            cb.SetBinding(Selector.SelectedItemProperty, propertyInfo.Name);
            return cb;
        }
    }
}
