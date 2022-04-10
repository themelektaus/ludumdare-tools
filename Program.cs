using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.Net;

using static LudumDareTools.Utils;

var builder = WebApplication.CreateBuilder();

var root = builder.Environment.ContentRootPath;

var app = builder.Build();

app.UseDefaultFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(root, "frontend")),
    RequestPath = ""
});

app.MapPost("/api/login", async (HttpContext context) =>
{
    string username = context.Request.Form["username"];
    string password = context.Request.Form["password"];
    
    using var httpClient = CreateHttpClient();
    var (message, cookies) = await httpClient.Post("user/login", ("login", username), ("pw", password));

    if (message.StatusCode != HttpStatusCode.OK || cookies.Length == 0)
        return Results.Unauthorized();

    var sids = cookies[0].Split(";")[0].Trim();
    if (sids.Length <= TOKEN_PREFIX.Length)
        return Results.Unauthorized();

    return Results.Text(sids[TOKEN_PREFIX.Length..]);
});

app.MapPost("/api/logout", async (HttpContext context) =>
{
    string token = context.Request.Form["token"];

    using var httpClient = CreateHttpClient(token);
    var (message, _) = await httpClient.Post("user/logout");

    if (message.StatusCode != HttpStatusCode.OK)
        return Results.StatusCode((int) message.StatusCode);

    return Results.Text(token);
});

app.MapPost("/api/ld{number}/ratings", async (HttpContext context, int number) =>
{
    string token = context.Request.Form["token"];
    
    var ratings = await GetRatings(number, token);

    if (ratings is null)
        return Results.Unauthorized();

    return Results.Json(ratings, GetJsonOptions());
});

app.Run();