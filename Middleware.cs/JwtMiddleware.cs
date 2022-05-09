using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using UserApi.Models;
using System.Text;
using System.Text.Json;
using ArticleApi.Models;

namespace Middleware1
{
    public class JwtMiddlewarePipeLine
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<JwtMiddleware>();
        }
    }
    public class JwtMiddleware
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtMiddleware(IHttpContextAccessor httpContextAccessor, RequestDelegate next, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _next = next;
            _configuration = configuration;
        }
        public async Task Invoke(HttpContext context)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            try
            {
                string? token1 = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string? token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token == null)
                {
                    context.Response.ContentType = "application/json";
                    resp.StatusCode = Status.header_err;
                    resp.Message = nameof(Status.header_err);
                    resp.Data = "";
                    string response = JsonSerializer.Serialize(resp);
                    await context.Response.WriteAsync(response);
                    return;
                }
                else
                {
                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SignKey"]);
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidIssuer = _configuration["JwtSettings:Issuer"],
                        ValidAudience = _configuration["JwtSettings:Issuer"],
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        // ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);
                    JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                    string jsonUser = jwtToken.Claims.First(claim => claim.Type == "Users").Value;
                    User? jwtUser = JsonSerializer.Deserialize<User>(jsonUser);
                    if (jwtUser != null)
                    {
                        context.Items.Add("id", jwtUser.Id);
                        context.Items.Add("roleId", jwtUser.RoleId);
                        context.Items.Add("name", jwtUser.Name);
                    }
                    else
                    {
                        resp.StatusCode = Status.jwtuser_not_exist;
                        resp.Message = nameof(Status.jwtuser_not_exist);
                        resp.Data = "";
                        string response = JsonSerializer.Serialize(resp);
                        await context.Response.WriteAsync(response);
                        return;
                    }
                }
            }
            catch (SecurityTokenExpiredException)
            {
                context.Response.ContentType = "application/json";
                resp.StatusCode = Status.login_timeout;
                resp.Message = nameof(Status.login_timeout);
                resp.Data = "";
                string response = JsonSerializer.Serialize(resp);
                await context.Response.WriteAsync(response);
                return;
            }
            catch (SecurityTokenValidationException)
            {
                context.Response.ContentType = "application/json";
                resp.StatusCode = Status.invalid_token;
                resp.Message = nameof(Status.invalid_token);
                resp.Data = "";
                string response = JsonSerializer.Serialize(resp);
                await context.Response.WriteAsync(response);
                return;
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
                string response = JsonSerializer.Serialize(resp);
                await context.Response.WriteAsync(response);
                return;
            }
            await _next.Invoke(context);
        }
    }
}
