namespace MathVerse.Web;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    /// <summary>
    /// Creates the host builder.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The host builder.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
