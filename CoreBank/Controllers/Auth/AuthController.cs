using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Services;
using System.Security.Claims;
using System.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private const int PasswordExpiryDays = 30;
    private readonly IUserService _userService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserService userService,
        IPasswordHasher passwordHasher,
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _userService = userService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation($"Login attempt for username: {request.username} from IP: {HttpContext.Connection.RemoteIpAddress}");

        try
        {
            // Get user by username
            var userResponse = await _userService.GetUserByUsername(request.username);

            _logger.LogInformation($"GetUserByUsername response - Success: {userResponse.Success}, Message: {userResponse.Message}");

            if (!userResponse.Success || userResponse.Data == null)
            {
                _logger.LogWarning($"User not found or error: {request.username}");
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid username or password."
                });
            }

            var user = userResponse.Data;
            _logger.LogInformation($"User found - ID: {user.Id}, Name: {user.Name_en}, Status: {user.Status}, LockedUntil: {user.AccountLockedUntil}, FailedAttempts: {user.FailedLoginAttempts}, MustChangePassword: {user.MustChangePassword}");

            // 1. Check if user is active
            if (user.Status.ToLower() != "active")
            {
                _logger.LogWarning($"User {user.Id} is not active. Status: {user.Status}");
                return Unauthorized(new
                {
                    success = false,
                    message = $"Account is {user.Status}. Please contact administrator."
                });
            }

            // 2. Check if account is locked - Use SERVER TIME
            if (user.AccountLockedUntil.HasValue)
            {
                var lockUntil = user.AccountLockedUntil.Value;
                var currentTime = DateTime.Now;

                _logger.LogInformation($"Lock check - LockUntil: {lockUntil}, Current: {currentTime}, IsLocked: {lockUntil > currentTime}");

                if (lockUntil > currentTime)
                {
                    var timeRemaining = lockUntil - currentTime;
                    var minutesRemaining = (int)Math.Ceiling(timeRemaining.TotalMinutes);
                    _logger.LogWarning($"Account {user.Id} is locked. Time remaining: {minutesRemaining} minutes");

                    return Unauthorized(new
                    {
                        success = false,
                        message = $"Account is locked. Please try again in {minutesRemaining} minutes."
                    });
                }
                else
                {
                    _logger.LogInformation($"Lock expired for user {user.Id}. Clearing lock.");
                    await _userService.ResetFailedLoginAttempts(user.Id);

                    // Update the user object after resetting
                    user.AccountLockedUntil = null;
                    user.FailedLoginAttempts = 0;
                }
            }

            // 3. Verify password
            _logger.LogInformation($"Verifying password for user {user.Id}");
            bool isPasswordValid = _passwordHasher.VerifyPassword(user.password_hash, request.Password);
            _logger.LogInformation($"Password verification result: {isPasswordValid}");

            if (!isPasswordValid)
            {
                _logger.LogWarning($"Invalid password for user: {user.Id}");

                // Increment failed attempts
                var lockResult = await _userService.HandleFailedLogin(user.Id);
                _logger.LogInformation($"Failed login - Attempts: {lockResult.FailedAttempts}, Locked: {lockResult.AccountLocked}");

                if (lockResult.AccountLocked)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Too many failed attempts. Account locked for 15 minutes."
                    });
                }

                // Show attempts remaining (5 total attempts)
                var attemptsRemaining = 5 - lockResult.FailedAttempts;
                var attemptsMessage = attemptsRemaining > 0 ?
                    $"{attemptsRemaining} attempt(s) remaining." :
                    "Account will be locked on next failed attempt.";

                return Unauthorized(new
                {
                    success = false,
                    message = $"Invalid password. {attemptsMessage}"
                });
            }

            _logger.LogInformation($"Password is valid for user {user.Id}");

            // 4. Reset failed attempts (since password is correct)
            await _userService.ResetFailedLoginAttempts(user.Id);

            // 5. Check if password change is required
            if (user.MustChangePassword)
            {
                _logger.LogInformation($"Password change required for user {user.Id}. Redirecting to change password.");

                // Create limited session for password change
                var limitedClaims = CreateLimitedClaims(user);
                var limitedIdentity = new ClaimsIdentity(limitedClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var limitedPrincipal = new ClaimsPrincipal(limitedIdentity);

                await SignInUser(limitedPrincipal, true);
                await _userService.UpdateLastLogin(user.Id);

                // Set test cookie for debugging
                SetTestCookie("limited_auth", "true");

                return Ok(new
                {
                    success = true,
                    requiresPasswordChange = true,
                    redirectUrl = "/AuthView/ChangePassword",
                    username = user.Name_en,
                    message = "Password change required"
                });
            }

            // 6. Check password expiry (30 days)
            var passwordExpiryDate = user.LastPasswordChange.AddDays(PasswordExpiryDays);
            if (passwordExpiryDate < DateTime.Now)
            {
                _logger.LogInformation($"Password expired for user {user.Id}. Requiring change.");
                await _userService.RequirePasswordChange(user.Id);

                // Expired passwords get the same limited session as other forced-change flows.
                var limitedClaims = CreateLimitedClaims(user);
                var limitedIdentity = new ClaimsIdentity(limitedClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var limitedPrincipal = new ClaimsPrincipal(limitedIdentity);

                await SignInUser(limitedPrincipal, true);

                // Set test cookie for debugging
                SetTestCookie("expired_password", "true");

                return Ok(new
                {
                    success = true,
                    requiresPasswordChange = true,
                    redirectUrl = "/AuthView/ChangePassword",
                    username = user.Name_en,
                    message = "Password has expired"
                });
            }

            // 7. Update last login
            await _userService.UpdateLastLogin(user.Id);

            // 8. Create claims and sign in (normal login)
            var normalClaims = CreateClaims(user, false);
            var normalIdentity = new ClaimsIdentity(normalClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var normalPrincipal = new ClaimsPrincipal(normalIdentity);

            await SignInUser(normalPrincipal, false);

            // Set test cookie for debugging
            SetTestCookie("normal_auth", "true");

            _logger.LogInformation($"User {user.Id} successfully logged in");

            return Ok(new
            {
                success = true,
                redirectUrl = "/Dashboard/Index",
                username = user.Name_en,
                requiresPasswordChange = false,
                message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Login error for {request.username}");
            return StatusCode(500, new
            {
                success = false,
                message = "System error. Please try again later."
            });
        }
    }

    // Helper method to create limited claims for password change
    private Claim[] CreateLimitedClaims(Users user)
    {
        return new[]
        {
            new Claim(ClaimTypes.Name, user.Name_en),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("MustChangePassword", "true"),
            new Claim("LimitedAccess", "password-change-only"),
            new Claim("UserId", user.Id),
            new Claim("AuthType", "limited")
        };
    }

    // Helper method to create full claims
    private Claim[] CreateClaims(Users user, bool mustChangePassword)
    {
        return new[]
        {
            new Claim(ClaimTypes.Name, user.Name_en),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("BranchCode", user.BranchId ?? ""),
            new Claim("MustChangePassword", mustChangePassword.ToString().ToLower()),
            new Claim("UserId", user.Id),
            new Claim("AuthType", mustChangePassword ? "expired" : "full")
        };
    }

    // Helper method to sign in user
    private async Task SignInUser(ClaimsPrincipal principal, bool isLimited)
    {
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTime.UtcNow.AddHours(1),
            AllowRefresh = true,
            IssuedUtc = DateTime.UtcNow
        };

        if (isLimited)
        {
            authProperties.ExpiresUtc = DateTime.UtcNow.AddMinutes(15);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);
    }

    // Helper method to set test cookie for debugging
    private void SetTestCookie(string name, string value)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false, // Allow JavaScript to read
            Secure = false, // For testing - set to true in production
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddHours(1),
            IsEssential = true
        };

        Response.Cookies.Append($"AuthTest_{name}", value, cookieOptions);
    }

    // Add this endpoint for debugging authentication
    [HttpGet("check-auth")]
    [AllowAnonymous]
    public IActionResult CheckAuthentication()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var userName = User.Identity?.Name;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var authType = User.FindFirst("AuthType")?.Value;
        var cookies = Request.Cookies.Keys.ToArray();

        _logger.LogInformation($"Auth check - Authenticated: {isAuthenticated}, User: {userName}, Cookies: {string.Join(", ", cookies)}");

        return Ok(new
        {
            authenticated = isAuthenticated,
            username = userName,
            userId = userId,
            authType = authType,
            cookies = cookies,
            requestHeaders = Request.Headers.Keys.Where(k => k.Contains("Cookie", StringComparison.OrdinalIgnoreCase)).ToArray()
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var mustChangePassword = User.FindFirst("MustChangePassword")?.Value;
        var limitedAccess = User.FindFirst("LimitedAccess")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "User not authenticated." });

        // Validate password confirmation
        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new { success = false, message = "New password and confirmation do not match." });
        }

        var result = await _userService.ChangePassword(userId, request.OldPassword, request.NewPassword);

        if (!result.Success)
            return BadRequest(result);

        // If this was a forced password change, update the user session
        if (mustChangePassword == "true" || limitedAccess == "password-change-only")
        {
            // Get updated user info
            var userResponse = await _userService.GetUserById(userId);
            if (userResponse.Success && userResponse.Data != null)
            {
                var user = userResponse.Data;

                // Create full access claims
                var fullClaims = CreateClaims(user, false);
                var fullIdentity = new ClaimsIdentity(fullClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var fullPrincipal = new ClaimsPrincipal(fullIdentity);

                await SignInUser(fullPrincipal, false);
            }
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userName = User.Identity?.Name;
            _logger.LogInformation($"Logout requested for user: {userName}");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear all auth cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith("AuthTest_") || cookie.Contains("MinCoreBank"))
                {
                    Response.Cookies.Delete(cookie);
                }
            }

            // Clear session
            HttpContext.Session.Clear();

            _logger.LogInformation($"User {userName} logged out successfully");

            return Ok(new
            {
                success = true,
                redirectUrl = "/AuthView/Login",
                message = "Logged out successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Ok(new
            {
                success = true,
                redirectUrl = "/AuthView/Login",
                message = "Logged out successfully"
            });
        }
    }

    [HttpGet("current-user")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userName = User.Identity?.Name;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var branchCode = User.FindFirst("BranchCode")?.Value;
        var mustChangePassword = User.FindFirst("MustChangePassword")?.Value;

        return Ok(new
        {
            username = userName,
            userId = userId,
            role = userRole,
            branchCode = branchCode,
            mustChangePassword = mustChangePassword == "true"
        });
    }

    public class LoginRequest
    {
        public string username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
