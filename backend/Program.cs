#region Usings

using LudumDareTools;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

using System.Net;

using CultureInfo = System.Globalization.CultureInfo;

#endregion

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder();
var root = builder.Environment.ContentRootPath;
var app = builder.Build();

var fileProvider = new PhysicalFileProvider(Path.Combine(root, "frontend"));

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = fileProvider
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = ""
});

#region POST /api/login

app.MapPost("/api/login", async (HttpContext context) =>
{
    string username = context.Request.Form["username"];
    string password = context.Request.Form["password"];

    using var httpClient = Utils.CreateHttpClient();
    var (message, cookies) = await httpClient.Post("user/login", ("login", username), ("pw", password));

    if (message.StatusCode != HttpStatusCode.OK || cookies.Length == 0)
        return Results.Unauthorized();

    var sids = cookies[0].Split(";")[0].Trim();
    if (sids.Length <= Constants.TOKEN_PREFIX.Length)
        return Results.Unauthorized();

    return Results.Text(sids[Constants.TOKEN_PREFIX.Length..]);
});

#endregion

#region POST /api/logout

app.MapPost("/api/logout", async (HttpContext context) =>
{
    string token = context.Request.Form["token"];

    using var httpClient = Utils.CreateHttpClient(token);
    var (message, _) = await httpClient.Post("user/logout");

    if (message.StatusCode != HttpStatusCode.OK)
        return Results.StatusCode((int) message.StatusCode);

    return Results.Text(token);
});

#endregion

#region POST /api/ld{number}

app.MapPost("/api/ld{number}", async (HttpContext context, int number) =>
{
    var runtimeData = App.instance;

    var ld = await runtimeData.GetLD(number);
    if (ld is null)
        return Results.NoContent();

    var @event = await runtimeData.GetEvent(ld.id);
    if (@event is null)
        return Results.NoContent();

    var games = await runtimeData.GetGames(@event);
    if (games is null)
        return Results.NoContent();

    var users = await runtimeData.GetUsers(games);
    if (users is null)
        return Results.NoContent();

    runtimeData.ApplyUsers(games, users);

    foreach (var game in games)
    {
        game.rating = new();
        game.userComments = 0;
    }

    string token = context.Request.Form?["token"];
    if (!string.IsNullOrEmpty(token))
    {
        var myGrades = await ld.GetMyGrades(token);
        var myComments = await ld.GetMyComments(token);
        runtimeData.ApplyGrades(games, myGrades);
        runtimeData.ApplyComments(games, myComments);
    }

    bool filterJam = context.Request.Form?["filterJam"].FirstOrDefault() == "true";
    bool filterCompo = context.Request.Form?["filterCompo"].FirstOrDefault() == "true";
    bool filterRated = context.Request.Form?["filterRated"].FirstOrDefault() == "true";
    bool filterUnrated = context.Request.Form?["filterUnrated"].FirstOrDefault() == "true";

    IEnumerable<LD_Game> result = games;

    if (!filterJam)
        result = result.Where(x => x.subsubtype != "jam");

    if (!filterCompo)
        result = result.Where(x => x.subsubtype != "compo");

    if (!filterRated)
        result = result.Where(x => x.rating.GetTotal() == 0);

    if (!filterUnrated)
        result = result.Where(x => x.rating.GetTotal() != 0);

    string search = context.Request.Query["search"].FirstOrDefault() ?? "";

    if (!string.IsNullOrWhiteSpace(search))
    {
        result = result.Where(x =>
        {
            var groups = search
                .Split('|')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var and = groups.Count > 1;

            foreach (var group in groups)
            {
                var parts = group
                    .Split(' ')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                foreach (var part in parts)
                {
                    if (
                        x.name.Has(part) ||
                        (x.user?.name ?? "").Has(part) ||
                        x.body.Has(part)
                    )
                    {
                        if (!and)
                            return true;
                    }
                    else if (and)
                        goto Next;
                }

                return and;
            Next:;
            }

            return false;
        });
    }

    string orderCategory = context.Request.Form?["orderCategory"].FirstOrDefault();

    switch (orderCategory)
    {
        case "smart":
            result = result.OrderByDescending(x => x.magic.smart);
            break;

        case "top":
            result = result.OrderByDescending(x => x.averageRating);
            break;

        case "overall":
            result = result.OrderByDescending(x => x.rating.overall.value);
            break;

        case "fun":
            result = result.OrderByDescending(x => x.rating.fun.value);
            break;

        case "innovation":
            result = result.OrderByDescending(x => x.rating.innovation.value);
            break;

        case "theme":
            result = result.OrderByDescending(x => x.rating.theme.value);
            break;

        case "graphics":
            result = result.OrderByDescending(x => x.rating.graphics.value);
            break;

        case "audio":
            result = result.OrderByDescending(x => x.rating.audio.value);
            break;

        case "humor":
            result = result.OrderByDescending(x => x.rating.humor.value);
            break;

        case "mood":
            result = result.OrderByDescending(x => x.rating.mood.value);
            break;
    }

    _ = int.TryParse(context.Request.Query["page"].FirstOrDefault(), out int page);

    result = result.Skip(page * 20).Take(20).ToList();

    return Results.Json(new
    {
        rateProgress = games.Sum(x => x.rateProgress),
        games = result.ToList()
    }, Utils.GetJsonOptions());
});

#endregion

#region POST /api/rate

app.MapPost("/api/rate", async (HttpContext context) =>
{
    string token = context.Request.Form?["token"];
    var name = context.Request.Form?["name"].FirstOrDefault();

    switch (name)
    {
        case "overall": name = "grade-01"; break;
        case "fun": name = "grade-02"; break;
        case "innovation": name = "grade-03"; break;
        case "theme": name = "grade-04"; break;
        case "graphics": name = "grade-05"; break;
        case "audio": name = "grade-06"; break;
        case "humor": name = "grade-07"; break;
        case "mood": name = "grade-08"; break;
    }

    var grade = new LD_Grade
    {
        id = long.Parse(context.Request.Form?["id"].FirstOrDefault()),
        name = name,
        value = float.Parse(context.Request.Form?["value"].FirstOrDefault())
    };

    if (grade.value == 0)
        await App.instance.RemoveGrade(token, grade);
    else
        await App.instance.AddGrade(token, grade);
});

#endregion

#region GET /api/thumbnail/{gameId}

app.MapGet("/api/thumbnail/{gameId}", async (int gameId) =>
{
    var path = Path.Combine("data", "thumbnails");
    Directory.CreateDirectory(path);

    var file = new FileInfo(Path.Combine(path, $"{gameId}.jpg"));
    if (!file.Exists)
    {
        var game = await App.instance.GetGame(gameId);
        if (game?.static_cover is not null)
        {
            using var httpClient = Utils.CreateHttpClient();
            var response = await httpClient.GetAsync(game.static_cover);

            using var stream = response.Content.ReadAsStream();
            using var bitmap = Utils.CreateBitmap(stream, new()
            {
                maxWidth = 640,
                maxHeight = 480,
                crop = true
            });
            using var data = bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 90);
            File.WriteAllBytes(file.FullName, data.ToArray());
            file.Refresh();
        }
    }

    if (!file.Exists)
        return Results.NoContent();

    return Results.File(file.FullName, "image/jpeg");
});

#endregion

var task = app.RunAsync();

App.instance.StartBackgroundTask();
await task;
App.instance.StopBackgroundTask();