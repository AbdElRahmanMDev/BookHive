namespace BookHive.Web.Core.Models
{
    public class BookCategory
    {
        public int BookId { get; set; }

        //Navigation Property
        public Book? Book { get; set; }

        public int CategoryId { get; set; }

        //Navigation Property
        public Category? Category { get; set; }
    }
}
