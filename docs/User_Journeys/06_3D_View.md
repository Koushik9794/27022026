# User Journey: 3D View (Render & Interactive)

Actor: Dealer / Design Consultant / Viewer

Preconditions
- Configuration contains 3D-capable product models or simplified geometry
- Client supports WebGL

Main Flow
1. User opens `3D View` from the configuration.
2. Frontend requests 3D assets from the Design Engine Service (`GET /design/3d/{configurationId}`) which may return a glTF/GLB, streaming tiles, or links to model assets in S3.
3. Frontend loads models into Three.js (or a WebGL engine) and composes the scene (camera, lights, controls).
4. User can orbit, pan, zoom, toggle layers, and hide/show product groups.
5. For large scenes, the frontend progressively loads LODs (levels of detail) and uses instancing for repeated products.
6. User can capture a high-resolution screenshot or export the scene as a shareable link.

Alternate Flows
- If client lacks WebGL: fallback to pre-rendered 2D images or a video walkthrough.
- If model assets are missing: show placeholder geometry and an explanatory message.

API Notes
- 3D Asset: `GET /design/3d/{configurationId}` (may return manifest for scene tiles)
- Streaming: use signed URLs to S3 assets
- Offload heavy pre-processing to background workers (Lambda/Fargate)

Performance Tips
- Use instancing for identical products
- Provide LODs and progressive streaming
- Keep initial scene lightweight for interactive responsiveness

Postconditions
- Interactive 3D visualization available in-browser; exports/links available in S3
