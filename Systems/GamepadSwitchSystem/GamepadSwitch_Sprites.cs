using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class GamepadSwitch_Sprites : GamepadSwitch
{
    SpriteRenderer spriteRenderer;

    protected override void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        base.OnEnable();
    }

    public override void SwitchValue(InputPrompt.InputPromptTexture texture)
    {
        base.SwitchValue(texture);
        if (spriteRenderer != null)
        {
            Sprite[] sprites = texture.sprites;
            if ((sprites != null) && (sprites.Length > 0))
                spriteRenderer.sprite = sprites[0];
        }
    }
}
