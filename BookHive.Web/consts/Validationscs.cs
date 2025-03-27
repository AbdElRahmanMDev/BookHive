namespace BookHive.Web.consts
{
    public static class Validationscs
    {
        //{0}--->points Name  {first parameter}
        public const string MaxLenErrorMessage = "{0} Has Max length {1}";
        public const string RequiredField = "Required field";

        public const string UniqueErrorMessage = "{0} Must be Unique";

        public const string AllowedExtension = ".png .jpeg .jpg  is Allowed Only ";
        public const string MaxSize = "The file cannot be more than 2MB ";
        public const string BookAuthorError = "Book with same Author is not Allowed";
        public const string ErrorDate = "The Date should not be in the future";
        public const string Password = "The {0} must be at least {2} and at max {1} characters long.";
        public const string ConfirmPassword = "The password and confirmation password do not match.";
        public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long";
        public const string Duplicated = "The {0} is already Exist";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";

    }
}
