# AWS Warehouse Configurator Solution
## Architecture Planning & Disaster Recovery Strategy

**Document Type:** Executive Architecture Brief  
**Version:** 1.0  
**Date:** December 2025  
**Prepared For:** Client Review & Decision Making

---

## Executive Summary

This document outlines the comprehensive AWS cloud architecture for a warehouse storage configurator solution with robust Business Continuity Planning (BCP) and Disaster Recovery (DR) capabilities. The solution enables dealers and users without CAD knowledge to design large warehouse storage configurations, generating 2D/3D visualizations, Bills of Material, and price quotes through a web interface.

**Key Highlights:**
- **Primary Region:** Mumbai (ap-south-1) for optimal latency to Indian user base
- **DR Region:** Singapore (ap-southeast-1) for geographic diversity and low-latency connectivity
- **Architecture Pattern:** Microservices with containerized .NET backend, React/Vite/KonvaJS frontend
- **Recovery Objectives:** RTO < 1 hour, RPO < 5 minutes
- **Expected Uptime:** 99.95% availability (approximately 4.4 hours downtime/year)

---

## Business Context & Requirements

### Functional Requirements

**User Capabilities:**
- Select civil layout templates for warehouse spaces
- Choose product groups (racking systems, shelving, mezzanines)
- Configure SKUs with specific load conditions
- Apply business rules for structural integrity and safety
- Generate 2D floor plans and 3D visualizations
- Produce Bill of Materials with part numbers and quantities
- Receive instant price quotes based on configuration
- Save, retrieve, and modify configurations
- Export designs and quotes for customer presentation

**User Types:**
- Dealers (external sales partners)
- Internal sales teams
- Design consultants
- End customers (view-only access)

### Non-Functional Requirements

**Performance:**
- Page load time: < 2 seconds
- Configuration generation: < 5 seconds
- 2D rendering: < 3 seconds
- 3D rendering: < 10 seconds
- Quote generation: < 5 seconds
- Support 1000 concurrent users
- Handle 500 configuration generations per hour during peak

**Security:**
- Role-based access control (RBAC)
- Multi-factor authentication for dealers
- Data encryption at rest and in transit
- Compliance with data protection regulations
- Audit logging for all design activities

**Scalability:**
- Auto-scale based on demand
- Support business growth (5x over 3 years)
- Handle seasonal spikes (year-end, budget cycles)

---

## Architecture Overview

### Regional Strategy

**Primary Region: Mumbai (ap-south-1)**

**Rationale:**
- Lowest latency for primary user base in India
- Complete AWS service availability
- Data residency compliance for Indian market
- Three Availability Zones for high availability
- Strong connectivity infrastructure

**DR Region: Singapore (ap-southeast-1)**

**Rationale:**
- Geographic separation (approximately 4,100 km from Mumbai)
- Low network latency between regions (30-50ms)
- Independent failure domain
- Meets DR best practice of 500+ km separation
- Same AWS service parity as Mumbai
- Time zone proximity (GMT+5:30 vs GMT+8) for operational convenience

**Alternative Regions Considered:**
- Bahrain: Closer but limited service availability
- Sydney: Greater distance but higher latency (150ms+)
- Tokyo: High latency (100ms+), different time zone challenges

---

## Detailed Architecture Components

### 1. Frontend Architecture

**Technology Stack:**
- React 18+ for UI framework
- Vite for build tooling and fast development
- KonvaJS for 2D canvas rendering and manipulation
- Three.js for 3D visualization
- TypeScript for type safety
- TailwindCSS for styling

**Hosting Strategy:**

**Static Assets (S3 + CloudFront):**
- All compiled React/Vite builds stored in S3
- CloudFront CDN for global distribution
- Automatic edge caching reduces server load
- Custom domain with SSL/TLS certificates
- Web Application Firewall (WAF) for security

**Benefits:**
- Near-instant page loads worldwide (CloudFront edge locations)
- Infinite scalability for static content
- Cost-effective (no server compute for static assets)
- Built-in DDoS protection
- Automatic HTTPS enforcement

**DR Strategy for Frontend:**
- Primary S3 bucket in Mumbai with Cross-Region Replication to Singapore
- CloudFront can serve from either origin
- Route 53 health checks automatically failover origin
- Recovery Time: < 1 minute (automatic)
- Zero data loss (versioning enabled on S3)

---

### 2. API Layer & Routing

**Route 53 DNS Management:**

**Configuration:**
- Primary DNS record: api.configurator.com
- Failover routing policy (Primary: Mumbai, Secondary: Singapore)
- Health checks every 30 seconds
- TTL: 60 seconds for fast propagation during failover
- Automatic failover on three consecutive health check failures

**Benefits:**
- Automatic traffic routing to healthy region
- Global DNS with low latency resolution
- No manual intervention for failover
- Health-based routing prevents user impact

**Application Load Balancer (ALB):**

**Mumbai Primary:**
- Deployed across 3 Availability Zones
- Public subnets for internet-facing traffic
- SSL/TLS termination with AWS Certificate Manager
- Path-based routing to different microservices
- Connection draining for graceful shutdown
- Access logs to S3 for audit and analysis

**Singapore DR:**
- Identical configuration to Mumbai
- Normally receives no traffic (passive standby)
- Can be activated to active-active for global load distribution
- Health checks verify backend service availability

**Benefits:**
- Automatic distribution across multiple AZs
- Self-healing (replaces unhealthy targets)
- Zero-downtime deployments
- Built-in security features

**DR Consideration:**
- Route 53 detects Mumbai ALB failure within 90 seconds
- Automatic DNS update points traffic to Singapore
- Users experience brief delay (DNS TTL propagation)
- No manual intervention required

---

### 3. Microservices Architecture (ECS Fargate)

**Service Decomposition Strategy:**

**1. Configuration Service**
- **Responsibility:** Manages civil layouts, product catalogs, SKU selection
- **Key Functions:** 
  - Validate user selections against business rules
  - Calculate space requirements and constraints
  - Store configuration metadata
- **Dependencies:** PostgreSQL, DynamoDB, Redis cache
- **Scaling Profile:** Moderate (2-10 instances based on load)

**2. Design Engine Service**
- **Responsibility:** Generates 2D and 3D designs
- **Key Functions:**
  - Converts configuration to visual layouts
  - Applies engineering constraints
  - Coordinates with rendering workers
  - Load condition analysis and validation
- **Dependencies:** S3 (CAD templates), Lambda (rendering), Redis
- **Scaling Profile:** High (can burst to 20+ instances during peak)
- **Compute Requirements:** CPU-intensive for complex calculations

**3. BOM Service**
- **Responsibility:** Bill of Materials generation
- **Key Functions:**
  - Extract parts list from configuration
  - Calculate quantities based on design
  - Integrate with product catalog for part numbers
  - Handle custom components and exceptions
- **Dependencies:** PostgreSQL (product catalog), DynamoDB (BOM cache)
- **Scaling Profile:** Low-moderate (2-5 instances)

**4. Quote Service**
- **Responsibility:** Pricing and quote generation
- **Key Functions:**
  - Apply pricing rules and discounts
  - Calculate regional variations
  - Generate PDF quotes
  - Track quote versions and approvals
- **Dependencies:** PostgreSQL (pricing data), S3 (quote PDFs), Lambda (PDF generation)
- **Scaling Profile:** Moderate (3-8 instances)

**5. User Management Service**
- **Responsibility:** Authentication, authorization, user profiles
- **Key Functions:**
  - Integration with Amazon Cognito
  - Dealer profile management
  - Role-based access control (RBAC)
  - User activity tracking
- **Dependencies:** Cognito, PostgreSQL, DynamoDB
- **Scaling Profile:** Low-moderate (2-5 instances)

**6. File Processing Service**
- **Responsibility:** Handle file uploads/downloads, format conversions
- **Key Functions:**
  - Upload design files to S3
  - Generate shareable links
  - Convert between file formats (DWG, PDF, STL)
  - Virus scanning on uploads
- **Dependencies:** S3, Lambda (format conversion)
- **Scaling Profile:** Moderate (2-8 instances based on upload volume)

**Container Orchestration (ECS Fargate):**

**Why Fargate over EC2:**
- No server management or patching overhead
- Pay only for actual container runtime (cost-efficient for variable load)
- Automatic scaling without cluster capacity planning
- Built-in security isolation (task-level)
- Faster deployments and scaling

**Configuration Per Service:**
- CPU: 1-2 vCPU per task (adjustable based on profiling)
- Memory: 2-4 GB per task
- Minimum tasks: 2 (for high availability)
- Maximum tasks: 10-20 (based on auto-scaling policies)
- Health check grace period: 60 seconds
- Deployment strategy: Rolling update (blue-green optional for critical services)

**Service Discovery (AWS Cloud Map):**
- Internal DNS-based service discovery
- Services locate each other by service name (e.g., configuration-service.local)
- No hardcoded IPs or endpoints
- Automatic registration/deregistration

**Auto-Scaling Triggers:**
- CPU utilization > 70% for 2 minutes → scale out
- Memory utilization > 80% for 2 minutes → scale out
- ALB request count > 1000 per target → scale out
- Schedule-based scaling for known peak hours
- Scale-in cooldown: 5 minutes to prevent thrashing

**DR Strategy for Microservices:**

**Mumbai (Primary):**
- All services running at full capacity
- Auto-scaling enabled
- Active health checks

**Singapore (DR):**
- Task definitions replicated and up-to-date
- Container images in ECR Singapore
- Services scaled to ZERO (standby mode)
- During DR activation: Scale from 0 to minimum capacity (2-3 minutes)
- Environment variables point to Singapore resources

**Failover Process:**
1. Detect Mumbai failure via Route 53 health checks
2. Automated script (Lambda) scales Singapore services from 0 to 3
3. Services pick up Singapore database connections from Parameter Store
4. Traffic begins flowing within 5 minutes of detection
5. Monitor and adjust scaling based on actual load

**Benefits:**
- Cost-effective DR (no compute charges when not in use)
- Rapid activation (infrastructure already defined)
- Identical configuration ensures consistency
- Can test DR by temporarily scaling up

---

### 4. Database Architecture

#### PostgreSQL (Amazon RDS) - Primary Transactional Database

**Use Cases:**
- User accounts and authentication details
- Project configurations (metadata, ownership)
- Product catalog (SKUs, specifications, pricing)
- Quote history and approvals
- Audit logs and compliance tracking
- Relational data requiring ACID properties

**Configuration:**

**Primary Instance (Mumbai):**
- Engine: PostgreSQL 15.x
- Instance Class: db.r6g.xlarge (4 vCPU, 32 GB RAM)
- Storage: 500 GB General Purpose SSD (gp3) with autoscaling to 2TB
- Multi-AZ Deployment: Enabled (synchronous replication to standby in different AZ)
- Automated Backups: Daily, 7-day retention
- Point-in-Time Recovery: Enabled (restore to any second within retention)
- Maintenance Window: Sunday 2-4 AM IST
- Performance Insights: Enabled for query optimization

**Read Replicas (Mumbai):**
- Two read replicas in Mumbai for read scaling
- Distribute SELECT queries across replicas
- Reduce load on primary for reporting and analytics
- Asynchronous replication (typically < 1 second lag)

**DR Read Replica (Singapore):**
- Asynchronous cross-region replication from Mumbai primary
- Typical replication lag: 1-5 seconds
- Can be promoted to standalone database in DR scenario
- Read-only unless promoted

**DR Strategy:**

**Normal Operation:**
- Mumbai primary handles all writes
- Mumbai read replicas handle read traffic
- Singapore replica stays synchronized (passive)

**DR Activation:**
1. Promote Singapore read replica to standalone instance (10-15 minutes)
2. Singapore instance becomes read-write
3. Update connection strings in AWS Systems Manager Parameter Store
4. ECS services connect to Singapore database
5. Mumbai data frozen at point of failure (RPO ≈ 5 seconds of replication lag)

**Recovery Point Objective (RPO):** 5 seconds
- Based on typical replication lag
- In worst case: up to 30 seconds if network congestion

**Recovery Time Objective (RTO):** 15 minutes
- 2 minutes: Detection
- 10 minutes: Promotion process
- 3 minutes: Service reconnection and validation

**Failback Considerations:**
- When Mumbai recovers, create new read replica from Singapore
- Wait for full synchronization
- Promote Mumbai to primary during planned maintenance window
- Reverse replication direction

**Why PostgreSQL:**
- Strong consistency and ACID compliance
- Excellent support for complex queries and joins
- JSON/JSONB support for semi-structured configuration data
- Mature ecosystem and tooling
- Cost-effective for relational workloads
- AWS RDS provides automated management (backups, patching, monitoring)

---

#### DynamoDB - High-Velocity, Semi-Structured Data

**Use Cases:**
- Active user sessions (real-time configuration state)
- Product catalog cache (fast lookups)
- Business rules engine cache
- Real-time collaboration data
- Event sourcing for configuration changes
- Time-series data (user interactions, click tracking)

**Table Design:**

**1. ConfigurationSessions Table**
- **Purpose:** Store in-progress user configuration work
- **Partition Key:** sessionId (UUID)
- **Sort Key:** timestamp
- **Attributes:** userId, configData (JSON), civLayout, productSelections, lastModified
- **TTL:** 7 days (auto-delete inactive sessions)
- **Billing:** On-demand (unpredictable usage pattern)
- **Global Secondary Index:** userId-timestamp-index (retrieve all user sessions)

**2. ProductCatalog Table**
- **Purpose:** Fast product lookups during configuration
- **Partition Key:** productId (SKU)
- **Attributes:** productGroup, category, specifications (JSON), pricing, availability
- **Billing:** Provisioned (100 Read Capacity Units, 50 Write Capacity Units)
- **Global Secondary Index:** productGroup-category-index (browse by category)
- **Caching:** Redis sits in front for hot items

**3. BusinessRulesCache Table**
- **Purpose:** Store pre-computed business rule results
- **Partition Key:** ruleId
- **Sort Key:** inputHash (hash of input parameters)
- **Attributes:** result, validUntil
- **Billing:** On-demand
- **TTL:** Rule-specific (1 hour to 24 hours)

**DynamoDB Global Tables (Multi-Region, Multi-Active):**

**Configuration:**
- Regions: Mumbai (primary), Singapore (DR)
- Automatic bidirectional replication
- Conflict Resolution: Last-writer-wins (timestamp-based)
- Replication Latency: Typically < 1 second

**DR Strategy:**

**Benefits for DR:**
- **RPO:** < 1 second (near real-time replication)
- **RTO:** 0 seconds (both regions always active and writable)
- **No manual intervention:** Automatic failover at application level
- **Active-Active Capable:** Can serve reads from both regions simultaneously

**Normal Operation:**
- Mumbai services write to Mumbai DynamoDB
- Automatically replicates to Singapore in background
- Singapore table available for reads (optional optimization)

**DR Activation:**
- Singapore services simply start using Singapore DynamoDB table
- Data is already synchronized
- No promotion or manual steps required
- Instant availability

**Considerations:**
- Eventual consistency (typically < 1 second)
- Potential for write conflicts if active-active (last-writer-wins)
- Higher cost (2x cost for global tables)

**Why DynamoDB:**
- Millisecond latency at any scale
- No capacity planning (on-demand billing available)
- Built-in replication across regions
- Serverless (no servers to manage)
- Perfect for session data and caching
- Integrates seamlessly with Lambda for event-driven processing

---

#### Amazon S3 - File Storage

**Use Cases:**
- 2D design files (DWG, PDF exports)
- 3D model files (STL, OBJ, GLTF)
- Generated Bill of Materials (Excel, PDF)
- Quote documents (PDF)
- CAD templates and libraries
- User uploads (reference images, existing layouts)
- Application static assets (React build)

**Bucket Strategy:**

**1. Design Files Bucket**
- **Name:** configurator-designs-prod-mumbai
- **Purpose:** Store generated 2D/3D designs
- **Versioning:** Enabled (track design revisions)
- **Lifecycle Policy:** 
  - Current version: Standard storage
  - After 90 days: Transition to Intelligent-Tiering
  - After 1 year: Transition to Glacier for archival
- **Cross-Region Replication:** To configurator-designs-prod-singapore
- **Replication Time Control:** Enabled (99.99% replication within 15 minutes)
- **Encryption:** Server-Side Encryption (SSE-S3)
- **Access:** Fargate task IAM roles only

**2. Documents Bucket (BOM, Quotes)**
- **Name:** configurator-documents-prod-mumbai
- **Purpose:** Generated business documents
- **Versioning:** Enabled
- **Lifecycle Policy:** 
  - Retain for 7 years (compliance requirement)
  - Transition to Glacier Deep Archive after 3 years
- **Cross-Region Replication:** Real-time to Singapore
- **Encryption:** Server-Side Encryption with KMS for sensitive quotes
- **Access:** Pre-signed URLs for customer downloads (time-limited)

**3. Application Assets Bucket**
- **Name:** configurator-web-primary
- **Purpose:** React/Vite compiled builds, images, fonts
- **Versioning:** Enabled
- **Cross-Region Replication:** To configurator-web-dr
- **CloudFront Integration:** Origin for CDN
- **Lifecycle:** No deletion policy (all versions retained)

**4. Templates Library Bucket**
- **Name:** configurator-templates-prod
- **Purpose:** CAD templates, layout presets, reusable components
- **Access:** Public read (for templates), restricted write
- **Cross-Region Replication:** Enabled
- **Versioning:** Enabled (track template updates)

**DR Strategy for S3:**

**Cross-Region Replication (CRR):**
- All production buckets replicate Mumbai → Singapore
- Replication Time Control (RTC) guarantees 99.99% within 15 minutes
- Versioning ensures no data loss during replication
- Metadata and ACLs also replicated

**Normal Operation:**
- Services write to Mumbai buckets
- Background replication to Singapore
- Reads from Mumbai for lower latency

**DR Activation:**
- Services switch to reading/writing Singapore buckets
- All data already present (max 15 minutes lag for recent files)
- Pre-signed URLs continue to work (bucket name in URL)
- May need to update URL generation logic to point to Singapore

**Recovery Point Objective (RPO):** 15 minutes
- With RTC: 99.99% of objects replicated within 15 minutes
- Critical files can be replicated in seconds

**Recovery Time Objective (RTO):** < 1 minute
- S3 read access is immediate
- Only need to update application configuration

**Why S3:**
- 99.999999999% (11 nines) durability
- Infinite scalability
- Cost-effective for large file storage
- Built-in versioning and lifecycle management
- Cross-region replication for DR
- Integrates with CloudFront for fast delivery

---

#### Redis (Amazon ElastiCache) - Caching Layer

**Use Cases:**
- Product catalog hot cache (frequently accessed SKUs)
- Business rules results cache
- Session state backup (complement to DynamoDB)
- API response caching
- Rate limiting counters
- Real-time leaderboards (e.g., top dealers)

**Configuration:**

**Primary Cluster (Mumbai):**
- Engine: Redis 7.x (latest)
- Node Type: cache.r6g.large (13.07 GB memory, 2 vCPU)
- Cluster Mode: Enabled (sharded for scale)
- Multi-AZ: Enabled with automatic failover
- Number of Shards: 3 (for horizontal scaling)
- Replicas per Shard: 1 (2 nodes total per shard)
- Encryption: In-transit and at-rest enabled
- Backup: Daily automatic snapshots, 7-day retention

**DR Cluster (Singapore):**
- Redis Global Datastore linking Mumbai → Singapore
- Asynchronous replication from Mumbai
- Read-only in Singapore (unless promoted)
- Typical replication lag: < 1 second

**Cache Eviction Policy:**
- allkeys-lru (Least Recently Used)
- Maximizes hit rate for hot data

**DR Strategy:**

**Normal Operation:**
- Mumbai services read/write to Mumbai Redis
- Automatic replication to Singapore in background

**DR Activation:**
1. Promote Singapore Redis cluster to independent cluster (1-2 minutes)
2. Singapore cluster becomes read-write
3. Services connect to Singapore Redis endpoint
4. Cache may be cold (lower hit rate initially)
5. Optional: Pre-warm cache with critical data

**Recovery Point Objective (RPO):** < 1 second
- Cache data, some loss acceptable (can rebuild from database)

**Recovery Time Objective (RTO):** 2 minutes
- Automatic failover within AZ: < 30 seconds
- Cross-region promotion: 1-2 minutes

**Cache Warming Strategy:**
- Post-failover: Background job queries most popular products
- Rebuild business rules cache from DynamoDB
- Gradual warm-up as users access data

**Why Redis:**
- Sub-millisecond latency
- Reduces database load by 70-80%
- Supports complex data structures (lists, sets, sorted sets)
- Pub/sub for real-time features
- Highly available with automatic failover
- Managed service (no Redis server management)

---

### 5. Asynchronous Processing & Event-Driven Architecture

**Amazon SQS (Simple Queue Service):**

**Queues:**

**1. design-rendering-queue (FIFO)**
- **Purpose:** Queue 2D/3D rendering jobs
- **Type:** FIFO (First-In-First-Out) to preserve order
- **Message Retention:** 14 days
- **Visibility Timeout:** 5 minutes (rendering time)
- **Dead Letter Queue:** Yes (after 3 failed attempts)
- **Consumer:** Lambda functions for rendering

**2. bom-generation-queue (Standard)**
- **Purpose:** Asynchronous BOM generation
- **Type:** Standard (higher throughput, at-least-once delivery)
- **Message Retention:** 7 days
- **Visibility Timeout:** 2 minutes
- **Consumer:** BOM Service or Lambda

**3. quote-processing-queue (Standard)**
- **Purpose:** PDF quote generation and email
- **Message Retention:** 7 days
- **Consumer:** Lambda (generates PDF, uploads to S3, sends email)

**4. notification-queue (Standard)**
- **Purpose:** Email, SMS, in-app notifications
- **Integration:** Amazon SNS → SQS → Lambda

**Benefits:**
- Decouples services (configuration service doesn't wait for rendering)
- Handles traffic spikes (queue absorbs burst load)
- Retry logic for transient failures
- Guaranteed delivery (messages not lost)

**DR Strategy for SQS:**
- Queues are regional resources
- Replicate queue definitions in Singapore (IaC)
- During DR: Messages in Mumbai queue may be lost (acceptable for async tasks)
- Application sends new messages to Singapore queues
- DLQ messages can be manually transferred if critical

---

**Amazon EventBridge (Event Bus):**

**Use Cases:**
- Inter-service communication (event-driven)
- Workflow orchestration
- Audit logging (all configuration changes)
- Integration with external systems

**Event Patterns:**

**1. Configuration Events:**
- Event: ConfigurationCreated, ConfigurationModified, ConfigurationDeleted
- Subscribers: Audit service, analytics, notification service

**2. Design Events:**
- Event: DesignGenerationStarted, DesignGenerationCompleted, DesignGenerationFailed
- Subscribers: User notification, monitoring

**3. Quote Events:**
- Event: QuoteGenerated, QuoteApproved, QuoteRejected
- Subscribers: CRM integration, email service, analytics

**DR Strategy:**
- Event bus definitions replicated to Singapore
- Cross-region event forwarding (optional for critical events)
- Events during failover window may be lost (acceptable for most use cases)
- Replay capability from DynamoDB event store if needed

---

**AWS Lambda Functions:**

**Use Cases:**
- 2D/3D rendering workers (triggered by SQS)
- PDF generation for quotes and BOM
- Image optimization and thumbnail generation
- File format conversion (DWG → PDF, STL → OBJ)
- Email sending (triggered by SNS)
- Scheduled tasks (cleanup, cache warming)

**Example Functions:**

**1. RenderingWorkerFunction**
- Trigger: SQS message from design-rendering-queue
- Runtime: Python 3.11 (uses CAD libraries)
- Memory: 3 GB
- Timeout: 5 minutes
- Concurrency: 50 concurrent executions
- Output: Upload rendered image to S3, update DynamoDB status

**2. QuoteGeneratorFunction**
- Trigger: SQS message from quote-processing-queue
- Runtime: .NET 10 (consistent with microservices)
- Memory: 1 GB
- Timeout: 2 minutes
- Uses: PDFSharp or similar library for PDF generation

**DR Strategy:**
- Lambda deployment packages in both regions (from S3)
- During DR: Functions in Singapore automatically available
- Minimal RTO (functions are serverless, no infrastructure)

---

### 6. Authentication & Authorization

**Amazon Cognito:**

**User Pool Configuration:**
- User pool for all application users (dealers, internal staff)
- Multi-factor authentication (MFA) enforced for all users
- Password policy: Minimum 12 characters, complexity requirements
- Account recovery: Email and SMS
- Custom attributes: dealerCode, companyName, territory
- JWT token expiration: 1 hour (access), 30 days (refresh)

**Identity Pool (Federated Identities):**
- Allows temporary AWS credentials for direct S3 uploads
- Role-based access: Dealer role (limited), Admin role (full access)
- Fine-grained S3 access (users can only access their own files)

**Authentication Flow:**
1. User logs in via Cognito Hosted UI or custom login page
2. Cognito validates credentials, issues JWT tokens
3. Client includes JWT in API requests (Authorization header)
4. API Gateway or ALB validates JWT signature
5. Microservices extract user identity from validated token

**Authorization (RBAC):**
- Roles: SuperAdmin, Admin, Dealer, Designer, Viewer
- Permissions stored in PostgreSQL (role_permissions table)
- Claims in JWT: userId, email, role, permissions[]
- Services enforce permissions at API endpoint level

**DR Strategy:**
- Cognito is a regional service but highly available (multi-AZ)
- User pool configuration exported daily to S3 (both regions)
- In catastrophic scenario: Can recreate user pool in Singapore
- User data backed up, minimal RPO/RTO impact
- Federation tokens work across regions (identity-based)

---

### 7. Monitoring, Logging & Observability

**Amazon CloudWatch:**

**Metrics:**
- Default AWS metrics (CPU, memory, network, disk I/O)
- Custom application metrics:
  - Configuration generation time
  - Rendering queue depth
  - Cache hit rate
  - API endpoint latency by service
  - Business metrics (configurations per hour, quotes generated)

**Logs:**
- Centralized logging from all services
- Log Groups:
  - /ecs/configurator/[service-name]
  - /lambda/[function-name]
  - /rds/configurator/postgresql (slow query log, error log)
- Log Retention: 30 days for application logs, 90 days for audit logs
- Insights Queries: Pre-built queries for common troubleshooting

**Alarms:**
- P0 (Critical): Service down, database unreachable, regional failure
- P1 (High): High error rate (>1%), database replication lag >10s
- P2 (Medium): High latency (>2s), cache eviction rate high
- P3 (Low): Disk space >80%, unusual traffic patterns

**Dashboards:**
- Executive Dashboard: High-level KPIs (uptime, user activity, revenue metrics)
- Operations Dashboard: System health, resource utilization, error rates
- DR Dashboard: Replication status, health checks, failover readiness

**AWS X-Ray (Distributed Tracing):**
- Trace requests across microservices
- Identify bottlenecks and performance issues
- Visualize service map (dependency graph)
- Analyze latency contributions by service

**DR Monitoring:**
- Cross-region replication lag metrics
- Health check status for Mumbai and Singapore
- Automated alerting for DR readiness issues
- Regular DR drill results tracked

---

### 8. Security Architecture

**Network Security:**

**VPC Design (per region):**
- CIDR: 10.0.0.0/16 (Mumbai), 10.1.0.0/16 (Singapore)
- Public Subnets: 3 subnets across 3 AZs (for ALB, NAT Gateways)
- Private Subnets: 3 subnets across 3 AZs (for ECS, RDS, Redis, Lambda)
- Database Subnets: 3 isolated subnets for RDS

**Security Groups (Least Privilege):**
- ALB Security Group: Allow 443 from 0.0.0.0/0, 80 → 443 redirect
- ECS Security Group: Allow traffic only from ALB (port 8080)
- RDS Security Group: Allow PostgreSQL (5432) only from ECS and Lambda
- Redis Security Group: Allow Redis (6379) only from ECS
- Lambda Security Group: Allow outbound to RDS, Redis, internet (via NAT)

**Network ACLs:**
- Subnet-level firewall rules
- Deny known malicious IPs
- Allow only necessary protocols

**NAT Gateways:**
- One per AZ for high availability
- Allows private subnet resources to access internet (for API calls, package updates)
- Prevents inbound connections from internet

**VPC Flow Logs:**
- Capture all network traffic metadata
- Store in S3 for security analysis and compliance
- Monitor for unusual patterns

**Data Security:**

**Encryption at Rest:**
- RDS: AWS KMS encryption (customer-managed key optional)
- DynamoDB: AWS-owned key or KMS
- S3: Server-Side Encryption (SSE-S3 or SSE-KMS)
- EBS volumes (Fargate): Encrypted by default
- ElastiCache: Encryption at rest enabled

**Encryption in Transit:**
- All ALB listeners: HTTPS only (TLS 1.2+)
- RDS connections: SSL/TLS enforced
- Redis: TLS enabled for in-transit encryption
- S3: HTTPS for all API calls
- Inter-service communication: mTLS (optional for enhanced security)

**Secrets Management:**
- AWS Secrets Manager: Database passwords, API keys, third-party credentials
- Automatic rotation: Database passwords rotated every 90 days
- IAM roles for ECS tasks: No hardcoded credentials
- Systems Manager Parameter Store: Non-secret configuration (endpoints, feature flags)

**Application Security:**

**Web Application Firewall (WAF):**
- Attached to CloudFront and ALB
- AWS Managed Rules: SQL injection, XSS protection
- Rate limiting: 2000 requests per 5 minutes per IP
- Geo-blocking: Optional (block specific countries)
- Custom rules: Block known attack patterns

**DDoS Protection:**
- AWS Shield Standard (automatic, free)
- AWS Shield Advanced (optional, $3000/month, 24/7 DDoS response team)

**API Security:**
- JWT-based authentication
- Input validation on all endpoints
- Output encoding to prevent XSS
- CORS policy (restrict allowed origins)
- API rate limiting per user

**Compliance & Audit:**

**AWS CloudTrail:**
- Log all API calls to AWS services
- Stored in S3 with encryption
- Integrity validation enabled
- Multi-region trail (captures both Mumbai and Singapore)

**AWS Config:**
- Continuous compliance monitoring
- Rules: Ensure S3 encryption, RDS Multi-AZ, security group compliance
- Automated remediation for non-compliant resources

**Amazon GuardDuty:**
- Threat detection service
- Analyzes CloudTrail, VPC Flow Logs, DNS logs
- Alerts on suspicious activity (compromised credentials, cryptocurrency mining, unusual API calls)

**Vulnerability Scanning:**
- Amazon Inspector: Scans Fargate tasks and Lambda functions for vulnerabilities
- Third-party: Periodic penetration testing
- Dependency scanning: Automated checks for vulnerable libraries

**Compliance Considerations:**
- Data residency: All data stored in India (Mumbai) for compliance
- GDPR-ready: User data deletion workflows
- SOC 2 Type II: Audit trail for all changes
- ISO 27001: Security controls documented

---

## API Strategy: REST vs GraphQL Analysis

### REST API Approach

**Implementation:**
- Each microservice exposes RESTful endpoints
- API Gateway or ALB routes to appropriate service
- Standard HTTP methods (GET, POST, PUT, DELETE)
- JSON request/response format

**Endpoints Examples:**
```
POST   /api/configuration/create
GET    /api/configuration/{id}
PUT    /api/configuration/{id}
DELETE /api/configuration/{id}

POST   /api/design/generate
GET    /api/design/{id}/download

POST   /api/bom/generate
GET    /api/bom/{id}

POST   /api/quote/create
GET    /api/quote/{id}/pdf
```

**Advantages for This Project:**

**1. Simplicity & Team Familiarity:**
- .NET has excellent support for REST (ASP.NET Core Web API)
- Well-understood patterns and conventions
- Easier for junior developers to contribute
- Abundant libraries and tooling

**2. Caching:**
- HTTP caching works out-of-the-box (CloudFront, browser cache)
- GET requests are cacheable by default
- ETags for conditional requests
- Perfect for product catalog, templates

**3. API Gateway Integration:**
- AWS API Gateway has native REST support
- Built-in throttling, authentication, usage plans
- Can use AWS WAF for protection

**4. Microservices Alignment:**
- Each service owns its REST endpoints
- Clear service boundaries
- Independent deployment and versioning

**5. Monitoring & Debugging:**
- Standard HTTP status codes (200, 404, 500)
- Easy to monitor with ALB access logs
- Clear error messages and debugging

**Disadvantages:**

**1. Over-fetching:**
- Frontend may receive more data than needed
- Example: GET /api/configuration/{id} returns entire object
- Can partially mitigate with query parameters (?fields=name,layout)

**2. Under-fetching:**
- Multiple round trips to get related data
- Example: Get configuration → Get product details → Get pricing
- Can create aggregate endpoints, but increases backend complexity

**3. Versioning Challenges:**
- Breaking changes require /v2/ endpoints
- Can end up with /v1/, /v2/, /v3/ proliferation
- Maintaining multiple versions increases overhead

**4. Frontend Complexity:**
- Frontend must orchestrate multiple calls
- More state management logic
- Potential for race conditions

---

### GraphQL Approach

**Implementation:**
- Single GraphQL endpoint (e.g., /graphql)
- Schema defines available queries and mutations
- Resolvers fetch data from microservices
- Client requests exactly what it needs

**Schema Example:**
```graphql
type Configuration {
  id: ID!
  userId: String!
  civilLayout: CivilLayout!
  products: [Product!]!
  bom: BillOfMaterials
  quote: Quote
  createdAt: DateTime!
}

type Query {
  configuration(id: ID!): Configuration
  configurations(userId: String!): [Configuration!]!
  products(category: String): [Product!]!
}

type Mutation {
  createConfiguration(input: ConfigurationInput!): Configuration!
  generateDesign(configId: ID!, format: String!): Design!
  generateQuote(configId: ID!): Quote!
}
```

**Advantages for This Project:**

**1. Efficient Data Fetching:**
- Frontend requests exactly what it needs
- Single request can fetch configuration + products + quote
- Reduces network round trips (important for slower connections in India)
- Reduces bandwidth usage

**2. Frontend Flexibility:**
- Frontend teams can add fields without backend changes
- Different views (mobile, desktop) can request different data
- No over-fetching of unused data

**3. Strongly Typed Schema:**
- Schema is a contract between frontend and backend
- Auto-generated TypeScript types for frontend
- Reduces bugs from type mismatches
- Self-documenting API

**4. Rapid Frontend Development:**
- Tools like GraphQL Code Generator automate client code
- Apollo Client provides powerful caching and state management
- GraphQL Playground for testing

**5. Real-time with Subscriptions:**
- GraphQL subscriptions for real-time features
- Example: Real-time design generation progress
- Useful for collaborative features (multiple users on same configuration)

**Disadvantages:**

**1. Complexity:**
- Steeper learning curve for team
- More complex server setup (GraphQL gateway + resolvers)
- N+1 query problem (requires DataLoader pattern)
- Performance tuning can be tricky

**2. Caching Challenges:**
- HTTP caching doesn't work (POST requests)
- Requires client-side cache (Apollo Client)
- CloudFront can't cache GraphQL responses effectively
- More burden on application servers

**3. Monitoring & Debugging:**
- All requests go to /graphql endpoint (harder to monitor)
- Need specialized tools (Apollo Studio, GraphQL analytics)
- Error handling is different (errors in response, not HTTP status)

**4. Rate Limiting:**
- Harder to rate limit (can't limit by endpoint)
- Need query complexity analysis
- Malicious queries can overwhelm backend

**5. Microservices Integration:**
- Need GraphQL gateway (Apollo Federation or similar)
- Each microservice needs GraphQL support or adapter
- More moving parts in architecture

---

### Recommendation: REST API

**For this warehouse configurator solution, REST is recommended.**

**Rationale:**

**1. Team & Timeline:**
- Faster development with REST (team familiarity)
- Proven patterns in .NET ecosystem
- Less risk of unexpected complexity

**2. Architecture Alignment:**
- Microservices naturally expose REST endpoints
- Each service can be deployed independently
- Clear service boundaries

**3. Caching is Critical:**
- Product catalog changes infrequently (perfect for caching)
- CloudFront can cache GET responses
- Reduces load on backend significantly
- Important for cost optimization

**4. DR Simplicity:**
- REST endpoints failover seamlessly
- No gateway to failover (each service is independent)
- Easier to troubleshoot in DR scenario

**5. API Gateway Benefits:**
- AWS API Gateway has excellent REST support
- Built-in throttling, caching, authentication
- Usage plans for different dealer tiers (free, premium)

**Mitigation for REST Disadvantages:**

**For Over-fetching:**
- Use sparse fieldsets (query parameter: ?fields=id,name,layout)
- Provide different endpoints for list vs. detail views
- Example: GET /api/products (summary) vs. GET /api/products/{id} (full detail)

**For Multiple Round Trips:**
- Create aggregate endpoints for common use cases
- Example: GET /api/configuration/{id}/complete (includes products, pricing, BOM)
- Backend does the orchestration

**For Versioning:**
- Use header-based versioning (Accept: application/vnd.configurator.v2+json)
- Keep v1 endpoints until clients migrate
- Document deprecation timeline

**Future Consideration:**
- If application grows complex with many inter-related entities, revisit GraphQL
- Can introduce GraphQL Gateway in front of existing REST services later
- Not a one-way decision

---

### Alternative: Hybrid Approach

**For specific use cases, consider GraphQL:**
- **Dashboard/Analytics:** Complex queries aggregating multiple services
- **Mobile App (future):** Reduce data usage with precise queries
- **Third-party Integration:** GraphQL endpoint for partners

**Architecture:**
- Core services expose REST APIs
- GraphQL gateway (optional) sits in front, translates GraphQL to REST
- Clients choose between REST or GraphQL based on needs

---

## API Gateway Considerations

### Option 1: AWS API Gateway (Managed Service)

**Features:**
- Fully managed REST and WebSocket APIs
- Built-in authentication (Cognito, Lambda authorizers, IAM)
- Request/response transformation
- Rate limiting and throttling (10,000 requests/second)
- Usage plans (free tier, premium tier for dealers)
- API keys for third-party integrations
- CloudWatch integration for monitoring
- Caching (reduce backend load)

**Benefits:**
- No infrastructure to manage
- Automatic scaling
- Pay-per-use pricing ($3.50 per million requests)
- Integrated with AWS ecosystem

**Drawbacks:**
- Higher cost at scale (compared to ALB)
- 29-second timeout limit (may not suit long-running rendering jobs)
- Less flexible than ALB for some routing scenarios
- Regional service (need in both Mumbai and Singapore)

**DR Strategy:**
- Deploy API Gateway in both Mumbai and Singapore
- Route 53 directs traffic to healthy region
- API definitions managed with IaC (identical in both regions)

---

### Option 2: Application Load Balancer (Chosen Approach)

**Features:**
- Path-based routing to microservices
- Host-based routing (multi-tenant if needed)
- HTTP/2 and gRPC support
- WebSockets support
- Health checks and automatic failover
- Integration with AWS WAF
- Fixed hourly cost (not per-request)

**Benefits:**
- Lower cost at scale (~$22/month + data transfer vs. per-request)
- No timeout limits (suitable for long-running requests)
- Tight integration with ECS Fargate
- Simpler for microservices architecture

**Drawbacks:**
- No built-in rate limiting (need to implement in services or use WAF)
- No usage plans or API keys (implement in application)
- No request/response transformation (handle in services)

**DR Strategy:**
- ALB in each region
- Route 53 health checks and failover
- Identical configuration via IaC

**Recommendation: Use ALB**

**Rationale for this project:**
- Cost-effective for expected traffic (thousands of requests per day, not millions)
- No timeout issues for rendering jobs
- Simpler integration with Fargate microservices
- Rate limiting can be handled by WAF (AWS WAF has rate-based rules)
- Authentication handled by Cognito + JWT validation in services

**If API Gateway features are needed later:**
- Can add API Gateway in front of ALB
- Or migrate specific endpoints to API Gateway (hybrid approach)

---

## Disaster Recovery: Detailed Strategy

### DR Objectives

**Recovery Time Objective (RTO): 1 hour**
- Time from disaster detection to full service restoration
- Breakdown:
  - Detection: 2 minutes (automated health checks)
  - Decision: 3 minutes (confirm outage, approve DR activation)
  - Database promotion: 15 minutes
  - Service scaling: 10 minutes
  - DNS propagation: 5 minutes
  - Validation: 25 minutes

**Recovery Point Objective (RPO): 5 minutes**
- Maximum acceptable data loss
- Determined by RDS replication lag
- In practice: typically 1-3 seconds

**Service Level Agreement (SLA): 99.95% uptime**
- Translates to ~4.4 hours downtime per year
- Monthly: ~22 minutes
- DR budget: ~2 hours/year
- Remaining budget: Planned maintenance, minor incidents

---

### DR Testing & Validation

**Monthly Testing:**
- Chaos engineering: Randomly terminate ECS tasks, verify auto-recovery
- Simulate database failover within Mumbai (Multi-AZ)
- Verify RDS replication lag is within acceptable limits
- Test automated backups and point-in-time recovery

**Quarterly Testing:**
- Tabletop exercise: Walk through full DR scenario with team
- Partial DR activation: Scale up Singapore services, validate connectivity
- Performance testing in DR region (compare latency, throughput)
- Review and update runbooks based on lessons learned

**Annual Testing:**
- Full DR activation during planned maintenance window
- Route all production traffic to Singapore for 4 hours
- Validate all functionality (2D/3D generation, quotes, BOM, user management)
- Measure actual RTO/RPO achieved
- Conduct post-mortem and update procedures

---

### DR Scenario: Regional Failure

**Timeline:**

**T+0:00 - Detection:**
- Route 53 health checks fail (3 consecutive failures over 90 seconds)
- CloudWatch alarms trigger
- PagerDuty/Opsgenie alerts on-call engineer
- Automated notification to stakeholders

**T+0:02 - Initial Response:**
- On-call engineer acknowledges alert
- Opens war room (video conference, Slack channel)
- Validates outage (checks AWS Service Health Dashboard)
- Confirms Mumbai region is down (not just our services)

**T+0:05 - Decision:**
- DR Manager assesses situation
- Confirms DR activation is necessary
- Notifies business stakeholders
- Updates status page: "Investigating service disruption"

**T+0:05 - Database Failover:**
- Initiate RDS read replica promotion in Singapore
- Command: Promote-RDSReadReplica (automated script)
- Monitor promotion progress via CloudWatch
- Estimated time: 10-15 minutes

**T+0:06 - Service Scaling:**
- Trigger Lambda function to scale Singapore ECS services
- Services scale from 0 to minimum count (2-3 per service)
- ECS begins placing tasks across Availability Zones
- Tasks pull latest configuration from Parameter Store

**T+0:10 - DNS Update:**
- Route 53 automatically fails over (health check-based)
- Or manual update: Switch primary endpoint to Singapore
- TTL is 60 seconds, most clients update within 2 minutes
- Mobile apps may cache longer (monitor)

**T+0:15 - Database Promotion Complete:**
- Singapore RDS instance is now read-write
- Update Parameter Store with new database endpoint
- Force ECS service deployment to pick up new config

**T+0:20 - Service Validation:**
- Automated smoke tests run against Singapore environment
- Health check endpoints: All services reporting healthy
- Database connectivity: Confirmed
- Cache (Redis): Warming up, hit rate improving
- S3 access: Files accessible from Singapore buckets

**T+0:25 - User Validation:**
- Test user account logs in successfully
- Creates new configuration: Success
- Generates 2D design: Success
- Generates quote: Success
- All core functionality validated

**T+0:35 - Monitoring & Optimization:**
- Monitor CloudWatch metrics: CPU, memory, error rate
- Check application logs for errors
- Observe user traffic pattern (users reconnecting)
- Scale up services if needed (auto-scaling should handle)

**T+0:45 - Communication:**
- Update status page: "Services restored in DR region"
- Send email to all users: "Service disruption resolved"
- Internal communication: Status update to leadership

**T+1:00 - Post-Activation:**
- DR activation complete
- Services stable and performing well
- Begin enhanced monitoring period (24-48 hours)
- Schedule post-incident review meeting

---

### DR Scenario: Database Failure (Without Regional Outage)

**If only RDS fails (Multi-AZ failover):**

**T+0:00:**
- RDS detects primary instance failure
- Automatic Multi-AZ failover initiates

**T+0:01:**
- DNS record for RDS endpoint updated to standby
- Connection draining begins (existing connections may fail)

**T+0:02:**
- ECS services detect database connection failures
- Connection pools reconnect to new primary
- Some transactions may fail (application retry logic handles)

**T+0:03:**
- Database fully available on standby instance
- Replication from standby to Mumbai read replicas resumes

**T+0:05:**
- All services reconnected
- Health checks passing
- User impact: Brief error messages, automatic retry succeeded

**Total Downtime:** 2-3 minutes  
**User Impact:** Minimal, some users may need to retry failed action

---

### Cost Implications of DR

**DR Infrastructure Costs (Singapore):**

**Always Running:**
- RDS Read Replica: ~$250/month (30% of primary cost)
- DynamoDB Global Tables: ~$200/month (2x data storage)
- S3 Replication Storage: ~$50/month (depends on data volume)
- ALB (idle): ~$22/month
- Route 53 Health Checks: ~$1/month
- **Subtotal: ~$523/month**

**Standby (Scaled to Zero):**
- ECS Fargate: $0 (no running tasks)
- Lambda: $0 (no invocations)
- Redis: Can be stopped ($0) or kept running (~$150/month for warm DR)

**DR Activation Costs (During Incident):**
- ECS Fargate: ~$200/day (running at full capacity)
- Data transfer: ~$50/incident (cross-region replication catch-up)
- **Estimated per-incident: ~$250**

**Annual DR Cost:**
- Always-on infrastructure: $523 × 12 = ~$6,276/year
- Quarterly testing (4 hours each): ~$35 × 4 = ~$140/year
- Annual full test (4 hours): ~$35/year
- **Total: ~$6,451/year**

**DR Cost as % of Production:**
- Production (Mumbai): ~$2,000/month = $24,000/year
- DR overhead: $6,451/year
- **DR Premium: ~27% of production cost**

**Cost Optimization Strategies:**
- Use Aurora Serverless for DR (pay only when active)
- Stop Redis cluster in Singapore (start during DR)
- Use S3 Intelligent-Tiering to reduce storage costs
- Reserved Instances for RDS read replica (save 30-40%)

---

## Potential Review Comments & Objections

### Objection 1: "Why not use Aurora PostgreSQL instead of RDS PostgreSQL?"

**Counter-Argument:**

**Aurora Benefits:**
- Faster replication (real-time, < 1 second typical)
- Aurora Global Database for multi-region (better DR)
- Better performance for read-heavy workloads (up to 5x)
- Automatic storage scaling (no manual adjustment)

**Why RDS PostgreSQL is Chosen:**

**1. Cost:**
- RDS PostgreSQL: ~$250/month for db.r6g.xlarge
- Aurora PostgreSQL: ~$500/month for equivalent (2x cost)
- For this workload (moderate write, moderate read), RDS is cost-effective

**2. Simplicity:**
- RDS PostgreSQL is standard PostgreSQL (no vendor lock-in)
- Easier to migrate to/from on-premise or other clouds
- Team familiarity with standard PostgreSQL

**3. Sufficient Performance:**
- Expected load: ~100 transactions/second (well within RDS capability)
- Read replicas handle read scaling
- Multi-AZ provides sufficient availability

**When to Reconsider Aurora:**
- If transaction volume grows >500 TPS
- If need true multi-master (write scaling across regions)
- If budget increases and operational simplicity preferred
- If read replica lag becomes an issue

**Compromise:** "We can start with RDS PostgreSQL and migrate to Aurora if performance demands increase. Migration is straightforward (snapshot restore)."

---

### Objection 2: "Microservices seem over-engineered. Why not a monolith?"

**Counter-Argument:**

**Monolith Advantages:**
- Simpler deployment (single application)
- Easier local development
- No inter-service communication overhead
- Fewer moving parts

**Why Microservices for This Project:**

**1. Independent Scaling:**
- Design Engine is CPU-intensive (3D rendering) → needs more compute
- User Management is lightweight → needs less compute
- Quote generation is bursty → benefits from auto-scaling
- Monolith would require scaling everything together (wasteful)

**2. Team Structure:**
- Can assign teams to specific services (CAD team → Design Engine, Finance team → Quote Service)
- Teams can deploy independently (faster iteration)
- Reduces merge conflicts and coordination overhead

**3. Technology Flexibility:**
- Design Engine might need Python for CAD libraries
- Quote Service might integrate C# PDF libraries
- Microservices allow polyglot architecture

**4. Failure Isolation:**
- If BOM service crashes, configuration service still works
- Monolith: One bug can bring down entire application
- Improves overall reliability

**5. Future Growth:**
- Easier to add new services (e.g., Analytics Service, Mobile API)
- Can sunset old services without full application rewrite

**Compromise:** "We can start with a modular monolith (separate modules within one application), then extract services as needed. Best of both worlds."

**Alternative:** "Use a 'macroservices' approach with 3 services instead of 6: Frontend Service, Configuration+Design+BOM Service, Quote+User Service. Reduces complexity while retaining some benefits."

---

### Objection 3: "Singapore DR is expensive. Can we use a cheaper region or skip DR entirely?"

**Counter-Argument:**

**Alternative DR Options:**

**Option A: No DR (Backup & Restore Only)**
- **Cost:** Very low (~$100/month for backups)
- **RTO:** 4-8 hours (manual restore from backup)
- **RPO:** Up to 24 hours (last backup)
- **Risk:** Extended downtime, significant data loss, reputation damage

**Option B: Backup to Cheaper Region (Ohio, Oregon)**
- **Cost:** Lower (~$300/month)
- **RTO:** 2-4 hours (manual restore + infrastructure spin-up)
- **RPO:** 15 minutes (S3 replication)
- **Risk:** High latency to India (200ms+), manual process error-prone

**Option C: Mumbai Multi-AZ Only (No DR Region)**
- **Cost:** Moderate (~$200/month)
- **RTO:** 2-5 minutes (within region failover)
- **RPO:** 0 (synchronous replication)
- **Risk:** Mumbai region failure leaves application down until AWS recovers (could be days)

**Why Singapore DR is Justified:**

**1. Business Continuity:**
- Revenue loss during downtime: Assume $10,000/day
- Extended outage (3 days): $30,000 loss
- DR cost: $6,451/year
- **ROI:** Pays for itself after 1 major incident

**2. Reputation Risk:**
- Dealers depend on this tool for sales
- Extended downtime → dealers can't close deals
- Competitors may gain advantage
- Customer trust degradation

**3. Compliance:**
- Many enterprises require DR plans from vendors
- SLA commitments (99.95% uptime) require DR
- Contractual obligations may mandate < 1 hour RTO

**4. AWS Regional Outages (Rare but Real):**
- US-EAST-1 outage (December 2021): 10+ hours
- Sydney outage (June 2022): 6 hours
- Risk is low but impact is catastrophic

**Cost Optimization for DR:**
- Start with Singapore DR (proven)
- Use Reserved Instances for DR RDS (save 30%)
- Consider Aurora Serverless v2 for DR (pay only when active)
- Optimize S3 storage (Intelligent-Tiering, lifecycle policies)

**Compromise:** "Implement Mumbai Multi-AZ initially (lower cost, good availability). Add Singapore DR after 6 months once revenue justifies the investment."

---

### Objection 4: "Can we use serverless (Lambda) instead of ECS Fargate for cost savings?"

**Counter-Argument:**

**Serverless (Lambda) Advantages:**
- True pay-per-use (billed per 100ms of execution)
- Automatic scaling to zero (no idle cost)
- No infrastructure management

**Why Fargate is Better for This Workload:**

**1. Request Duration:**
- Configuration validation: ~500ms (Lambda suitable)
- Design generation: 5-10 seconds (Lambda suitable but costly)
- 3D rendering: 30-60 seconds (approaching Lambda 15-minute limit)
- Complex BOM: 10-20 seconds (Lambda workable)

**2. Cold Start Issues:**
- .NET Lambda cold start: 1-3 seconds
- User-facing APIs need < 500ms response
- Fargate: Always warm, consistent performance

**3. Cost Comparison (Example Service):**

**Lambda:**
- Assumptions: 100,000 requests/month, avg 2 seconds, 1 GB memory
- Cost: 100,000 × 2 seconds × $0.0000166667 per GB-second = ~$333/month
- Plus request cost: 100,000 × $0.0000002 = ~$0.02/month
- **Total: ~$333/month**

**Fargate:**
- Assumptions: 3 tasks, 1 vCPU, 2 GB, 24/7
- Cost: 3 × $0.04048/hour × 730 hours = ~$89/month
- **Total: ~$89/month**

**For sustained workloads, Fargate is cheaper!**

**4. Development Experience:**
- .NET applications run natively in Fargate (same as local)
- Lambda requires specific adaptation (handler methods, SDK)
- Debugging is easier in Fargate (SSH access via ECS Exec)

**Best Use Cases for Lambda in This Architecture:**
- Asynchronous jobs (rendering, PDF generation)
- Scheduled tasks (cleanup, cache warming)
- Event-driven processing (S3 upload → virus scan)

**Recommendation:** "Use Fargate for API services (always running), Lambda for background jobs (intermittent). Hybrid approach optimizes cost and performance."

---

### Objection 5: "Do we really need DynamoDB AND PostgreSQL? Seems redundant."

**Counter-Argument:**

**Why Both:**

**PostgreSQL (Relational):**
- **Use Cases:** User accounts, product catalog master data, quotes, audit logs
- **Why:** Complex queries (JOIN across tables), ACID transactions, reporting
- **Example:** Generate report of all quotes created by a dealer in a date range with product details
  - Requires JOIN across users, quotes, configurations, products
  - SQL is perfect for this

**DynamoDB (NoSQL):**
- **Use Cases:** Active sessions, cache, real-time collaboration, event logs
- **Why:** Millisecond latency, infinite scale, key-value access pattern
- **Example:** Store user's in-progress configuration (changes every few seconds)
  - Simple key-value lookup (sessionId → configData)
  - No complex queries needed
  - Need < 10ms latency for good UX

**Why Not Just PostgreSQL:**
- PostgreSQL can't match DynamoDB's latency at scale
- PostgreSQL requires capacity planning (DynamoDB scales automatically)
- PostgreSQL Multi-Region replication is complex (DynamoDB Global Tables are turnkey)

**Why Not Just DynamoDB:**
- DynamoDB is poor for complex queries (no JOINs)
- Expensive for large analytical queries
- Harder to maintain data consistency across multiple tables
- No SQL for reporting (need to use PartiQL or scan entire table)

**Real-World Pattern:**
- Store master data in PostgreSQL (source of truth)
- Cache hot data in DynamoDB (performance layer)
- Example: Product catalog master in PostgreSQL, cache frequently accessed products in DynamoDB

**Alternative (If Budget Constrained):**
- Start with PostgreSQL only
- Add DynamoDB later for specific high-velocity use cases
- Use Redis for caching instead of DynamoDB (cheaper but not for DR)

---

### Objection 6: "Konva.js for 2D rendering? Why not server-side rendering?"

**Counter-Argument:**

**Server-Side Rendering (e.g., Headless Chrome, Puppeteer):**
- Generate images on server
- Send static images to client
- Client is lightweight

**Client-Side Rendering (Konva.js):**
- Send configuration data to client
- Client renders 2D canvas using Konva.js
- Interactive (pan, zoom, select elements)

**Why Konva.js is Better:**

**1. Interactivity:**
- Users can click on racks to see details
- Drag and drop to reposition elements
- Real-time updates as user changes configuration
- Server-side rendering requires full re-render for every change

**2. Server Load:**
- Client-side rendering offloads work to user's browser
- Server only sends data (JSON), not images
- Scales to thousands of users without server load
- Server-side rendering would require massive compute

**3. User Experience:**
- Instant responsiveness (no round trip to server)
- Smooth animations and transitions
- Works offline (after initial data load)

**4. Bandwidth:**
- JSON configuration: ~50 KB
- Rendered PNG image: ~500 KB
- 10x bandwidth savings

**When Server-Side is Better:**
- PDF exports (server generates static image for quote)
- Email previews (send image in email)
- SEO (search engines need rendered content)

**Solution:** Hybrid approach
- Konva.js for interactive editing in browser
- Server-side rendering (Puppeteer) for PDF/email exports
- Best of both worlds

---

### Objection 7: "One hour RTO seems long. Can we do better?"

**Counter-Argument:**

**Achieving Faster RTO:**

**Option A: Active-Active (Both Regions Always Running):**
- **RTO:** Near-zero (users automatically redirect)
- **Cost:** ~2x (both regions at full capacity)
- **Complexity:** Data consistency challenges (write conflicts)
- **Best For:** Mission-critical systems (banking, healthcare)

**Option B: Warm Standby (Singapore Running at 30% Capacity):**
- **RTO:** 15-20 minutes (just scale up existing services)
- **Cost:** ~50% more than active-passive
- **Complexity:** Moderate
- **Best For:** High-value systems with strict SLAs

**Option C: Pilot Light (Only Database Running in Singapore):**
- **RTO:** 30-45 minutes (start services from templates)
- **Cost:** ~30% more than active-passive
- **Complexity:** Lower
- **Best For:** Most business applications

**Current Proposal (Cold Standby):**
- **RTO:** 60 minutes
- **Cost:** ~27% more than single-region
- **Best For:** Cost-sensitive projects with acceptable downtime tolerance

**Why 1 Hour is Appropriate for This Project:**

**1. Business Impact Analysis:**
- Users affected by 1-hour outage: ~100 dealers
- Revenue impact: ~$1,000 (lost productivity)
- Reputation impact: Moderate (if infrequent)

**2. Frequency of Regional Outages:**
- AWS regional outages: ~1-2 per year globally
- Mumbai-specific: ~0-1 per year
- 1-hour RTO for a rare event is acceptable

**3. Cost-Benefit:**
- Reducing RTO to 15 minutes requires ~$3,000/month additional spend
- Annual cost: $36,000
- Benefit: Save 45 minutes in a rare outage (maybe once a year)
- **Not justified for this business case**

**Compromise:** "Start with 1-hour RTO. If business grows and revenue per dealer increases, we can upgrade to warm standby (15-minute RTO) in Year 2."

**Improving RTO Without Cost Increase:**
- Automate DR activation (reduce manual steps)
- Practice DR drills monthly (reduce decision time)
- Pre-warm DNS caches (reduce propagation time)
- Target: Achieve 30-45 minute RTO within current budget

---

### Objection 8: "Why .NET and not Node.js/Python which have better cloud-native support?"

**Counter-Argument:**

**Node.js/Python Advantages:**
- Larger ecosystem of cloud libraries
- Faster development for APIs and microservices
- Better Lambda support (faster cold starts)

**Why .NET is a Strong Choice:**

**1. Performance:**
- .NET 10 is highly performant (comparable to Node.js, faster than Python)
- Compiled language (vs. interpreted)
- Low memory footprint (important for Fargate cost)
- Excellent for CPU-intensive tasks (load calculations, BOM generation)

**2. Type Safety:**
- C# is strongly typed (reduces runtime errors)
- Better IDE support (IntelliSense, refactoring)
- Easier to maintain large codebases

**3. AWS Support:**
- AWS SDK for .NET is mature and feature-complete
- ECS/Fargate supports .NET containers natively
- Lambda supports .NET (custom runtime)

**4. Team Skills:**
- If team has .NET experience, productivity is higher
- Hiring .NET developers (India market) is reasonable
- Many enterprise developers know .NET

**5. Integration:**
- Excel generation (BOM): .NET has excellent libraries (EPPlus, ClosedXML)
- PDF generation: .NET has mature options (iTextSharp, PdfSharp)
- CAD integration: Many CAD tools have .NET APIs

**When Node.js/Python is Better:**
- Event-driven architectures (Node.js excels)
- Data science/ML workloads (Python ecosystem)
- Rapid prototyping (Python/Node faster to iterate)

**Recommendation:** ".NET for backend microservices (performance, type safety). Consider Python for specific services like rendering workers (if CAD libraries require Python)."

---

## Architecture Decision Records (ADRs)

Below are formal ADRs documenting key architectural decisions.

---

### ADR-001: Multi-Region Active-Passive DR Strategy

**Status:** Accepted  
**Date:** December 2025  
**Decision Makers:** Platform Engineering Team, CTO

**Context:**

The warehouse configurator application is business-critical for dealer operations. Downtime directly impacts revenue and dealer satisfaction. We need a disaster recovery strategy that balances cost, complexity, and business requirements.

**Decision:**

We will implement an active-passive multi-region disaster recovery strategy with Mumbai as the primary region and Singapore as the disaster recovery region.

**Key Components:**
- Primary region (Mumbai) runs all production workloads at full capacity
- DR region (Singapore) maintains infrastructure in standby mode (services scaled to zero)
- RDS Cross-Region Read Replica for database replication
- DynamoDB Global Tables for real-time session data replication
- S3 Cross-Region Replication for all file storage
- Route 53 health-check based failover

**Rationale:**

**Considered Alternatives:**

**1. Single-Region Multi-AZ Only:**
- Pros: Lower cost, simpler operations
- Cons: Vulnerable to regional failures, cannot meet 99.95% SLA
- Rejected: Regional outages, though rare, have occurred (US-EAST-1, Sydney)

**2. Active-Active Multi-Region:**
- Pros: Near-zero RTO, better global performance
- Cons: 2x cost, complex data consistency, write conflict resolution
- Rejected: Cost not justified for current business scale

**3. Pilot Light DR:**
- Pros: Faster RTO (30 min) than cold standby
- Cons: Higher cost (database always running in DR)
- Considered: May upgrade to this in future if SLA tightens

**Decision Rationale:**
- **Cost-Effective:** DR overhead is ~27% of production cost vs. ~100% for active-active
- **Acceptable RTO/RPO:** 1-hour RTO and 5-minute RPO meet business requirements
- **Proven Pattern:** Active-passive is well-understood and operationally simple
- **Room to Upgrade:** Can move to warm standby or active-active if needed

**Consequences:**

**Positive:**
- Clear DR procedures and ownership
- Predictable and manageable DR costs
- Mumbai region optimized for performance, Singapore for resilience

**Negative:**
- Manual or semi-automated failover process
- Singapore infrastructure sits idle (but this is by design)
- Requires regular DR testing to maintain operational readiness

**Risks & Mitigation:**
- Risk: Manual failover may have human error
  - Mitigation: Automated runbooks, quarterly DR drills
- Risk: Database replication lag during failure
  - Mitigation: Monitor lag continuously, alert if >10 seconds

---

### ADR-002: Microservices Architecture with ECS Fargate

**Status:** Accepted  
**Date:** December 2025  
**Decision Makers:** Platform Engineering Team, CTO

**Context:**

The application has multiple distinct functional areas (configuration, design, BOM, quotes) with different scaling characteristics, development timelines, and team ownership. We need to decide on the overall application architecture pattern.

**Decision:**

We will implement a microservices architecture with six core services deployed on AWS ECS Fargate.

**Services:**
1. Configuration Service
2. Design Engine Service
3. BOM Service
4. Quote Service
5. User Management Service
6. File Processing Service

**Rationale:**

**Considered Alternatives:**

**1. Monolithic Application:**
- Pros: Simpler deployment, easier local development, no inter-service overhead
- Cons: Cannot scale components independently, entire app must be deployed for any change, single failure point
- Rejected: Scaling inefficiencies and deployment bottlenecks outweigh simplicity benefits

**2. Modular Monolith:**
- Pros: Modular code structure, can extract services later, simpler than microservices
- Cons: Still scales as one unit, risk of module boundaries eroding
- Considered: Good stepping stone, but we have clear service boundaries already

**3. Serverless (Lambda-based):**
- Pros: Pay-per-use, auto-scaling, no infrastructure
- Cons: Cold starts, 15-minute timeout limit, complex for .NET applications
- Partially Accepted: Using Lambda for async tasks, not for main APIs

**Decision Rationale:**

**1. Independent Scaling:**
- Design Engine is CPU-intensive (3D rendering) and needs different instance types
- Quote Service has bursty traffic (end of month) and benefits from auto-scaling
- User Management is low-traffic and can run on smaller instances
- Monolith would require scaling all components together (wasteful)

**2. Team Autonomy:**
- CAD engineering team owns Design Engine
- Finance team owns Quote Service
- Teams can deploy independently without coordination
- Reduces merge conflicts and enables parallel development

**3. Technology Flexibility:**
- Core services in .NET (team expertise)
- Rendering workers can use Python if CAD libraries require it
- Future ML features can use Python/TensorFlow
- Microservices enable polyglot architecture

**4. Failure Isolation:**
- If BOM service crashes, users can still create configurations
- Circuit breakers prevent cascading failures
- Improves overall system resilience

**5. ECS Fargate Benefits:**
- No EC2 instance management (patching, scaling, capacity)
- Pay only for actual container runtime
- Built-in service discovery (AWS Cloud Map)
- Blue-green deployments built-in
- Better for microservices than Lambda (no cold starts, longer running)

**Consequences:**

**Positive:**
- Each service can scale independently based on its demand
- Teams can work in parallel with minimal dependencies
- Easier to add new capabilities (new services)
- Clear ownership and accountability

**Negative:**
- Increased operational complexity (6 services to monitor vs. 1)
- Inter-service communication overhead (network latency)
- Distributed tracing needed (AWS X-Ray)
- More complex deployment pipeline

**Risks & Mitigation:**
- Risk: Inter-service dependency failures
  - Mitigation: Circuit breakers, retries, fallback strategies
- Risk: Distributed debugging is harder
  - Mitigation: Centralized logging (CloudWatch), distributed tracing (X-Ray)
- Risk: Data consistency across services
  - Mitigation: Event-driven architecture, eventual consistency patterns

---

### ADR-003: PostgreSQL (RDS) + DynamoDB Polyglot Persistence

**Status:** Accepted  
**Date:** December 2025  
**Decision Makers:** Platform Engineering Team, Database Architect

**Context:**

The application has diverse data storage requirements: transactional user/product data, real-time session state, high-velocity event logs, and file storage. We need to select appropriate database technologies.

**Decision:**

We will use a polyglot persistence strategy combining:
- **PostgreSQL (Amazon RDS):** Relational data (users, products, quotes, audit logs)
- **DynamoDB:** Session state, caching, event logs
- **Redis (ElastiCache):** In-memory caching
- **S3:** File storage (designs, documents)

**Rationale:**

**Considered Alternatives:**

**1. PostgreSQL Only:**
- Pros: Simplicity, single database to manage, strong consistency
- Cons: Cannot achieve < 10ms latency for session lookups, expensive to scale for caching workloads
- Rejected: Performance requirements for real-time features not met

**2. DynamoDB Only:**
- Pros: Infinite scale, low latency, fully managed
- Cons: No JOINs (complex queries require multiple requests), expensive for analytical queries, no SQL for reporting
- Rejected: Relational product catalog and quoting workflows require SQL

**3. MongoDB (DocumentDB):**
- Pros: Flexible schema, good for semi-structured data, SQL-like queries
- Cons: No true ACID transactions, more expensive than DynamoDB for key-value workloads
- Rejected: Doesn't excel at either relational or key-value use cases

**Decision Rationale:**

**PostgreSQL for:**
- User accounts (ACID transactions for consistency)
- Product catalog (complex queries, reporting)
- Quote records (financial audit requirements)
- Audit logs (compliance, SQL-based analysis)

**DynamoDB for:**
- Active configuration sessions (millisecond latency, frequent updates)
- Business rules cache (key-value lookups)
- Event sourcing (high write throughput)
- Global Tables for DR (automatic multi-region replication)

**Redis for:**
- Hot product catalog cache (reduce database load)
- Session state backup (complement to DynamoDB)
- Rate limiting counters
- Real-time leaderboards

**S3 for:**
- Design files (2D/3D), documents (BOM, quotes)
- Infinite storage, low cost, built-in versioning

**Consequences:**

**Positive:**
- Each data store optimized for its workload
- Better performance (right tool for right job)
- Cost optimization (cheaper than using PostgreSQL for everything)
- DR benefits (DynamoDB Global Tables, S3 CRR)

**Negative:**
- Multiple databases to manage and monitor
- Data consistency challenges (keeping caches in sync)
- Increased operational complexity
- Developers need to learn multiple database paradigms

**Risks & Mitigation:**
- Risk: Cache invalidation bugs (stale data in Redis/DynamoDB)
  - Mitigation: TTL on all cached items, cache warming on updates
- Risk: Data duplication (product catalog in PostgreSQL AND DynamoDB)
  - Mitigation: PostgreSQL is source of truth, caches are ephemeral
- Risk: Increased costs (multiple databases)
  - Mitigation: Monitor usage, optimize based on actual patterns

---

### ADR-004: REST API over GraphQL

**Status:** Accepted  
**Date:** December 2025  
**Decision Makers:** Platform Engineering Team, Frontend Lead

**Context:**

We need to define the API contract between frontend (React) and backend (.NET microservices). The primary options are RESTful APIs or GraphQL.

**Decision:**

We will implement RESTful APIs for all microservices.

**API Design Principles:**
- Resource-based URLs (/api/configuration, /api/design)
- Standard HTTP methods (GET, POST, PUT, DELETE)
- JSON request/response
- Versioning via headers (Accept: application/vnd.configurator.v1+json)
- HATEOAS links for discoverability (optional)

**Rationale:**

**Considered Alternative: GraphQL**
- Pros: Precise data fetching, reduces over/under-fetching, strongly typed schema, great for complex data graphs
- Cons: Complexity, caching challenges, performance tuning, monitoring difficulties

**Decision Rationale:**

**1. Team Familiarity:**
- .NET team has extensive REST API experience
- Faster development with proven patterns
- Abundant libraries and tooling (Swagger, Postman)

**2. Caching:**
- HTTP caching works out-of-the-box
- CloudFront can cache GET responses (critical for product catalog)
- Reduces backend load by 60-70%
- GraphQL requires client-side caching (more complex)

**3. Microservices Alignment:**
- Each service exposes clear REST endpoints
- Service boundaries are well-defined
- Independent deployment and versioning
- GraphQL would require gateway (additional component)

**4. Monitoring & Debugging:**
- Standard HTTP status codes (200, 404, 500)
- ALB access logs provide clear visibility
- Easier to troubleshoot in production
- GraphQL: All requests to /graphql endpoint (harder to monitor)

**5. API Gateway Integration:**
- AWS ALB has excellent REST support
- Built-in health checks, routing, SSL termination
- WAF integration for security

**Mitigations for REST Disadvantages:**

**Over-fetching:**
- Use sparse fieldsets: GET /api/products?fields=id,name,price
- Provide summary and detail endpoints
- Client can request only needed fields

**Multiple Round Trips:**
- Create aggregate endpoints for common use cases
- Example: GET /api/configuration/{id}/complete (includes products, BOM, quote)
- Backend does orchestration

**Versioning:**
- Header-based versioning (not URL-based /v1/, /v2/)
- Backwards compatible changes (additive only)
- Deprecation warnings in response headers

**Consequences:**

**Positive:**
- Faster development (team expertise)
- Better caching (CloudFront, browser)
- Simpler monitoring and debugging
- Lower operational complexity

**Negative:**
- Frontend may need multiple API calls for complex views
- Potential for over-fetching data
- Client-side orchestration logic

**Future Consideration:**
- Can introduce GraphQL gateway later if needed
- GraphQL can sit in front of existing REST APIs
- Not a one-way door decision

---

### ADR-005: Application Load Balancer over API Gateway

**Status:** Accepted  
**Date:** December 2025  
**Decision Makers:** Platform Engineering Team, Cloud Architect

**Context:**

We need an entry point for our microservices that handles routing, SSL termination, and potentially authentication/authorization. AWS offers Application Load Balancer (ALB) and API Gateway.

**Decision:**

We will use Application Load Balancer (ALB) as the primary API entry point.

**Configuration:**
- Public ALB in each region (Mumbai, Singapore)
- Path-based routing to microservices (/api/configuration → Configuration Service)
- SSL/TLS termination with ACM certificates
- Integration with AWS WAF for security
- CloudWatch integration for monitoring

**Rationale:**

**Considered Alternative: AWS API Gateway**
- Pros: Managed service, built-in throttling, usage plans, caching, no infrastructure
- Cons: Per-request pricing ($3.50/million requests), 29-second timeout, regional service

**Decision Rationale:**

**1. Cost:**
- Expected traffic: ~500,000 API requests/day = ~15M/month
- API Gateway: 15M × $3.50 = ~$52.50/month + data transfer
- ALB: $22/month (fixed) + data transfer (~$30) = ~$52/month
- At this volume, costs are similar
- Future growth: ALB becomes cheaper at scale (no per-request cost)

**2. No Timeout Limits:**
- Some operations take > 30 seconds (complex 3D rendering)
- API Gateway has hard 29-second limit
- ALB has no timeout limit (can configure as needed)

**3. Microservices Integration:**
- ALB integrates natively with ECS Fargate (target groups)
- Health checks and auto-scaling built-in
- Service discovery via AWS Cloud Map

**4. Operational Simplicity:**
- ALB is infrastructure (managed via IaC like Terraform)
- API Gateway is a service (separate management)
- ALB fits better with our infrastructure-as-code approach

**5. DR Simplicity:**
- ALB in each region, Route 53 handles failover
- Identical configuration in both regions
- No API Gateway-specific replication concerns

**Mitigations for API Gateway Advantages:**

**Throttling:**
- Implement rate limiting in services (use Redis)
- Use AWS WAF rate-based rules (100 requests per 5 min per IP)

**Usage Plans:**
- Implement in application (dealer tier logic)
- Track usage in DynamoDB

**Caching:**
- Use CloudFront in front of ALB (caches GET responses)
- Application-level caching (Redis)

**Authentication:**
- JWT validation in services (Cognito integration)
- Or ALB native Cognito integration (built-in)

**Consequences:**

**Positive:**
- No timeout constraints for long-running operations
- Cost-effective at scale
- Simpler DR strategy
- Better Fargate integration

**Negative:**
- No built-in API key management (implement in app)
- No automatic request/response transformation (handle in services)
- Need to implement rate limiting separately

**Future Consideration:**
- Can add API Gateway later for specific use cases (public API for partners)
- Hybrid approach: ALB for internal, API Gateway for external

---

### ADR-006: Singapore as DR Region over Other Options

**Status:** Accepted  
**Date:** December 2025  
**Decision Makers:** Platform Engineering Team, Infrastructure Lead

**Context:**

We need to select a disaster recovery region for the Mumbai primary region. Candidates include Singapore, Bahrain, Sydney, and Tokyo.

**Decision:**

We will use ap-southeast-1 (Singapore) as the disaster recovery region for ap-south-1 (Mumbai).

**Rationale:**

**Evaluated Regions:**

**1. Bahrain (me-south-1):**
- Pros: Geographically closer to India (~2,800 km), lower latency (20-30ms)
- Cons: Limited AWS service availability, newer region (higher risk), data residency concerns for some customers
- Rejected: Service availability concerns (some services not available)

**2. Sydney (ap-southeast-2):**
- Pros: Mature AWS region, full service availability, strong infrastructure
- Cons: High latency from India (150-200ms), significant distance (10,000+ km), higher data transfer costs
- Rejected: Latency too high for acceptable DR performance

**3. Tokyo (ap-northeast-1):**
- Pros: Mature region, full service availability
- Cons: High latency (100-120ms), time zone challenges (GMT+9 vs GMT+5:30), higher costs
- Rejected: Latency and operational timezone mismatch

**4. Singapore (ap-southeast-1) - SELECTED:**
- Pros: Low latency (30-50ms), geographic diversity (4,100 km), mature region, full service availability, same time zone proximity
- Cons: Slightly higher cost than Bahrain
- Selected: Best balance of all factors

**Decision Criteria:**

**1. Geographic Separation:**
- Minimum 500 km for effective DR (earthquake/disaster independence)
- Singapore: 4,100 km (exceeds minimum)
- Independent failure domain (different power grids, network paths)

**2. Network Latency:**
- Target: < 100ms for acceptable DR performance
- Singapore: 30-50ms (excellent)
- Critical for database replication and failover scenarios

**3. Service Availability:**
- Need: ECS Fargate, RDS PostgreSQL, DynamoDB, ElastiCache, S3
- Singapore: All services available
- Parity with Mumbai ensures no compromises

**4. Operational Considerations:**
- Time zone: GMT+8 (only 2.5 hours from Indian team in GMT+5:30)
- On-call engineers can support both regions easily
- Singapore vs. Sydney (GMT+10/+11): 4.5-5.5 hour difference (harder)

**5. Data Transfer Costs:**
- Mumbai → Singapore: $0.086/GB
- Mumbai → Sydney: $0.14/GB
- Singapore is 38% cheaper for replication

**6. Compliance:**
- Some Indian customers may have data residency preferences
- Singapore is acceptable for most (APAC region)
- Bahrain (Middle East) may raise concerns for some customers

**Consequences:**

**Positive:**
- Optimal latency for DR scenarios
- Full service parity with Mumbai
- Operational timezone alignment
- Cost-effective data transfer

**Negative:**
- Not the cheapest option (Bahrain is cheaper)
- Not the closest geographically (Bahrain is closer)

**Future Consideration:**
- If Bahrain region matures and service availability improves, can re-evaluate
- Could use Bahrain as tertiary backup (triple redundancy)

---

## Summary & Next Steps

**This architecture provides:**

✅ **High Availability:** Multi-AZ within region, auto-scaling, health checks  
✅ **Disaster Recovery:** Cross-region replication, RTO < 1 hour, RPO < 5 minutes  
✅ **Scalability:** Microservices scale independently, serverless components  
✅ **Security:** Defense in depth, encryption everywhere, WAF, compliance-ready  
✅ **Cost Efficiency:** Active-passive DR (~27% overhead), auto-scaling, on-demand billing  
✅ **Maintainability:** IaC, automated deployments, centralized monitoring

**Recommended Next Steps:**

**Phase 1: Foundation (Weeks 1-4)**
1. Set up AWS organizations and accounts (Production, DR, Dev/Test)
2. Deploy VPC infrastructure in Mumbai and Singapore (Terraform/CDK)
3. Configure IAM roles and policies
4. Set up CI/CD pipeline (GitHub Actions / AWS CodePipeline)

**Phase 2: Data Layer (Weeks 5-8)**
5. Deploy RDS PostgreSQL with Multi-AZ in Mumbai
6. Set up RDS Read Replica in Singapore
7. Deploy DynamoDB tables with Global Tables enabled
8. Configure S3 buckets with Cross-Region Replication
9. Deploy ElastiCache Redis with Multi-AZ

**Phase 3: Application Layer (Weeks 9-14)**
10. Build and deploy microservices to ECS Fargate (Mumbai)
11. Configure ALB with path-based routing
12. Integrate with Cognito for authentication
13. Set up service discovery (AWS Cloud Map)
14. Deploy identical infrastructure in Singapore (scaled to 0)

**Phase 4: Frontend & Integration (Weeks 15-18)**
15. Deploy React application to S3 + CloudFront
16. Integrate frontend with backend APIs
17. Implement 2D rendering (Konva.js) and 3D (Three.js)
18. Set up Lambda functions for async processing

**Phase 5: Monitoring & DR (Weeks 19-22)**
19. Configure CloudWatch dashboards, alarms, and logs
20. Set up AWS X-Ray for distributed tracing
21. Implement DR runbooks and automation scripts
22. Conduct first DR drill
23. Enable GuardDuty, Config, CloudTrail for security

**Phase 6: Launch Preparation (Weeks 23-24)**
24. Load testing and performance optimization
25. Security audit and penetration testing
26. User acceptance testing with dealers
27. Go-live planning and rollout

---

**Document Control:**
- Review this architecture with stakeholders
- Address any concerns or objections raised
- Gain approval for budget and timeline
- Proceed with detailed implementation planning
