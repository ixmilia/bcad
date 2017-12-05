// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Entities;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    public class JsonFileHandlerTests : FileHandlerTestsBase
    {
        public override IFileHandler FileHandler => new JsonFileHandler();

        [Fact]
        public void RoundTripLayerTest()
        {
            VerifyRoundTrip(new Layer("green-layer", color: CadColor.Green));
        }

        [Fact]
        public void RoundTripArcTest()
        {
            VerifyRoundTrip(new Arc(new Point(1.0, 2.0, 0.0), 3.0, 4.0, 5.0, Vector.ZAxis));
        }

        [Fact]
        public void RoundTripLineTest()
        {
            VerifyRoundTrip(new Line(new Point(1.0, 2.0, 0.0), new Point(3.0, 4.0, 0.0)));
        }
    }
}
