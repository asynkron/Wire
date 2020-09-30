// -----------------------------------------------------------------------
//   <copyright file="TypeExTests.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using Wire.Extensions;
using Xunit;

namespace Wire.Tests.Extensions
{
    public class TypeExTests
    {
        [Theory]
        [InlineData(
            "Wire.Tests.Extensions.TypeExTests+FakeEntity, Wire.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")]
        [InlineData(
            "Wire.Tests.Extensions.TypeExTests+FakeEntity, Wire.Tests, Version=1.3.9, Culture=neutral, PublicKeyToken=null")]
        [InlineData(
            "Wire.Tests.Extensions.TypeExTests+FakeEntity, Wire.Tests, Version=1.666.32323232323.32232, Culture=neutral, PublicKeyToken=null")]
        [InlineData(
            "Wire.Tests.Extensions.TypeExTests+FakeEntity, Wire.Tests, Version=1, Culture=neutral, PublicKeyToken=null")]
        [InlineData(
            "Wire.Tests.Extensions.TypeExTests+FakeEntity, Wire.Tests, Version=1.0, Culture=neutral, PublicKeyToken=null")]
        [InlineData("Wire.Tests.Extensions.TypeExTests+FakeEntity, Wire.Tests")]
        public void ShouldRemoveVersionFromAssemblyQualifiedName(string input)
        {
            var result = TypeEx.ReplaceTokens(input);
            Assert.Equal("Wire.Tests.Extensions.TypeExTests+FakeEntity, Wire.Tests", result);
        }
    }
}