
namespace BookHive.Web.Core.Models
{
    public class Category : BaseModel
    {
        //Category table ---> 1 , Drama , true ---> As isdeleted is true so i will ignore these values

        public int Id { get; set; }
        [MaxLength(100)]

        public string Name { get; set; } = null!;

        
        public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>();

    }

    
}
