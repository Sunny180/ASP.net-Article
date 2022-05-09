using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ArticleApi.Models;
// using Microsoft.AspNetCore.Authorization;
using JwtAuthDemo.Helpers;
using Filter;
using Middleware1;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace ArticleApi.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtHelpers jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ArticlesController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, JwtHelpers jwt)
        {
            _configuration = configuration;
            this.jwt = jwt;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Articles
        [HttpGet]
        public ResponseFormat<List<GetArticleOverview>> GetArticles(int userId, string? keyword)
        {
            ResponseFormat<List<GetArticleOverview>> resp = new ResponseFormat<List<GetArticleOverview>>();
            List<GetArticleOverview> empty = new List<GetArticleOverview>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    StringBuilder sqlStr = new StringBuilder(@"
                            SELECT [article].[Id]
                                ,[article].[Title]
                                ,[article].[Content]
                                ,[article].[User_Id] AS [UserId]
                                ,[user].[Name] 
                            FROM [F62ND_Test].[dbo].[ArticleChing] AS [article] 
                            INNER JOIN [F62ND_Test].[dbo].[UserChing] AS [user] 
                            ON [article].[User_Id] = [user].[Id]");
                    if (userId == 0 && keyword == null)
                    {
                        sqlStr.Append("");
                    }
                    else if (userId == 0 && keyword != null)
                    {
                        sqlStr.Append(@$"WHERE [article].[Title] LIKE '%{keyword}%' 
                                OR [user].[Name] LIKE '%{keyword}%'");
                    }
                    else if (userId != 0 && keyword == null)
                    {
                        sqlStr.Append($"WHERE [user].[Id] = {userId}");
                    }
                    else
                    {
                        sqlStr.Append($@"WHERE [user].[Id] = {userId} 
                                AND [article].[Title] LIKE '%{keyword}%'");
                    }
                    SqlDataAdapter adapter = new SqlDataAdapter(sqlStr.ToString(), connection);
                    DataSet data = new DataSet();
                    adapter.Fill(data);
                    string temp = JsonConvert.SerializeObject(data.Tables[0], Formatting.Indented);
                    List<GetArticleOverview>? result = JsonConvert.DeserializeObject<List<GetArticleOverview>>(temp);
                    resp.StatusCode = Status.success;
                    resp.Message = nameof(Status.success);
                    resp.Data = result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = empty;
            }
            return resp;
        }

        // GET: api/Articles/5
        [HttpGet("{id}")]
        public ResponseFormat<GetArticleDetail> GetArticle(int id)
        {
            GetArticleDetail empty = new GetArticleDetail();
            ResponseFormat<GetArticleDetail> resp = new ResponseFormat<GetArticleDetail>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {

                    string sqlStr = @$"
                        SELECT [User].[Name]
                            ,[Article].[Id]
                            ,[Article].[Title]
                            ,[Article].[Content]
                            ,[Article].[User_Id] AS [UserId]
                            ,[Article].[CreateTime]
                            ,[Article].[UpdateTime]
                        FROM [F62ND_Test].[dbo].[ArticleChing] AS [Article]
                        INNER JOIN [F62ND_Test].[dbo].[UserChing] AS [User] 
                        ON [Article].[User_Id] = [User].[Id]
                        WHERE [Article].[Id] = {id}";

                    SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, connection);
                    DataSet data = new DataSet();
                    adapter.Fill(data);
                    string temp = JsonConvert.SerializeObject(data.Tables[0], Formatting.Indented);
                    List<GetArticleDetail>? result = JsonConvert.DeserializeObject<List<GetArticleDetail>>(temp);
                    if ((result != null) && (result.Count == 1))
                    {
                        resp.StatusCode = Status.success;
                        resp.Message = nameof(Status.success);
                        resp.Data = result[0];
                    }
                    else
                    {
                        resp.StatusCode = Status.data_not_found;
                        resp.Message = nameof(Status.data_not_found);
                        resp.Data = empty;
                    }
                }
            }
            catch (Exception)
            {
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = empty;
            }
            return resp;
        }

        // PUT: api/Articles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPut("{id}")]
        public ResponseFormat<string> UpdateArticle(int id, PutArticle putArticle)
        {
            int roleId = (int)HttpContext.Items["roleId"];
            int userId = (int)HttpContext.Items["id"];
            ResponseFormat<string> resp = new ResponseFormat<string>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    // JwtHelperAuth Auth = new JwtHelperAuth(_httpContextAccessor);
                    // var user = Auth.Authorization();
                    string sqlStr1 = @"
                        SELECT [Id] 
                        FROM [F62ND_Test].[dbo].[ArticleChing] 
                        WHERE [Id] = @Id";
                    string sqlStr2 = "";
                    if (roleId == 1)
                    {
                        sqlStr2 = @"
                        UPDATE [F62ND_Test].[dbo].[ArticleChing] 
                        SET [Title]=@Title
                            ,[Content]=@Content
                            ,[AdminId]=@AdminId 
                            ,[UpdateTime]=CURRENT_TIMESTAMP
                        WHERE [Id] = @Id";
                    }
                    else if (roleId == 2)
                    {
                        sqlStr2 = @"
                        UPDATE [F62ND_Test].[dbo].[ArticleChing] 
                        SET [Title]=@Title
                            ,[Content]=@Content
                            ,[AdminId]= @AdminId 
                            ,[UpdateTime]=CURRENT_TIMESTAMP
                        WHERE [Id] = @Id
                        AND [User_Id] = @UserId";
                    }
                    else
                    {
                        resp.StatusCode = Status.permission_denied;
                        resp.Message = nameof(Status.permission_denied);
                        resp.Data = "";
                    }
                    SqlParameter[] sqlParameters = new[]
                    {
                        new SqlParameter("@Title",putArticle.Title),
                        new SqlParameter("@Content",putArticle.Content),
                        new SqlParameter("@AdminId",userId),
                        new SqlParameter("@Id", id ),
                        new SqlParameter("@UserId", userId )
                    };

                    SqlDataAdapter adapter1 = new SqlDataAdapter(sqlStr1, connection);
                    adapter1.SelectCommand.Parameters.AddRange(sqlParameters);
                    DataSet data = new DataSet();
                    adapter1.Fill(data);
                    string temp = JsonConvert.SerializeObject(data.Tables[0], Formatting.Indented);
                    List<GetArticleId>? rows = JsonConvert.DeserializeObject<List<GetArticleId>>(temp);
                    adapter1.SelectCommand.Parameters.Clear();
                    if ((rows != null) && (rows.Count == 1))
                    {
                        SqlDataAdapter adapter2 = new SqlDataAdapter(sqlStr2, connection);
                        adapter2.SelectCommand.Parameters.AddRange(sqlParameters);  //InsertCommand錯誤
                        DataSet data2 = new DataSet();
                        adapter2.Fill(data2);
                        resp.StatusCode = Status.success;
                        resp.Message = nameof(Status.success);
                        resp.Data = "";
                    }
                    else
                    {
                        resp.StatusCode = Status.data_not_found;
                        resp.Message = nameof(Status.data_not_found);
                        resp.Data = "";
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


        // POST: api/Articles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [MiddlewareFilter(typeof(JwtMiddlewarePipeLine))]
        // [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPost]
        public ResponseFormat<string> PostArticle(PostArticle postArticle)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            int roleId = (int)HttpContext.Items["roleId"];
            int userId = (int)HttpContext.Items["id"];
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string sqlStr = @"
                        INSERT INTO [F62ND_Test].[dbo].[ArticleChing] (
                            [Title]
                            , [Content]
                            , [User_Id] 
                            , [AdminId]
                            , [UpdateTime]) 
                        VALUES (@Title
                            , @Content
                            , @UserId
                            , @AdminId
                            , CURRENT_TIMESTAMP)";
                    SqlParameter[] sqlParameters = new[]
                    {
                        new SqlParameter("@Title", postArticle.Title ),
                        new SqlParameter("@Content", postArticle.Content ),
                        new SqlParameter("@UserId", postArticle.UserId ),
                        new SqlParameter("@AdminId", postArticle.AdminId )
                    };
                    if (!ModelState.IsValid)
                    {
                        resp.StatusCode = Status.invalid_parameter;
                        resp.Message = nameof(Status.invalid_parameter);
                        resp.Data = "";
                    }
                    else if (postArticle.UserId != userId)
                    {
                        resp.StatusCode = Status.permission_denied;
                        resp.Message = nameof(Status.permission_denied);
                        resp.Data = "";
                    }
                    else
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, connection);
                        adapter.SelectCommand.Parameters.AddRange(sqlParameters);
                        DataSet data = new DataSet();
                        adapter.Fill(data);
                        resp.StatusCode = Status.success;
                        resp.Message = nameof(Status.success);
                        resp.Data = "";
                    }
                }
            }
            catch (Exception)
            {
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }

        // DELETE: api/Articles/5
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpDelete("{id}")]
        public ResponseFormat<string> DeleteArticle(int id)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    // JwtHelperAuth Auth = new JwtHelperAuth(_httpContextAccessor);
                    // var user = Auth.Authorization();
                    int roleId = (int)HttpContext.Items["roleId"];
                    int userId = (int)HttpContext.Items["id"];
                    string sqlStr1 = @"
                        SELECT [Id] 
                        FROM [F62ND_Test].[dbo].[ArticleChing] 
                        WHERE [Id] = @Id";
                    string sqlStr2 = "";
                    if (roleId == 1)
                    {
                        sqlStr2 = @"
                        DELETE 
                        FROM [F62ND_Test].[dbo].[ArticleChing] 
                        WHERE [Id] = @Id";
                    }
                    else if (roleId == 2)
                    {
                        sqlStr2 = @"
                        DELETE 
                        FROM [F62ND_Test].[dbo].[ArticleChing] 
                        WHERE [Id] = @Id
                        AND [User_Id] = @UserId";
                    }
                    else
                    {
                        resp.StatusCode = Status.permission_denied;
                        resp.Message = nameof(Status.permission_denied);
                        resp.Data = "";
                        return resp;
                    }
                    SqlParameter[] sqlParameters = new[]
                    {
                            new SqlParameter("@Id", id ),
                            new SqlParameter("@UserId", userId )
                        };
                    SqlDataAdapter adapter1 = new SqlDataAdapter(sqlStr1, connection);
                    adapter1.SelectCommand.Parameters.AddRange(sqlParameters);
                    DataSet data = new DataSet();
                    adapter1.Fill(data);
                    string temp = JsonConvert.SerializeObject(data.Tables[0], Formatting.Indented);
                    List<GetArticleId>? rows = JsonConvert.DeserializeObject<List<GetArticleId>>(temp);
                    adapter1.SelectCommand.Parameters.Clear();
                    if (rows == null)
                    {
                        resp.StatusCode = Status.data_not_found;
                        resp.Message = nameof(Status.data_not_found);
                        resp.Data = "";
                    }
                    else
                    {
                        if (rows[0] == null)
                        {
                            resp.StatusCode = Status.data_not_found;
                            resp.Message = nameof(Status.data_not_found);
                            resp.Data = "";
                        }
                        else
                        {
                            SqlDataAdapter adapter2 = new SqlDataAdapter(sqlStr2, connection);
                            adapter2.SelectCommand.Parameters.AddRange(sqlParameters);
                            DataSet data2 = new DataSet();
                            adapter2.Fill(data2);
                            resp.StatusCode = Status.success;
                            resp.Message = nameof(Status.success);
                            resp.Data = "";
                        }
                    }

                }
            }
            catch (Exception)
            {
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }
    }
}
