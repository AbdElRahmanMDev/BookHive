namespace BookHive.Web.Core.ViewModels
{
    public class AuthorViewModel
    {
        public int Id { get; set; }
        [MaxLength(100)]

        public string Name { get; set; } 

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; } 

        public DateTime? LastUpdateOn { get; set; }
    }
}
