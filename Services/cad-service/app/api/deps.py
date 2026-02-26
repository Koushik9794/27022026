from datetime import datetime, timezone
from typing import Optional

from fastapi import Header, HTTPException, status

class UserContext:
    def __init__(self, name: str, role: str, login_time: datetime):
        self.name = name
        self.role = role
        self.login_time = login_time

async def get_user_context(
    x_user_name: Optional[str] = Header(default=None, alias="X-User-Name"),
    x_user_role: Optional[str] = Header(default=None, alias="X-User-Role"),
    x_login_time: Optional[str] = Header(default=None, alias="X-Login-Time"),
) -> UserContext:
    # Decide your policy: strict (422) vs. lenient defaults.
    if not x_user_name or not x_user_role:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail="Missing required user context headers: X-User-Name, X-User-Role"
        )

    if x_login_time:
        try:
            # Let Python parse; if no tz, assume UTC.
            dt = datetime.fromisoformat(x_login_time.replace("Z", "+00:00"))
        except Exception as e:
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail=f"Invalid X-Login-Time: {e}"
            )
    else:
        # If not provided, fallback to server time
        dt = datetime.now(timezone.utc)

    return UserContext(
        name=x_user_name.strip(),
        role=x_user_role.strip(),
        login_time=dt,
    )
