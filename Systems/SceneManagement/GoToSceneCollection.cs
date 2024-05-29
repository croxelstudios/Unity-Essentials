using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToSceneCollection : MonoBehaviour
{
    [SerializeField]
    ScenesCollection scenesCollection = null;

    int current;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += SceneChange;
        GetCurrent(gameObject.scene);
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= SceneChange;
    }

    public void GoNextScene()
    {
        current = (int)Mathf.Repeat(current + 1, scenesCollection.scenes.Length);
        SceneManager.LoadScene(scenesCollection.scenes[current]);
    }

    public void GoRandomScene()
    {
        SceneManager.LoadScene(scenesCollection.scenes[
            Random.Range(0, scenesCollection.scenes.Length)]);
    }

    void SceneChange(Scene previous, Scene newone)
    {
        GetCurrent(newone);
    }

    void GetCurrent(Scene thisScene)
    {
        current = -1;
        for (int i = 0; i < scenesCollection.scenes.Length; i++)
        {
            if (scenesCollection.scenes[i].ScenePath == thisScene.path)
            {
                current = i;
                break;
            }
        }
    }
}
