using FluentValidation;

namespace BookHive.Web.Validators
{
    public class AuthorValidator : AbstractValidator<AuthorFormViewModel>
    {
        public AuthorValidator() {


            RuleFor(x => x.Name).MaximumLength(100).WithMessage(Validationscs.MaxLength);
            RuleFor(x => x.Name).Matches(RegexPatterns.CharactersOnly_Eng).WithMessage(Validationscs.OnlyEnglishLetters);

        }

    }
}
