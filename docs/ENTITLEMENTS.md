# Entitlements System - Complete Guide

## Overview

The **Entitlements System** controls what features, product groups, export formats, and limits are available to users based on their role and dealer subscription tier.

## Key Concepts

### Role vs Subscription Tier

**Role** = What you ARE (job function)
- `ADMIN` - System administrator
- `DEALER` - Dealer/reseller
- `DESIGNER` - Internal designer
- `VIEWER` - Read-only user

**Subscription Tier** = What you PAID FOR (plan level)
- `BASIC` - Entry-level subscription
- `PROFESSIONAL` - Mid-tier subscription
- `ENTERPRISE` - Full-featured subscription

### Example

```
User: john@abc-dealers.com
├─ Role: DEALER (job function)
├─ Dealer: ABC Dealers Inc.
└─ Subscription Tier: BASIC (what ABC Dealers paid for)
    └─ Entitlements:
        ├─ Product Groups: [PALLET_RACKING]
        ├─ Export Formats: [PDF]
        └─ Max Configurations: 10

User: jane@xyz-dealers.com
├─ Role: DEALER (same job function)
├─ Dealer: XYZ Dealers Ltd.
└─ Subscription Tier: ENTERPRISE (different subscription)
    └─ Entitlements:
        ├─ Product Groups: [PALLET_RACKING, SHELVING, MEZZANINE]
        ├─ Export Formats: [PDF, PNG, DXF, DWG]
        └─ Max Configurations: Unlimited
```

**Both users have the same ROLE but different ENTITLEMENTS based on subscription tier.**

## Entitlement Hierarchy

Entitlements are resolved from multiple levels, with **most restrictive wins**:

```
1. Role-Level Entitlements (Base defaults)
   ↓
2. Dealer-Level Entitlements (Subscription tier)
   ↓
3. User-Level Overrides (Optional special cases)
   ↓
Final Entitlements (Intersection of all)
```

### Resolution Example

```
Role: DEALER
├─ Allowed Product Groups: [PALLET_RACKING, SHELVING, MEZZANINE, CANTILEVER]
├─ Allowed Export Formats: [PDF, PNG, DXF, DWG]
└─ Max Configurations: 100

Dealer: ABC Dealers (Subscription: PROFESSIONAL)
├─ Allowed Product Groups: [PALLET_RACKING, SHELVING] ❌ No MEZZANINE
├─ Allowed Export Formats: [PDF, PNG] ❌ No DXF/DWG
└─ Max Configurations: 50

User: john@abc.com (Junior user)
└─ Max Configurations: 20 (User-level override)

Final Entitlements:
├─ Allowed Product Groups: [PALLET_RACKING, SHELVING] (dealer restriction)
├─ Allowed Export Formats: [PDF, PNG] (dealer restriction)
└─ Max Configurations: 20 (user override - most restrictive)
```

## Database Schema

### 1. Role Entitlements (Defaults)

```sql
CREATE TABLE role_entitlements (
    role VARCHAR(50) PRIMARY KEY,
    allowed_product_groups TEXT[],
    allowed_export_formats TEXT[],
    max_configurations INTEGER,
    max_users_per_dealer INTEGER,
    enable_3d_view BOOLEAN DEFAULT TRUE,
    enable_bulk_import BOOLEAN DEFAULT TRUE,
    enable_advanced_reports BOOLEAN DEFAULT TRUE,
    enable_ai_assistant BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_role_entitlements_role ON role_entitlements(role);
```

### 2. Dealer Entitlements (Subscription Tiers)

```sql
CREATE TABLE dealer_entitlements (
    dealer_id UUID PRIMARY KEY REFERENCES dealers(id) ON DELETE CASCADE,
    subscription_tier VARCHAR(50) NOT NULL,
    
    -- Product and export restrictions (NULL = use role defaults)
    allowed_product_groups TEXT[],
    allowed_export_formats TEXT[],
    
    -- Limits (NULL = use role defaults)
    max_configurations INTEGER,
    max_users INTEGER,
    allowed_regions TEXT[],
    
    -- Feature flags (NULL = use role defaults)
    enable_3d_view BOOLEAN,
    enable_bulk_import BOOLEAN,
    enable_advanced_reports BOOLEAN,
    enable_ai_assistant BOOLEAN,
    
    -- Subscription validity
    valid_from TIMESTAMP NOT NULL DEFAULT NOW(),
    valid_until TIMESTAMP, -- NULL = no expiration
    
    -- Audit
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by UUID REFERENCES users(id),
    updated_by UUID REFERENCES users(id)
);

-- Indexes
CREATE INDEX idx_dealer_entitlements_dealer ON dealer_entitlements(dealer_id);
CREATE INDEX idx_dealer_entitlements_tier ON dealer_entitlements(subscription_tier);
CREATE INDEX idx_dealer_entitlements_validity ON dealer_entitlements(valid_from, valid_until);
```

### 3. User Entitlements (Individual Overrides)

```sql
CREATE TABLE user_entitlements (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    
    -- Overrides (NULL = use dealer/role defaults)
    allowed_product_groups TEXT[],
    allowed_export_formats TEXT[],
    max_configurations INTEGER,
    
    -- Feature overrides
    enable_3d_view BOOLEAN,
    enable_bulk_import BOOLEAN,
    enable_advanced_reports BOOLEAN,
    
    -- Metadata
    notes TEXT, -- Why this user has special entitlements
    expires_at TIMESTAMP, -- NULL = no expiration
    
    -- Audit
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by UUID REFERENCES users(id),
    updated_by UUID REFERENCES users(id)
);

-- Indexes
CREATE INDEX idx_user_entitlements_user ON user_entitlements(user_id);
CREATE INDEX idx_user_entitlements_expiry ON user_entitlements(expires_at);
```

## Seed Data

### Role-Based Defaults

```sql
-- ADMIN - Full access to everything
INSERT INTO role_entitlements (
    role, 
    allowed_product_groups, 
    allowed_export_formats, 
    max_configurations,
    enable_3d_view,
    enable_bulk_import,
    enable_advanced_reports,
    enable_ai_assistant
) VALUES (
    'ADMIN',
    ARRAY['PALLET_RACKING', 'SHELVING', 'MEZZANINE', 'CANTILEVER'],
    ARRAY['PDF', 'PNG', 'DXF', 'DWG', 'EXCEL'],
    NULL, -- Unlimited
    TRUE, TRUE, TRUE, TRUE
);

-- DEALER - Standard dealer access
INSERT INTO role_entitlements (
    role,
    allowed_product_groups,
    allowed_export_formats,
    max_configurations,
    enable_3d_view,
    enable_bulk_import,
    enable_advanced_reports,
    enable_ai_assistant
) VALUES (
    'DEALER',
    ARRAY['PALLET_RACKING', 'SHELVING', 'MEZZANINE'],
    ARRAY['PDF', 'PNG', 'DXF'],
    100,
    TRUE, FALSE, FALSE, FALSE
);

-- DESIGNER - Internal designer access
INSERT INTO role_entitlements (
    role,
    allowed_product_groups,
    allowed_export_formats,
    max_configurations,
    enable_3d_view,
    enable_bulk_import,
    enable_advanced_reports,
    enable_ai_assistant
) VALUES (
    'DESIGNER',
    ARRAY['PALLET_RACKING', 'SHELVING', 'MEZZANINE', 'CANTILEVER'],
    ARRAY['PDF', 'PNG', 'DXF', 'DWG'],
    NULL, -- Unlimited
    TRUE, TRUE, TRUE, FALSE
);

-- VIEWER - Read-only access
INSERT INTO role_entitlements (
    role,
    allowed_product_groups,
    allowed_export_formats,
    max_configurations,
    enable_3d_view,
    enable_bulk_import,
    enable_advanced_reports,
    enable_ai_assistant
) VALUES (
    'VIEWER',
    ARRAY['PALLET_RACKING', 'SHELVING'],
    ARRAY['PDF'],
    0, -- Cannot create
    FALSE, FALSE, FALSE, FALSE
);
```

### Subscription Tier Examples

```sql
-- BASIC Tier - Entry level
INSERT INTO dealer_entitlements (
    dealer_id,
    subscription_tier,
    allowed_product_groups,
    allowed_export_formats,
    max_configurations,
    enable_3d_view,
    enable_bulk_import,
    valid_from
) VALUES (
    'dealer-basic-uuid',
    'BASIC',
    ARRAY['PALLET_RACKING'], -- Only basic product
    ARRAY['PDF'], -- Only PDF export
    10, -- Max 10 configurations
    FALSE, -- No 3D view
    FALSE, -- No bulk import
    NOW()
);

-- PROFESSIONAL Tier - Mid-level
INSERT INTO dealer_entitlements (
    dealer_id,
    subscription_tier,
    allowed_product_groups,
    allowed_export_formats,
    max_configurations,
    enable_3d_view,
    enable_bulk_import,
    valid_from
) VALUES (
    'dealer-pro-uuid',
    'PROFESSIONAL',
    ARRAY['PALLET_RACKING', 'SHELVING'],
    ARRAY['PDF', 'PNG', 'DXF'],
    50,
    TRUE, -- 3D view enabled
    FALSE,
    NOW()
);

-- ENTERPRISE Tier - Full features
INSERT INTO dealer_entitlements (
    dealer_id,
    subscription_tier,
    allowed_product_groups,
    allowed_export_formats,
    max_configurations,
    enable_3d_view,
    enable_bulk_import,
    enable_advanced_reports,
    valid_from
) VALUES (
    'dealer-enterprise-uuid',
    'ENTERPRISE',
    NULL, -- Use role defaults (all products)
    NULL, -- Use role defaults (all formats)
    NULL, -- Unlimited
    TRUE,
    TRUE, -- Bulk import enabled
    TRUE, -- Advanced reports enabled
    NOW()
);
```

## Entitlement Resolution Logic

### Implementation in admin-service

```csharp
public class EntitlementService
{
    private readonly IRoleEntitlementRepository _roleRepo;
    private readonly IDealerEntitlementRepository _dealerRepo;
    private readonly IUserEntitlementRepository _userRepo;

    public async Task<Entitlements> ResolveEntitlementsAsync(User user)
    {
        // 1. Get role-based defaults
        var roleEntitlements = await _roleRepo.GetByRoleAsync(user.Role);
        if (roleEntitlements == null)
        {
            throw new InvalidOperationException($"No entitlements defined for role: {user.Role}");
        }

        // 2. Get dealer-level entitlements (if applicable)
        DealerEntitlements? dealerEntitlements = null;
        if (user.DealerId.HasValue)
        {
            dealerEntitlements = await _dealerRepo.GetByDealerIdAsync(user.DealerId.Value);
            
            // Check if dealer subscription is valid
            if (dealerEntitlements != null && !IsSubscriptionValid(dealerEntitlements))
            {
                // Subscription expired - downgrade to BASIC or block access
                dealerEntitlements = null;
            }
        }

        // 3. Get user-level overrides (if any)
        var userEntitlements = await _userRepo.GetByUserIdAsync(user.Id);
        
        // Check if user override is expired
        if (userEntitlements != null && userEntitlements.ExpiresAt.HasValue 
            && userEntitlements.ExpiresAt < DateTime.UtcNow)
        {
            userEntitlements = null;
        }

        // 4. Merge entitlements (most restrictive wins)
        return new Entitlements
        {
            AllowedProductGroups = MergeProductGroups(
                roleEntitlements.AllowedProductGroups,
                dealerEntitlements?.AllowedProductGroups,
                userEntitlements?.AllowedProductGroups
            ),
            
            AllowedExportFormats = MergeExportFormats(
                roleEntitlements.AllowedExportFormats,
                dealerEntitlements?.AllowedExportFormats,
                userEntitlements?.AllowedExportFormats
            ),
            
            MaxConfigurations = MergeMaxValue(
                roleEntitlements.MaxConfigurations,
                dealerEntitlements?.MaxConfigurations,
                userEntitlements?.MaxConfigurations
            ),
            
            Enable3DView = MergeBoolean(
                roleEntitlements.Enable3DView,
                dealerEntitlements?.Enable3DView,
                userEntitlements?.Enable3DView
            ),
            
            EnableBulkImport = MergeBoolean(
                roleEntitlements.EnableBulkImport,
                dealerEntitlements?.EnableBulkImport,
                userEntitlements?.EnableBulkImport
            ),
            
            EnableAdvancedReports = MergeBoolean(
                roleEntitlements.EnableAdvancedReports,
                dealerEntitlements?.EnableAdvancedReports,
                null
            ),
            
            SubscriptionTier = dealerEntitlements?.SubscriptionTier ?? "BASIC",
            
            Source = new EntitlementSource
            {
                Role = user.Role,
                DealerTier = dealerEntitlements?.SubscriptionTier,
                HasUserOverrides = userEntitlements != null
            }
        };
    }

    private bool IsSubscriptionValid(DealerEntitlements entitlements)
    {
        var now = DateTime.UtcNow;
        return now >= entitlements.ValidFrom 
            && (!entitlements.ValidUntil.HasValue || now <= entitlements.ValidUntil.Value);
    }

    private List<string> MergeProductGroups(
        List<string> role, 
        List<string>? dealer, 
        List<string>? user)
    {
        // Start with role defaults
        var result = role ?? new List<string>();
        
        // Apply dealer restrictions (intersection)
        if (dealer != null && dealer.Any())
        {
            result = result.Intersect(dealer).ToList();
        }
        
        // Apply user restrictions (intersection)
        if (user != null && user.Any())
        {
            result = result.Intersect(user).ToList();
        }
        
        return result;
    }

    private List<string> MergeExportFormats(
        List<string> role,
        List<string>? dealer,
        List<string>? user)
    {
        // Same logic as product groups
        var result = role ?? new List<string>();
        
        if (dealer != null && dealer.Any())
        {
            result = result.Intersect(dealer).ToList();
        }
        
        if (user != null && user.Any())
        {
            result = result.Intersect(user).ToList();
        }
        
        return result;
    }

    private int? MergeMaxValue(int? role, int? dealer, int? user)
    {
        // Take the minimum (most restrictive)
        var values = new[] { role, dealer, user }
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();
        
        return values.Any() ? values.Min() : null;
    }

    private bool MergeBoolean(bool? role, bool? dealer, bool? user)
    {
        // Most restrictive = false wins
        // Priority: user > dealer > role
        if (user.HasValue) return user.Value;
        if (dealer.HasValue) return dealer.Value;
        return role ?? false;
    }
}
```

## API Response Format

### UserContextResponse

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john@abc-dealers.com",
  "fullName": "John Dealer",
  "role": "DEALER",
  
  "permissions": [
    "configurations.create",
    "configurations.read",
    "configurations.update",
    "bom.generate",
    "quote.generate"
  ],
  
  "entitlements": {
    "allowedProductGroups": [
      "PALLET_RACKING",
      "SHELVING"
    ],
    "allowedExportFormats": [
      "PDF",
      "PNG"
    ],
    "maxConfigurations": 50,
    "subscriptionTier": "PROFESSIONAL",
    "enable3DView": true,
    "enableBulkImport": false,
    "enableAdvancedReports": false,
    "enableAIAssistant": false,
    "source": {
      "role": "DEALER",
      "dealerTier": "PROFESSIONAL",
      "hasUserOverrides": false
    }
  },
  
  "dealer": {
    "dealerId": "d123e456-e89b-12d3-a456-426614174000",
    "dealerCode": "ABC-001",
    "companyName": "ABC Dealers Inc.",
    "territory": "NORTH_INDIA"
  },
  
  "preferences": {
    "region": "ap-south-1",
    "country": "IN",
    "currency": "INR",
    "language": "en",
    "measurementUnit": "METRIC"
  }
}
```

## Usage in BFF

### Checking Entitlements

```csharp
// BFF - ConfigurationController
[HttpPost("configurations")]
public async Task<IActionResult> CreateConfiguration([FromBody] CreateConfigurationRequest request)
{
    var userContext = User.GetUserContext();
    
    // 1. Check feature permission
    if (!userContext.Permissions.Contains("configurations.create"))
    {
        return Forbid("Missing configurations.create permission");
    }
    
    // 2. Check product group entitlement
    if (!userContext.Entitlements.AllowedProductGroups.Contains(request.ProductGroup))
    {
        return new ObjectResult(new ErrorResponse
        {
            CorrelationId = HttpContext.GetCorrelationId(),
            ErrorCode = "ENTITLEMENT_RESTRICTION",
            Message = $"Product group '{request.ProductGroup}' is not available in your {userContext.Entitlements.SubscriptionTier} subscription",
            Details = new List<ErrorDetail>
            {
                new ErrorDetail
                {
                    Field = "productGroup",
                    Message = $"Upgrade to access {request.ProductGroup}. Available groups: {string.Join(", ", userContext.Entitlements.AllowedProductGroups)}"
                }
            },
            Timestamp = DateTime.UtcNow,
            Path = HttpContext.Request.Path
        })
        {
            StatusCode = 403
        };
    }
    
    // 3. Check configuration limit
    var configCount = await _catalogClient.GetConfigurationCountAsync(userContext.DealerId);
    if (userContext.Entitlements.MaxConfigurations.HasValue 
        && configCount >= userContext.Entitlements.MaxConfigurations.Value)
    {
        return new ObjectResult(new ErrorResponse
        {
            CorrelationId = HttpContext.GetCorrelationId(),
            ErrorCode = "LIMIT_EXCEEDED",
            Message = $"Maximum configuration limit reached ({userContext.Entitlements.MaxConfigurations})",
            Details = new List<ErrorDetail>
            {
                new ErrorDetail
                {
                    Field = "maxConfigurations",
                    Message = "Upgrade your subscription to create more configurations"
                }
            },
            Timestamp = DateTime.UtcNow,
            Path = HttpContext.Request.Path
        })
        {
            StatusCode = 403
        };
    }
    
    // 4. Proceed with creation
    var command = new CreateConfigurationCommand
    {
        UserId = userContext.UserId,
        DealerId = userContext.Dealer.DealerId,
        Request = request
    };
    
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetConfiguration), new { id = result.Id }, result);
}

// BFF - Export endpoint
[HttpPost("configurations/{id}/export")]
public async Task<IActionResult> ExportConfiguration(
    Guid id, 
    [FromBody] ExportRequest request)
{
    var userContext = User.GetUserContext();
    
    // Check export format entitlement
    if (!userContext.Entitlements.AllowedExportFormats.Contains(request.Format))
    {
        return new ObjectResult(new ErrorResponse
        {
            CorrelationId = HttpContext.GetCorrelationId(),
            ErrorCode = "ENTITLEMENT_RESTRICTION",
            Message = $"Export format '{request.Format}' is not available in your {userContext.Entitlements.SubscriptionTier} subscription",
            Details = new List<ErrorDetail>
            {
                new ErrorDetail
                {
                    Field = "format",
                    Message = $"Upgrade to access {request.Format} export. Available formats: {string.Join(", ", userContext.Entitlements.AllowedExportFormats)}"
                }
            },
            Timestamp = DateTime.UtcNow,
            Path = HttpContext.Request.Path
        })
        {
            StatusCode = 403
        };
    }
    
    // Proceed with export
    var result = await _exportService.ExportAsync(id, request.Format);
    return File(result.Data, result.ContentType, result.FileName);
}
```

## Frontend Usage

```typescript
// React component
function CreateConfigurationForm() {
  const { entitlements } = useUserContext();
  
  return (
    <div>
      <label>Product Group</label>
      <select>
        {/* Only show allowed product groups */}
        {entitlements.allowedProductGroups.map(group => (
          <option key={group} value={group}>
            {group}
          </option>
        ))}
      </select>
      
      {/* Show upgrade prompt for unavailable groups */}
      {!entitlements.allowedProductGroups.includes('MEZZANINE') && (
        <UpgradePrompt 
          feature="Mezzanine Systems"
          currentTier={entitlements.subscriptionTier}
          requiredTier="PROFESSIONAL"
        />
      )}
      
      {/* Show configuration limit */}
      {entitlements.maxConfigurations && (
        <p className="text-sm text-gray-500">
          {configCount} / {entitlements.maxConfigurations} configurations used
        </p>
      )}
    </div>
  );
}

function ExportOptions({ configurationId }) {
  const { entitlements } = useUserContext();
  
  return (
    <div>
      <h3>Export As:</h3>
      
      {/* Show available export formats */}
      {entitlements.allowedExportFormats.includes('PDF') && (
        <Button onClick={() => exportAs('PDF')}>
          <PdfIcon /> Export as PDF
        </Button>
      )}
      
      {entitlements.allowedExportFormats.includes('PNG') && (
        <Button onClick={() => exportAs('PNG')}>
          <ImageIcon /> Export as PNG
        </Button>
      )}
      
      {/* Show upgrade prompt for unavailable formats */}
      {!entitlements.allowedExportFormats.includes('DXF') && (
        <UpgradeButton tier="PROFESSIONAL">
          <LockIcon /> DXF Export (Professional)
        </UpgradeButton>
      )}
      
      {!entitlements.allowedExportFormats.includes('DWG') && (
        <UpgradeButton tier="ENTERPRISE">
          <LockIcon /> DWG Export (Enterprise)
        </UpgradeButton>
      )}
    </div>
  );
}

function FeatureGate({ feature, children }) {
  const { entitlements } = useUserContext();
  
  const isEnabled = {
    '3d-view': entitlements.enable3DView,
    'bulk-import': entitlements.enableBulkImport,
    'advanced-reports': entitlements.enableAdvancedReports,
    'ai-assistant': entitlements.enableAIAssistant
  }[feature];
  
  if (!isEnabled) {
    return <UpgradePrompt feature={feature} />;
  }
  
  return <>{children}</>;
}
```

## Admin Management APIs

### Update Dealer Subscription

```csharp
// admin-service - DealerController
[HttpPut("dealers/{dealerId}/subscription")]
[Authorize(Roles = "ADMIN")]
public async Task<IActionResult> UpdateDealerSubscription(
    Guid dealerId,
    [FromBody] UpdateSubscriptionRequest request)
{
    var entitlements = await _dealerEntitlementRepo.GetByDealerIdAsync(dealerId);
    
    if (entitlements == null)
    {
        // Create new entitlements
        entitlements = new DealerEntitlements
        {
            DealerId = dealerId,
            SubscriptionTier = request.Tier,
            ValidFrom = request.ValidFrom ?? DateTime.UtcNow,
            ValidUntil = request.ValidUntil,
            CreatedBy = User.GetUserId()
        };
    }
    else
    {
        // Update existing
        entitlements.SubscriptionTier = request.Tier;
        entitlements.ValidUntil = request.ValidUntil;
        entitlements.UpdatedBy = User.GetUserId();
        entitlements.UpdatedAt = DateTime.UtcNow;
    }
    
    // Apply tier-specific entitlements
    ApplyTierDefaults(entitlements, request.Tier);
    
    await _dealerEntitlementRepo.SaveAsync(entitlements);
    
    // Invalidate cache for all users in this dealer
    await _cacheService.InvalidateDealerUsersAsync(dealerId);
    
    return Ok(entitlements);
}

private void ApplyTierDefaults(DealerEntitlements entitlements, string tier)
{
    switch (tier)
    {
        case "BASIC":
            entitlements.AllowedProductGroups = new[] { "PALLET_RACKING" };
            entitlements.AllowedExportFormats = new[] { "PDF" };
            entitlements.MaxConfigurations = 10;
            entitlements.Enable3DView = false;
            entitlements.EnableBulkImport = false;
            break;
            
        case "PROFESSIONAL":
            entitlements.AllowedProductGroups = new[] { "PALLET_RACKING", "SHELVING" };
            entitlements.AllowedExportFormats = new[] { "PDF", "PNG", "DXF" };
            entitlements.MaxConfigurations = 50;
            entitlements.Enable3DView = true;
            entitlements.EnableBulkImport = false;
            break;
            
        case "ENTERPRISE":
            entitlements.AllowedProductGroups = null; // Use role defaults
            entitlements.AllowedExportFormats = null; // Use role defaults
            entitlements.MaxConfigurations = null; // Unlimited
            entitlements.Enable3DView = true;
            entitlements.EnableBulkImport = true;
            entitlements.EnableAdvancedReports = true;
            break;
    }
}
```

## Caching Strategy

Entitlements should be cached to avoid database lookups on every request:

```csharp
public class CachedEntitlementService
{
    private readonly IEntitlementService _entitlementService;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

    public async Task<Entitlements> GetEntitlementsAsync(User user)
    {
        var cacheKey = $"entitlements:user:{user.Id}";
        
        // Try cache first
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            return JsonSerializer.Deserialize<Entitlements>(cached);
        }
        
        // Resolve from database
        var entitlements = await _entitlementService.ResolveEntitlementsAsync(user);
        
        // Cache result
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(entitlements),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            }
        );
        
        return entitlements;
    }

    public async Task InvalidateUserAsync(Guid userId)
    {
        await _cache.RemoveAsync($"entitlements:user:{userId}");
    }

    public async Task InvalidateDealerUsersAsync(Guid dealerId)
    {
        // Get all users for this dealer and invalidate their cache
        var users = await _userRepository.GetByDealerIdAsync(dealerId);
        foreach (var user in users)
        {
            await InvalidateUserAsync(user.Id);
        }
    }
}
```

## Summary

**Key Points:**

1. **Role Entitlements** - Base defaults for each role (ADMIN, DEALER, DESIGNER, VIEWER)
2. **Dealer Entitlements** - Subscription tier restrictions (BASIC, PROFESSIONAL, ENTERPRISE)
3. **User Entitlements** - Individual overrides (optional, for special cases)
4. **Resolution** - Most restrictive wins (intersection of allowed values, minimum of limits)
5. **Storage** - Database tables for dynamic management
6. **Caching** - Cache resolved entitlements for performance
7. **Enforcement** - BFF checks before calling services
8. **Frontend** - Uses entitlements to show/hide features and upgrade prompts

This provides a flexible, scalable entitlement system that supports subscription tiers, role-based access, and individual customization.
