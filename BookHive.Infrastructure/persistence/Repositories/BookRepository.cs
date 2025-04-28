using BookHive.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHive.Infrastructure.persistence.Repositories
{
    internal class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<Book> GetDetails()
        {
            return  _context.Books.Include(x => x.Author).
                Include(x => x.Copies).
                ThenInclude(x => x.Rentals).
                Include(x => x.Categories).
                ThenInclude(x => x.Category);
          
        }
    }
}
