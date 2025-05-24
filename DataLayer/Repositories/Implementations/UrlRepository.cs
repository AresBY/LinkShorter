using LinkShorter.Data.Models;
using LinkShorter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LinkShorter.Data.Repositories.Implementations
{
    public class UrlRepository : BaseRepository<UrlDl>, IUrlRepository
    {
        public UrlRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<UrlDl> GetByLongUrlAsync(string longUrl)
        {
            return await _entities.FirstOrDefaultAsync(x => x.LongUrl == longUrl);
        }

        public async Task<UrlDl> GetItemByShortUrl(string shortUrl)
        {
            return await _entities.FirstOrDefaultAsync(x => x.ShortUrl == shortUrl);
        }

        public Task<bool> ItemExist(string longUrl)
        {
            return _entities.AnyAsync(x => x.LongUrl == longUrl);
        }

        public Task<bool> ShortUrlExist(string shortUrl)
        {
            return _entities.AnyAsync(x => x.ShortUrl == shortUrl);
        }
    }
}
