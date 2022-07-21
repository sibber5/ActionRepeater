using Windows.Globalization.NumberFormatting;

namespace ActionRepeater.UI.Helpers;

public static class NumberFormatterHelper
{
    private static DecimalFormatter? _roundToOneFormatter;
    public static DecimalFormatter RoundToOneFormatter => _roundToOneFormatter ??= new()
    {
        IntegerDigits = 1,
        FractionDigits = 0,
        NumberRounder = new IncrementNumberRounder()
        {
            Increment = 1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp,
        },
    };
}
