namespace UrlShortener.LinkForge.LinkForge.Domain.Entities
{
    public class Shortlink
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string OriginalUrl { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
