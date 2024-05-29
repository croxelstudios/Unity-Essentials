using UnityEngine;
using System.Collections;

public class QuitGame : MonoBehaviour
{
    [SerializeField]
    float time = 0f;

    Coroutine co;

    public void QuitAfterTime()
    {
        if (this.IsActiveAndEnabled())
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
                co = StartCoroutine(QuitAfterSeconds(time));
        }
    }

    void OnDisable()
    {
        if(co != null) StopCoroutine(co);
    }

    IEnumerator QuitAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        Quit();
    }

    public void Quit()
    {
        if (this.IsActiveAndEnabled())
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

}
