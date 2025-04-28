
using BookHive.Application.Common.Interfaces;
using System.Linq.Dynamic.Core;
namespace BookHive.Application.Services.Books;

public class BookService : IbookService
{
    private readonly IUnitOfWork _unitOfWork;
    public BookService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public (IQueryable<Book> books, int count) GetFiltered(GetFilteredDto dto)
    {
        IQueryable<Book> books = _unitOfWork.Books.GetDetails();

        if (!string.IsNullOrEmpty(dto.SearchValue))
        {
            books = books.Where(x => x.Title.Contains(dto.SearchValue) || x.Author!.Name.Contains(dto.SearchValue));
        }

        books = books.OrderBy($"{dto.SortColumn} {dto.SortColumnDirection}").Skip(dto.Skip).Take(dto.PageSize);

        var recordTotal=_unitOfWork.Books.Count();
        return (books, recordTotal);
    }
}
