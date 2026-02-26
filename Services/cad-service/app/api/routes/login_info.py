from typing import Dict
from uuid import UUID, uuid4
from datetime import datetime, timezone

from fastapi import APIRouter, Depends, status

from app.api.deps import get_user_context, UserContext
from app.models.login_models import (
    LoginInfoRequest,
    LoginInfoResponse,
    _file_name_from_url,
    _z,
)

router = APIRouter(prefix="/api/dxf-metadata", tags=["DXF Metadata"])

# In-memory store for demo
_STORE: Dict[UUID, LoginInfoResponse] = {}


@router.post(
    "",
    response_model=LoginInfoResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Create metadata Info",
    response_description="The created dxf-metadata Info record",
)
def create_login_info(
    payload: LoginInfoRequest,
    ctx: UserContext = Depends(get_user_context),
) -> LoginInfoResponse:
    now = datetime.now(timezone.utc)

    # Derive values not supplied by client
    file_name = _file_name_from_url(str(payload.s3_url_path_dxf))

    # Create IDs
    entity_id = uuid4()
    revision_id = uuid4()

    # Fill server defaults for optional numeric fields
    total_layers = payload.total_layers if payload.total_layers is not None else 0
    total_entities = payload.total_entities if payload.total_entities is not None else 0

    # Build response
    resp = LoginInfoResponse(
        id=entity_id,
        name=ctx.name,
        role=ctx.role,
        enquiry_id=payload.enquiry_id,
        login_time=_z(ctx.login_time),
        file_name=file_name,
        s3_url_path_dxf=payload.s3_url_path_dxf,
        s3_url_path_json=payload.s3_url_path_json,
        revision_id=revision_id,
        status="active",
        created_at=_z(now),
        updated_at=_z(now),
        warehouse_type=payload.warehouse_type or "Gable",
        total_layers=total_layers,
        total_entities=total_entities,
        file_size=payload.file_size,             # may be None
        dxf_version=payload.dxf_version or "AutoCAD 2018",
        upload_status=payload.upload_status or "uploaded",
    )

    _STORE[entity_id] = resp
    return resp
