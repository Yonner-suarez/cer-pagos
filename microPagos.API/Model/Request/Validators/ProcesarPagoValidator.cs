using FluentValidation;

namespace microPagos.API.Model.Request.Validators
{
    public class ProcesarPagoValidator : AbstractValidator<ProcesarPagoRequest>
    {
        public ProcesarPagoValidator()
        {
            RuleFor(x => x.MyProperty)
               .Must(m => m != 0)
               .WithMessage("Debe ingresar una marca válida.");
        }
    }
}
