using System;
using System.Collections;
using System.Collections.Generic;
using IxMilia.BCad.Collections;

namespace IxMilia.BCad
{
    public class DimensionStyleCollection : IEnumerable<DimensionStyle>
    {
        private ReadOnlyTree<string, DimensionStyle> _dimStyles;

        private DimensionStyleCollection(ReadOnlyTree<string, DimensionStyle> dimStyles)
        {
            _dimStyles = dimStyles;
        }

        public DimensionStyleCollection()
            : this(new ReadOnlyTree<string, DimensionStyle>().Insert(DimensionStyle.DefaultDimensionStyleName, DimensionStyle.CreateDefault()))
        {
        }

        public DimensionStyle this[string name] => _dimStyles.GetValue(name);

        public bool ContainsStyle(string name) => _dimStyles.KeyExists(name);

        public bool TryGetStyle(string name, out DimensionStyle dimStyle) => _dimStyles.TryFind(name, out dimStyle);

        public DimensionStyleCollection Add(DimensionStyle dimStyle)
        {
            if (ContainsStyle(dimStyle.Name))
            {
                throw new ArgumentException(nameof(dimStyle.Name), $"Item with name '{dimStyle.Name}' already exists");
            }

            var newDimStyles = _dimStyles.Insert(dimStyle.Name, dimStyle);
            return new DimensionStyleCollection(newDimStyles);
        }

        public DimensionStyleCollection Remove(string name)
        {
            if (!ContainsStyle(name))
            {
                return this;
            }

            return new DimensionStyleCollection(_dimStyles.Delete(name));
        }

        public DimensionStyleCollection Replace(DimensionStyle dimStyle) => Remove(dimStyle.Name).Add(dimStyle);

        public static DimensionStyleCollection FromEnumerable(IEnumerable<DimensionStyle> dimStyles)
        {
            var collection = new ReadOnlyTree<string, DimensionStyle>();
            foreach (var dimStyle in dimStyles)
            {
                collection = collection.Insert(dimStyle.Name, dimStyle);
            }

            return new DimensionStyleCollection(collection);
        }

        public IEnumerator<DimensionStyle> GetEnumerator() => _dimStyles.GetValues().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
