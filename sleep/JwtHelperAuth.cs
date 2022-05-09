// using System.IdentityModel.Tokens.Jwt;
// using UserApi.Models;
// using System.Text.Json;

// namespace JwtAuthDemo.Helpers
// {
//     public class JwtHelperAuth
//     {
//         private readonly IHttpContextAccessor _httpContextAccessor;

//         public JwtHelperAuth(IHttpContextAccessor httpContextAccessor)
//         {
//             _httpContextAccessor = httpContextAccessor;
//         }

//         public User Authorization()
//         {           
//             string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
//             string jwt = authHeader.Replace("Bearer ", string.Empty);
//             var stream = jwt;
//             var handler = new JwtSecurityTokenHandler();
//             var jsonToken = handler.ReadToken(stream);
//             var token = jsonToken as JwtSecurityToken;
//             var jsonUser = token.Claims.First(claim => claim.Type == "Users").Value;
//             User jwtUser = JsonSerializer.Deserialize<User>(jsonUser);
//             User user = new User();
//             user.Id = jwtUser.Id;
//             user.RoleId = jwtUser.RoleId;
//             user.Name = jwtUser.Name;
//             return user;
//         }
//     }
// }
