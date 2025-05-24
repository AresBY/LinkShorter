using LinkShorter.Data.Models;

namespace LinkShorter.Data.Repositories.Interfaces
{
    public interface IUrlRepository : IBaseRepository<UrlDl>
    {
        Task<UrlDl> GetByLongUrlAsync(string longUrl);
        Task<UrlDl> GetItemByShortUrl(string shortUrl);
        Task<bool> ItemExist(string longUrl);
        Task<bool> ShortUrlExist(string shortUrl);
    }
}
