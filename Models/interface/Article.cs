using System.ComponentModel.DataAnnotations;

namespace ArticleApi.Models
{
    public class GetArticleOverview
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
    }

    public class GetArticleDetail
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public class GetArticleId
    {
        public int Id { get; set; }
    }

    public class PutArticle
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }

    }

    public class PostArticle
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Content { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int AdminId { get; set; }
        [Required]
        public DateTime UpdateTime { get; set; }
    }
    public class Role
    {
        public int Id { get; set; }
        public string? Name { get; set; }

    }
}


