

namespace FluentValidation
{
    public class PasswordValidator: AbstractValidator<string>
    {
        public PasswordValidator() {
            RuleFor(password => password)
                .NotEmpty().WithMessage("El campo contraseña no debe estar vacío.")
                .MinimumLength(8).WithMessage("La contraseña debe tener 8 caracteres (mínimo).")
                .Matches("[A-Z]").WithMessage("La contraseña debe tener una letra mayúscula.")
                .Matches("[a-z]").WithMessage("La contraseña debe tener una letra minúscula.")
                .Matches("[0-9]").WithMessage("La contraseña debe tener un número.")
                .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe tener un caracter especial.");
        }
    }
}
