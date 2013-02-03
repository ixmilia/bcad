namespace BCad.Stl
{
    public class StlTriangle
    {
        public StlNormal Normal { get; set; }

        public StlVertex Vertex1 { get; set; }

        public StlVertex Vertex2 { get; set; }

        public StlVertex Vertex3 { get; set; }

        public StlTriangle(StlNormal normal, StlVertex v1, StlVertex v2, StlVertex v3)
        {
            Normal = normal;
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
        }
    }
}
