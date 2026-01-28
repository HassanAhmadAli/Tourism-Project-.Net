using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tourism.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserManagementController(UserManager<User> userManager) : ControllerBase
{
    [HttpPost("block/{userId}")]
    public async Task<ActionResult> BlockUser(string userId)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }
        if (user.Id == UserId)
        {
            return BadRequest(new { Message = "YOU MUST NOT BLOCK YOURSELF" });
        }
        await userManager.SetLockoutEnabledAsync(user, true);
        var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (result.Succeeded)
        {
            return Ok(new { Message = $"User {user.UserName} has been permanently blocked." });
        }
        return BadRequest(new { Message = "Could not block user." });
    }

    [HttpPost("unblock/{userId}")]
    public async Task<ActionResult> UnblockUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }
        var result = await userManager.SetLockoutEndDateAsync(user, null);
        if (result.Succeeded)
        {
            return Ok(new { Message = $"User {user.UserName} has been unblocked." });
        }
        return BadRequest(new { Message = "Could not unblock user." });
    }

    [HttpGet]
    public async Task<ActionResult> getUsers()
    {
        var users = await userManager.GetUsersInRoleAsync("User");
        return Ok(users);
    }
}
