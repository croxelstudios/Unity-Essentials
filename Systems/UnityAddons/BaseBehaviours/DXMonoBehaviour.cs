using UnityEngine;

public class DXMonoBehaviour : MonoBehaviour
{
    GameObject _gameObject;
    public new GameObject gameObject
    {
        get
        {
            if (_gameObject == null)
                _gameObject = base.gameObject;
            return _gameObject;
        }
    }

    Transform _transform;
    public new Transform transform
    {
        get
        {
            if (_transform == null)
                _transform = base.transform;
            return _transform;
        }
    }

    public new string tag
    {
        get
        {
            return gameObject.tag;
        }
        set
        {
            gameObject.tag = value;
        }
    }
}
