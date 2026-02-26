using Moq;
using AdminService.Application.Handlers;
using AdminService.Application.Queries;
using AdminService.Application.Dtos;
using AdminService.Domain.Aggregates;
using AdminService.Domain.Services;

namespace AdminService.Tests
{
    public class GetAllRolesQueryHandlerTests
    {
        private readonly Mock<IRoleRepository> _repoMock;
        private readonly GetAllRolesQueryHandler _handler;

        public GetAllRolesQueryHandlerTests()
        {
            _repoMock = new Mock<IRoleRepository>();
            _handler = new GetAllRolesQueryHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnListOfRoles()
        {
            // Arrange
            var roles = new List<Role>
            {
                Role.Create("Admin", "Desc", "sys"),
                Role.Create("User", "Desc", "sys")
            };
            
            _repoMock.Setup(x => x.ListAsync(null, null, 1, 1000, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((roles.AsReadOnly(), 2L));

            // Act
            var result = await _handler.Handle(new GetAllRolesQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Admin", result[0].RoleName);
            Assert.Equal("User", result[1].RoleName);
        }
    }
}
