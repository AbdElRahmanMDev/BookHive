using BookHive.Application.Common.Interfaces;
using BookHive.Application.Services;
using BookHive.Application.Services.Books;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHive.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationservices(this IServiceCollection services)
        {
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IbookService, BookService>();
            return services;
        }
    }
}
