using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToScene_Launcher : BRemoteLauncher, ISceneHolder
{
    [SerializeField]
    SceneReference scene = null;
    public SceneReference Scene { get { return scene; } set { scene = value; } }

    GoToScene[] goToScenes;

    public void GoScene()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                {
                    if (scene.ScenePath != "") goTo.SetScene(scene);
                    goTo.GoScene();
                }
        }
    }

    public void GoScene(float seconds)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                {
                    if (scene.ScenePath != "") goTo.SetScene(scene);
                    goTo.GoScene(seconds);
                }
        }
    }

    public void GoSceneInstant()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                {
                    if (scene.ScenePath != "") goTo.SetScene(scene);
                    goTo.GoSceneInstant();
                }
        }
    }

    public void SetScene()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                    if (scene.ScenePath != "") goTo.SetScene(scene);
        }
    }

    public void SetScene(SceneReference scene)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                    if (scene.ScenePath != "") goTo.SetScene(scene);
        }
    }

    public void SetCurrentScene()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                    goTo.SetScene(gameObject.scene);
        }
    }
}
