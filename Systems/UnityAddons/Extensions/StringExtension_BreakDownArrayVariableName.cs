using System;
using System.Linq;
using System.Text.RegularExpressions;

public static class StringExtension_BreakDownArrayVariableName
{
    public static string BreakDownArrayVariableName(this string text, out int index)
    {
        Regex rgx = new Regex(@"\[\d+\]");
        if (rgx.Matches(text).Count > 0)
        {
            string numbers = new string(text.Where(c => char.IsDigit(c)).ToArray());
            index = Convert.ToInt32(numbers);
            text = rgx.Replace(text, "");
        }
        else index = -1;
        return text;
    }
}
