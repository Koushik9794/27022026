"""
Application configuration settings
"""
import os
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    """Application settings"""
    
    # App settings
    APP_NAME: str = "DXF Metadata Extractor API"
    APP_VERSION: str = "1.0.0"
    DEBUG: bool = True
    
    # CORS settings
    CORS_ORIGINS: list[str] = ["http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:5174", "http://127.0.0.1:5174"]
    
    # File upload settings
    MAX_FILE_SIZE: int = 100 * 1024 * 1024  # 100 MB max
    ALLOWED_EXTENSIONS: list[str] = [".dxf"]
    
    class Config:
        env_file = ".env"
        case_sensitive = True


settings = Settings()
