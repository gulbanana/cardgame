using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cardgame.Hosting;
using Cardgame.Protocol;

namespace Cardgame
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<UserManager>();
            services.AddSingleton<IUserEndpoint, NameclaimUserEndpoint>();

            services.AddScoped<IUserSession, NameclaimUserSession>();
            services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IUserSession>() as AuthenticationStateProvider);

            services.AddSingleton<SharedGameEndpoint>();
            services.AddTransient<IGameEndpoint>(s => s.GetRequiredService<SharedGameEndpoint>());
        }

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
}
