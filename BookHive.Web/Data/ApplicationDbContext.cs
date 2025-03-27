using BookHive.Web.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Author> Authors { get; set; }  

        public DbSet<Book> Books { get; set; }
        
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }

        public DbSet<Category> categories { get; set; }

        
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

            builder.HasSequence<int>(name:"SerialNumber", schema:"shared").
                StartsAt(startValue:1000001);

            builder.Entity<BookCopy>().
                Property(x => x.SerialNumber).
                HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            base.OnModelCreating(builder);
        }
    }
}
