#pragma warning disable IL2026

namespace MathVerse.Web;

/// <summary>
/// Application startup configuration.
/// </summary>
public class Startup
{
    /// <summary>
    /// Configures services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();
    }

    /// <summary>
    /// Configures the application.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="env">Web host environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
