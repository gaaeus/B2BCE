using FluentValidation;

namespace Application.Companies;

public sealed class RefreshStateRegistrationFromSefazValidator : AbstractValidator<RefreshStateRegistrationFromSefazCommand>
{
    public RefreshStateRegistrationFromSefazValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Uf).NotEmpty().Length(2);
    }
}
