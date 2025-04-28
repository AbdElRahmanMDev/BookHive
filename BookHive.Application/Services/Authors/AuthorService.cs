
using BookHive.Application.Common.Interfaces;
using BookHive.Domain.Entities;
using System.Security.Claims;

namespace BookHive.Application.Services;
public class AuthorService : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IEnumerable<Author> GetAll()
    {
       return _unitOfWork.Authors.GetAll(withNoTracking:false);
    }

    public Author Add(string name , string CreatedById)
    {
        Author author = new Author()
        {
            Name = name,
            CreatedById = CreatedById
            
        };

        _unitOfWork.Authors.Add(author);
        _unitOfWork.Complete();
        return author;
    }

    public Author? GetById(int id)
    {
      return  _unitOfWork.Authors.GetById(id);
    }

    public Author? Edit(int id,string name, string UpdatedById)
    {
        var author = _unitOfWork.Authors.GetById(id);
        if (author is null)
            return null;

        author.Name = name;
        author.LastUpdateOn = DateTime.Now;
        author.LastUpdatedById = UpdatedById;
        _unitOfWork.Complete();

        return author;

    }

    public Author? ToggleStatus(int id, string user)
    {
        var author = _unitOfWork.Authors.GetById(id);
        if (author is null)
            return null;
        author.LastUpdateOn = DateTime.Now;
        author.IsDeleted = !author.IsDeleted;
        author.LastUpdatedById = user;
        _unitOfWork.Complete();

        return author;
    }

    public bool check(string name,int id)
    {
        var author = _unitOfWork.Authors.Find(x => x.Name ==name);

        var IsValid = author is null || author.Id.Equals(id);

        return IsValid;
    }
}
