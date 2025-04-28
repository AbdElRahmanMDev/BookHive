namespace BookHive.Web.Core.ViewModels
{
    public class BookRowViewModel
    {
        public int Id { get; set; }

        public string Author { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? ImageUrlThumb { get; set; }

        public string Publisher { get; set; } = null!;

        public DateTime PublishingDate { get; set; }

        public bool IsAvailableForRental { get; set; }

        public bool IsDeleted { get; set; }

        public string Hall { get; set; } = null!;




    }
}
