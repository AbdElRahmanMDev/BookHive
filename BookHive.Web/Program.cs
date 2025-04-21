

using BookHive.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BookHive.Web.Core.Mapping;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using BookHive.Web.consts;
using BookHive.Web.Seeds;
using BookHive.Web.TagHelpers;
using BookHive.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using BookHive.Web.Settings;
using Microsoft.AspNetCore.DataProtection;
using Hangfire;
using Hangfire.Dashboard;
using BookHive.Web.Tasks;
namespace BookHive.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

                 

            //builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();
            builder.Services.Configure<IdentityOptions>(options =>
            {
                //// Default Lockout settings.
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                //options.Lockout.MaxFailedAccessAttempts = 5;
            });

            builder.Services.AddDataProtection().SetApplicationName(nameof(BookHive));
            builder.Services.AddTransient<IImageService, ImageService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));   
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
            builder.Services.AddExpressiveAnnotations();
            builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            builder.Services.AddHangfireServer();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("adminsOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(AppRoles.Admin);
                });
            });
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>options.ValidationInterval= TimeSpan.Zero);
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

          

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

            HangFireTask hangFireTask = new HangFireTask(dbcontext,emailBody,emailSender);
            RecurringJob.AddOrUpdate(() => hangFireTask.PrepareExpirationAlert(), "0 14 * * *");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}