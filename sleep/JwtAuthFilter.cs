// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Web;
// // using System.Web.Mvc;
// // using Microsoft.AspNetCore.Mvc.Filters;
// using System.Web.Http.Filters;
// // using System.Web.Http.Controllers;

// namespace WebApi.JWT.Security
// {
//     public class JwtAuthFilterAttribute : ActionFilterAttribute
//     {
        
//         public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
//         {
//             string secret = "myJwtAuth";//加解密的key,如果不一樣會無法成功解密
//             var request = actionContext.Request;
//             if (!WithoutVerifyToken(request.RequestUri.ToString()))
//             {
//                 if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
//                 {
//                     throw new System.Exception("Lost Token");
//                 }
//                 else
//                 {
//                     var jwtObject = Jose.JWT.Decode<Dictionary<string, Object>>(
//                     request.Headers.Authorization.Parameter,
//                     Encoding.UTF8.GetBytes(secret),
//                     JwsAlgorithm.HS512);

//                     Console.WriteLine(jwtObject);

//                     if (IsTokenExpired(jwtObject["Exp"].ToString()))
//                     {
//                         throw new System.Exception("Token Expired");
//                     }
//                 }
//             }

//             base.OnActionExecuting(actionContext);
//         }

//         //Login不需要驗證因為還沒有token
//         public bool WithoutVerifyToken(string requestUri)
//         {
//             if (requestUri.EndsWith("/Login"))
//                 return true;
//             return false;
//         }

//         //驗證token時效
//         public bool IsTokenExpired(string dateTime)
//         {
//             return Convert.ToDateTime(dateTime) < DateTime.Now;
//         }
//     }
// }