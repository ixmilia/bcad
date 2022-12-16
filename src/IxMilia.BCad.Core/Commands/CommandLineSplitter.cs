using System.Collections.Generic;
using System.Text;

namespace IxMilia.BCad.Commands
{
    public static class CommandLineSplitter
    {
        public static bool TrySplitCommandLine(string line, out string[] commandParts)
        {
            commandParts = default;
            var parts = new List<string>();
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
                    var isEscaped = false;
                    var foundEnd = false;
                    for (int j = i + 1; j < line.Length && !foundEnd; j++)
                    {
                        c = line[j];
                        if (isEscaped)
                        {
                            sb.Append(c);
                            isEscaped = false;
                        }
                        else
                        {
                            switch (c)
                            {
                                case '\\':
                                    isEscaped = true;
                                    break;
                                case '"':
                                    i = j;
                                    foundEnd = true;
                                    parts.Add(sb.ToString());
                                    break;
                                default:
                                    sb.Append(c);
                                    break;
                            }
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

            commandParts = parts.ToArray();
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
