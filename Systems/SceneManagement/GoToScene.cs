using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour, ISceneHolder
{
    [SerializeField]
    SceneReference scene = null;
    public SceneReference Scene { get { return scene; } set { scene = value; } }
    [SerializeField]
    float seconds = 0.5f;
    [SerializeField]
    bool unscaledTime = false;
    [SerializeField]
    bool canGoToSameScene = true;
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
        if (CanLoadScene())
        {
            loadNextScene?.Invoke();
            GoSceneInstant_Internal();
        }
    }

    void GoSceneInstant_Internal()
    {
        SceneManager.LoadScene(scene);
    }

    public void GoScene(float seconds)
    {
        if (CanLoadScene())
        {
            loadNextScene?.Invoke();
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(WaitForSceneTransition(seconds));
        }
    }

    public void SetScene(SceneReference scene)
    {
        this.scene = scene;
    }

    public void SetScene(Scene scene)
    {
        this.scene.ScenePath = scene.path;
    }

    public void SetScene(string path)
    {
        this.scene.ScenePath = path;
    }

    bool CanLoadScene()
    {
        return CanLoadScene(scene);
    }

    bool CanLoadScene(SceneReference scene)
    {
        Scene scn = SceneManager.GetSceneByPath(scene);
        return canGoToSameScene || !scn.isLoaded;
    }

    public void SetCurrentScene()
    {
        scene.ScenePath = gameObject.scene.path;
    }
}
