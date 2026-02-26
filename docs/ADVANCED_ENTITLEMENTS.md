# Advanced Entitlements Architecture

## Multiple Roles per User

### User-Role Relationship

Users can have **multiple roles** (e.g., "Designer within a Dealership"):

```sql
-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    dealer_id UUID REFERENCES dealers(id), -- NULL for internal users
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- User-Role junction table (many-to-many)
CREATE TABLE user_roles (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    role VARCHAR(50) NOT NULL,
    is_primary BOOLEAN DEFAULT FALSE,
    assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    assigned_by UUID REFERENCES users(id),
    expires_at TIMESTAMP, -- Optional: temporary role assignment
    PRIMARY KEY (user_id, role)
);

-- Ensure only one primary role per user
CREATE UNIQUE INDEX idx_user_roles_primary 
ON user_roles (user_id) 
WHERE is_primary = TRUE;

-- Index for role lookups
CREATE INDEX idx_user_roles_role ON user_roles(role);
CREATE INDEX idx_user_roles_user ON user_roles(user_id);
```

### Example Scenarios

```sql
-- Scenario 1: Internal Designer (no dealer)
INSERT INTO users (id, email, full_name, dealer_id)
VALUES ('user-1', 'designer@godrej.com', 'Internal Designer', NULL);

INSERT INTO user_roles (user_id, role, is_primary)
VALUES ('user-1', 'DESIGNER', TRUE);

-- Scenario 2: Designer within a Dealership
INSERT INTO users (id, email, full_name, dealer_id)
VALUES ('user-2', 'designer@abc-dealers.com', 'Dealer Designer', 'dealer-abc');

INSERT INTO user_roles (user_id, role, is_primary)
VALUES 
    ('user-2', 'DESIGNER', TRUE),   -- Primary role
    ('user-2', 'DEALER', FALSE);    -- Secondary role

-- Scenario 3: Admin who is also a Dealer (for testing)
INSERT INTO users (id, email, full_name, dealer_id)
VALUES ('user-3', 'admin@abc-dealers.com', 'Dealer Admin', 'dealer-abc');

INSERT INTO user_roles (user_id, role, is_primary)
VALUES 
    ('user-3', 'ADMIN', TRUE),
    ('user-3', 'DEALER', FALSE);

-- Scenario 4: Temporary elevated access
INSERT INTO user_roles (user_id, role, is_primary, expires_at)
VALUES ('user-4', 'ADMIN', FALSE, NOW() + INTERVAL '7 days');
```

### Multi-Role Entitlement Resolution

```csharp
public class MultiRoleEntitlementService
{
    public async Task<Entitlements> ResolveEntitlementsAsync(User user)
    {
        // 1. Get all active roles for user
        var userRoles = await _userRoleRepository.GetActiveRolesAsync(user.Id);
        
        if (!userRoles.Any())
        {
            throw new InvalidOperationException($"User {user.Id} has no assigned roles");
        }
        
        // 2. Get entitlements for each role
        var roleEntitlementsList = new List<RoleEntitlements>();
        foreach (var userRole in userRoles)
        {
            var roleEntitlements = await _roleEntitlementRepository.GetByRoleAsync(userRole.Role);
            if (roleEntitlements != null)
            {
                roleEntitlementsList.Add(roleEntitlements);
            }
        }
        
        // 3. Merge role entitlements (union of permissions, most restrictive for limits)
        var mergedRoleEntitlements = MergeRoleEntitlements(roleEntitlementsList);
        
        // 4. Apply dealer-level restrictions (if user belongs to a dealer)
        DealerEntitlements? dealerEntitlements = null;
        if (user.DealerId.HasValue)
        {
            dealerEntitlements = await _dealerEntitlementRepository.GetByDealerIdAsync(user.DealerId.Value);
        }
        
        // 5. Apply user-level overrides
        var userEntitlements = await _userEntitlementRepository.GetByUserIdAsync(user.Id);
        
        // 6. Final merge
        return MergeFinalEntitlements(mergedRoleEntitlements, dealerEntitlements, userEntitlements);
    }
    
    private RoleEntitlements MergeRoleEntitlements(List<RoleEntitlements> roleEntitlements)
    {
        // Union of allowed values (most permissive)
        var allowedProductGroups = roleEntitlements
            .SelectMany(r => r.Entitlements.ProductGroups.Allowed)
            .Distinct()
            .ToList();
        
        var allowedExportFormats = roleEntitlements
            .SelectMany(r => r.Entitlements.ExportFormats.Allowed)
            .Distinct()
            .ToList();
        
        // Maximum of limits (most permissive)
        var maxConfigurations = roleEntitlements
            .Select(r => r.MaxConfigurations)
            .Where(m => m.HasValue)
            .Max();
        
        // OR of boolean features (if ANY role allows it)
        var features = new FeaturesConfig
        {
            ThreeDView = roleEntitlements.Any(r => r.Entitlements.Features.ThreeDView),
            BulkImport = roleEntitlements.Any(r => r.Entitlements.Features.BulkImport),
            AdvancedReports = roleEntitlements.Any(r => r.Entitlements.Features.AdvancedReports),
            AIAssistant = roleEntitlements.Any(r => r.Entitlements.Features.AIAssistant)
        };
        
        return new RoleEntitlements
        {
            Entitlements = new EntitlementsData
            {
                ProductGroups = new ProductGroupsConfig { Allowed = allowedProductGroups },
                ExportFormats = new ExportFormatsConfig { Allowed = allowedExportFormats },
                Features = features
            },
            MaxConfigurations = maxConfigurations
        };
    }
}
```

## Future-Proof Schema with JSONB

### Updated Database Schema

```sql
-- Role entitlements with JSONB for flexibility
CREATE TABLE role_entitlements (
    role VARCHAR(50) PRIMARY KEY,
    
    -- Commonly-queried fields (for performance)
    max_configurations INTEGER,
    max_users_per_dealer INTEGER,
    
    -- Dynamic entitlements (JSONB for future extensibility)
    entitlements JSONB NOT NULL DEFAULT '{}'::jsonb,
    
    -- Metadata
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    -- Validation constraint
    CONSTRAINT valid_entitlements CHECK (
        jsonb_typeof(entitlements) = 'object'
    )
);

-- GIN index for JSONB queries
CREATE INDEX idx_role_entitlements_jsonb 
ON role_entitlements USING gin (entitlements);

-- Specific indexes for common queries
CREATE INDEX idx_role_entitlements_3d_view 
ON role_entitlements ((entitlements->'features'->>'3dView'));

CREATE INDEX idx_role_entitlements_product_groups 
ON role_entitlements USING gin ((entitlements->'productGroups'->'allowed'));
```

### JSONB Structure

```json
{
  "productGroups": {
    "allowed": ["PALLET_RACKING", "SHELVING", "MEZZANINE", "CANTILEVER"],
    "metadata": {
      "description": "Product categories available to this role"
    }
  },
  "exportFormats": {
    "allowed": ["PDF", "PNG", "DXF", "DWG", "EXCEL"],
    "limits": {
      "maxFileSize": 100,
      "maxResolution": "4K"
    }
  },
  "features": {
    "3dView": true,
    "bulkImport": true,
    "advancedReports": true,
    "aiAssistant": false,
    "realTimeCollaboration": false,
    "customBranding": false,
    "apiAccess": true
  },
  "limits": {
    "maxFileSize": 100,
    "maxConcurrentUsers": 10,
    "maxApiCallsPerDay": 10000
  },
  "integrations": {
    "allowedProviders": ["AWS", "AZURE", "GCP"],
    "webhooks": {
      "enabled": true,
      "maxEndpoints": 5
    }
  },
  "customFields": {
    // Future unknown fields can go here
  }
}
```

### Seed Data with JSONB

```sql
-- ADMIN role - full access
INSERT INTO role_entitlements (role, max_configurations, entitlements)
VALUES (
    'ADMIN',
    NULL, -- Unlimited
    '{
        "productGroups": {
            "allowed": ["PALLET_RACKING", "SHELVING", "MEZZANINE", "CANTILEVER"]
        },
        "exportFormats": {
            "allowed": ["PDF", "PNG", "DXF", "DWG", "EXCEL"]
        },
        "features": {
            "3dView": true,
            "bulkImport": true,
            "advancedReports": true,
            "aiAssistant": true,
            "realTimeCollaboration": true,
            "customBranding": true,
            "apiAccess": true
        },
        "limits": {
            "maxFileSize": 500,
            "maxConcurrentUsers": 100
        },
        "integrations": {
            "allowedProviders": ["AWS", "AZURE", "GCP"]
        }
    }'::jsonb
);

-- DEALER role - standard access
INSERT INTO role_entitlements (role, max_configurations, entitlements)
VALUES (
    'DEALER',
    100,
    '{
        "productGroups": {
            "allowed": ["PALLET_RACKING", "SHELVING", "MEZZANINE"]
        },
        "exportFormats": {
            "allowed": ["PDF", "PNG", "DXF"]
        },
        "features": {
            "3dView": true,
            "bulkImport": false,
            "advancedReports": false,
            "aiAssistant": false,
            "realTimeCollaboration": false,
            "customBranding": false,
            "apiAccess": false
        },
        "limits": {
            "maxFileSize": 50,
            "maxConcurrentUsers": 5
        }
    }'::jsonb
);

-- DESIGNER role
INSERT INTO role_entitlements (role, max_configurations, entitlements)
VALUES (
    'DESIGNER',
    NULL, -- Unlimited
    '{
        "productGroups": {
            "allowed": ["PALLET_RACKING", "SHELVING", "MEZZANINE", "CANTILEVER"]
        },
        "exportFormats": {
            "allowed": ["PDF", "PNG", "DXF", "DWG"]
        },
        "features": {
            "3dView": true,
            "bulkImport": true,
            "advancedReports": true,
            "aiAssistant": false,
            "realTimeCollaboration": true,
            "customBranding": false,
            "apiAccess": true
        },
        "limits": {
            "maxFileSize": 200,
            "maxConcurrentUsers": 10
        }
    }'::jsonb
);
```

### Querying JSONB

```sql
-- Find roles that allow 3D view
SELECT role, entitlements->'features'->>'3dView' as has_3d_view
FROM role_entitlements
WHERE entitlements->'features'->>'3dView' = 'true';

-- Find roles that allow DXF export
SELECT role
FROM role_entitlements
WHERE entitlements->'exportFormats'->'allowed' @> '["DXF"]'::jsonb;

-- Find roles with AI assistant enabled
SELECT role
FROM role_entitlements
WHERE entitlements->'features'->>'aiAssistant' = 'true';

-- Update to add new feature (no schema change!)
UPDATE role_entitlements
SET entitlements = jsonb_set(
    entitlements,
    '{features,videoExport}',
    'true'::jsonb
)
WHERE role = 'ADMIN';

-- Add new export format
UPDATE role_entitlements
SET entitlements = jsonb_set(
    entitlements,
    '{exportFormats,allowed}',
    (entitlements->'exportFormats'->'allowed')::jsonb || '["SVG"]'::jsonb
)
WHERE role IN ('ADMIN', 'DESIGNER');
```

### C# Implementation with JSONB

```csharp
// Entity
public class RoleEntitlements
{
    public string Role { get; set; }
    public int? MaxConfigurations { get; set; }
    public int? MaxUsersPerDealer { get; set; }
    
    // JSONB column - mapped to C# object
    [Column(TypeName = "jsonb")]
    public EntitlementsData Entitlements { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Flexible entitlements structure
public class EntitlementsData
{
    public ProductGroupsConfig ProductGroups { get; set; } = new();
    public ExportFormatsConfig ExportFormats { get; set; } = new();
    public FeaturesConfig Features { get; set; } = new();
    public LimitsConfig Limits { get; set; } = new();
    public IntegrationsConfig Integrations { get; set; } = new();
    
    // For unknown future fields
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extensions { get; set; } = new();
}

public class ProductGroupsConfig
{
    public List<string> Allowed { get; set; } = new();
}

public class ExportFormatsConfig
{
    public List<string> Allowed { get; set; } = new();
    public ExportLimits? Limits { get; set; }
}

public class ExportLimits
{
    public int? MaxFileSize { get; set; }
    public string? MaxResolution { get; set; }
}

public class FeaturesConfig
{
    [JsonPropertyName("3dView")]
    public bool ThreeDView { get; set; }
    
    public bool BulkImport { get; set; }
    public bool AdvancedReports { get; set; }
    public bool AIAssistant { get; set; }
    public bool RealTimeCollaboration { get; set; }
    public bool CustomBranding { get; set; }
    public bool ApiAccess { get; set; }
    
    // Future features - nullable so old data doesn't break
    public bool? VideoExport { get; set; }
    public bool? MobileApp { get; set; }
}

public class LimitsConfig
{
    public int? MaxFileSize { get; set; }
    public int? MaxConcurrentUsers { get; set; }
    public int? MaxApiCallsPerDay { get; set; }
}

public class IntegrationsConfig
{
    public List<string> AllowedProviders { get; set; } = new();
    public WebhooksConfig? Webhooks { get; set; }
}

public class WebhooksConfig
{
    public bool Enabled { get; set; }
    public int? MaxEndpoints { get; set; }
}
```

### Adding New Features (No Schema Change!)

```csharp
// Adding a new feature without database migration
public async Task AddNewFeatureToRoleAsync(string role, string featureName, bool enabled)
{
    var entitlements = await _repository.GetByRoleAsync(role);
    
    // Use reflection or dynamic to add new feature
    var featuresJson = JsonSerializer.SerializeToElement(entitlements.Entitlements.Features);
    var featuresDict = JsonSerializer.Deserialize<Dictionary<string, object>>(featuresJson);
    
    featuresDict[featureName] = enabled;
    
    entitlements.Entitlements.Features = JsonSerializer.Deserialize<FeaturesConfig>(
        JsonSerializer.Serialize(featuresDict)
    );
    
    await _repository.UpdateAsync(entitlements);
}

// Usage
await AddNewFeatureToRoleAsync("ADMIN", "videoExport", true);
await AddNewFeatureToRoleAsync("ADMIN", "mobileAppAccess", true);
```

## Migration Strategy

### From Array Columns to JSONB

```sql
-- Step 1: Add JSONB column
ALTER TABLE role_entitlements 
ADD COLUMN entitlements_new JSONB;

-- Step 2: Migrate data
UPDATE role_entitlements
SET entitlements_new = jsonb_build_object(
    'productGroups', jsonb_build_object(
        'allowed', to_jsonb(allowed_product_groups)
    ),
    'exportFormats', jsonb_build_object(
        'allowed', to_jsonb(allowed_export_formats)
    ),
    'features', jsonb_build_object(
        '3dView', enable_3d_view,
        'bulkImport', enable_bulk_import,
        'advancedReports', enable_advanced_reports
    )
);

-- Step 3: Drop old columns (after verification)
ALTER TABLE role_entitlements
DROP COLUMN allowed_product_groups,
DROP COLUMN allowed_export_formats,
DROP COLUMN enable_3d_view,
DROP COLUMN enable_bulk_import,
DROP COLUMN enable_advanced_reports;

-- Step 4: Rename new column
ALTER TABLE role_entitlements
RENAME COLUMN entitlements_new TO entitlements;
```

## Summary

**Multiple Roles:**
- Use `user_roles` junction table for many-to-many relationship
- Support primary + secondary roles
- Merge entitlements: Union of permissions, most restrictive for limits

**Future-Proof Schema:**
- Use JSONB for dynamic entitlements structure
- Add new features/formats without schema changes
- Maintain backward compatibility
- Use indexes for query performance

**Benefits:**
- ✅ No schema migrations for new features
- ✅ Flexible role combinations
- ✅ Queryable with PostgreSQL JSONB operators
- ✅ Type-safe in C# with proper models
- ✅ Extensible for unknown future requirements
