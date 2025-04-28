using BookHive.Application.Common.Interfaces.Repositories;

namespace BookHive.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Author> Authors { get; }
        IRepository<Category> Categories { get; }
        IRepository<BookCopy> BookCopies { get; }
        //IRepository<Book> Books { get; }
        IBookRepository Books { get; }
        IRepository<RentalCopy> RentalCopies { get; }
        int Complete();
    }
}
