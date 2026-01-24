using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tourism.DTO;
using Tourism.Models;

namespace Tourism.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
    {
        var user = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            UserName = registerDto.UserName
        };
        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { Message = "Registration successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, [FromQuery] bool useCookies = true,
        [FromQuery] bool useSessionCookies = false)
    {
        if (!useCookies)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok("Credentials are valid.");
        }

        var isPersistent = !useSessionCookies;

        if (!string.IsNullOrEmpty(loginDto.TwoFactorCode))
        {
            var result = await signInManager.TwoFactorSignInAsync("Email", loginDto.TwoFactorCode, isPersistent,
                rememberClient: isPersistent);
            if (result.Succeeded) return Ok(new { Message = "Login successful." });
            return Unauthorized("Invalid two-factor code.");
        }

        if (!string.IsNullOrEmpty(loginDto.TwoFactorRecoveryCode))
        {
            var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(loginDto.TwoFactorRecoveryCode);
            if (result.Succeeded) return Ok(new { Message = "Login successful." });
            return Unauthorized("Invalid recovery code.");
        }
        
        var userToLogin = await userManager.FindByEmailAsync(loginDto.Email);
        if (userToLogin == null) return Unauthorized("Invalid credentials.");

        var signInResult =
            await signInManager.PasswordSignInAsync(userToLogin, loginDto.Password, isPersistent,
                lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            return Ok(new { Message = "Login successful." });
        }

        if (signInResult.RequiresTwoFactor)
        {
            return Ok(new { RequiresTwoFactor = true });
        }

        if (signInResult.IsLockedOut)
        {
            return StatusCode(423, "This account has been locked out.");
        }
        
        return Unauthorized("Invalid credentials or login attempt.");
    }
}