// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.Entities
{
    public enum ProjectionStyle
    {
        /// <summary>
        /// Windows-style coordinates where the origin (0, 0) is in the top left corner with the X and Y axes
        /// increasing to the right and down respectively.
        /// </summary>
        OriginTopLeft,

        /// <summary>
        /// Cartesian-style coordinates where the origin (0, 0) is in the bottom left corner with the X and Y axes
        /// increasing to the right and up respectively.
        /// </summary>
        OriginBottomLeft,

        /// <summary>
        /// DirectX-style coordinates where the origin (0, 0) is in the middle with the X and Y axes increasing to the
        /// right and up respectively, where the view port has X and Y values in the range [-1, 1].
        /// </summary>
        OriginCenter
    }
}
