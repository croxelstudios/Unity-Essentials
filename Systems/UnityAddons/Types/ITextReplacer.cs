using UnityEngine;

public interface ITextReplacer
{
    public static int Count(GameObject gameObject)
    {
        int count = 0;
        foreach (ITextReplacer replacer in gameObject.GetComponents<ITextReplacer>())
            count++;
        return count;
    }

    //TO DO: Should detect if the number is already used or not and take the first one available
    public static string DefaultReplaceText(GameObject gameObject)
    {
        return "{" + (Count(gameObject) - 1) + "}";
    }
}
