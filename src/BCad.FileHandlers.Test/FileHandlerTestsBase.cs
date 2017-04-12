// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Entities;
using BCad.Extensions;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public abstract class FileHandlerTestsBase
    {
        protected abstract Entity RoundTripEntity(Entity entity);

        protected void VerifyRoundTrip(Entity entity)
        {
            var afterRoundTrip = RoundTripEntity(entity);
            Assert.True(entity.EquivalentTo(afterRoundTrip));
        }
    }
}
