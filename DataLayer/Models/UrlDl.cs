using System.ComponentModel.DataAnnotations;

namespace LinkShorter.Data.Models
{
    public class UrlDl:BaseDl
    {
        public string? LongUrl { get; set; }
        public string? ShortUrl { get; set; }
        public DateTime? Creation { get; set; }
        public int TransitionCount { get; set; }
    }
}
