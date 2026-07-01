using System;

public static class FloatExtension_StringFormat
{
    public static string StringFormat(this int value, string format)
    {
        return StringFormat((float)value, format);
    }

    public static string StringFormat(this float value, string format)
    {
        string valueText;
        if (format == "Time") valueText = TimeSpan.FromSeconds(value).ToString("c");
        else if (format.Contains('D', StringComparison.CurrentCultureIgnoreCase) ||
            format.Contains('X', StringComparison.CurrentCultureIgnoreCase))
            valueText = ((int)value).ToString(format);
        else valueText = value.ToString(format);
        return valueText;
    }
}
