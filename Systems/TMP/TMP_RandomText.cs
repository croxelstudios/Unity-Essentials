using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMP_RandomText : MonoBehaviour
{
    TMP_Text text;

    [SerializeField]
    [TextArea(0, 10)]
    string[] texts = null;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        text.SetText(texts[Random.Range(0, texts.Length)]);
    }
}
