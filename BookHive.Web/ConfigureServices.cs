
using BookHive.Web.Controllers;
using BookHive.Web.Core.Mapping;
using BookHive.Web.Services;
using BookHive.Web.TagHelpers;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using BookHive.Domain.Common;
using FluentValidation;
using BookHive.Web.Validators;
using FluentValidation.AspNetCore;
namespace BookHive.Web
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
             {
                 //// Default Lockout settings.
                 //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                 //options.Lockout.MaxFailedAccessAttempts = 5;
             });

            services.AddDataProtection().SetApplicationName(nameof(BookHive));
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
            services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            services.AddControllersWithViews();
            services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
            services.AddExpressiveAnnotations();
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();
            services.AddAuthorization(options =>
             {
                 options.AddPolicy("adminsOnly", policy =>
                 {
                     policy.RequireAuthenticatedUser();
                     policy.RequireRole(AppRoles.Admin);
                 });
             });
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            //services.AddScoped<IValidator<AuthorFormViewModel>, AuthorValidator>();
            ////AddScoped<Ivalidator<ViewModelClass>,validatorClass>()
            //services.AddScoped<IValidator<BookCopyFormViewModel>, BookCopyValidator>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());    
            services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
            services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));

            return services;
        }
    }
}
