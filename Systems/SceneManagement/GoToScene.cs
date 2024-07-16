using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    [SerializeField]
    SceneReference scene = null;
    [SerializeField]
    float seconds = 0.5f;
    [SerializeField]
    DXEvent loadNextScene = null;

    Coroutine co;

    IEnumerator WaitForSceneTransition(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GoSceneInstant_Internal();
    }

    public void GoScene()
    {
        if (seconds <= 0f) GoSceneInstant_Internal();
        else GoScene(seconds);
    }

    public void GoSceneInstant()
    {
        loadNextScene?.Invoke();
        GoSceneInstant_Internal();
    }

    void GoSceneInstant_Internal()
    {
        SceneManager.LoadScene(scene);
    }

    public void GoScene(float seconds)
    {
        loadNextScene?.Invoke();
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(WaitForSceneTransition(seconds));
    }

    public void SetScene(SceneReference scene)
    {
        this.scene = scene;
    }
}
