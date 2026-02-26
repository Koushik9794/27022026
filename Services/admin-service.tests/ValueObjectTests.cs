using Xunit;
using AdminService.Domain.ValueObjects;

namespace AdminService.Tests
{
    public class ValueObjectTests
    {
        [Theory]
        [InlineData("Admin", "Admin")]
        [InlineData("  Manager  ", "Manager")]
        public void RoleName_Create_ShouldNormalize(string input, string expected)
        {
            var result = RoleName.Create(input);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RoleName_Create_ShouldThrowOnEmpty(string input)
        {
            Assert.Throws<ArgumentException>(() => RoleName.Create(input));
        }

        [Fact]
        public void RoleDescription_Normalize_ShouldTrimAndHandleNull()
        {
            Assert.Null(RoleDescription.Normalize(null));
            Assert.Null(RoleDescription.Normalize("   "));
            Assert.Equal("A valid description", RoleDescription.Normalize("  A valid description  "));
        }
    }
}
