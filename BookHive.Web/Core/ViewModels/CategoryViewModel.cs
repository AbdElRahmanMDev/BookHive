using BookHive.Web.Core.Models;

namespace BookHive.Web.Core.ViewModels
{
    public class CategoryViewModel 
    {
        public int Id { get; set; }
        public string Name { get; set; } 

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdateOn { get; set; }
    }


    public static class ExtenClass
    {
        public static CategoryViewModel ToCategoryViewModel(this Category category)
        {
            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                IsDeleted = category.IsDeleted,
                CreatedOn = category.CreatedOn,
                LastUpdateOn = category.LastUpdateOn
            };
        }
    }
}
