using AutoMapper;
using LinkShorter.Business.Interfaces;
using LinkShorter.Business.Models;
using LinkShorter.Presentation.Controllers;
using LinkShorter.Presentation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LinkShorter.Tests
{
    public class UrlControllerTests
    {
        private readonly Mock<IUrlService> _mockUrlService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly UrlController _controller;

        public UrlControllerTests()
        {
            _mockUrlService = new Mock<IUrlService>();
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
         
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost");
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            _controller = new UrlController(
                _mockUrlService.Object,
                _mockMapper.Object,
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithData()
        {
            // Arrange
            var urlBlList = new List<UrlBl> { new UrlBl { Id = 1, LongUrl = "http://example.com" } };
            var urlPlList = new List<UrlPl> { new UrlPl { Id = 1, LongUrl = "http://example.com" } };

            _mockUrlService.Setup(s => s.GetDataAsync()).ReturnsAsync(urlBlList);
            _mockMapper.Setup(m => m.Map<IEnumerable<UrlBl>, IEnumerable<UrlPl>>(It.IsAny<IEnumerable<UrlBl>>())).Returns(urlPlList);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<(IEnumerable<UrlPl>, string)>(viewResult.Model);

            Assert.Equal(urlPlList, model.Item1);
            Assert.Equal("http://localhost/", model.Item2);
        }


        [Fact]
        public async Task EditCreatePressAsync_ReturnsViewResult_WithEditModel()
        {
            // Arrange
            var urlBl = new UrlBl { Id = 1, LongUrl = "http://example.com" };
            var urlPl = new UrlPl { Id = 1, LongUrl = "http://example.com" };

            _mockUrlService.Setup(s => s.GetEditPressAsync(1)).ReturnsAsync(urlBl);
            _mockMapper.Setup(m => m.Map<UrlBl, UrlPl>(urlBl)).Returns(urlPl);

            // Act
            var result = await _controller.EditCreatePressAsync(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<(UrlPl, string)>(viewResult.Model);

            Assert.Equal(urlPl, model.Item1);
            Assert.Equal("http://localhost/", model.Item2);
        }


        [Fact]
        public async Task OnCreateUpdateAsync_ReturnsViewResult_OnSuccess()
        {
            // Arrange
            var urlPl = new UrlPl { Id = 1, LongUrl = "http://example.com" };
            var urlBl = new UrlBl { Id = 1, LongUrl = "http://example.com" };

            _mockUrlService.Setup(s => s.IsUrl(urlPl.LongUrl)).Returns(true);
            _mockUrlService.Setup(s => s.OnCreateOrFindExistAsync(It.Is<UrlBl>(bl => bl.Id == urlBl.Id))).ReturnsAsync(urlBl);
            _mockMapper.Setup(m => m.Map<UrlPl, UrlBl>(urlPl)).Returns(urlBl);
            _mockMapper.Setup(m => m.Map<UrlBl, UrlPl>(urlBl)).Returns(urlPl);

            // Act
            var result = await _controller.OnCreateUpdateAsync(urlPl);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<(UrlPl, string)>(viewResult.Model);

            Assert.Equal(urlPl, model.Item1);
            Assert.Equal("http://localhost/", model.Item2);
        }


        [Fact]
        public async Task OnCreateUpdateAsync_ReturnsView_WithModelError_OnInvalidUrl()
        {
            // Arrange
            var urlPl = new UrlPl { LongUrl = "invalid-url" };

            _mockUrlService.Setup(s => s.IsUrl(urlPl.LongUrl)).Returns(false);

            // Act
            var result = await _controller.OnCreateUpdateAsync(urlPl);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditCreate", viewResult.ViewName);

            var modelState = _controller.ModelState;
            Assert.False(modelState.IsValid);
            Assert.True(modelState.ContainsKey(nameof(urlPl.LongUrl)));
        }


        [Fact]
        public async Task OnDeleteAsync_ReturnsRedirectToAction_OnSuccess()
        {
            // Arrange
            _mockUrlService.Setup(s => s.OnDeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.OnDeleteAsync(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task OnDeleteAsync_ReturnsStatusCode500_OnFailure()
        {
            // Arrange
            _mockUrlService.Setup(s => s.OnDeleteAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _controller.OnDeleteAsync(1);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ShortUrlRequest_ReturnsRedirect_OnSuccess()
        {
            // Arrange
            var shortUrl = "shortUrl";
            var longUrl = "http://example.com";

            _mockUrlService.Setup(s => s.GetLongUrlAndIncreaseCounter(shortUrl)).ReturnsAsync(longUrl);

            // Act
            var result = await _controller.ShortUrlRequest(shortUrl) as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(longUrl, result.Url);
        }

        [Fact]
        public async Task ShortUrlRequest_ReturnsBadRequest_OnInvalidUrl()
        {
            // Act
            var result = await _controller.ShortUrlRequest(null) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Не корректная сокращенная ссылка", result.Value);
        }

        [Fact]
        public async Task ShortUrlRequest_ReturnsNotFound_OnNotFoundUrl()
        {
            // Arrange
            var shortUrl = "shortUrl";

            _mockUrlService.Setup(s => s.GetLongUrlAndIncreaseCounter(shortUrl)).ReturnsAsync((string)null);

            // Act
            var result = await _controller.ShortUrlRequest(shortUrl) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Полный Url не найден в БД", result.Value);
        }
    }
}
