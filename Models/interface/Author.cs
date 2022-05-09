namespace AuthorApi.Models
{
    public class GetAuthorOverview
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        // public int AdminId { get; set; }
    }

    public class PutAuthor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int AdminId { get; set; }
    }
    public class GetAuthorId
    {
        public int Id { get; set; }
    }
}
