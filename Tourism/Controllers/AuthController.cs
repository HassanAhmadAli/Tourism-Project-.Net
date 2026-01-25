using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Tourism.DTO;
using Tourism.Models;

namespace Tourism.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IEmailSender<User> emailSender,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    TimeProvider timeProvider)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto registration)
    {
        if (!userManager.SupportsUserEmail)
        {
            return StatusCode(500, CreateValidationProblem("Configuration", "User store requires email support."));
        }

        var user = new User
        {
            UserName = registration.Email,
            Email = registration.Email,
            FullName = registration.FullName,
        };

        var result = await userManager.CreateAsync(user, registration.Password);

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        await SendConfirmationEmailAsync(user, registration.Email);
        return Ok(new { Message = "Registration successful. Please check your email to confirm." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login, [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);

        signInManager.AuthenticationScheme =
            useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
        var result =
            await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);
        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(login.TwoFactorCode))
            {
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent,
                    rememberClient: isPersistent);
            }
            else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
            {
                result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
            }
        }

        if (!result.Succeeded)
        {
            return Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        return new EmptyResult();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
    {
        var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);


        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            timeProvider.GetUtcNow() >= expiresUtc ||
            await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not User user)
        {
            return Challenge();
        }

        var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
        return SignIn(newPrincipal, IdentityConstants.BearerScheme);
    }

    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code,
        [FromQuery] string? changedEmail)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Unauthorized();
        }

        IdentityResult result;

        if (string.IsNullOrEmpty(changedEmail))
        {
            result = await userManager.ConfirmEmailAsync(user, code);
        }
        else
        {
            result = await userManager.ChangeEmailAsync(user, changedEmail, code);
            if (result.Succeeded)
            {
                result = await userManager.SetUserNameAsync(user, changedEmail);
            }
        }

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        return Ok("Thank you for confirming your email.");
    }

    [HttpPost("resendConfirmationEmail")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest resendRequest)
    {
        var user = await userManager.FindByEmailAsync(resendRequest.Email);
        if (user is null)
        {
            return Ok();
        }

        await SendConfirmationEmailAsync(user, resendRequest.Email);
        return Ok();
    }

    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest resetRequest)
    {
        var user = await userManager.FindByEmailAsync(resetRequest.Email);

        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));


            await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
        }

        return Ok();
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest)
    {
        var user = await userManager.FindByEmailAsync(resetRequest.Email);

        if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
        }

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
            result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
        }

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return Ok();
    }

    [Authorize]
    [HttpPost("manage/2fa")]
    public async Task<IActionResult> TwoFactor([FromBody] TwoFactorRequest tfaRequest)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return NotFound();

        if (tfaRequest.Enable == true)
        {
            if (tfaRequest.ResetSharedKey)
            {
                return CreateValidationProblem("CannotResetSharedKeyAndEnable",
                    "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
            }

            if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
            {
                return CreateValidationProblem("RequiresTwoFactor",
                    "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
            }

            if (!await userManager.VerifyTwoFactorTokenAsync(user,
                    userManager.Options.Tokens.AuthenticatorTokenProvider, tfaRequest.TwoFactorCode))
            {
                return CreateValidationProblem("InvalidTwoFactorCode",
                    "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
            }

            await userManager.SetTwoFactorEnabledAsync(user, true);
        }
        else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
        {
            await userManager.SetTwoFactorEnabledAsync(user, false);
        }

        if (tfaRequest.ResetSharedKey)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
        }

        string[]? recoveryCodes = null;
        if (tfaRequest.ResetRecoveryCodes ||
            (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
        {
            var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            recoveryCodes = recoveryCodesEnumerable?.ToArray();
        }

        if (tfaRequest.ForgetMachine)
        {
            await signInManager.ForgetTwoFactorClientAsync();
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        return Ok(new TwoFactorResponse
        {
            SharedKey = key!,
            RecoveryCodes = recoveryCodes,
            RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
            IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
            IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
        });
    }

    [Authorize]
    [HttpGet("manage/info")]
    public async Task<IActionResult> GetInfo()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return NotFound();

        return Ok(await CreateInfoResponseAsync(user));
    }

    [Authorize]
    [HttpPost("manage/info")]
    public async Task<IActionResult> PostInfo([FromBody] InfoRequest infoRequest)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return NotFound();

        if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !_emailAddressAttribute.IsValid(infoRequest.NewEmail))
        {
            return CreateValidationProblem(
                IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
        }

        if (!string.IsNullOrEmpty(infoRequest.NewPassword))
        {
            if (string.IsNullOrEmpty(infoRequest.OldPassword))
            {
                return CreateValidationProblem("OldPasswordRequired",
                    "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");
            }

            var changePasswordResult =
                await userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return CreateValidationProblem(changePasswordResult);
            }
        }

        if (!string.IsNullOrEmpty(infoRequest.NewEmail))
        {
            var email = await userManager.GetEmailAsync(user);
            if (email != infoRequest.NewEmail)
            {
                await SendConfirmationEmailAsync(user, infoRequest.NewEmail, isChange: true);
            }
        }

        return Ok(await CreateInfoResponseAsync(user));
    }


    private async Task SendConfirmationEmailAsync(User user, string email, bool isChange = false)
    {
        var code = isChange
            ? await userManager.GenerateChangeEmailTokenAsync(user, email)
            : await userManager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var userId = await userManager.GetUserIdAsync(user);


        var confirmEmailUrl = Url.Action(
            nameof(ConfirmEmail),
            "Identity",
            new { userId, code, changedEmail = isChange ? email : null },
            protocol: HttpContext.Request.Scheme);

        if (confirmEmailUrl is null) throw new NotSupportedException("Could not generate confirmation URL.");

        await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
    }

    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    private async Task<InfoResponse> CreateInfoResponseAsync(User user)
    {
        return new InfoResponse
        {
            Email = await userManager.GetEmailAsync(user) ??
                    throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
        };
    }

    private IActionResult CreateValidationProblem(string errorCode, string errorDescription) =>
        ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { errorCode, [errorDescription] }
        }));

    private IActionResult CreateValidationProblem(IdentityResult result)
    {
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;
            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return ValidationProblem(new ValidationProblemDetails(errorDictionary));
    }
}