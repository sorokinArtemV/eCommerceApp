using eCommerce.Core.DTO;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUsersService _usersService;

    public AuthController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost("register")] // api/auth/register
    public async Task<IActionResult> Register(RegisterRequest? registerRequest)
    {
        if (registerRequest is null)
        {
            return BadRequest("Invalid registration data");
        }

        AuthenticationResponse? authResponse = await _usersService.Register(registerRequest);

        if (authResponse is null || !authResponse.Success)
        {
            return BadRequest(authResponse);
        }

        return Ok(authResponse);
    }

    [HttpPost("login")] // api/auth/login
    public async Task<IActionResult> Login(LoginRequest? loginRequest)
    {
        if (loginRequest is null)
        {
            return BadRequest("Invalid login data");
        }

        AuthenticationResponse? authResponse = await _usersService.Login(loginRequest);

        if (authResponse is null || !authResponse.Success)
        {
            return Unauthorized(authResponse);
        }

        return Ok(authResponse);
    }
}