namespace ArticleApi.Models
{
    public class ResponseFormat<T>
    {
        public Status StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}