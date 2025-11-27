using eCommerce.Core.DTO;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet("{userId:guid}")] // GET api/users/{userId}
    public async Task<IActionResult> GetUserByUserId(Guid? userId)
    {
        if (userId != Guid.Empty)
        {
            return BadRequest("Invalid user id");
        }

        UserDto? userDto = await _usersService.GetUserById(userId);

        if (userDto is null)
        {
            return NotFound(userDto);
        }
        
        return Ok(userDto);
    }
}