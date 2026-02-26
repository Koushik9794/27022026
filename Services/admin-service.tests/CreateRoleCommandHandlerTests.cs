using Moq;
using AdminService.Application.Handlers;
using AdminService.Application.Commands;
using AdminService.Domain.Aggregates;
using AdminService.Domain.Services;

namespace AdminService.Tests
{
    public class CreateRoleCommandHandlerTests
    {
        private readonly Mock<IRoleRepository> _repoMock;
        private readonly CreateRoleCommandHandler _handler;

        public CreateRoleCommandHandlerTests()
        {
            _repoMock = new Mock<IRoleRepository>();
            _handler = new CreateRoleCommandHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateRoleAndReturnId()
        {
            // Arrange
            var command = new CreateRoleCommand("Manager", "Manager desc", "admin");
            _repoMock.Setup(x => x.CreateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _repoMock.Verify(x => x.CreateAsync(It.Is<Role>(r => r.RoleName == "Manager"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
