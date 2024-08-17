using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToScene_Launcher : BRemoteLauncher
{
    [SerializeField]
    SceneReference scene = null;

    GoToScene[] goToScenes;

    public void GoScene()
    {
        if (this.IsActiveAndEnabled())
        {
            if (goToScenes == null)
                goToScenes = FindObjectsOfType<GoToScene>();
            //FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                {
                    if (scene != null) goTo.SetScene(scene);
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
                    if (scene != null) goTo.SetScene(scene);
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
                    if (scene != null) goTo.SetScene(scene);
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
                    if (scene != null) goTo.SetScene(scene);
        }
    }

    public void SetScene(SceneReference scene)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref goToScenes);
            foreach (GoToScene goTo in goToScenes)
                if (goTo != null)
                    if (scene != null) goTo.SetScene(scene);
        }
    }
}
