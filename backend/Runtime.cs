using Microsoft.AspNetCore.Builder;
using System.Threading;

namespace LudumDareTools;

public class Runtime
{
    public static Runtime instance { get; } = new();

    CancellationTokenSource backgroundTaskTokenSource;

    public async Task Execute(WebApplication app)
    {
        var task = app.RunAsync();

        backgroundTaskTokenSource = new();
        _ = RunBackgroundTask(backgroundTaskTokenSource.Token);
        Console.WriteLine("Background Task started");

        await task;

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
                for (int number = Constants.MIN_LD_NUMBER; number <= Constants.MAX_LD_NUMBER; number++)
                    await this.Fetch(number);
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