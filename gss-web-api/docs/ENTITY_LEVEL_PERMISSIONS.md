# Entity-Level Permissions - Architecture Guide

## Overview

**Entity-level permissions** (also called resource-level or row-level permissions) control access to specific instances of resources, not just resource types.

**Example:**
- **Feature-level**: "Can user create configurations?" → BFF checks
- **Entity-level**: "Can user edit THIS configuration?" → Service checks

## Architecture

### Two-Layer Permission Model

```
┌─────────────────────────────────────────────────────────────┐
│ Layer 1: Feature-Level Permissions (BFF)                    │
│ - Can user access this feature?                             │
│ - Based on role and permissions                             │
│ - Example: configurations.create, bom.generate              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Layer 2: Entity-Level Permissions (Domain Services)         │
│ - Can user access THIS specific resource?                   │
│ - Based on ownership, sharing, hierarchy                    │
│ - Example: Can dealer X edit configuration owned by dealer Y│
└─────────────────────────────────────────────────────────────┘
```

## Where to Implement

### BFF (Feature-Level Only)

**Responsibilities:**
- Check if user has permission to access feature
- Validate JWT token
- Pass user context to services
- **DO NOT** check entity ownership

**Example:**
```csharp
// BFF - ConfigurationController
[HttpPut("configurations/{id}")]
[Authorize] // JWT validation
public async Task<IActionResult> UpdateConfiguration(Guid id, UpdateConfigurationRequest request)
{
    // ✅ BFF checks: Does user have 'configurations.update' permission?
    if (!User.HasPermission("configurations.update"))
    {
        return Forbid("Missing configurations.update permission");
    }
    
    // ✅ BFF orchestrates, but delegates entity check to service
    var command = new UpdateConfigurationCommand
    {
        ConfigurationId = id,
        UserId = User.GetUserId(), // Pass user context
        DealerId = User.GetDealerId(),
        Request = request
    };
    
    // Service will check if this user can edit THIS configuration
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### Domain Services (Entity-Level)

**Responsibilities:**
- Check ownership (e.g., "Is this configuration owned by this dealer?")
- Check sharing rules (e.g., "Is this configuration shared with this user?")
- Check hierarchy (e.g., "Is this user a manager of the owner?")
- Enforce business rules

**Example:**
```csharp
// catalog-service - UpdateConfigurationHandler
public class UpdateConfigurationHandler : ICommandHandler<UpdateConfigurationCommand>
{
    private readonly IConfigurationRepository _repository;
    private readonly IAuthorizationService _authService;

    public async Task<ConfigurationResponse> Handle(UpdateConfigurationCommand command)
    {
        // 1. Fetch the configuration
        var configuration = await _repository.GetByIdAsync(command.ConfigurationId);
        if (configuration == null)
        {
            throw new NotFoundException("Configuration not found");
        }

        // 2. ✅ Entity-level permission check
        if (!await _authService.CanEditConfiguration(command.UserId, configuration))
        {
            throw new ForbiddenException("You do not have permission to edit this configuration");
        }

        // 3. Update the configuration
        configuration.Update(command.Request);
        await _repository.SaveAsync(configuration);

        return MapToResponse(configuration);
    }
}

// Authorization Service (in catalog-service)
public class ConfigurationAuthorizationService : IAuthorizationService
{
    public async Task<bool> CanEditConfiguration(Guid userId, Configuration configuration)
    {
        // Rule 1: Owner can always edit
        if (configuration.CreatedBy == userId)
        {
            return true;
        }

        // Rule 2: Same dealer can edit (if sharing enabled)
        var user = await _userRepository.GetByIdAsync(userId);
        if (user.DealerId == configuration.DealerId && configuration.IsSharedWithDealer)
        {
            return true;
        }

        // Rule 3: Admin can edit any
        if (user.Role == "ADMIN")
        {
            return true;
        }

        // Rule 4: Explicitly shared with this user
        if (configuration.SharedWith.Contains(userId))
        {
            return true;
        }

        return false;
    }
}
```

## Common Entity-Level Permission Patterns

### 1. Ownership-Based

**Rule:** Only the creator can access the resource.

```csharp
public bool CanAccess(Guid userId, Entity entity)
{
    return entity.CreatedBy == userId;
}
```

**Use Cases:**
- Personal configurations
- Draft documents
- Private notes

### 2. Organization-Based (Dealer/Company)

**Rule:** Anyone in the same organization can access.

```csharp
public bool CanAccess(User user, Entity entity)
{
    return user.DealerId == entity.DealerId;
}
```

**Use Cases:**
- Shared configurations within a dealer
- Company-wide resources
- Team documents

### 3. Hierarchical

**Rule:** Managers can access subordinates' resources.

```csharp
public async Task<bool> CanAccess(User user, Entity entity)
{
    // Owner can access
    if (entity.CreatedBy == user.Id) return true;
    
    // Manager can access subordinates' resources
    var owner = await _userRepository.GetByIdAsync(entity.CreatedBy);
    if (owner.ManagerId == user.Id) return true;
    
    return false;
}
```

**Use Cases:**
- Manager reviewing team configurations
- Approval workflows
- Audit access

### 4. Explicit Sharing

**Rule:** Resource is explicitly shared with specific users.

```csharp
public bool CanAccess(Guid userId, Entity entity)
{
    return entity.SharedWith.Contains(userId) || entity.CreatedBy == userId;
}
```

**Use Cases:**
- Collaborative configurations
- Shared quotes
- Partner access

### 5. Role-Based Override

**Rule:** Certain roles bypass entity checks.

```csharp
public bool CanAccess(User user, Entity entity)
{
    // Admin can access anything
    if (user.Role == "ADMIN") return true;
    
    // Otherwise check ownership
    return entity.CreatedBy == user.Id;
}
```

**Use Cases:**
- Admin override
- Support access
- Audit access

## Implementation in GSS

### Recommended Approach

**For Configurations:**

```csharp
// Entity: Configuration
public class Configuration
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid DealerId { get; set; }
    public bool IsSharedWithDealer { get; set; }
    public List<Guid> SharedWith { get; set; } = new();
    public ConfigurationStatus Status { get; set; }
}

// Authorization Rules
public class ConfigurationAuthorizationService
{
    public async Task<bool> CanView(User user, Configuration config)
    {
        // Admin can view all
        if (user.Role == "ADMIN") return true;
        
        // Owner can view
        if (config.CreatedBy == user.Id) return true;
        
        // Same dealer can view if shared
        if (user.DealerId == config.DealerId && config.IsSharedWithDealer)
            return true;
        
        // Explicitly shared
        if (config.SharedWith.Contains(user.Id))
            return true;
        
        return false;
    }

    public async Task<bool> CanEdit(User user, Configuration config)
    {
        // Admin can edit all
        if (user.Role == "ADMIN") return true;
        
        // Owner can edit if not locked
        if (config.CreatedBy == user.Id && config.Status != ConfigurationStatus.Locked)
            return true;
        
        // Explicitly shared with edit permission
        var sharePermission = await _shareRepository.GetPermissionAsync(config.Id, user.Id);
        if (sharePermission?.CanEdit == true)
            return true;
        
        return false;
    }

    public async Task<bool> CanDelete(User user, Configuration config)
    {
        // Admin can delete all
        if (user.Role == "ADMIN") return true;
        
        // Only owner can delete (if not locked)
        if (config.CreatedBy == user.Id && config.Status != ConfigurationStatus.Locked)
            return true;
        
        return false;
    }
}
```

## BFF vs Service Responsibilities

| Concern | BFF | Service |
|---------|-----|---------|
| JWT validation | ✅ Yes | ❌ No (trusts BFF) |
| Feature permission check | ✅ Yes | ❌ No |
| User context extraction | ✅ Yes | ❌ No |
| Entity ownership check | ❌ No | ✅ Yes |
| Business rule enforcement | ❌ No | ✅ Yes |
| Data access control | ❌ No | ✅ Yes |

## Error Handling

### BFF Returns

```json
// Feature-level permission denied
{
  "correlationId": "abc-123",
  "errorCode": "FORBIDDEN",
  "message": "Missing required permission: configurations.update",
  "timestamp": "2026-01-08T04:30:00Z"
}
```

### Service Returns

```json
// Entity-level permission denied
{
  "correlationId": "abc-123",
  "errorCode": "FORBIDDEN",
  "message": "You do not have permission to edit this configuration",
  "details": [
    {
      "reason": "Configuration is owned by another dealer",
      "configurationId": "c789e012-e34b-56d7-a890-123456789abc"
    }
  ],
  "timestamp": "2026-01-08T04:30:00Z"
}
```

## Database Schema for Sharing

```sql
-- Configuration ownership
CREATE TABLE configurations (
    id UUID PRIMARY KEY,
    created_by UUID NOT NULL,
    dealer_id UUID NOT NULL,
    is_shared_with_dealer BOOLEAN DEFAULT FALSE,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Explicit sharing
CREATE TABLE configuration_shares (
    id UUID PRIMARY KEY,
    configuration_id UUID NOT NULL REFERENCES configurations(id),
    shared_with_user_id UUID NOT NULL,
    can_view BOOLEAN DEFAULT TRUE,
    can_edit BOOLEAN DEFAULT FALSE,
    can_delete BOOLEAN DEFAULT FALSE,
    shared_by UUID NOT NULL,
    shared_at TIMESTAMP NOT NULL,
    expires_at TIMESTAMP,
    UNIQUE(configuration_id, shared_with_user_id)
);

-- Indexes for performance
CREATE INDEX idx_configurations_created_by ON configurations(created_by);
CREATE INDEX idx_configurations_dealer_id ON configurations(dealer_id);
CREATE INDEX idx_configuration_shares_user ON configuration_shares(shared_with_user_id);
```

## Testing Entity-Level Permissions

```csharp
[Fact]
public async Task UpdateConfiguration_WhenNotOwner_ShouldReturnForbidden()
{
    // Arrange
    var ownerId = Guid.NewGuid();
    var otherUserId = Guid.NewGuid();
    
    var configuration = new Configuration
    {
        Id = Guid.NewGuid(),
        CreatedBy = ownerId,
        DealerId = Guid.NewGuid()
    };
    
    await _repository.SaveAsync(configuration);
    
    // Act
    var command = new UpdateConfigurationCommand
    {
        ConfigurationId = configuration.Id,
        UserId = otherUserId, // Different user
        Request = new UpdateConfigurationRequest { ... }
    };
    
    // Assert
    await Assert.ThrowsAsync<ForbiddenException>(
        () => _handler.Handle(command)
    );
}

[Fact]
public async Task UpdateConfiguration_WhenSharedWithUser_ShouldSucceed()
{
    // Arrange
    var ownerId = Guid.NewGuid();
    var sharedUserId = Guid.NewGuid();
    
    var configuration = new Configuration
    {
        Id = Guid.NewGuid(),
        CreatedBy = ownerId,
        SharedWith = new List<Guid> { sharedUserId }
    };
    
    await _repository.SaveAsync(configuration);
    
    // Act
    var command = new UpdateConfigurationCommand
    {
        ConfigurationId = configuration.Id,
        UserId = sharedUserId, // Shared user
        Request = new UpdateConfigurationRequest { ... }
    };
    
    var result = await _handler.Handle(command);
    
    // Assert
    result.Should().NotBeNull();
}
```

## Summary

**Key Principles:**

1. **BFF checks features** - "Can user access this type of resource?"
2. **Services check entities** - "Can user access THIS specific resource?"
3. **Never trust the client** - Always validate on backend
4. **Services are authoritative** - Final permission decision at service level
5. **Pass user context** - BFF sends userId, dealerId to services
6. **Fail securely** - Default deny, explicit allow

**Flow:**
```
User → BFF (feature check) → Service (entity check) → Database
       ✅ Has permission?      ✅ Owns resource?       ✅ Fetch data
```

This separation ensures:
- **Security**: Multiple layers of defense
- **Flexibility**: Services can implement complex business rules
- **Scalability**: BFF doesn't need to know all business logic
- **Maintainability**: Permission logic lives where data lives
