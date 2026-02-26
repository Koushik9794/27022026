using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GssCommon.Common;

public static class CustomErrorResults
{
    public static IActionResult FromError(Error error, ControllerBase controller)
    {
        // Adjust property names if your Error uses Description instead of Message.
        var payload = new { code = error.Code, message = error.Message };

        return error.Type switch
        {
            ErrorType.Validation => controller.BadRequest(payload),
            ErrorType.Conflict => controller.Conflict(payload),
            ErrorType.NotFound => controller.NotFound(payload),
            ErrorType.Unauthorized => controller.Unauthorized(),
            ErrorType.Forbidden => controller.Forbid(),
            ErrorType.Failure => controller.UnprocessableEntity(error.Message),

            _ => controller.BadRequest(payload)
        };
    }
}
