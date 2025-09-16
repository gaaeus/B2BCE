using FluentValidation;

namespace Application.Companies;

public sealed class AddOrUpdateStateRegistrationValidator : AbstractValidator<AddOrUpdateStateRegistrationCommand>
{
    public AddOrUpdateStateRegistrationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Uf).NotEmpty().Length(2);
        RuleFor(x => x.Ie).NotEmpty().MaximumLength(32);
    }
}
