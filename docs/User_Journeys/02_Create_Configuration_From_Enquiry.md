# User Journey: Create Configuration from Enquiry

Actor: Dealer / Sales Consultant

Preconditions
- A customer enquiry exists (lead record) with basic requirements (building foot-print, desired capacity, country, currency)
- User is authenticated and has permission to create configurations

Main Flow
1. User selects an existing Enquiry from the dashboard.
2. User clicks `Create Configuration from Enquiry`.
3. Frontend requests `/enquiries/{id}` and pre-fills a new `Configuration` with enquiry data (customer, site, target throughput).
4. System opens the Configuration Editor (canvas) with pre-filled parameters.
5. User selects product groups (racking, shelving) and drags components into layout.
6. Frontend sends incremental changes via WebSocket to the Validation Hub (`ConfigurationHub.UpdateConfiguration`).
7. Rules Engine evaluates applicable `BusinessRule`s and returns results (errors/warnings/suggestions) within ~100ms.
8. Frontend displays validation feedback inline; user applies suggestions or edits design.
9. User clicks `Save Draft` — frontend calls POST `/configurations` to persist configuration (status=Draft) in PostgreSQL and DynamoDB session store.
10. User clicks `Generate BOM & Quote` — system calls BOM Service and Quote Service; pricing rules and exchange rates are applied.
11. System returns BOM (CSV/Excel) and quote (PDF) and stores outputs in S3; links displayed to user.

Alternate Flows
- If validation returns critical errors: block `Generate Quote` until resolved.
- If network/WebSocket disconnects: frontend falls back to periodic REST save and resumes WebSocket upon reconnect.

API Notes
- REST: `POST /configurations` (create), `GET /configurations/{id}` (load)
- WebSocket: `ConfigurationHub.UpdateConfiguration(sessionId, change)` for incremental validation
- BOM/Quote: `POST /bom/generate`, `POST /quote/generate`
- Storage: S3 for exports; PostgreSQL for canonical data; DynamoDB for session state

Data Created
- `Configuration` record (sessionId, configurationData, country, currency)
- `ConfigurationHistory` entry
- `ValidationResult` entries for saved validation snapshots
- BOM and Quote artifacts in S3

Postconditions
- Draft configuration persisted and associated with the Enquiry
- BOM and Quote generated and accessible

Performance
- Validation: aim <100ms per incremental change
- BOM/Quote generation: depends on complexity; optimize with background workers and caching
