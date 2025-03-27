using BookHive.Web.consts;
using System.ComponentModel;

namespace BookHive.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Validationscs.MaxLenErrorMessage)]
        [Required]
        [Remote(action: "check", controller:nameof(Author), AdditionalFields = nameof(Id), ErrorMessage = Validationscs.UniqueErrorMessage)]
        [DisplayName("Author Name")]
        public string Name { get; set; }
    }
}
