using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecureStudentManagement.Services;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHubSecureOAuth";
})
.AddCookie(options =>
{
    options.LoginPath = "/User/PresentLogin";
    options.AccessDeniedPath = "/User/DisplayAccessDenied";
})
.AddOAuth("GitHubSecureOAuth", options =>
{
    var oauthSection = builder.Configuration.GetSection("UserAuth:OAuthGitHubApp");
    options.ClientId = oauthSection["AppClientId"];
    options.ClientSecret = oauthSection["AppClientSecret"];
    options.CallbackPath = "/oauth-callback-github";
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.SaveTokens = true;

    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
    options.ClaimActions.MapJsonKey("profilePic", "avatar_url");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            using var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            context.RunClaimActions(user.RootElement);

            context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"));
        }
    };
});

builder.Services.AddSingleton<StudentCloudStorageService>();
builder.Services.AddSingleton<StudentCosmosDbService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
