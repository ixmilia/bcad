using System;
using IxMilia.BCad.Display;

namespace IxMilia.BCad.Helpers
{
    public static class ViewPortHelpers
    {
        public static Matrix4 GetProjectionMatrix(this ViewPort viewPort, double width, double height, ProjectionStyle projectionStyle)
        {
            // create transform
            Matrix4 transform;
            switch (projectionStyle)
            {
                case ProjectionStyle.OriginTopLeft:
                    transform = viewPort.GetTransformationMatrixWindowsStyle(width, height);
                    break;
                case ProjectionStyle.OriginBottomLeft:
                    transform = viewPort.GetTransformationMatrixCartesianStyle(width, height);
                    break;
                case ProjectionStyle.OriginCenter:
                    transform = viewPort.GetTransformationMatrixDirect3DStyle(width, height);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return transform;
        }
    }
}
