
namespace BookHive.Web.Core.ViewModels
{
    public class SearchFormViewModel
    {
        [Required(ErrorMessage = "You Should Enter Value")]
        public string Value { get; set; } = null!;
    }
}
