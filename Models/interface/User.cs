using System.ComponentModel.DataAnnotations;

namespace UserApi.Models
{
    public class TokenRecord
    {
        public int UserId { get; set; }
        public string? Token { get; set; }
        public DateTime UpdateTime { get; set; }

        // public int AdminId { get; set; }
    }

    public class Login
    {
        [Required]
        public string? Account { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int RoleId { get; set; }
        // public int AdminId { get; set; }
    }
    public class UserId
    {
        public int Id { get; set; }
    }

    public class JwtUser
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? RoleId { get; set; }
        // public int AdminId { get; set; }
    }

}