using System.Collections.Generic;
using System.Text;

namespace IxMilia.BCad.Commands
{
    public static class CommandLineSplitter
    {
        public static bool TrySplitIntoTokens(string line, out string[] tokens)
        {
            tokens = default;
            var parts = new List<string>();

            if (line.Length > 0 && line[0] == ';')
            {
                // comments are only recognized at the start of a line
                tokens = new string[0];
                return true;
            }

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (IsWhitespace(c))
                {
                    // skip
                    continue;
                }

                if (c == '"')
                {
                    // get a string
                    var sb = new StringBuilder();
                    var foundEnd = false;
                    for (int j = i + 1; j < line.Length && !foundEnd; j++)
                    {
                        c = line[j];
                        if (c == '"')
                        {
                            i = j;
                            foundEnd = true;
                            parts.Add(sb.ToString());
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    if (!foundEnd)
                    {
                        return false;
                    }
                }
                else
                {
                    // get anything
                    var end = i;
                    for (; end < line.Length; end++)
                    {
                        c = line[end];
                        if (IsWhitespace(c))
                        {
                            break;
                        }
                    }

                    var part = line.Substring(i, end - i);
                    parts.Add(part);
                    i = end;
                }
            }

            if (parts.Count == 0)
            {
                // at least one entry is always created
                parts.Add(string.Empty);
            }

            tokens = parts.ToArray();
            return true;
        }

        private static bool IsWhitespace(char c)
        {
            switch (c)
            {
                case ' ':
                case '\f':
                case '\r':
                case '\t':
                case '\v':
                    return true;
                default:
                    return false;
            }
        }
    }
}
