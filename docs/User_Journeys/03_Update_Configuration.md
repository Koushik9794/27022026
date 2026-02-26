# User Journey: Update Existing Configuration

Actor: Dealer / Design Consultant / Owner

Preconditions
- User has access to an existing `Configuration` (Draft or InProgress)
- Configuration session may be loaded from DynamoDB or PostgreSQL

Main Flow
1. User opens the Configuration Editor and selects `Open Configuration`.
2. Frontend retrieves the configuration (`GET /configurations/{id}`) and opens the canvas.
3. Frontend subscribes to WebSocket updates for the session (SignalR hub join).
4. User edits layout (move/remove/add components) or updates parameters (aisleWidth, equipmentType).
5. Each change is sent as a delta to `ConfigurationHub.UpdateConfiguration(sessionId, change)`.
6. Rules Engine validates changes and responds with updated `ValidationResult` (errors/warnings/suggestions).
7. Frontend presents suggestions; user can `Apply Suggestion` which updates configuration and triggers re-validation.
8. User clicks `Save` to persist the latest configuration snapshot; `ConfigurationHistory` records the change.
9. For collaboration: lock ownership mechanism or optimistic concurrency (ETag) to prevent conflicting edits.

Alternate Flows
- Concurrent edits: system detects conflicts and offers merge UI or last-writer-wins policy with warnings.
- Large edits (bulk import): process via background job and show progress.

API Notes
- REST: `PUT /configurations/{id}` for full updates, `PATCH /configurations/{id}` for partial
- WebSocket: real-time validation and suggestion application
- History: `GET /configurations/{id}/history`

Data & Audit
- `Configuration` updated
- `ConfigurationHistory` entry with change delta
- `ValidationResult` snapshot saved
- `AuditLog` entry for user action

Postconditions
- Configuration reflects latest user changes
- Validation state updated and persisted
- Change history available for rollback or review
