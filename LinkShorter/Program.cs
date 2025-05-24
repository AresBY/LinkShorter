using LinkShorter.Business.Implementations;
using LinkShorter.Business.Interfaces;
using LinkShorter.Business.Profiles;
using LinkShorter.Data;
using LinkShorter.Data.Repositories.Implementations;
using LinkShorter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddAutoMapper(typeof(ModelProfile));
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
   
    dbContext.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "short",
        pattern: "{shortUrl}",
        defaults: new { controller = "Url", action = "ShortUrlRequest" }
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Url}/{action=Index}/{id?}");
});

app.Run();
