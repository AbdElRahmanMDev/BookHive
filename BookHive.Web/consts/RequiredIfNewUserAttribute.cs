namespace BookHive.Web.consts
{
    public class RequiredIfNewUserAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var userModel = (UserFormViewModel)validationContext.ObjectInstance;
            bool isNewUser = string.IsNullOrWhiteSpace(userModel.Id);

            if (isNewUser && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult($"{validationContext.DisplayName} is required for new users.");
            }

            return ValidationResult.Success;
        }
    }
}
