using System.ComponentModel.DataAnnotations;

namespace LinkShorter.Presentation.Models
{
    public class UrlPl
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "URL обязателен")]
        [Url(ErrorMessage = "Введите корректный URL")]
        public string? LongUrl { get; set; }
        public string? ShortUrl { get; set; }
        public DateTime? Creation { get; set; }
        public int TransitionCount { get; set; }
    }
}
