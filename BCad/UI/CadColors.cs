using System.Collections.Generic;
using System.Linq;

namespace BCad.UI
{
    public static class CadColors
    {
        public static IEnumerable<CadColor?> AllColors = new[] { new CadColor?() }.Concat(CadColor.Defaults.Select(c => new CadColor?(c)));
    }
}
