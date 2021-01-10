using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WebAPIApp.Auth;

namespace WebAPIApp.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AuthController : Controller
    {
        private AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("/token")]
        public IActionResult GetToken(string login, string password)
        {
            ClaimsIdentity identity = _authService.GetIdentity(login, password);

            if(identity == null)
            {
                return BadRequest("Wrong Login and/or Password");
            }

            string token = _authService.GetToken(identity);

            return Json(new {
                login = identity.Name,
                token
            });
        }

        [Authorize, HttpGet("/whoami")]
        public IActionResult GetWhoAmI()
        {
            return Json(new {
                login = User.Identity.Name,
                role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        [Authorize(Roles = "admin"), HttpGet("/admin")]
        public IActionResult DoAdminStuff()
        {
            return Ok("Access granted");
        }
    }
}
