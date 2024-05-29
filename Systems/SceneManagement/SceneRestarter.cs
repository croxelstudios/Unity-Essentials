using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

public class SceneRestarter : MonoBehaviour
{

    [SerializeField]
    DXEvent onRestart = null;
    public void SceneRestart()
    {
        onRestart?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
