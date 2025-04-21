using BookHive.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Web.Tasks
{
    public class HangFireTask
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public HangFireTask(ApplicationDbContext context,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender)
        {
            _context = context;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }
        public async Task PrepareExpirationAlert()
        {
            var subscribers = _context.Subscribers
                .Include(x => x.subscribtions)
                .Where(x => x.subscribtions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(5))
                .ToList();


            foreach (var subscriber in subscribers)
            {
                var body = _emailBodyBuilder.GetEmailBody("https://th.bing.com/th/id/OIP.9oxlutL9_TtNvUIxctfT0wHaHa?rs=1&pid=ImgDetMain",
                           $"Hey {subscriber.FirstName}, Alert!!!",
                           "https://www.google.com/",
                           "Active Account",
                           $"Your subscribtion will be ended within 5 days {subscriber.subscribtions.Last().EndDate.ToString()}");

                await _emailSender.SendEmailAsync(subscriber.Email, "New Subscription", body);
            }
        }

    }
}
