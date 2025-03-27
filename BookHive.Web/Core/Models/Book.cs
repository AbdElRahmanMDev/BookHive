using BookHive.Web.consts;

namespace BookHive.Web.Core.Models
{
    public class Book : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Title { get; set; }
        public int AuthorId { get; set; }
       //Navigation Property It doesn't Exist in the database
        public Author? Author { get; set; }
        public string Publisher { get; set; } = null!;

        public DateTime PublishingDate { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImageUrlThumb { get; set; }

        public string? ImagePublicId { get; set; }

        public string Hall { get; set; } = null!;





        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;

        public ICollection<BookCategory> Categories { get; set; } = new List<BookCategory>();
        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();

    }
}
