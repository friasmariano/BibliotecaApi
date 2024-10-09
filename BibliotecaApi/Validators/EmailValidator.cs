

namespace FluentValidation;

public class EmailValidator: AbstractValidator<string>
{
    public EmailValidator()
    {
        RuleFor(email => email)
            .NotEmpty().WithMessage("El campo email no debe estar vacío.")
            .EmailAddress().WithMessage("Se require un email válido.");
    }
}
