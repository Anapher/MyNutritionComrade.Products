using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentValidation;

namespace BuildProducts.Extensions
{
    public static class FluentValidatorExtensions
    {
        private static readonly ISet<string> SupportedCultures;

        static FluentValidatorExtensions()
        {
            var cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .ToArray();
            var allNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allNames.UnionWith(cultureInfos.Select(x => x.TwoLetterISOLanguageName));
            allNames.UnionWith(cultureInfos.Select(x => x.Name));

            SupportedCultures = allNames;
        }

        public static IRuleBuilderOptions<T, string> IsCulture<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(x =>
            {
                if (string.IsNullOrEmpty(x))
                    return true;

                return SupportedCultures.Contains(x);
            }).WithMessage("A localization code is expected");
        }

        public static IRuleBuilderOptions<T, TProperty> OneOf<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder,
            ISet<TProperty> set)
        {
            return ruleBuilder.Must(set.Contains).WithMessage($"The value must be one of [{string.Join(", ", set)}]");
        }

        public static IRuleBuilderOptions<T, IEnumerable<TProperty>> UniqueItems<T, TProperty>(
            this IRuleBuilder<T, IEnumerable<TProperty>> ruleBuilder, IEqualityComparer<TProperty>? comparer = null)
        {
            return ruleBuilder.Must(x =>
                    x == null || (comparer == null ? x.Distinct() : x.Distinct(comparer)).Count() == x.Count())
                .WithMessage("The list must not contain duplicate items.");
        }
    }
}