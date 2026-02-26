"""
FastAPI Main Application Entry Point
"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.config import settings
from app.api.routes.dxf import router as dxf_router
from app.api.routes.login_info import router as login_router  # <-- NEW


def create_app() -> FastAPI:
    """Create and configure the FastAPI application"""

    openapi_tags = [
        {
            "name": "DXF Metadata",
            "description": "Endpoints for creating and managing dxf-metadata info records.",
        },
        {
            "name": "DXF",
            "description": "Endpoints for DXF processing and metadata extraction.",
        },
    ]

    app = FastAPI(
        title=settings.APP_NAME,
        version=settings.APP_VERSION,
        description="API for extracting metadata and entities from DXF files",
        docs_url="/docs",
        redoc_url="/redoc",
        openapi_tags=openapi_tags,  # <-- order in Swagger UI
    )

    # Configure CORS
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.CORS_ORIGINS,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    # Include routers (Login Info first so it shows above DXF)
    app.include_router(login_router)  # <-- NEW
    app.include_router(dxf_router)

    # Root endpoint
    @app.get("/")
    async def root():
        return {
            "message": "DXF Metadata Extractor API",
            "version": settings.APP_VERSION,
            "docs": "/docs",
        }

    # Health check
    @app.get("/api/health")
    async def health():
        return {"status": "healthy"}

    return app


app = create_app()


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)

# """
# FastAPI Main Application Entry Point
# """
# from fastapi import FastAPI
# from fastapi.middleware.cors import CORSMiddleware

# from app.config import settings
# from app.api.routes.dxf import router as dxf_router


# def create_app() -> FastAPI:
#     """Create and configure the FastAPI application"""
    
#     app = FastAPI(
#         title=settings.APP_NAME,
#         version=settings.APP_VERSION,
#         description="API for extracting metadata and entities from DXF files",
#         docs_url="/docs",
#         redoc_url="/redoc",
#     )
    
#     # Configure CORS
#     app.add_middleware(
#         CORSMiddleware,
#         allow_origins=settings.CORS_ORIGINS,
#         allow_credentials=True,
#         allow_methods=["*"],
#         allow_headers=["*"],
#     )
    
#     # Include routers
#     app.include_router(dxf_router)
    
#     # Root endpoint
#     @app.get("/")
#     async def root():
#         return {
#             "message": "DXF Metadata Extractor API",
#             "version": settings.APP_VERSION,
#             "docs": "/docs",
#         }
    
#     # Health check
#     @app.get("/api/health")
#     async def health():
#         return {"status": "healthy"}
    
#     return app


# app = create_app()


# if __name__ == "__main__":
#     import uvicorn
#     uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)
