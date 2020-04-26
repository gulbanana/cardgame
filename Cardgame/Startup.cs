using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cardgame.ClientServer;
using Cardgame.Model.ClientServer;

namespace Cardgame
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var cardImplementations = typeof(Cards.CardBase).Assembly;
            All.Cards.Init(cardImplementations);
            All.Effects.Init(cardImplementations);
            All.Mats.Init(cardImplementations);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<UserManager>();
            services.AddSingleton<IUserEndpoint, NameclaimUserEndpoint>();

            services.AddScoped<IUserSession, NameclaimUserSession>();
            services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IUserSession>() as AuthenticationStateProvider);

            services.AddSingleton<SharedLobbyEndpoint>();
            services.AddTransient<ILobbyEndpoint>(s => s.GetRequiredService<SharedLobbyEndpoint>());
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
