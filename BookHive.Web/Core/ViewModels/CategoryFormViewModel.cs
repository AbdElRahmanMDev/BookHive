
using BookHive.Web.consts;
using System.ComponentModel;

namespace BookHive.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Validationscs.MaxLenErrorMessage)]
        [Required]
        [Remote(action: "checkUnique", controller: null, AdditionalFields = nameof(Id), ErrorMessage = Validationscs.UniqueErrorMessage)]
        public string Name { get; set; }
    }
}
