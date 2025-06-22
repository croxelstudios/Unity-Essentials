using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    [SerializeField]
    SceneReference scene = null;
    [SerializeField]
    float seconds = 0.5f;
    [SerializeField]
    bool unscaledTime = false;
    [SerializeField]
    DXEvent loadNextScene = null;

    Coroutine co;

    IEnumerator WaitForSceneTransition(float seconds)
    {
        float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        for (float time = 0f; time < seconds; time += deltaTime)
        {
            yield return null;
            deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime > 0.1f) deltaTime = 0f; //Don't progress if lag spike
        }
        GoSceneInstant_Internal();
    }

    public void GoScene()
    {
        if (seconds <= 0f) GoSceneInstant();
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
