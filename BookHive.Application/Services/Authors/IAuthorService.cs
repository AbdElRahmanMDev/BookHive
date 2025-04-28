namespace BookHive.Application.Services;

public interface IAuthorService
{
   IEnumerable<Author> GetAll();

    Author Add(string name, string CreatedById);

    Author? Edit(int id, string name, string UpdatedById);

    Author? ToggleStatus(int id, string user);

    bool check(string name,int id);
    Author? GetById(int id);
}
