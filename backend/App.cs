using System.Threading;

namespace LudumDareTools;

public class App
{
    public static App instance { get; } = new();

    CancellationTokenSource backgroundTaskTokenSource;

    readonly List<LudumDare> ld = new();

    public readonly LD_NodeCollection<LD_Game> games = new();
    public readonly LD_NodeCollection<LD_User> users = new();
    public readonly LD_EventCollection events = new();

    public async Task<LudumDare> GetLD(int number)
    {
        if (!ld.Any(x => x.number == number))
        {
            using var httpClient = Utils.CreateHttpClient();
            var (data, _) = await httpClient.Get($"node2/walk/1/events/ludum-dare/{number}");
            ld.Add(new LudumDare
            {
                id = data is null ? 0 : data.node_id,
                number = number
            });
        }
        return ld.Single(x => x.number == number);
    }

    public async Task<LD_Event> GetEvent(long ldId)
    {
        return (await events.Get(new List<long>() { ldId })).FirstOrDefault();
    }

    public async Task<List<LD_Game>> GetGames(LD_Event @event)
    {
        return await games.Get(@event.feeds.Select(x => x.id).Distinct());
    }

    public async Task<LD_Game> GetGame(long id)
    {
        return await games.Get(id);
    }

    public async Task<List<LD_User>> GetUsers(List<LD_Game> games)
    {
        return await users.Get(games.Select(x => x.author).Distinct());
    }

    public void ApplyUsers(List<LD_Game> games, List<LD_User> users)
    {
        foreach (var game in games)
            game.user = users.FirstOrDefault(x => x.id == game.author);
    }

    public void ApplyGrades(List<LD_Game> games, List<LD_Grade> grades)
    {
        foreach (var grade in grades)
        {
            var game = games.FirstOrDefault(x => x.id == grade.id);
            if (game is null)
                continue;

            switch (grade.name)
            {
                case "grade-01": game.rating.overall = grade; break;
                case "grade-02": game.rating.fun = grade; break;
                case "grade-03": game.rating.innovation = grade; break;
                case "grade-04": game.rating.theme = grade; break;
                case "grade-05": game.rating.graphics = grade; break;
                case "grade-06": game.rating.audio = grade; break;
                case "grade-07": game.rating.humor = grade; break;
                case "grade-08": game.rating.mood = grade; break;
                default: break;
            }
        }
    }

    public void ApplyComments(List<LD_Game> games, List<LD_Comment> comments)
    {
        foreach (var game in games)
            game.userComments = comments.Count(x => x.node == game.id);
    }

    public async Task<object> AddGrade(string token, LD_Grade grade)
    {
        using var httpClient = Utils.CreateHttpClient(token);

        var (data, ex) = await httpClient.Get($"grade/add/{grade.id}/{grade.name}/{grade.value}");

        if (ex is not null)
            throw ex;

        return data;
    }

    public async Task<object> RemoveGrade(string token, LD_Grade grade)
    {
        using var httpClient = Utils.CreateHttpClient(token);

        var (data, ex) = await httpClient.Get($"grade/remove/{grade.id}/{grade.name}");

        if (ex is null)
            return data;

        return ex;
    }

    Task backgroundTask;

    public void StartBackgroundTask()
    {
        backgroundTaskTokenSource = new();
        backgroundTask = RunBackgroundTask(backgroundTaskTokenSource.Token);
        Console.WriteLine("Background Task started");
    }

    public void StopBackgroundTask()
    {
        backgroundTaskTokenSource.Cancel();
        Console.WriteLine("Background Task stopped");
    }

    async Task RunBackgroundTask(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(420_000, cancellationToken);
            Console.WriteLine("Fetching...");
            try
            {
                for (int number = 45; number <= 51; number++)
                {
                    var ld = await GetLD(number);

                    await events.Fetch(new List<long>() { ld.id });
                    var @event = await GetEvent(ld.id);

                    await this.games.Fetch(@event.feeds.Select(x => x.id).Distinct());
                    var games = await GetGames(@event);

                    await users.Fetch(games.Select(x => x.author).Distinct());
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            await Task.Delay(420_000, cancellationToken);
        }
    }
}