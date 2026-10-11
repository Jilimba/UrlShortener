
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UrlShortener.LinkForge.LinkForge.Domain.Entities;
using UrlShortener.LinkForge.LinkForge.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register the database context.
builder.Services.AddDbContext<LinkForgeDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LinkForge")));

// Configure Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health check.
app.MapGet("/health", () =>
    Results.Ok(new { status = "Healthy" }));

// Create a short link.
app.MapPost("/api/links", async (
    CreateShortlinkRequest request,
    LinkForgeDbContext db,
    HttpContext httpContext) =>
{
    if (string.IsNullOrWhiteSpace(request.OriginalUrl) ||
        !Uri.TryCreate(
            request.OriginalUrl,
            UriKind.Absolute,
            out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp &&
         uri.Scheme != Uri.UriSchemeHttps))
    {
        return Results.BadRequest(new
        {
            message = "Please provide a valid HTTP or HTTPS URL."
        });
    }

    string code;

    do
    {
        code = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(8))
            .ToLowerInvariant();
    }
    while (await db.Shortlinks.AnyAsync(link => link.Code == code));

    var link = new Shortlink
    {
        OriginalUrl = uri.ToString(),
        Code = code,
        CreatedAtUtc = DateTime.UtcNow
    };

    db.Shortlinks.Add(link);
    await db.SaveChangesAsync();

    var shortUrl =
        $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{link.Code}";

    return Results.Created(
        $"/{link.Code}",
        new
        {
            link.Id,
            link.OriginalUrl,
            link.Code,
            shortUrl,
            link.CreatedAtUtc
        });
});

// Redirect a short link to its original URL.
app.MapGet("/{code}", async (
    string code,
    LinkForgeDbContext db) =>
{
    var link = await db.Shortlinks
        .SingleOrDefaultAsync(link => link.Code == code);

    if (link is null)
    {
        return Results.NotFound(new
        {
            message = "Short link not found."
        });
    }

    return Results.Redirect(link.OriginalUrl);
});

app.Run();

// Request model for creating a short link.
public record CreateShortlinkRequest(string OriginalUrl);

