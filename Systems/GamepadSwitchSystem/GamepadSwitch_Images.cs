using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class GamepadSwitch_Images : GamepadSwitch
{
    Image image;

    protected override void OnEnable()
    {
        image = GetComponent<Image>();
        base.OnEnable();
    }

    public override void SwitchValue(InputPrompt.InputPromptTexture texture)
    {
        base.SwitchValue(texture);
        if (image != null)
        {
            Sprite[] sprites = texture.sprites;
            if ((sprites != null) && (sprites.Length > 0))
                image.sprite = sprites[0];
        }
    }
}
