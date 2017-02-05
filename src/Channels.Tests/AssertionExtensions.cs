using FluentAssertions;
using FluentAssertions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels.Tests
{
    public static class AssertionExtensions
    {
        public static void ShouldHaveValue<T>(this PotentialValue<T> potentialValue, T expectedValue)
        {
            potentialValue.HasValue.Should().BeTrue();
            potentialValue.Value.Should().Be(expectedValue);
        }

        public static void ShouldNotHaveValue<T>(this PotentialValue<T> potentialValue)
        {
            potentialValue.HasValue.Should().BeFalse();
        }
    }
}
