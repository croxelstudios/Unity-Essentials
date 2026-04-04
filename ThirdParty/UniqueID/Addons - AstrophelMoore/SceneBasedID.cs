using CleverCrow.Fluid.UniqueIds;
using UnityEngine;

public class SceneBasedID : MonoBehaviour, IUniqueId
{
    [SerializeField]
    string extra = "";
    [SerializeField]
    SceneSource sceneSource = SceneSource.CurrentThenHolders;
    public enum SceneSource { CurrentThenHolders, HoldersThenCurrent, CurrentScene, SceneHolders }

    ISceneHolder[] sceneHolders = null;

    const string SEPARATOR = "_";

    public string Id
    {
        get
        {
            string id = "";
            switch (sceneSource)
            {
                default:
                    id = CurrentScene() + SEPARATOR + SceneHolders();
                    break;
                case SceneSource.HoldersThenCurrent:
                    id = SceneHolders() + SEPARATOR + CurrentScene();
                    break;
                case SceneSource.CurrentScene:
                    id = CurrentScene();
                    break;
                case SceneSource.SceneHolders:
                    id = SceneHolders();
                    break;
            }
            id += Extra();
            return id;
        }
    }

    string CurrentScene()
    {
        return gameObject.scene.path;
    }

    string SceneHolders()
    {
        string id = "";

        if (sceneHolders.IsNullOrEmpty())
            sceneHolders = GetComponents<ISceneHolder>();
        for (int i = 0; i < sceneHolders.Length; i++)
        {
            id += sceneHolders[i].Scene.ScenePath;
            if (i < (sceneHolders.Length - 1))
                id += SEPARATOR;
        }

        return id;
    }

    string Extra()
    {
        if (!extra.IsNullOrEmpty())
            return SEPARATOR + extra;
        else return "";
    }

    [EnumSelector(typeof(SceneSource))]
    public void SetSource(int source)
    {
        sceneSource = (SceneSource)source;
    }
}
