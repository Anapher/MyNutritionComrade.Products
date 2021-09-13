using BuildProducts.Models;
using FluentValidation;

namespace BuildProducts.Validation
{
    public class NutritionalInfoValidator : AbstractValidator<NutritionalInfo>
    {
        public NutritionalInfoValidator()
        {
            RuleFor(x => x.Energy).GreaterThan(0);
            RuleFor(x => x.Fat).GreaterThanOrEqualTo(0);
            RuleFor(x => x.SaturatedFat).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Carbohydrates).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Sugars).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Protein).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DietaryFiber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Sodium).GreaterThanOrEqualTo(0);
            RuleFor(x => x).Must(x => x.Carbohydrates >= x.Sugars)
                .WithMessage("The carbohydrates must not exceed the sugars");
            RuleFor(x => x).Must(x => x.Fat >= x.SaturatedFat).WithMessage("The fat must not exceed the saturated fat");
            RuleFor(x => x).Must(x => x.Carbohydrates + x.Fat + x.Protein + x.Sodium <= x.Volume)
                .WithMessage("The nutrition must not exceed the total volume.");
        }
    }
}