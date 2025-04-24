namespace BookHive.Domain.Entities
{
    public class Rental : BaseEntity
    {
        public int Id { get; set; }

        public int SubscriberId { get; set; }

        public Subscriber? Subscriber { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public bool PenaltyPaid { get; set; }

        public ICollection<RentalCopy> RentalCopy { get; set; } = new List<RentalCopy>();

    }
}
