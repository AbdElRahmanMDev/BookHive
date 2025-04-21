namespace BookHive.Web.Core.ViewModels
{
    public class ReturnCopyViewModel
    {
        public int Id { get; set; }

        public bool? IsReturned { get; set; }  //Radio button can be null but chechkbox Not
    }
}
