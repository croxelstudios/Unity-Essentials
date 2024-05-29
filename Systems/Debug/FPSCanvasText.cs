using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class FPSCanvasText : MonoBehaviour
{
    Text text;
    [SerializeField]
    int averageEveryXFrames = 10;

    int frameCounter = 0;
    int frameSum = 0;
    void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        frameSum += (int)(1 / Time.deltaTime);
        frameCounter++;
        if (frameCounter > averageEveryXFrames)
        {
            text.text = (frameSum/averageEveryXFrames).ToString();
            frameCounter = 0;
            frameSum = 0;
        }
    }
}
