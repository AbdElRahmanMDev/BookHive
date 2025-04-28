using BookHive.Domain.consts;
using BookHive.Domain.Entities;
using BookHive.Infrastructure;
using BookHive.Web.Seeds;
using BookHive.Application;
using BookHive.Web.Services;
using BookHive.Web.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Serilog;
using Serilog.Context;
namespace BookHive.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddApplicationservices()
                .AddInfrastructureservices(builder.Configuration).AddWebServices(builder);

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Host.UseSerilog();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
         

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "Deny");

                await next();
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();

            var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManger = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await DefaultRoles.SeedRolesAsync(roleManger);
            await DefaultUsers.SeedAdminUserAync(userManger);

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "BookHive Dashboard",
                Authorization = new IDashboardAuthorizationFilter[]
                {
                    new HangFireAuthorizationFilter("adminsOnly")
                }
            });

            var dbcontext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailBody = scope.ServiceProvider.GetRequiredService<IEmailBodyBuilder>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            HangFireTask hangFireTask = new HangFireTask(dbcontext, emailBody, emailSender);
            RecurringJob.AddOrUpdate(() => hangFireTask.PrepareExpirationAlert(), "0 14 * * *");

            app.Use(async (context, next) =>
            {
                LogContext.PushProperty("UserId", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                LogContext.PushProperty("UserName", context.User.FindFirst(ClaimTypes.Name)?.Value);

                await next();
            });


            app.UseSerilogRequestLogging();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}