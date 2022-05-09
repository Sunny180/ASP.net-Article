// using System.IdentityModel.Tokens.Jwt;
// using Microsoft.IdentityModel.Tokens;
// using UserApi.Models;
// using System.Text;
// using System.Text.Json;
// using ArticleApi.Models;


// namespace Middleware
// {
//     public class JwtMiddleware01
//     {
//         // public class JwtMiddlewarePipeLine
//         // {
//         //     public void Configuration(IApplicationBuilder app)
//         //     {
//         //         app.UseMiddleware<JwtMiddleware>();
//         //     }
//         // }
//         private readonly IHttpContextAccessor _httpContextAccessor;
//         public readonly RequestDelegate _next;
//         private readonly IConfiguration _configuration;
//         // private readonly IUserService _userService;

//         public JwtMiddleware01(IHttpContextAccessor httpContextAccessor, RequestDelegate next, IConfiguration configuration)
//         {
//             _httpContextAccessor = httpContextAccessor;
//             _next = next;
//             _configuration = configuration;
//             // _userService = userService;
//         }
//         public async Task Invoke(HttpContext context)
//         {
//             string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
//             Console.WriteLine("token");

//             if (token != null)
//             {
//                 attachAccountToContext(context, token);
//             }
//             await _next(context);
//         }

//         public void attachAccountToContext(HttpContext context, string token)
//         {
//             Console.WriteLine("1");
//             ResponseFormat<string> resp = new ResponseFormat<string>();
//             try
//             {
//                 // string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
//                 // Console.WriteLine(authHeader);
//                 // if (authHeader == null)
//                 // {
//                 // resp.StatusCode = (int)Status.header_err;
//                 // resp.Message = nameof(Status.header_err);
//                 // resp.Data = "";
//                 // context.Result = new JsonResult(resp);
//                 // return;
//                 // context.HttpContext.Items["resp"] = resp;
//                 // context.Items.Add("resp",resp);
//                 // }
//                 // else
//                 // {
//                 // string jwt = authHeader.Replace("Bearer ", string.Empty);
//                 // var stream = jwt;
//                 // var tokenHandler = new JwtSecurityTokenHandler();
//                 // var jsonToken = tokenHandler.ReadToken(token);
//                 // var token = jsonToken as JwtSecurityToken;
//                 // var jsonUser = token.Claims.First(claim => claim.Type == "Users").Value;
//                 // User jwtUser = JsonSerializer.Deserialize<User>(jsonUser);
//                 // User user = new User();
//                 // user.Id = jwtUser.Id;
//                 // user.RoleId = jwtUser.RoleId;
//                 // user.Name = jwtUser.Name;
//                 // context.Items.Add("user",user);


//                 var tokenHandler = new JwtSecurityTokenHandler();
//                 var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SignKey"]);
//                 tokenHandler.ValidateToken(token, new TokenValidationParameters
//                 {
//                     //                 ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
//                     //                 ValidAudience = builder.Configuration["JwtSettings:Issuer"],
//                     ValidateIssuer = true,
//                     ValidateIssuerSigningKey = false,
//                     ValidateAudience = false,
//                     ValidateLifetime = true,
//                     IssuerSigningKey = new SymmetricSecurityKey(key),
//                     // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
//                     ClockSkew = TimeSpan.Zero
//                 }, out SecurityToken validatedToken);

//                 var jwtToken = (JwtSecurityToken)validatedToken;
//                 var jsonUser = jwtToken.Claims.First(claim => claim.Type == "Users").Value;
//                 User jwtUser = JsonSerializer.Deserialize<User>(jsonUser);
//                 User user = new User();
//                 user.Id = jwtUser.Id;
//                 user.RoleId = jwtUser.RoleId;
//                 user.Name = jwtUser.Name;
//                 context.Items.Add("user", user);
//                 // attach account to context on successful jwt validation
//                 // context.Items["User"] = _userService.GetUserDetails();
//             }
//             // }

//             catch (SecurityTokenExpiredException)
//             {
//                 resp.StatusCode = (int)Status.login_timeout;
//                 resp.Message = nameof(Status.login_timeout);
//                 resp.Data = "";
//                 context.Items.Add("resp", resp);
//                 // context.Result = new JsonResult(resp);
//                 return;
//                 // context.Response.Headers.Add("Token-Expired", "true");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine(ex);
//                 // context.Response.Headers.Add("system_fail", "true");
//                 resp.StatusCode = (int)Status.system_fail;
//                 resp.Message = nameof(Status.system_fail);
//                 resp.Data = "";
//                 context.Items.Add("resp", resp);
//                 // context.Result = new JsonResult(resp);
//                 return;
//                 // context.Items.Add("resp",resp);
//             }
//         }
//     }
// }
