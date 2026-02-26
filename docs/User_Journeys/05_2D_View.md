# User Journey: 2D View (Render & Export)

Actor: Dealer / Design Consultant / Viewer

Preconditions
- A saved `Configuration` exists with layout and product placements

Main Flow
1. User opens `2D View` from the Configuration Editor.
2. Frontend requests a 2D rendering from the Design Engine Service (`POST /design/2d/render` or cached URL).
3. Design Engine converts the configuration into a 2D canvas representation (Konva or SVG generation) and returns either a tile/JSON representation or a pre-rendered SVG/PNG.
4. Frontend composes the tiles or renders the SVG in the canvas for panning/zoom.
5. User can toggle layers (clearance, dimensions, product labels) and export the view (`Export as PDF/SVG`).
6. Export triggers the backend to assemble high-resolution assets and stores them in S3; link returned to user.

Alternate Flows
- If generation is heavy, show progress and allow asynchronous retrieval when ready.
- For very large layouts, provide simplified/overview rendering and on-demand detailed tile rendering.

API Notes
- 2D Render: `POST /design/2d/render` (synchronous for small configs) or `POST /design/2d/render/async`
- Caching: CDN/S3 for previously rendered assets
- Exports: `POST /exports/2d` → S3

Performance
- Aim for <3s for initial 2D render for typical layouts
- Use vector formats (SVG/PDF) for lossless printing

Postconditions
- 2D visualization available in UI and exportable as PDF/SVG
