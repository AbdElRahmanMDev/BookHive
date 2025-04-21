using BookHive.Web.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Area> Areas { get; set; }  
        public DbSet<Author> Authors { get; set; }  

        public DbSet<Book> Books { get; set; }
        


        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }

        public DbSet<Category> categories { get; set; }

        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }

        public DbSet<Subscribtion> Subscribtions { get; set; }

        public DbSet<Rental> Rentals { get; set; }

        public DbSet<RentalCopy> RentalCopies { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Category>().HasIndex(x => x.Name).IsUnique();
            builder.Entity<Author>().HasIndex(x=> x.Name).IsUnique();

            builder.Entity<BookCategory>().HasKey(e => new { e.BookId, e.CategoryId });

            builder.Entity<Book>().HasIndex(x=> new {x.AuthorId , x.Title}).IsUnique();

            builder.Entity<RentalCopy>().HasKey(x=>new {x.RentalId , x.BookCopyId});

            builder.Entity<Rental>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<RentalCopy>().HasQueryFilter(x => !x.Rentals!.IsDeleted);

            builder.HasSequence<int>(name:"SerialNumber", schema:"shared").
                StartsAt(startValue:1000001);

            builder.Entity<BookCopy>().
                Property(x => x.SerialNumber).
                HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            var cascadeFKs = builder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetForeignKeys())
                    .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;


            base.OnModelCreating(builder);
        }
    }
}
