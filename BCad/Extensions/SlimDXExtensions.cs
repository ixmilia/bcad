using SlimDX;
using SlimDX.Direct3D9;

namespace BCad.Extensions
{
    public static class SlimDXExtensions
    {
        public static SlimDX.BoundingBox GetBoundingBox(this Mesh mesh)
        {
            var count = mesh.VertexCount;
            var bpv = mesh.BytesPerVertex;
            var verts = new Vector3[count];
            var ds = mesh.LockVertexBuffer(LockFlags.ReadOnly);
            for (int i = 0; ds.Position < ds.Length && i < count; i++)
            {
                var old = ds.Position;
                verts[i] = ds.Read<Vector3>();
                ds.Position = old + bpv;
            }

            mesh.UnlockVertexBuffer();
            return SlimDX.BoundingBox.FromPoints(verts);
        }

        public static Vector3 ToVector3(this Vector4 vector)
        {
            return new Vector3(vector.X / vector.W, vector.Y / vector.W, vector.Z / vector.W);
        }

        public static Matrix ToMatrix(this Matrix4 matrix)
        {
            return new Matrix()
            {
                M11 = (float)matrix.M11,
                M12 = (float)matrix.M21,
                M13 = (float)matrix.M31,
                M14 = (float)matrix.M41,
                M21 = (float)matrix.M12,
                M22 = (float)matrix.M22,
                M23 = (float)matrix.M32,
                M24 = (float)matrix.M42,
                M31 = (float)matrix.M13,
                M32 = (float)matrix.M23,
                M33 = (float)matrix.M33,
                M34 = (float)matrix.M43,
                M41 = (float)matrix.M14,
                M42 = (float)matrix.M24,
                M43 = (float)matrix.M34,
                M44 = (float)matrix.M44,
            };
        }
    }
}
