using LinkUp.Application.Interfaces;
using LinkUp.Shared.Options;
using LinkUp.Shared.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Las imágenes subidas se guardan en wwwroot con nombres generados por el servidor.
builder.Services.AddSingleton<IImageStorageService>(new LocalImageStorageService(
    new ImageStorageOptions
    {
        StorageDirectory = Path.Combine(
            builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot"),
            "uploads", "images")
    }));

// El correo se configura externamente (User Secrets o variables de entorno).
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddTransient<IEmailService>(serviceProvider =>
    new SmtpEmailService(serviceProvider.GetRequiredService<IOptions<SmtpOptions>>().Value));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// MapStaticAssets sirve los recursos compilados; UseStaticFiles permite servir archivos subidos en ejecución.
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
