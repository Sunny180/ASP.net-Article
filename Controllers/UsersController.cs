using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using UserApi.Models;
using ArticleApi.Models;
using JwtAuthDemo.Helpers;
using Microsoft.AspNetCore.Authorization;
using Filter;
using System.Data;
using Newtonsoft.Json;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly JwtHelpers jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UsersController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, JwtHelpers jwt)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            this.jwt = jwt;
        }

        // POST api/users/login
        [AllowAnonymous]
        [HttpPost, Route("login")]
        public ResponseFormat<string> Login(Login login)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string sqlStr1 = @"
                        SELECT [user].[Id]
                            , [user].[Name]
                            , [user].[Role_Id] AS [RoleId] 
                        FROM [F62ND_Test].[dbo].[UserChing] AS [user] 
                        INNER JOIN [F62ND_Test].[dbo].[RoleChing] AS [role] 
                        ON [user].[Role_Id] = [role].[Id] 
                        WHERE [user].[Account]= @Account 
                        AND [user].[Password]= @Password";
                    string sqlStr2 = @"
                        IF EXISTS 
                            (SELECT [token].[User_Id] AS [Id] 
                            FROM [F62ND_Test].[dbo].[UserChing] AS [user] 
                            INNER JOIN [F62ND_Test].[dbo].[TokenChing] AS [token] 
                            ON [user].[Id] = [token].[User_Id] 
                            WHERE [user].[Id] = @Id)
                        BEGIN
                            UPDATE [F62ND_Test].[dbo].[TokenChing] 
                            SET [Token] = @Token
                                , [UpdateTime] = CURRENT_TIMESTAMP 
                            WHERE [User_Id] = @Id 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO [F62ND_Test].[dbo].[TokenChing] (
                                [User_Id]
                                , [Token]
                                , [UpdateTime])
                            VALUES (@Id
                                , @Token
                                , CURRENT_TIMESTAMP) 
                        END";
                    SqlParameter[] sqlParameters = new[]
                    {
                        new SqlParameter("@Account", login.Account ),
                        new SqlParameter("@Password", login.Password )
                    };
                    SqlDataAdapter adapter1 = new SqlDataAdapter(sqlStr1, connection);
                    adapter1.SelectCommand.Parameters.AddRange(sqlParameters);
                    DataSet data = new DataSet();
                    adapter1.Fill(data);
                    string temp = JsonConvert.SerializeObject(data.Tables[0], Formatting.Indented);
                    List<User>? rows = JsonConvert.DeserializeObject<List<User>>(temp);
                    if (!ModelState.IsValid)
                    {
                        resp.StatusCode = Status.invalid_parameter;
                        resp.Message = nameof(Status.invalid_parameter);
                        resp.Data = "";
                    }
                    else if (rows == null)
                    {
                        resp.StatusCode = Status.incorrect_account_or_password;
                        resp.Message = nameof(Status.incorrect_account_or_password);
                        resp.Data = "";
                    }
                    else if (rows.Count != 1)
                    {
                        resp.StatusCode = Status.incorrect_account_or_password;
                        resp.Message = nameof(Status.incorrect_account_or_password);
                        resp.Data = "";
                    }
                    else
                    {
                        JwtHelpers jwtHelpers = new JwtHelpers(_configuration);
                        User user = new User();
                        user.Id = rows[0].Id;
                        user.RoleId = rows[0].RoleId;
                        user.Name = rows[0].Name;
                        string jwtToken = jwtHelpers.GenerateToken(user);
                        SqlParameter[] sqlParameters2 = new[]
                        {
                                new SqlParameter("@Id", rows[0].Id),
                                new SqlParameter("@Token", jwtToken)
                            };
                        SqlDataAdapter adapter2 = new SqlDataAdapter(sqlStr2, connection);
                        adapter2.SelectCommand.Parameters.AddRange(sqlParameters2);
                        DataSet data2 = new DataSet();
                        adapter2.Fill(data2);
                        resp.StatusCode = Status.success;
                        resp.Message = nameof(Status.success);
                        resp.Data = jwtToken;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }

        // POST api/users/logout
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPost, Route("logout")]
        // [Authorize]
        public ResponseFormat<string> Logout()
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            int userId = (int)HttpContext.Items["id"];
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    // JwtHelperAuth Auth = new JwtHelperAuth(_httpContextAccessor);
                    // var user = Auth.Authorization();
                    string sqlStr1 = @"
                        UPDATE [F62ND_Test].[dbo].[TokenChing] 
                        SET [Token] = '' 
                        WHERE [User_Id] = @Id";
                    SqlParameter[] sqlParameters = new[]
                   {
                        new SqlParameter("@Id", userId)
                    };
                    SqlDataAdapter adapter2 = new SqlDataAdapter(sqlStr1, connection);
                    adapter2.SelectCommand.Parameters.AddRange(sqlParameters);
                    DataSet data2 = new DataSet();
                    adapter2.Fill(data2);
                    resp.StatusCode = Status.success;
                    resp.Message = nameof(Status.success);
                    resp.Data = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }
    }
}





// string sqlStr1 = @"
//     SELECT [token].[User_Id] AS [Id]
//     FROM [F62ND_Test].[dbo].[TokenChing] AS [token] 
//     INNER JOIN [F62ND_Test].[dbo].[UserChing] AS [user] 
//     ON [user].[Id] = [token].[User_Id] 
//     WHERE [token].[User_Id]= @Id";


