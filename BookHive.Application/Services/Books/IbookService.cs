namespace BookHive.Application.Services.Books;

public interface IbookService
{
    (IQueryable<Book> books, int count) GetFiltered(GetFilteredDto dto);
}
