using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace pharmacyBackend.Controllers
{
    [ApiController]
    public class BaseController : Controller
    {
        protected int GetUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            return int.Parse(userIdString);
        }
    }
}
