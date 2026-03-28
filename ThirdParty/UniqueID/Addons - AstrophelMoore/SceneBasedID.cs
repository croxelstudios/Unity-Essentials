using CleverCrow.Fluid.UniqueIds;
using UnityEngine;

public class SceneBasedID : MonoBehaviour, IUniqueId
{
    [SerializeField]
    string extra = "";
    [SerializeField]
    bool _useSceneHolder = false;
    public bool useSceneHolder { get { return _useSceneHolder; }  set { _useSceneHolder = value; } }

    ISceneHolder sceneHolder = null;

    public string Id
    {
        get
        {
            if (useSceneHolder)
            {
                if (sceneHolder == null)
                    sceneHolder = GetComponent<ISceneHolder>();
                return sceneHolder.Scene.ScenePath + "_" + extra;
            }
            else return gameObject.scene.path + "_" + extra;
        }
    }
}
