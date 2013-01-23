﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Entities
{
    public class DxfVertex : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Vertex; } }

        public override string SubclassMarker { get { return "AcDbVertex"; } }

        public DxfPoint Location { get; set; }

        public DxfVertex()
            : this(DxfPoint.Origin)
        {
        }

        public DxfVertex(DxfPoint location)
        {
            this.Location = location;
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            yield return new DxfCodePair(0, DxfEntity.VertexType);
            yield return new DxfCodePair(10, Location.X);
            yield return new DxfCodePair(20, Location.Y);
            yield return new DxfCodePair(30, Location.Z);
        }

        internal static DxfVertex VertexFromBuffer(DxfCodePairBufferReader buffer)
        {
            var vertex = new DxfVertex();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                if (!vertex.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 10:
                            vertex.Location.X = pair.DoubleValue;
                            break;
                        case 20:
                            vertex.Location.Y = pair.DoubleValue;
                            break;
                        case 30:
                            vertex.Location.Z = pair.DoubleValue;
                            break;
                        default:
                            // unknown or unsupported attribute
                            break;
                    }
                }
            }

            return vertex;
        }
    }
}
