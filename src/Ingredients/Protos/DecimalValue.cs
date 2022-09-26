namespace Ingredients.Protos;

public partial class DecimalValue
{
    private const decimal NanoDivisor = 1_000_000_000m;

    public DecimalValue(decimal value)
    {
        Units = (long)decimal.Truncate(value);
        Nanos = (int)((value - Units) * NanoDivisor);
    }

    public static implicit operator decimal(DecimalValue value) =>
        value.Units + (value.Nanos / NanoDivisor);

    public static implicit operator DecimalValue(decimal value) =>
        new DecimalValue(value);
    
    public static implicit operator DecimalValue(double value) =>
        new DecimalValue(Convert.ToDecimal(value));
}