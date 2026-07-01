using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class FPSCanvasText : MonoBehaviour
{
    Text text;
    [SerializeField]
    string addText = "fps";
    [SerializeField]
    string format = "F2";
    [SerializeField]
    int averageEveryXFrames = 10;

    const string replaceText = "{0}";

    int frameCounter = 0;
    float frameSum = 0;
    void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        string textValue;
        frameSum += 1 / Time.unscaledDeltaTime;
        frameCounter++;
        if (frameCounter > averageEveryXFrames)
        {
            textValue = (frameSum/averageEveryXFrames).StringFormat(format);
            frameCounter = 0;
            frameSum = 0;

            if (addText.Contains(replaceText))
                text.text = addText.Replace(replaceText, textValue);
            else text.text = textValue + addText;
        }
    }
}
