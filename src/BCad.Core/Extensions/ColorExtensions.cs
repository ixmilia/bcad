// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Globalization;

namespace BCad.Extensions
{
    public static class ColorExtensions
    {
        public static CadColor ParseColor(this string s)
        {
            if (s == null || !(s.Length == 4 || s.Length == 7 || s.Length == 9))
            {
                return new CadColor();
            }

            if (s[0] != '#')
            {
                return new CadColor();
            }

            if (s.Length == 4)
            {
                // color looks like #RGB where it needs to be expanded to #RRGGBB
                var rs = s[1];
                var gs = s[2];
                var bs = s[3];
                s = string.Concat('#', rs, rs, gs, gs, bs, bs);
            }

            uint c = uint.Parse(s.Substring(1), NumberStyles.HexNumber);
            uint a, r, g, b;
            if (s.Length == 7)
            {
                // rgb
                a = 255;
            }
            else
            {
                // argb
                Debug.Assert(s.Length == 9);
                a = (c & 0xFF000000) >> 24;
            }

            r = (c & 0x00FF0000) >> 16;
            g = (c & 0x0000FF00) >> 8;
            b = (c & 0x000000FF);

            return CadColor.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public static string ToRGBString(this CadColor color)
        {
            var r = string.Format("{0:X2}", color.R);
            var g = string.Format("{0:X2}", color.G);
            var b = string.Format("{0:X2}", color.B);
            return string.Format("#{0}{1}{2}", r, g, b);
        }

        public static string ToARGBString(this CadColor color)
        {
            var a = string.Format("{0:X2}", color.A);
            var r = string.Format("{0:X2}", color.R);
            var g = string.Format("{0:X2}", color.G);
            var b = string.Format("{0:X2}", color.B);
            return string.Format("#{0}{1}{2}{3}", a, r, g, b);
        }
    }
}
