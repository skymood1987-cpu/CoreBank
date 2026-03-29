using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
public class TimeRestrictionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimeRestrictionMiddleware> _logger;

    public TimeRestrictionMiddleware(
        RequestDelegate next,
        ILogger<TimeRestrictionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


public async Task<DateTime> GetBaghdadTimeAsync()
{
    using var client = new HttpClient();
    var response = await client.GetStringAsync("https://worldtimeapi.org/api/timezone/Asia/Baghdad");
    using var json = JsonDocument.Parse(response);
    var dateTimeString = json.RootElement.GetProperty("datetime").GetString();
    return DateTime.TryParse(dateTimeString, out var parsed) ? parsed : DateTime.UtcNow;
}


public async Task InvokeAsync(HttpContext context)
    {
        // Get Baghdad time (UTC+3)
        TimeZoneInfo baghdadTimeZone;
        try
        {
            baghdadTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baghdad");
        }
        catch (TimeZoneNotFoundException)
        {
            baghdadTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabic Standard Time");
        }
        var baghdadTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, baghdadTimeZone);
        // FOR TESTING - Uncomment to simulate time outside window
        // var baghdadTime = new DateTime(baghdadTime.Year, baghdadTime.Month, baghdadTime.Day, 15, 0, 0); // 3PM Baghdad

        var start = new TimeSpan(6, 30, 0); // 8:30 AM Baghdad Time
        var end = new TimeSpan(14, 30, 0);  // 2:30 PM Baghdad Time

        // Always allowed paths (case insensitive)
        var allowedPaths = new[] {
            "/AuthView/Login",
            "/Shared/AccessDenied",
            "/css/",
            "/js/",
            "/lib/",
            "/favicon.ico"
        };

        var requestedPath = context.Request.Path.ToString().ToLower();
        var isAllowedPath = allowedPaths.Any(p => requestedPath.StartsWith(p));

        if (!isAllowedPath && (baghdadTime.TimeOfDay < start || baghdadTime.TimeOfDay > end))
        {
            _logger.LogWarning($"BLOCKED ACCESS at {baghdadTime:HH:mm:ss} Baghdad Time to {context.Request.Path}");

            context.Response.StatusCode = 403;
            await context.Response.WriteAsync($$"""
                <!DOCTYPE html>
                <html lang="ar" dir="rtl">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>الخدمة غير متاحة</title>
                    <style>
                        * {
                            margin: 0;
                            padding: 0;
                            box-sizing: border-box;
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        }

                        body {
                            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
                            min-height: 100vh;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            padding: 20px;
                            color: #333;
                        }

                        .container {
                            background-color: #fff;
                            border-radius: 12px;
                            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
                            width: 100%;
                            max-width: 600px;
                            padding: 40px;
                            text-align: center;
                            transition: transform 0.3s ease;
                        }

                        .container:hover {
                            transform: translateY(-5px);
                        }

                        h1 {
                            color: #d9534f;
                            font-size: 2.5rem;
                            margin-bottom: 25px;
                            font-weight: 700;
                        }

                        .info-box {
                            background-color: #f8f9fa;
                            border-radius: 8px;
                            padding: 25px;
                            margin: 25px 0;
                            border-right: 5px solid #d9534f;
                            text-align: right;
                        }

                        p {
                            font-size: 1.2rem;
                            line-height: 1.8;
                            margin-bottom: 15px;
                        }

                        strong {
                            color: #2c3e50;
                            font-weight: 700;
                        }

                      

                        .login-btn {
                            display: inline-block;
                            background-color: #5cb85c;
                            color: white;
                            padding: 14px 35px;
                            text-decoration: none;
                            border-radius: 6px;
                            font-size: 1.2rem;
                            font-weight: 600;
                            margin-top: 20px;
                            transition: background-color 0.3s ease;
                        }

                        .login-btn:hover {
                            background-color: #4cae4c;
                        }

                        .footer {
                            margin-top: 30px;
                            color: #6c757d;
                            font-size: 0.9rem;
                        }

                        @media (max-width: 768px) {
                            .container {
                                padding: 25px;
                            }

                            h1 {
                                font-size: 2rem;
                            }

                            p {
                                font-size: 1.1rem;
                            }

                            .time-display {
                                font-size: 1.1rem;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class="container">
                        <h1>الخدمة غير متاحة حاليا</h1>

                        <div class="info-box">
                            <p><bold>ساعات العمل المتاحة:</bold> من 8:30 صباحاً إلى 2:30 مساءً بتوقيت بغداد</p>
                           <p class="info-text">
                    <span class="server-time">الوقت الحالي على الخادم:</span>
                    <span class="time-display" id="baghdad-time">--:--:--</span>
                    بتوقيت بغداد
                </p>
                        </div>

                        <a href="/AuthView/Login" class="btn btn-primary w-100 mt-3">الانتقال إلى صفحة تسجيل الدخول</a>

                        <div class="footer">
                          &copy; 2025 AG Bank. All rights reserved.
                        </div>
                    </div>

                    <script>
                        function updateBaghdadTime() {
                            const options = {
                                timeZone: 'Asia/Baghdad',
                                hour12: true,
                                hour: '2-digit',
                                minute: '2-digit',
                                second: '2-digit'
                            };

                            const formatter = new Intl.DateTimeFormat('ar-IQ', options);
                            const timeString = formatter.format(new Date());

                            document.getElementById('baghdad-time').textContent = timeString;
                        }

                        // تحديث الوقت فوراً ثم كل ثانية
                        updateBaghdadTime();
                        setInterval(updateBaghdadTime, 1000);
                    </script>
                </body>
                </html>
                """);
            return;
        }

        var requiresPasswordChange =
            context.User.Identity?.IsAuthenticated == true &&
            (string.Equals(context.User.FindFirst("MustChangePassword")?.Value, "true", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(context.User.FindFirst("LimitedAccess")?.Value, "password-change-only", StringComparison.OrdinalIgnoreCase));

        if (requiresPasswordChange)
        {
            var forcedChangePaths = new[]
            {
                "/authview/changepassword",
                "/authview/logout",
                "/api/auth/change-password",
                "/api/auth/logout",
                "/api/auth/current-user"
            };

            var isForcedChangePath = forcedChangePaths.Any(p => requestedPath.StartsWith(p));

            if (!isForcedChangePath)
            {
                if (requestedPath.StartsWith("/api/"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        requiresPasswordChange = true,
                        redirectUrl = "/AuthView/ChangePassword",
                        message = "Password change required before accessing the system."
                    });
                    return;
                }

                context.Response.Redirect("/AuthView/ChangePassword");
                return;
            }
        }

        await _next(context);
    }
}
