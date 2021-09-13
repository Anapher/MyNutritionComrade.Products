#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

using BuildProducts.Extensions;
using BuildProducts.Models;
using FluentValidation;

namespace BuildProducts.Validation
{
    public class ProductInfoValidator : AbstractValidator<Product>
    {
        public ProductInfoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.NutritionalInfo.Volume).Equal(100);
            RuleFor(x => x.NutritionalInfo).SetValidator(new NutritionalInfoValidator());

            RuleFor(x => x.Code).NotEqual("");
            RuleFor(x => x.Label).NotEmpty();
            RuleForEach(x => x.Label).NotNull().ChildRules(labels =>
            {
                labels.RuleFor(x => x.Value.Value).NotEmpty();
                labels.RuleFor(x => x.Value.Tags).UniqueItems();
                labels.RuleFor(x => x.Key).NotEmpty().IsCulture();
            });
            RuleForEach(x => x.Tags).OneOf(Product.AllowedTags);
            RuleFor(x => x.Tags).UniqueItems();
            RuleFor(x => x.Servings).NotEmpty();
            RuleForEach(x => x.Servings).ChildRules(serving =>
            {
                serving.RuleFor(x => x.Value).GreaterThan(0);
                serving.RuleFor(x => x.Key).NotNull().OneOf(ServingType.AvailableTypes);
            });
            RuleFor(x => x).Must(x =>
            {
                if (x.Tags?.Contains(Product.TAG_LIQUID) == true)
                {
                    return x.Servings.TryGetValue(ServingType.Milliliter, out var baseValue) && baseValue == 1 &&
                           !x.Servings.ContainsKey(ServingType.Gram);
                }
                else
                {
                    return x.Servings.TryGetValue(ServingType.Gram, out var baseValue) && baseValue == 1 &&
                           !x.Servings.ContainsKey(ServingType.Milliliter);
                }
            }).WithMessage(
                "If the product is in liquid form, it must have a serving 'ml' with a value of 1, else it must have 'g' with a value of 1");
            RuleFor(x => x.DefaultServing).NotEmpty();
            RuleFor(x => x).Must(x => x.Servings.ContainsKey(x.DefaultServing))
                .WithMessage("The default serving must be defined in servings");
        }
    }
}