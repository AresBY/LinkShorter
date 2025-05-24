using AutoMapper;
using LinkShorter.Business.Interfaces;
using LinkShorter.Business.Models;
using LinkShorter.Data.Models;
using LinkShorter.Data.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace LinkShorter.Business.Implementations
{
    public class UrlService : BaseService<UrlBl, UrlDl>, IUrlService
    {
        private readonly IUrlRepository _urlRepository;
        private readonly IConfiguration _configuration;
        private readonly int _generationAttempts;
        private readonly int _shortUrlLength;
        public UrlService(IUrlRepository urlRepository, IMapper mapper, IConfiguration configuration) : base(urlRepository, mapper)
        {
            _urlRepository = urlRepository;
            _configuration = configuration;
            _generationAttempts = Convert.ToInt32(_configuration["Settings:GenerationAttempts"]);
            _shortUrlLength = Convert.ToInt32(_configuration["Settings:ShortUrlLength"]);
        }

        public async Task<bool> OnDeleteAsync(int id)
        {
            return await DeleteAsync(id);
        }

        public async Task<IEnumerable<UrlBl>> GetDataAsync()
        {
            return await GetAllAsync();
        }

        public async Task<UrlBl> GetEditPressAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<UrlBl> OnCreateOrFindExistAsync(UrlBl data)
        {
            var exist = await GetByLongUrlAsync(data.LongUrl);
            if (exist != null)
                return exist;

            if (data.Id > 0)
            {
                await DeleteAsync(data.Id);
            }

            UrlBl urlBl = new UrlBl();
            urlBl.LongUrl = data.LongUrl;
            urlBl.ShortUrl = await GenerateShortUrlAsync();
            urlBl.Creation = DateTime.Now;

            bool add = await AddAsync(urlBl);
            return add ? urlBl : null;
        }

        public async Task<bool> OnUpdateAsync(UrlBl data)
        {
            return await UpdateAsync(data);
        }
        public async Task<string> GetLongUrlAndIncreaseCounter(string shortUrl)
        {
            var data = await _urlRepository.GetItemByShortUrl(shortUrl);
            if (data == null) return null;

            data.TransitionCount += 1;
            await _urlRepository.UpdateAsync(data);
            return data.LongUrl;
        }
        public bool IsUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        public async Task<string> GenerateShortUrlAsync()
        {
            const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string result = null;
            int countIteration = 0;
            do
            {
                if (countIteration++ > _generationAttempts)
                {
                    throw new Exception($"Кол-во итераций цикла в  UrlService.GenerateShortUrl больше {_generationAttempts}. Вероятен бесконечный цикл");
                }
                Random random = new Random();
                StringBuilder shortUrlBuilder = new StringBuilder();
                for (int i = 0; i < _shortUrlLength; i++)
                {
                    int randomIndex = random.Next(Characters.Length);
                    shortUrlBuilder.Append(Characters[randomIndex]);
                }
                result = shortUrlBuilder.ToString();
            }
            while (await _urlRepository.ShortUrlExist(result));

            return result;
        }

        public async Task<UrlBl> GetByLongUrlAsync(string longUrl)
        {
            var content = await _urlRepository.GetByLongUrlAsync(longUrl);
            return _mapper.Map<UrlDl, UrlBl>(content);
        }
    }
}
