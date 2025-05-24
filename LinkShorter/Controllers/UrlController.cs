using AutoMapper;
using LinkShorter.Business.Interfaces;
using LinkShorter.Business.Models;
using Microsoft.AspNetCore.Mvc;
using LinkShorter.Presentation.Models;

namespace LinkShorter.Presentation.Controllers
{
    public class UrlController : Controller
    {
        private readonly IUrlService _urlService;
        private readonly IMapper _mapper;
        private readonly string _absoluteUri;
        private const int DefaultPageSize = 50;
        public UrlController(IUrlService urlService, IMapper mapper, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _urlService = urlService;
            _mapper = mapper;

            var request = httpContextAccessor.HttpContext.Request;
            _absoluteUri = request.Scheme + "://" + request.Host + "/";
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize)
        {
            if (page < 1)
            {
                ModelState.AddModelError("page", "Номер страницы не может быть меньше 1.");
            }

            if (pageSize < 1)
            {
                ModelState.AddModelError("pageSize", "Размер страницы должен быть больше 0.");
            }

            if (!ModelState.IsValid)
            {
                return View((Enumerable.Empty<UrlPl>(), _absoluteUri));
            }

            var data = await _urlService.GetPagedDataAsync(page, pageSize);
            var result = _mapper.Map<IEnumerable<UrlBl>, IEnumerable<UrlPl>>(data);
            return View((result, _absoluteUri));
        }


        [HttpGet]
        public async Task<IActionResult> EditCreatePressAsync(int id)
        {
            var data = await _urlService.GetEditPressAsync(id);
            return data != null ?
                View("EditCreate", (_mapper.Map<UrlBl, UrlPl>(data), _absoluteUri)) :
                View("EditCreate", (new UrlPl(), _absoluteUri));
        }
        [HttpPost]
        public async Task<IActionResult> OnCreateUpdateAsync(UrlPl urlPl)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCreate", (urlPl, _absoluteUri));
            }

            if (!_urlService.IsUrl(urlPl.LongUrl))
            {
                ModelState.AddModelError(nameof(urlPl.LongUrl), $"Некорректный URL: {urlPl.LongUrl}");
                return View("EditCreate", (urlPl, _absoluteUri));
            }

            var result = await _urlService.OnCreateOrFindExistAsync(_mapper.Map<UrlPl, UrlBl>(urlPl));
            if (result != null)
                return View("EditCreate", (_mapper.Map<UrlBl, UrlPl>(result), _absoluteUri));
            else
                return StatusCode(500, "Внутренняя ошибка сервера: сохранение не удалось.");
        }

        [HttpGet]
        public async Task<IActionResult> OnDeleteAsync(int id)
        {
            var success = await _urlService.OnDeleteAsync(id);
            return success ? RedirectToAction("Index") : StatusCode(500, "Внутренняя ошибка сервера: удаление не удалось.");
        }

        [HttpGet]
        public async Task<IActionResult> ShortUrlRequest(string shortUrl)
        {
            if (string.IsNullOrEmpty(shortUrl)) return BadRequest("Не корректная сокращенная ссылка");

            string fullUrl = await _urlService.GetLongUrlAndIncreaseCounter(shortUrl);

            return !string.IsNullOrEmpty(fullUrl) ? Redirect(fullUrl) : NotFound("Полный Url не найден в БД");
        }
    }
}