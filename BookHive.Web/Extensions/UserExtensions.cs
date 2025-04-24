namespace BookHive.Web.Extensions
{
    public static class UserExtensions
    {
        public static string getUserId(this ClaimsPrincipal User) => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    }
}
