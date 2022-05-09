using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AuthorApi.Models;
using ArticleApi.Models;
using JwtAuthDemo.Helpers;
using Filter;
using Newtonsoft.Json;
using System.Data;

namespace AuthorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtHelpers jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthorsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, JwtHelpers jwt)
        {
            this.jwt = jwt;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Authors
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpGet]
        public ResponseFormat<List<GetAuthorOverview>> GetAuthors()
        {
            ResponseFormat<List<GetAuthorOverview>> resp = new ResponseFormat<List<GetAuthorOverview>>();
            List<GetAuthorOverview> empty = new List<GetAuthorOverview>();
            int roleId = (int)HttpContext.Items["roleId"];
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    // JwtHelperAuth Auth = new JwtHelperAuth(_httpContextAccessor);
                    // var user = Auth.Authorization();
                    string sqlStr = @"
                        SELECT 
                            [Id]
                            ,[Name] 
                        FROM [F62ND_Test].[dbo].[UserChing]";

                    if (roleId != 1)
                    {
                        resp.StatusCode = Status.permission_denied;
                        resp.Message = nameof(Status.permission_denied);
                        resp.Data = empty;
                    }
                    else
                    {
                       SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, connection);
                        DataSet data = new DataSet();
                        adapter.Fill(data);
                        string temp = JsonConvert.SerializeObject(data.Tables[0], Formatting.Indented);
                        List<GetAuthorOverview>? result = JsonConvert.DeserializeObject<List<GetAuthorOverview>>(temp);
                        if ((result != null) && (result.Count > 0))
                        {
                            resp.StatusCode = Status.success;
                            resp.Message = nameof(Status.success);
                            resp.Data = result;
                        }
                        else
                        {
                            resp.StatusCode = Status.data_not_found;
                            resp.Message = nameof(Status.data_not_found);
                            resp.Data = empty;
                        } 
                    }
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


        // PUT: api/Authors/5
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPut("{id}")]
        public ResponseFormat<string> UpdateAuthors(int id, PutAuthor putAuthor)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            int roleId = (int)HttpContext.Items["roleId"];
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    // JwtHelperAuth Auth = new JwtHelperAuth(_httpContextAccessor);
                    // var user = Auth.Authorization();
                    string sqlStr1 = @"
                        SELECT [Id] 
                        FROM [F62ND_Test].[dbo].[UserChing] 
                        WHERE [Id] = @AuthorId";
                    string sqlStr2 = @"
                        UPDATE [F62ND_Test].[dbo].[UserChing] 
                        SET [Name]= @Name
                            , [AdminId]= @AdminId 
                        WHERE [Id] = @AuthorId";
                    SqlParameter[] sqlParameters = new[]
                    {
                        new SqlParameter("@Name",putAuthor.Name),
                        new SqlParameter("@AdminId",putAuthor.AdminId),
                        new SqlParameter("@AuthorId", id )
                    };

                    if (roleId != 1)
                    {
                        resp.StatusCode = Status.permission_denied;
                        resp.Message = nameof(Status.permission_denied);
                        resp.Data = "";
                    }
                    else
                    {
                        SqlDataAdapter adapter1 = new SqlDataAdapter(sqlStr1, connection);
                        adapter1.SelectCommand.Parameters.AddRange(sqlParameters);
                        DataSet data1 = new DataSet();
                        adapter1.Fill(data1);
                        string temp = JsonConvert.SerializeObject(data1.Tables[0], Formatting.Indented);
                        List<GetAuthorId>? rows = JsonConvert.DeserializeObject<List<GetAuthorId>>(temp);
                        adapter1.SelectCommand.Parameters.Clear();
                        if ((rows != null) && (rows.Count == 1))
                        {
                            SqlDataAdapter adapter2 = new SqlDataAdapter(sqlStr2, connection);
                            adapter2.SelectCommand.Parameters.AddRange(sqlParameters);
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

        // DELETE: api/Articles/5
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpDelete("{id}")]
        public ResponseFormat<string> DeleteAuthor(int id)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            int roleId = (int)HttpContext.Items["roleId"];
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DBconn");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    // JwtHelperAuth Auth = new JwtHelperAuth(_httpContextAccessor);
                    // var user = Auth.Authorization();
                    string sqlStr1 = @"
                        SELECT [Id] 
                        FROM [F62ND_Test].[dbo].[UserChing] 
                        WHERE [Id] = @Id";
                    string sqlStr2 = "";
                    if (roleId == 1)
                    {
                        sqlStr2 = @"
                        DELETE [article] 
                        FROM [F62ND_Test].[dbo].[UserChing] AS [user]
                        INNER JOIN [F62ND_Test].[dbo].[ArticleChing] AS [article]
                        ON [user].[Id] = [article].[User_Id] 
                        WHERE [user].[Id] = @Id;
                        DELETE 
                        FROM [F62ND_Test].[dbo].[UserChing] 
                        WHERE [Id] = @Id;
                        DELETE 
                        FROM [F62ND_Test].[dbo].[TokenChing] 
                        WHERE [User_Id] = @Id";
                    }
                    else
                    {
                        resp.StatusCode = Status.permission_denied;
                        resp.Message = nameof(Status.permission_denied);
                        resp.Data = "";
                    }
                    SqlParameter[] sqlParameters = new[]
                    {
                        new SqlParameter("@Id", id )
                    };
                    SqlDataAdapter adapter1 = new SqlDataAdapter(sqlStr1, connection);
                    adapter1.SelectCommand.Parameters.AddRange(sqlParameters);
                    DataSet data1 = new DataSet();
                    adapter1.Fill(data1);
                    string temp = JsonConvert.SerializeObject(data1.Tables[0], Formatting.Indented);
                    List<GetAuthorId>? rows = JsonConvert.DeserializeObject<List<GetAuthorId>>(temp);
                    adapter1.SelectCommand.Parameters.Clear();
                    if ((rows != null) && (rows.Count == 1))
                    {
                        SqlDataAdapter adapter2 = new SqlDataAdapter(sqlStr2, connection);
                        adapter2.SelectCommand.Parameters.AddRange(sqlParameters);
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
    }
}