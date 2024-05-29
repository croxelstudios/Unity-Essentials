using UnityEngine;

public class SceneRestartByKey : SceneRestarter
{
    [SerializeField]
    KeyCode key = KeyCode.R;

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            SceneRestart();
        }
    }
}
