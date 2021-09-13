namespace BuildProducts.Models
{
    /// <summary>
    ///     Macro nutrition information
    /// </summary>
    public record NutritionalInfo(double Volume, double Energy, double Fat, double SaturatedFat, double Carbohydrates,
        double Sugars, double Protein,
        double DietaryFiber, double Sodium)
    {
        public static NutritionalInfo Empty => new(0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
}