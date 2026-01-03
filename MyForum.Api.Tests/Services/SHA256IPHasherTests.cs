using MyForum.Api.Infrastructure.Services;

namespace MyForum.Api.Tests.Services
{
    public class SHA256IPHasherTests
    {
        private readonly SHA256IPHasher _hasher;

        public SHA256IPHasherTests()
        {
            _hasher = new SHA256IPHasher();
        }

        [Fact]
        public void HashIP_ShouldReturnHashedValue_ForValidIPAddress()
        {
            // Arrange
            var ipAddress = "192.168.1.1";

            // Act
            var hashedIp = _hasher.HashIP(ipAddress);

            // Assert
            Assert.NotNull(hashedIp);
            Assert.NotEqual(ipAddress, hashedIp);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("unknown")]
        public void HashIP_ShouldReturnUnknown_ForNullOrEmptyOrUnknownIPAddress(string ipAddress)
        {
            // Act
            var hashedIp = _hasher.HashIP(ipAddress);

            // Assert
            Assert.Equal("unknown", hashedIp);
        }
    }
}