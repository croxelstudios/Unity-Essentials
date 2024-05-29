using UnityEngine;
using UnityEditor;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class GamepadSwitch_Materials : GamepadSwitch
{
    Renderer rend;

    [SerializeField]
    int materialIndex = 0;
    [SerializeField]
    Material[] materials = null;

    protected override void OnEnable()
    {
        rend = GetComponent<Renderer>();
        base.OnEnable();
    }

    public override void SwitchValue(InputPrompt.InputPromptTexture texture)
    {
        base.SwitchValue(texture); //TO DO: Support texture swapping with property blocks
        if (rend != null)
        {
            Material[] array = rend.materials;
            array[materialIndex] = materials[current];
            rend.materials = array;
        }
    }
}
