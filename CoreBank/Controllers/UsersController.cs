using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Services;
using System.Security.Claims;

namespace MinCoreBank.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

      
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
        {
            var currentUserId = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var result = await _userService.CreateUser(userDto, currentUserId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("DeleteUser/{userId}")]
       // Require authentication
        public async Task<IActionResult> DeleteUser(string userId)
        {
            // Get user ID from claims (more reliable than User.Identity.Name)
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid or expired token"
                });
            }

            var result = await _userService.DeleteUser(userId, currentUserId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpPut("UpdateUser/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UserUpdateDto userDto)
        {
            var currentUserId = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var result = await _userService.UpdateUser(userId, userDto, currentUserId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("UpdateUserstatus/{userId}")]
        public async Task<IActionResult> UpdateUserstatus(string userId, [FromBody] UserUpdateStatusDto userDto)
        {
            var currentUserId = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var result = await _userService.UpdateUserstatus(userId, userDto, currentUserId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("GetAllUsers")]
   
        public async Task<IActionResult> GetAllUsers()
        {
            // Debug: Verify authentication
            Console.WriteLine($"Authenticated User: {User.Identity?.Name}");
            Console.WriteLine($"User Roles: {string.Join(",", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

            var result = await _userService.GetAllUsers();

            if (!result.Success)
            {
                Console.WriteLine($"Service error: {result.Message}");
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var result = await _userService.GetUserById(userId);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Debug: Print all claims
            Console.WriteLine("Current User Claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Extracted UserId: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ApiResponse<Users>
                {
                    Success = false,
                    Message = "User ID not found in token"
                });
            }

            var result = await _userService.GetUserById(userId);
            return Ok(result);
        }
    }
}