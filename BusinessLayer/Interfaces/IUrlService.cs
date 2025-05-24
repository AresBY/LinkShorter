using LinkShorter.Business.Interfaces;
using LinkShorter.Business.Models;
using LinkShorter.Data.Models;

namespace LinkShorter.Business.Interfaces
{
    public interface IUrlService : IBaseService<UrlBl, UrlDl>
    {
        Task<bool> OnDeleteAsync(int id);
        Task<IEnumerable<UrlBl>> GetDataAsync();
        Task<UrlBl> GetEditPressAsync(int id);
        Task<UrlBl> OnCreateOrFindExistAsync(UrlBl data);
        Task<bool> OnUpdateAsync(UrlBl data);
        Task<string> GenerateShortUrlAsync();
        bool IsUrl(string url);
        Task<string> GetLongUrlAndIncreaseCounter(string shortUrl);

        Task<UrlBl> GetByLongUrlAsync(string longUrl);
    }
}
