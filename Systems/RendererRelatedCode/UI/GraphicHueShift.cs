using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class GraphicHueShift : MonoBehaviour
{
    Graphic graphic;
    
    [SerializeField]
    float shiftSpeed = 1f;

    float originalH;

    void OnEnable()
    {
        graphic = GetComponent<Graphic>();
        Color.RGBToHSV(graphic.color, out originalH, out float oldS, out float oldV);
    }

    void Update()
    {
        Color.RGBToHSV(graphic.color, out float oldH, out float oldS, out float oldV);
        Color newColor = Color.HSVToRGB((originalH + (Time.time * 0.05f * shiftSpeed)) % 1f, oldS, oldV);
        newColor.a = graphic.color.a;
        graphic.color = newColor;
    }
}
