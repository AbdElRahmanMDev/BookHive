namespace BookHive.Application.Common.Interfaces.Repositories
{
    public interface IBookRepository: IRepository<Book>
    {
        IQueryable<Book> GetDetails(); 

    }
}
