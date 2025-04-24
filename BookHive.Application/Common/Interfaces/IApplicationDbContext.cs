using BookHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHive.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Area> Areas { get; set; }
        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }



        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }

        public DbSet<Category> categories { get; set; }

        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }

        public DbSet<Subscribtion> Subscribtions
        {
            get; set;
        }
        public DbSet<Rental> Rentals { get; set; }

        public DbSet<RentalCopy> RentalCopies { get; set; }

        int SaveChanges();
    }
}
