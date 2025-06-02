using Microsoft.EntityFrameworkCore;
using EduSyncwebapi.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using EduSyncwebapi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Configure file upload size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Add EF Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Enable CORS for React frontend (http://localhost:3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
          
            "https://witty-smoke-07babad00.6.azurestaticapps.net"  // Add your deployed frontend URL
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<EventHubService>();

var app = builder.Build();

// ✅ Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Use HTTPS, CORS, Auth, Controllers
app.UseHttpsRedirection();

// Enable static file serving
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{ctx.File.Name}\"";
        }
    }
});

app.UseCors("AllowFrontend"); // Must go before UseAuthorization

app.UseAuthorization();

app.MapControllers();

app.Run();
