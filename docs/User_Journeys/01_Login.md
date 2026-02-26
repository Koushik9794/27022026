# User Journey: Login

Actor: Dealer / Internal user / Admin

Preconditions
- User has a registered account (Cognito or identity provider)
- Browser or client is available

Main Flow
1. User opens the app and navigates to the Login page.
2. User enters email/username and password.
3. Frontend calls Authentication API (Cognito) to validate credentials.
4. If configured, second-factor (MFA) challenge is performed (SMS/Authenticator app).
5. On success, the backend returns an access token (JWT) and refresh token.
6. Frontend stores tokens in secure storage (httpOnly cookie or secure store).
7. Frontend establishes WebSocket (SignalR) connection using the access token.
8. User is redirected to the dashboard.

Alternate Flows
- Invalid credentials: show error message, increment login attempt counter.
- MFA failure: allow retry or fallback to recovery flow.
- Account locked: show contact/support instructions.

API Notes
- Authentication: POST `/auth/login` (or Cognito hosted endpoints)
- Tokens: JWT with roles and `sub` claim
- WebSocket: SignalR Hub connection with token in header or query string

Security & UX
- Use httpOnly cookies where possible to protect tokens.
- Short-lived access tokens; use refresh token rotation.
- Log login attempts to `AuditLog`.

Postconditions
- User session is active; userId available in context.
- WebSocket connection ready for real-time validation and updates.
