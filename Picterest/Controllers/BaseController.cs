using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Picterest.Controllers
{
    public class BaseController : ControllerBase
    {
        public BaseController() { }

        public string UserId
        {
            get { return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty; }
            
        }
    }
}
