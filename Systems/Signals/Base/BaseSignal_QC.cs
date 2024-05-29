using QFSW.QC;
using System;
using System.Collections.Generic;
using System.Linq;

public class ComponentParser : PolymorphicQcParser<BaseSignal>
{
    public override BaseSignal Parse(string value, Type type)
    {
        return BaseSignal.activeSignals[type].Find(x => x.name == value);
    }
}

public class SignalSuggestor : IQcSuggestor
{
    public IEnumerable<IQcSuggestion> GetSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        Type targetType = context.TargetType;
        string prompt = context.Prompt;
        if ((targetType != null) && typeof(BaseSignal).IsAssignableFrom(targetType))
        {
            IOrderedEnumerable<BaseSignal> signals = BaseSignal.activeSignals[targetType]
                .Where(x => IsElementValid(x.name, prompt))
                .OrderBy(x => ReorderFromPrompt(x.name, prompt));

            foreach (BaseSignal signal in signals)
                yield return new RawSuggestion(signal.name);
        }
    }

    bool IsElementValid(string original, string prompt)
    {
        return original.ToLower().Contains(prompt.ToLower());
    }

    string ReorderFromPrompt(string original, string prompt)
    {
        if (original.ToLower().StartsWith(prompt.ToLower())) return "00" + original;
        else return original;
    } 
}
