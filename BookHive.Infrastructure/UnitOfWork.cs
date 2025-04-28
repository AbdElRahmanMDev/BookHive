using BookHive.Infrastructure.persistence;
using BookHive.Infrastructure.persistence.Migrations;
using BookHive.Infrastructure.persistence.Repositories;

namespace BookHive.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _contex;
        public UnitOfWork(ApplicationDbContext context)
        {
            _contex = context;
        }
        public IRepository<Author> Authors =>new Repository<Author>(_contex);
        public IRepository<Category> Categories => new Repository<Category>(_contex);
        public IRepository<BookCopy> BookCopies => new Repository<BookCopy>(_contex);
        public IBookRepository Books => new BookRepository(_contex);

        public IRepository<RentalCopy> RentalCopies => new Repository<RentalCopy>(_contex);

        public int Complete()
        {
            return _contex.SaveChanges();
        }
    }
}
