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
    string username = context.GetFormString("username");
    string password = context.GetFormString("password");

    using var httpClient = context.CreateHttpClient();
    var response = await httpClient.Post("user/login", ("login", username), ("pw", password));

    var cookies = response.GetCookies();

    if (response.StatusCode != HttpStatusCode.OK || cookies.Count == 0)
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
    using var httpClient = context.CreateHttpClient();
    var response = await httpClient.Post("user/logout");

    if (response.StatusCode != HttpStatusCode.OK)
        return Results.StatusCode((int) response.StatusCode);

    return Results.Ok();
});

#endregion

#region POST /api/ld{number}

app.MapPost("/api/ld{number}", async (HttpContext context, int number) =>
{
    var ludumDare = await Runtime.instance.GetLudumDare(number);
    if (ludumDare is null)
        return Results.NoContent();

    var @event = await ludumDare.GetEvent();
    if (@event is null)
        return Results.NoContent();

    var games = await @event.GetGames();
    if (games is null)
        return Results.NoContent();

    var users = await games.GetUsers();
    if (users is null)
        return Results.NoContent();

    games.FillUsers(users);

    games.ClearGrades();
    games.ClearComments();

    users.ClearSettings();

    LD_User user = null;
    LD_User.Settings settings = new();

    user = await context.GetUser();
    if (user is not null)
    {
        user.LoadSettings();
        settings = user.settings;

        var myGrades = await context.GetMyGradesOf(ludumDare);
        var myComments = await context.GetMyCommentsOf(ludumDare);
        games.FillGrades(myGrades);
        games.FillComments(myComments);
    }

    IEnumerable<LD_Game> result = games;

    if (!settings.options.filterJam)
        result = result.Where(x => x.subsubtype != "jam");

    if (!settings.options.filterCompo)
        result = result.Where(x => x.subsubtype != "compo");

    if (!settings.options.filterRated)
        result = result.Where(x => x.rating.GetTotal() == 0);

    if (!settings.options.filterUnrated)
        result = result.Where(x => x.rating.GetTotal() != 0);

    var search = context.GetFormString("search");

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

    if (settings.options.filterOnlyFavorites)
        result = result.Where(x => settings.favoriteGameIds.Contains(x.id));

    switch (settings.options.orderCategory)
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

    _ = int.TryParse(context.Request.Query["page"], out int page);

    result = result
        .Skip(page * Constants.LD_PAGE_CAPACITY)
        .Take(Constants.LD_PAGE_CAPACITY)
        .ToList();

    return Results.Json(new
    {
        user,
        rateProgress = games.Sum(x => x.rateProgress),
        games = result.ToList()
    }, Utils.GetJsonOptions());
});

#endregion

#region POST /api/rate

app.MapPost("/api/rate", async (HttpContext context) =>
{
    var name = context.GetFormString("name");

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
        id = context.GetFormLong("id"),
        name = name,
        value = context.GetFormFloat("value")
    };

    if (grade.value == 0)
    {
        await context.RemoveGrade(grade);
        return;
    }

    await context.AddGrade(grade);
});

#endregion

#region POST /api/options/get

app.MapPost("/api/options/get", async (HttpContext context) =>
{
    var options = await context.GetSettings(x => x.options);
    return Results.Json(options, Utils.GetJsonOptions());
});

#endregion

#region POST /api/options/set

app.MapPost("/api/options/set", async (HttpContext context) =>
{
    return await context.ModifySettings(settings =>
    {
        var o = settings.options;
        o.filterOnlyFavorites = context.GetFormBoolean("filterOnlyFavorites");
        o.filterJam = context.GetFormBoolean("filterJam");
        o.filterCompo = context.GetFormBoolean("filterCompo");
        o.filterRated = context.GetFormBoolean("filterRated");
        o.filterUnrated = context.GetFormBoolean("filterUnrated");
        o.orderCategory = context.GetFormString("orderCategory");
    });
});

#endregion

#region POST /api/favorite/add

app.MapPost("/api/favorite/add", async (HttpContext context) =>
{
    await context.ModifyFavoriteGames((favoriteGameIds, gameId) =>
    {
        if (!favoriteGameIds.Contains(gameId))
            favoriteGameIds.Add(gameId);
    });
});

#endregion

#region POST /api/favorite/remove

app.MapPost("/api/favorite/remove", async (HttpContext context) =>
    await context.ModifyFavoriteGames((favoriteGameIds, gameId) =>
    {
        favoriteGameIds.RemoveAll(x => x == gameId);
    })
);

#endregion

#region GET /api/thumbnail/{gameId}

app.MapGet("/api/thumbnail/{gameId}", async (HttpContext context, int gameId) =>
{
    var path = Path.Combine("data", "thumbnails");
    Directory.CreateDirectory(path);

    var file = new FileInfo(Path.Combine(path, $"{gameId}.jpg"));
    if (!file.Exists)
    {
        var game = await Cache.games.Get(gameId);
        if (game?.static_cover is not null)
        {
            using var httpClient = context.CreateHttpClient();
            var response = await httpClient.GetAsync(game.static_cover);

            using var stream = response.Content.ReadAsStream();
            using var bitmap = Utils.CreateBitmap(stream, new()
            {
                maxWidth = Constants.THUMBNAIL_WIDTH,
                maxHeight = Constants.THUMBNAIL_HEIGHT,
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

await Runtime.instance.Execute(app);