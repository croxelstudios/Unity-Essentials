using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class DontDestroyUniqueName : MonoBehaviour
{
    [SerializeField]
    bool destroyOnScenesWithoutIt = true;
    [SerializeField]
    DXEvent onFirstLoad = null;
    [SerializeField]
    DXSceneEvent sceneLoaded = null;
    [SerializeField]
    DXSceneEvent sceneChanged = null;
    [SerializeField]
    DXEvent sceneRestarted = null;

    int currentScene;
    bool imChosen;
    bool duplicateDestroyed;
    bool isFirstLoad;
    bool sceneReset;
    bool initialized;

    void Awake()
    {
        if (this.IsActiveAndEnabled())
            StartUp(true);
    }

    void Start()
    {
        if (!initialized)
            StartUp(false);
    }

    void StartUp(bool firstLoad)
    {
        DontDestroyUniqueName[] objs = FindObjectsOfType<DontDestroyUniqueName>();
        if (objs.Length > 1)
        {
            foreach (DontDestroyUniqueName obj in objs)
                if (obj != this)
                {
                    if (obj.gameObject.name == gameObject.name)
                    {
                        obj.ChooseThisOne();
                        break;
                    }
                }
            DestroyImmediate(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SceneLoaded;
            onFirstLoad?.Invoke();
            isFirstLoad = firstLoad;
            duplicateDestroyed = true;
            imChosen = true;
        }
        initialized = true;
    }

    void OnDisable()
    {
        if (imChosen)
        {
            Scene sceneCurrent = SceneManager.GetActiveScene();
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            SceneManager.sceneLoaded -= SceneLoaded;
            imChosen = false;
        }
    }

    public void ChooseThisOne()
    {
        duplicateDestroyed = true;
    }

    void LateUpdate()
    {
        if (sceneReset)
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        else
        {
            if (isFirstLoad) isFirstLoad = false;
            duplicateDestroyed = false;
        }
        if(gameObject.scene.name != "DontDestroyOnLoad")
            DontDestroyOnLoad(gameObject);
    }

    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isFirstLoad) isFirstLoad = false;
        else
        {
            if (destroyOnScenesWithoutIt && !duplicateDestroyed) SelfDestruct();
            else
            {
                sceneLoaded?.Invoke(scene);
                if (scene.buildIndex != currentScene)
                {
                    sceneChanged?.Invoke(scene);
                    currentScene = scene.buildIndex;
                }
                else sceneRestarted?.Invoke();
            }
        }
    }
    
    public void SelfDestruct()
    {
        Destroy(gameObject);
    }

    public void Restart() //TO DO: This could be done much better (as of right now, it loads the scene twice)
    {
        if (this.IsActiveAndEnabled())
        {
            OnDisable();
            sceneReset = true;
        }
    }
}
