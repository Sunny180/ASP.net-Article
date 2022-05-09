// using System.Text;

// using UserApi.Models;

// namespace WebApi.JWT.Security
// {
//     public class JwtAuth
//     {
//         public string GenerateToken(User user)
//         {
//             string secretKey = "myJwtAuth";//加解密的key,如果不一樣會無法成功解密
//             Dictionary<string, Object> claim = new Dictionary<string, Object>();//payload 需透過token傳遞的資料
//             claim.Add("UserId", user.Id);
//             claim.Add("RoleId", user.RoleId);
//             claim.Add("UserName", user.Name);
//             claim.Add("Exp", DateTime.Now.AddSeconds(Convert.ToInt32("100")).ToString());//Token 時效設定100秒
//             var payload = claim;
//             var token = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);//產生token
//             return token;
//         }
//     }
// }
