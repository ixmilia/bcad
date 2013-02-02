using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Stl
{
    internal class StlReader
    {
        private Stream baseStream;
        private BinaryReader binReader;
        private string[] tokens;
        private int tokenPos;

        public StlReader(Stream stream)
        {
            baseStream = stream;
            binReader = new BinaryReader(stream);
        }

        public string ReadSolidName()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.Append(binReader.ReadChar());
            }

            if (sb.ToString() == "solid")
            {
                // eat one more space
                var c = binReader.ReadChar();
                Debug.Assert(c == ' ');

                // read all whitespace-separated tokens
                var textReader = new StreamReader(baseStream);
                tokens = textReader.ReadToEnd()
                    .Split(" \t\n\r\f\v".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();
                tokenPos = 0;
                return tokens[tokenPos++];
            }
            else
            {
                // swallow the remainder of the 80 byte header
                for (int i = 0; i < 75; i++)
                {
                    binReader.ReadChar();
                }

                return null;
            }
        }

        public List<StlTriangle> ReadTriangles()
        {
            var triangles = new List<StlTriangle>();
            var t = ReadTriangle();
            while (t != null)
            {
                triangles.Add(t);
                t = ReadTriangle();
            }

            return triangles;
        }

        private StlTriangle ReadTriangle()
        {
            StlTriangle triangle = null;
            switch (PeekToken())
            {
                case "facet":
                    AdvanceToken();
                    SwallowToken("normal");
                    var normal = new StlNormal(ConsumeNumber(), ConsumeNumber(), ConsumeNumber());
                    SwallowToken("outer");
                    SwallowToken("loop");
                    SwallowToken("vertex");
                    var v1 = ConsumeVertex();
                    SwallowToken("vertex");
                    var v2 = ConsumeVertex();
                    SwallowToken("vertex");
                    var v3 = ConsumeVertex();
                    SwallowToken("endloop");
                    SwallowToken("endfacet");
                    triangle = new StlTriangle(v1, v2, v3, normal);
                    break;
                case "endsolid":
                    return null;
                default:
                    throw new StlReadException("Unexpected token " + PeekToken());
            }

            return triangle;
        }

        private void SwallowToken(string token)
        {
            if (PeekToken() == token)
            {
                AdvanceToken();
            }
            else
            {
                throw new StlReadException("Expected token " + token);
            }
        }

        private double ConsumeNumber()
        {
            var text = PeekToken();
            AdvanceToken();
            double value;
            if (!double.TryParse(text, out value))
                throw new StlReadException("Expected number");
            return value;
        }

        private StlVertex ConsumeVertex()
        {
            return new StlVertex(ConsumeNumber(), ConsumeNumber(), ConsumeNumber());
        }

        private string PeekToken()
        {
            if (tokenPos >= tokens.Length)
                return null;
            return tokens[tokenPos];
        }

        private void AdvanceToken()
        {
            tokenPos++;
        }
    }
}
