using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IxMilia.BCad.Settings;
using IxMilia.Config;

namespace IxMilia.BCad.Services
{
    internal class SettingsService : ISettingsService
    {
        private delegate object ValueReader(string value);
        private delegate string ValueWriter(object value);

        private IWorkspace _workspace;
        private Dictionary<string, Tuple<Type, object>> _settings = new();
        private Dictionary<Type, Tuple<ValueReader, ValueWriter>> _typeConverters = new();

        public event SettingChangedEventHandler SettingChanged;

        private SettingsService()
        {
            _typeConverters.Add(typeof(bool), Tuple.Create<ValueReader, ValueWriter>(BoolReader, BoolWriter));
            _typeConverters.Add(typeof(CadColor), Tuple.Create<ValueReader, ValueWriter>(CadColorReader, CadColorWriter));
            _typeConverters.Add(typeof(double), Tuple.Create<ValueReader, ValueWriter>(DoubleReader, DoubleWriter));
            _typeConverters.Add(typeof(double[]), Tuple.Create(CreateArrayReader<double>(DoubleReader), CreateArrayWriter(DoubleWriter)));
            _typeConverters.Add(typeof(DrawingUnits), Tuple.Create<ValueReader, ValueWriter>(DrawingUnitsReader, DrawingUnitsWriter));
            _typeConverters.Add(typeof(int), Tuple.Create<ValueReader, ValueWriter>(IntReader, IntWriter));
            _typeConverters.Add(typeof(string), Tuple.Create<ValueReader, ValueWriter>(StringReader, StringWriter));
            _typeConverters.Add(typeof(UnitFormat), Tuple.Create<ValueReader, ValueWriter>(UnitFormatReader, UnitFormatWriter));
        }

        public SettingsService(IWorkspace workspace)
            : this()
        {
            _workspace = workspace;
        }

        public void RegisterSetting(string name, Type type, object value)
        {
            if (_settings.ContainsKey(name))
            {
                _workspace.OutputService.WriteLine($"The setting '{name}' has already been exported by another component and will not be used again.");
            }
            else
            {
                _settings[name] = Tuple.Create(type, (object)null);
                SetValue(name, value);
            }
        }

        public string ValueToString(Type type, object value)
        {
            if (!_typeConverters.TryGetValue(type, out var pair))
            {
                throw new InvalidOperationException($"No type converter for {type.Name}");
            }

            var writer = pair.Item2;
            var s = writer(value);
            return s;
        }

        public object StringToValue(Type type, string value)
        {
            if (!_typeConverters.TryGetValue(type, out var pair))
            {
                throw new InvalidOperationException($"No type converter for {type.Name}");
            }

            var reader = pair.Item1;
            var o = reader(value);
            return o;
        }

        public T GetValue<T>(string settingName)
        {
            if (!_settings.TryGetValue(settingName, out var pair))
            {
                //return default(T);
                throw new InvalidOperationException($"The requested setting '{settingName}' does not exist.");
            }

            if (pair.Item1 != typeof(T))
            {
                throw new InvalidOperationException($"The setting '{settingName}' is of type '{pair.Item1.Name}' but type '{typeof(T).Name}' was requested.");
            }

            return (T)pair.Item2;
        }

        public void SetValue<T>(string settingName, T value)
        {
            if (!_settings.TryGetValue(settingName, out var pair))
            {
                // no such setting
                return;
            }

            var type = pair.Item1;
            var oldValue = pair.Item2;
            object valueToSet = value;
            if (Equals(pair.Item2, valueToSet))
            {
                // no change
                return;
            }

            _settings[settingName] = Tuple.Create(type, valueToSet);
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingName, type, oldValue, value));
        }

        public void SetValueFromString(string settingName, string value)
        {
            if (!_settings.TryGetValue(settingName, out var pair))
            {
                // no such setting
                return;
            }

            var nativeValue = StringToValue(pair.Item1, value);
            SetValue(settingName, nativeValue);
        }

        public void LoadFromLines(string[] lines)
        {
            var fileValues = new Dictionary<string, string>();
            fileValues.ParseConfig(lines);
            foreach (var kvp in fileValues)
            {
                if (!_settings.TryGetValue(kvp.Key, out var pair))
                {
                    // no such setting
                    continue;
                }

                var v = StringToValue(pair.Item1, kvp.Value);
                SetValue(kvp.Key, v);
            }
        }

        public string WriteWithLines(string[] existingLines)
        {
            var values = new Dictionary<string, string>();
            foreach (var kvp in _settings)
            {
                var s = ValueToString(kvp.Value.Item1, kvp.Value.Item2);
                values[kvp.Key] = s;
            }

            var newContent = values.WriteConfig(existingLines);
            return newContent;
        }

        private class SettingData
        {
            public string Name { get; }
            public Type Type { get; }
            public object Value { get; }

            public SettingData(string name, Type type, object value)
            {
                Name = name;
                Type = type;
                Value = value;
            }
        }

        private static object BoolReader(string value) => bool.Parse(value);
        private static string BoolWriter(object value) => ((bool)value).ToString(CultureInfo.InvariantCulture);

        private static object CadColorReader(string value) => CadColor.Parse(value);
        private static string CadColorWriter(object value) => ((CadColor)value).ToString();

        private static object DoubleReader(string value) => double.Parse(value, CultureInfo.InvariantCulture);
        private static string DoubleWriter(object value) => ((double)value).ToString(CultureInfo.InvariantCulture);

        private static object DrawingUnitsReader(string value) => Enum.Parse(typeof(DrawingUnits), value);
        private static string DrawingUnitsWriter(object value) => ((DrawingUnits)value).ToString();

        private static object IntReader(string value) => int.Parse(value, CultureInfo.InvariantCulture);
        private static string IntWriter(object value) => ((int)value).ToString(CultureInfo.InvariantCulture);

        private static object StringReader(string value) => value;
        private static string StringWriter(object value) => value.ToString();

        private static object UnitFormatReader(string value) => Enum.Parse(typeof(UnitFormat), value);
        private static string UnitFormatWriter(object value) => ((UnitFormat)value).ToString();

        private static ValueReader CreateArrayReader<T>(Func<string, object> elementReader) => value => value.Split(';').Select(elementReader).Cast<T>().ToArray();
        private static ValueWriter CreateArrayWriter(Func<object, string> elementWriter)
        {
            return value =>
            {
                var array = (Array)value;
                var values = new List<object>();
                foreach (var x in array)
                {
                    values.Add(elementWriter(x));
                }

                return string.Join(";", values);
            };
        }
    }
}
