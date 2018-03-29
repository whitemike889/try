using System;
using FluentAssertions;
using OmniSharp.Client;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class LinePositionTests
    {
        [Fact]
        public void OneBased_does_not_modify_coordinates_for_LinePosition_that_is_already_one_based()
        {
            var alreadyOneBasedPosition = new LinePosition(1, 1, isOneBased: true);

            alreadyOneBasedPosition
                .OneBased()
                .Should().BeEquivalentTo(new LinePosition(1, 1, isOneBased: true));
        }

        [Fact]
        public void OneBased_modifies_the_coordinates_for_LinePosition_that_is_zero_based()
        {
            var zeroBasedPosition = new LinePosition(0, 0);

            var oneBasedPosition = zeroBasedPosition.OneBased();

            oneBasedPosition.Should().BeEquivalentTo(new LinePosition(1, 1, isOneBased: true));
        }
    }
}
