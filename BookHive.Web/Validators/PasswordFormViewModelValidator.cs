//using FluentValidation;

//namespace BookHive.Web.Validators
//{
//    public class PasswordFormViewModelValidator : AbstractValidator<PasswordFormViewModel>
//    {
//        public PasswordFormViewModelValidator()
//        {
//            RuleFor(customer => customer.Password).Equal(customer => customer.ConfirmPassword).WithMessage(Validationscs.ConfirmPassword);
//            RuleFor(x => x.Password)
//            .Length(min: 10, max: 100).WithMessage(Validationscs.Password);

//        }
//    }
//}
