using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalesWebMvc.Data;
namespace SalesWebMvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<SalesWebMvcContext>(options =>
                options.UseMySql(builder.Configuration.GetConnectionString("SalesWebMvcContext") ?? throw new InvalidOperationException("Connection string 'SalesWebMvcContext' not found."),
                    new MySqlServerVersion(new Version(8, 0, 32))));

            builder.Services.AddScoped<SeedingService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute
                (
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
                );


            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            SeedDatabase(app);

            app.Run();
        }

        private static void SeedDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            try
            {
                SeedingService seedingService = services.GetRequiredService<SeedingService>();
                seedingService.Seed();
            }
            catch (Exception ex)
            {
                ILogger<IStartup> logger = services.GetRequiredService<ILogger<IStartup>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
