using UnityEngine;

public static class ObjectExtension_GameObj
{
    public static GameObject GameObj(this Object obj)
    {
        switch (obj)
        {
            case GameObject g:
                return g;
            case Component c:
                return c.gameObject;
            default:
                return null;
        }
    }
}
