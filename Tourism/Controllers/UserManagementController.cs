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
        var user = await userManager.FindByIdAsync(userId);
        var me = await userManager.GetUserAsync(User);

        if (user == null || me == null)
        {
            return NotFound("User not found.");
        }
        if (user.Id == me.Id)
        {
            return BadRequest("YOU MUST NOT BLOCK YOURSELF");
        }
        await userManager.SetLockoutEnabledAsync(user, true);
        var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (result.Succeeded)
        {
            return Ok($"User {user.UserName} has been permanently blocked.");
        }
        return BadRequest("Could not block user.");
    }

    [HttpPost("unblock/{userId}")]
    public async Task<ActionResult> UnblockUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }
        var result = await userManager.SetLockoutEndDateAsync(user, null);
        if (result.Succeeded)
        {
            return Ok($"User {user.UserName} has been unblocked.");
        }
        return BadRequest("Could not unblock user.");
    }
    [HttpGet]
    public async Task<ActionResult> getUsers()
    {
        var users = await userManager.GetUsersInRoleAsync("User");
        return Ok(users);
    }
}
