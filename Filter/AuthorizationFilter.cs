using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using UserApi.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ArticleApi.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace Filter
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        //用來獲取httpContext
        // private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthorizationFilter(IConfiguration configuration)
        {
            // _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            try
            {
                string? token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                // string? token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token == null)
                {
                    resp.StatusCode = Status.header_err;
                    resp.Message = nameof(Status.header_err);
                    resp.Data = "";
                    context.Result = new JsonResult(resp);
                    return;
                }
                else
                {
                    // string jwt = authHeader.Replace("Bearer ", string.Empty);
                    // var stream = jwt;
                    // var handler = new JwtSecurityTokenHandler();
                    // var jsonToken = handler.ReadToken(token);
                    // var token = jsonToken as JwtSecurityToken;
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
                        context.HttpContext.Items.Add("id", jwtUser.Id);
                        context.HttpContext.Items.Add("roleId", jwtUser.RoleId);
                        context.HttpContext.Items.Add("name", jwtUser.Name);
                    }
                    else
                    {
                        resp.StatusCode = Status.jwtuser_not_exist;
                        resp.Message = nameof(Status.jwtuser_not_exist);
                        resp.Data = "";
                        string response = JsonSerializer.Serialize(resp);
                        context.Result = new JsonResult(resp);
                        return;
                    }
                }
            }
            catch (SecurityTokenExpiredException)
            {
                resp.StatusCode = Status.login_timeout;
                resp.Message = nameof(Status.login_timeout);
                resp.Data = "";
                context.Result = new JsonResult(resp);
                return;
            }
            catch (SecurityTokenValidationException)
            {
                resp.StatusCode = Status.invalid_token;
                resp.Message = nameof(Status.invalid_token);
                resp.Data = "";
                context.Result = new JsonResult(resp);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
                context.Result = new JsonResult(resp);
                return;
            }
        }
    }
}
