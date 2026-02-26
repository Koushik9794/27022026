using Xunit;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;

namespace AdminService.Tests
{
    public class RoleTests
    {
        [Fact]
        public void Create_ShouldInitializeCorrectly()
        {
            // Arrange
            var roleName = "Admin";
            var description = "Administrator description";
            var createdBy = "system";

            // Act
            var role = Role.Create(roleName, description, createdBy);

            // Assert
            Assert.NotEqual(Guid.Empty, role.Id);
            Assert.Equal("Admin", role.RoleName);
            Assert.Equal("Administrator description", role.Description);
            Assert.True(role.IsActive);
            Assert.False(role.IsDeleted);
            Assert.Equal(createdBy, role.CreatedBy);
            Assert.Equal(DateTimeOffset.UtcNow.Date, role.CreatedAt.Date);
        }

        [Fact]
        public void Create_WithExplicitDate_ShouldInitializeCorrectly()
        {
            // Arrange
            var roleName = "Admin";
            var description = "Desc";
            var createdBy = "system";
            var createdAt = new DateTime(2023, 1, 1);

            // Act
            var role = Role.Create(roleName, description, createdBy, createdAt);

            // Assert
            Assert.Equal(createdAt, role.CreatedAt);
            Assert.Equal(roleName, role.RoleName);
        }

        [Fact]
        public void Rehydrate_ShouldRestoreState()
        {
            // Arrange
            var id = Guid.NewGuid();
            var roleName = "Restored";
            var desc = "Restored Desc";
            var isActive = false;
            var isDeleted = true;
            var createdBy = "user";
            var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
            var modifiedBy = "admin";
            var modifiedAt = DateTimeOffset.UtcNow;

            // Act
            var role = Role.Rehydrate(id, roleName, desc, isActive, isDeleted, createdBy, createdAt, modifiedBy, modifiedAt);

            // Assert
            Assert.Equal(id, role.Id);
            Assert.Equal(roleName, role.RoleName);
            Assert.Equal(desc, role.Description);
            Assert.Equal(isActive, role.IsActive);
            Assert.Equal(isDeleted, role.IsDeleted);
            Assert.Equal(createdBy, role.CreatedBy);
            Assert.Equal(createdAt, role.CreatedAt);
            Assert.Equal(modifiedBy, role.ModifiedBy);
            Assert.Equal(modifiedAt, role.ModifiedAt);
        }

        [Fact]
        public void Update_ShouldModifyProperties()
        {
            // Arrange
            var role = Role.Create("OldName", "OldDesc", "user1");
            var newName = "NewName";
            var newDesc = "New description";
            var modifiedBy = "admin";

            // Act
            role.Update(newName, newDesc, modifiedBy);

            // Assert
            Assert.Equal(newName, role.RoleName);
            Assert.Equal(newDesc, role.Description);
            Assert.Equal(modifiedBy, role.ModifiedBy);
            Assert.NotNull(role.ModifiedAt);
        }

        [Fact]
        public void Activate_Deactivate_ShouldToggleState()
        {
            // Arrange
            var role = Role.Create("Test", null, null);
            
            // Act & Assert (Deactivate)
            role.Deactivate("user");
            Assert.False(role.IsActive);
            
            // Act & Assert (Activate)
            role.Activate("user");
            Assert.True(role.IsActive);
        }

        [Fact]
        public void SoftDelete_ShouldSetIsDeleted()
        {
            // Arrange
            var role = Role.Create("Test", null, null);

            // Act
            role.SoftDelete("user");

            // Assert
            Assert.True(role.IsDeleted);
        }
    }
}
