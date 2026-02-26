# Authentication Flow - GSS Web API

## Overview

The GSS Web API uses a two-step authentication process:
1. **Login** - Obtain JWT tokens
2. **Context Resolution** - Get user permissions, roles, and preferences

## Authentication Sequence

```
┌─────────┐          ┌──────────┐          ┌─────────────┐          ┌──────────────┐
│ Frontend│          │ BFF      │          │ Admin       │          │ Cognito/     │
│ (UI)    │          │ (Web API)│          │ Service     │          │ Auth Service │
└────┬────┘          └────┬─────┘          └──────┬──────┘          └──────┬───────┘
     │                    │                       │                        │
     │ 1. POST /auth/login│                       │                        │
     │ (email, password)  │                       │                        │
     ├───────────────────>│                       │                        │
     │                    │                       │                        │
     │                    │ 2. Validate credentials                        │
     │                    ├──────────────────────────────────────────────>│
     │                    │                       │                        │
     │                    │ 3. JWT tokens         │                        │
     │                    │<──────────────────────────────────────────────┤
     │                    │                       │                        │
     │ 4. LoginResponse   │                       │                        │
     │ (accessToken,      │                       │                        │
     │  refreshToken)     │                       │                        │
     │<───────────────────┤                       │                        │
     │                    │                       │                        │
     │ 5. Store tokens    │                       │                        │
     │ (secure storage)   │                       │                        │
     │                    │                       │                        │
     │ 6. GET /me/context │                       │                        │
     │ Authorization:     │                       │                        │
     │ Bearer <token>     │                       │                        │
     ├───────────────────>│                       │                        │
     │                    │                       │                        │
     │                    │ 7. Validate JWT       │                        │
     │                    │ (extract userId)      │                        │
     │                    │                       │                        │
     │                    │ 8. Get user details   │                        │
     │                    ├──────────────────────>│                        │
     │                    │                       │                        │
     │                    │ 9. User + permissions │                        │
     │                    │<──────────────────────┤                        │
     │                    │                       │                        │
     │                    │ 10. Aggregate context │                        │
     │                    │ (role, permissions,   │                        │
     │                    │  dealer, preferences, │                        │
     │                    │  feature flags)       │                        │
     │                    │                       │                        │
     │ 11. UserContext    │                       │                        │
     │ (full context)     │                       │                        │
     │<───────────────────┤                       │                        │
     │                    │                       │                        │
     │ 12. Store context  │                       │                        │
     │ (app state)        │                       │                        │
     │                    │                       │                        │
     │ 13. Render UI      │                       │                        │
     │ (based on role &   │                       │                        │
     │  permissions)      │                       │                        │
     │                    │                       │                        │
```

## Step-by-Step Flow

### Step 1-4: Login & Token Acquisition

**Frontend Request:**
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "dealer@example.com",
  "password": "SecurePassword123!"
}
```

**BFF Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "requiresMfa": false
}
```

**JWT Token Payload (Decoded):**
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "dealer@example.com",
  "role": "DEALER",
  "exp": 1704672000,
  "iat": 1704668400
}
```

**Frontend Action:**
- Store `accessToken` in secure storage (httpOnly cookie or secure store)
- Store `refreshToken` for token renewal
- Extract `userId` from response

### Step 5-11: Context Resolution

**Frontend Request:**
```http
GET /api/v1/me/context
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Correlation-ID: abc-123-def
```

**BFF Processing:**
1. Validate JWT token
2. Extract `userId` from token claims
3. Call `admin-service` to get user details
4. Aggregate permissions, dealer info, preferences
5. Apply feature flags based on user role/region
6. Return complete context

**BFF Response:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "dealer@example.com",
  "fullName": "John Dealer",
  "role": "DEALER",
  "permissions": [
    "configurations.create",
    "configurations.read",
    "configurations.update",
    "configurations.delete",
    "bom.generate",
    "quote.generate",
    "quote.approve"
  ],
  "dealer": {
    "dealerId": "d123e456-e89b-12d3-a456-426614174000",
    "dealerCode": "DLR-001",
    "companyName": "ABC Warehouse Solutions",
    "territory": "NORTH_INDIA"
  },
  "preferences": {
    "region": "ap-south-1",
    "country": "IN",
    "currency": "INR",
    "language": "en",
    "measurementUnit": "METRIC"
  },
  "featureFlags": {
    "enable3DView": true,
    "enableRealTimeValidation": true,
    "enableBulkImport": false,
    "enableAdvancedReports": true
  }
}
```

### Step 12-13: UI Rendering

**Frontend Action:**
```javascript
// Store user context in app state (Redux/Zustand/Context API)
const userContext = await api.get('/me/context');
store.dispatch(setUserContext(userContext));

// Conditionally render UI based on permissions
function ConfigurationPage() {
  const { permissions } = useUserContext();
  
  return (
    <div>
      {permissions.includes('configurations.create') && (
        <CreateConfigButton />
      )}
      {permissions.includes('bom.generate') && (
        <GenerateBOMButton />
      )}
      {permissions.includes('quote.approve') && (
        <ApproveQuoteButton />
      )}
    </div>
  );
}

// Show/hide features based on feature flags
function DesignTools() {
  const { featureFlags } = useUserContext();
  
  return (
    <div>
      <Design2DView />
      {featureFlags.enable3DView && <Design3DView />}
      {featureFlags.enableAdvancedReports && <AdvancedReports />}
    </div>
  );
}
```

## Permission-Based Access Control

### Permission Format

Permissions follow the pattern: `{resource}.{action}`

**Examples:**
- `configurations.create` - Can create configurations
- `configurations.read` - Can view configurations
- `configurations.update` - Can edit configurations
- `configurations.delete` - Can delete configurations
- `bom.generate` - Can generate BOM
- `quote.generate` - Can generate quotes
- `quote.approve` - Can approve quotes
- `users.manage` - Can manage users (admin only)

### Role-Based Permissions

| Role | Typical Permissions |
|------|---------------------|
| **DEALER** | configurations.*, bom.generate, quote.generate |
| **DESIGNER** | configurations.*, bom.generate, design.* |
| **ADMIN** | *.* (all permissions) |
| **VIEWER** | configurations.read, bom.read, quote.read |

### Feature Flags

Feature flags control UI features based on:
- User role
- Region/country
- Subscription tier
- Beta program enrollment

**Example Feature Flags:**
```json
{
  "enable3DView": true,           // 3D visualization enabled
  "enableRealTimeValidation": true, // WebSocket validation
  "enableBulkImport": false,      // Bulk configuration import
  "enableAdvancedReports": true,  // Advanced reporting features
  "enableAIAssistant": false      // AI-powered design assistant
}
```

## Token Refresh Flow

When access token expires:

```
┌─────────┐          ┌──────────┐
│ Frontend│          │ BFF      │
└────┬────┘          └────┬─────┘
     │                    │
     │ API call fails     │
     │ (401 Unauthorized) │
     │<───────────────────┤
     │                    │
     │ POST /auth/refresh │
     │ {refreshToken}     │
     ├───────────────────>│
     │                    │
     │ New accessToken    │
     │<───────────────────┤
     │                    │
     │ Retry original     │
     │ API call           │
     ├───────────────────>│
     │                    │
```

## Security Considerations

1. **Token Storage**
   - Use httpOnly cookies for web apps (prevents XSS)
   - Use secure storage for mobile apps (Keychain/Keystore)
   - Never store tokens in localStorage

2. **Token Expiration**
   - Access tokens: Short-lived (1 hour)
   - Refresh tokens: Long-lived (7 days)
   - Implement automatic refresh before expiration

3. **Permission Checks**
   - Always validate permissions on backend (never trust frontend)
   - Frontend permissions are for UI rendering only
   - Backend enforces actual authorization

4. **Correlation IDs**
   - Include `X-Correlation-ID` in all requests
   - Enables request tracing across services
   - Helps with debugging and monitoring

## Example Frontend Implementation

```typescript
// auth.service.ts
class AuthService {
  async login(email: string, password: string): Promise<LoginResponse> {
    const response = await fetch('/api/v1/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });
    
    const data = await response.json();
    
    // Store tokens securely
    this.storeTokens(data.accessToken, data.refreshToken);
    
    return data;
  }
  
  async getUserContext(): Promise<UserContext> {
    const response = await fetch('/api/v1/me/context', {
      headers: {
        'Authorization': `Bearer ${this.getAccessToken()}`,
        'X-Correlation-ID': this.generateCorrelationId()
      }
    });
    
    return response.json();
  }
  
  hasPermission(permission: string): boolean {
    const context = this.getUserContext();
    return context.permissions.includes(permission);
  }
  
  isFeatureEnabled(feature: string): boolean {
    const context = this.getUserContext();
    return context.featureFlags[feature] === true;
  }
}

// Usage in React component
function ConfigurationList() {
  const auth = useAuth();
  const canCreate = auth.hasPermission('configurations.create');
  const canDelete = auth.hasPermission('configurations.delete');
  
  return (
    <div>
      {canCreate && <Button>Create New</Button>}
      {canDelete && <Button>Delete</Button>}
    </div>
  );
}
```

## Summary

**Key Points:**
1. Login returns JWT tokens (authentication)
2. `/me/context` returns full user context (authorization)
3. Frontend uses permissions for UI rendering
4. Backend enforces actual authorization
5. Feature flags control feature availability
6. Tokens are refreshed automatically when expired

This two-step approach separates authentication (who you are) from authorization (what you can do), providing flexibility and security.

## Entity-Level Permissions

**Important:** The permissions in `/me/context` are **feature-level** permissions (e.g., "Can user create configurations?").

**Entity-level** permissions (e.g., "Can user edit THIS specific configuration?") are handled at the **service level**, not the BFF.

### Two-Layer Permission Model

```
Layer 1: Feature-Level (BFF)
├─ Can user access this feature?
├─ Based on role and permissions
└─ Example: configurations.create

Layer 2: Entity-Level (Service)
├─ Can user access THIS specific resource?
├─ Based on ownership, sharing, hierarchy
└─ Example: Can dealer X edit config owned by dealer Y?
```

### Example Flow

```
1. Frontend → BFF: PUT /configurations/123
2. BFF checks: Does user have 'configurations.update'? ✅
3. BFF → Service: UpdateConfiguration(userId, configId)
4. Service checks: Does user own this config? ✅
5. Service → Database: Update configuration
```

**Detailed Guide:** See [`docs/ENTITY_LEVEL_PERMISSIONS.md`](./ENTITY_LEVEL_PERMISSIONS.md) for:
- Architecture patterns
- Implementation examples
- Common permission patterns (ownership, hierarchy, sharing)
- Database schema for sharing
- Testing strategies

## Permissions vs Entitlements vs Feature Flags

Understanding the difference between these three concepts is critical for frontend implementation:

### **Permissions** (What you can DO)
- **Purpose:** Action-based access control
- **Format:** `{resource}.{action}`
- **Examples:** `configurations.create`, `bom.generate`, `quote.approve`
- **Question:** "Can this user perform this action?"
- **Based on:** User role
- **Location in response:** `permissions[]` array

### **Entitlements** (What you have ACCESS TO)
- **Purpose:** Subscription/tier-based access to resources
- **Examples:** Allowed product groups, export formats, configuration limits
- **Question:** "What product groups/formats can this user use?"
- **Based on:** Subscription tier (BASIC, PROFESSIONAL, ENTERPRISE)
- **Location in response:** `entitlements{}` object

### **Feature Flags** (What UI features are ENABLED)
- **Purpose:** UI feature toggles based on subscription tier
- **Examples:** `enable3DView`, `enableBulkImport`, `enableAIAssistant`
- **Question:** "Should we show this UI feature?"
- **Based on:** Subscription tier (part of entitlements)
- **Location in response:** `featureFlags{}` object (subcategory of entitlements)

### Complete Example

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "role": "DEALER",
  
  "permissions": [
    "configurations.create",  // ← Can CREATE configurations
    "configurations.read",
    "configurations.update",
    "bom.generate",          // ← Can GENERATE BOM
    "quote.generate"
  ],
  
  "entitlements": {
    "allowedProductGroups": [
      "PALLET_RACKING",      // ← Can use these products
      "SHELVING"
    ],
    "allowedExportFormats": [
      "PDF",                 // ← Can export in these formats
      "PNG"
    ],
    "maxConfigurations": 50, // ← Limit
    "subscriptionTier": "PROFESSIONAL"
  },
  
  "featureFlags": {
    "enable3DView": true,        // ← UI feature enabled
    "enableBulkImport": false,   // ← UI feature disabled
    "enableAIAssistant": false
  }
}
```

### Frontend Usage Patterns

```javascript
// 1. Check PERMISSION - Can user perform action?
function CreateConfigButton() {
  const { permissions } = useUserContext();
  
  if (!permissions.includes('configurations.create')) {
    return null; // Don't show button at all
  }
  
  return <Button onClick={createConfig}>Create Configuration</Button>;
}

// 2. Check ENTITLEMENT - What resources can user access?
function ProductGroupSelector() {
  const { entitlements } = useUserContext();
  
  return (
    <select>
      {entitlements.allowedProductGroups.map(group => (
        <option key={group} value={group}>{group}</option>
      ))}
    </select>
  );
}

// 3. Check FEATURE FLAG - Is UI feature enabled?
function DesignView() {
  const { featureFlags } = useUserContext();
  
  return (
    <div>
      <Design2DView />
      
      {featureFlags.enable3DView ? (
        <Design3DView />
      ) : (
        <UpgradePrompt 
          feature="3D View" 
          requiredTier="PROFESSIONAL" 
        />
      )}
    </div>
  );
}

// 4. Combined checks
function ExportButton() {
  const { permissions, entitlements } = useUserContext();
  
  // Check permission first
  if (!permissions.includes('configurations.export')) {
    return null;
  }
  
  // Then check entitlement
  return (
    <div>
      {entitlements.allowedExportFormats.includes('PDF') && (
        <Button onClick={() => exportAs('PDF')}>Export PDF</Button>
      )}
      
      {entitlements.allowedExportFormats.includes('DXF') ? (
        <Button onClick={() => exportAs('DXF')}>Export DXF</Button>
      ) : (
        <UpgradeButton tier="PROFESSIONAL">
          Unlock DXF Export
        </UpgradeButton>
      )}
    </div>
  );
}
```

### Summary Table

| Type | Example | Checks | Based On | Location |
|------|---------|--------|----------|----------|
| **Permission** | `configurations.create` | Can user CREATE? | Role | `permissions[]` |
| **Entitlement** | `allowedProductGroups: [...]` | What can user ACCESS? | Subscription Tier | `entitlements{}` |
| **Feature Flag** | `enable3DView: true` | Is feature ENABLED? | Subscription Tier | `featureFlags{}` |

### Key Takeaways

1. **Permissions** = Actions (create, read, update, delete)
2. **Entitlements** = Resources (product groups, export formats, limits)
3. **Feature Flags** = UI Features (3D view, bulk import, AI assistant)

All three are returned in the `/me/context` response and should be used together to control the UI experience.
