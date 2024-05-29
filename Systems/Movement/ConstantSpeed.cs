using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

public class ConstantSpeed : MonoBehaviour
{
    [SerializeField]
    protected float _speed = 10f;
    public float speed
    {
        get { return _speed; }
        set
        {
            _speed = value;
            speed3 = direction * speed;
        }
    }
    [SerializeField]
    protected Vector3 direction = Vector3.up;
    [SerializeField]
    bool worldSpace = false;
    [SerializeField]
    bool toChildren = false;
    [SerializeField]
    bool randomize = false;
    [SerializeField]
    [ShowIf("@randomize")]
    bool lockX = false;
    [SerializeField]
    [ShowIf("@randomize")]
    bool lockY = false;
    [SerializeField]
    [ShowIf("@randomize")]
    bool lockZ = false;
    [SerializeField]
    [ShowIf("@randomize")]
    bool cardinalOnly = false;
    [SerializeField]
    [ShowIf("@randomize")]
    Vector2 speedRange = Vector2.zero;
    [SerializeField]
    TimeMode timeMode = TimeMode.FixedUpdate;

    [HideInInspector]
    public Vector3 speed3;

    void OnEnable()
    {
        if (randomize)
        {
            if (lockX && lockY && lockZ) direction = Vector3.zero;
            else
            {
                if (cardinalOnly) SetCardinalDirection(FilteredRandomCardinal(lockX, lockY, lockZ));
                else direction = Vector3.one.GetRandom();
                direction = new Vector3(lockX ? 0 : direction.x, lockY ? 0 : direction.y, lockZ ? 0 : direction.z);
            }

            _speed = Random.Range(speedRange.x, speedRange.y);
        }

        direction = direction.normalized;
        speed3 = direction * speed;
    }

    void Update()
    {
        if (timeMode.IsSmooth()) UpdatePosition(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) UpdatePosition(timeMode.DeltaTime());
    }

    void UpdatePosition(float deltaTime)
    {
        if (toChildren)
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).Translate(speed3 * Time.deltaTime, worldSpace ? Space.World : Space.Self);
        else transform.Translate(speed3 * Time.deltaTime, worldSpace ? Space.World : Space.Self);
    }

    public enum Direction { Up, Right, Down, Left, Forward, Backwards }

#if UNITY_EDITOR
    [EnumPopup(typeof(Direction))]
#endif
    public void SetCardinalDirection(int direction)
    {
        switch ((Direction)direction)
        {
            case Direction.Right:
                SetDirection(Vector3.right);
                break;
            case Direction.Down:
                SetDirection(Vector3.down);
                break;
            case Direction.Left:
                SetDirection(Vector3.left);
                break;
            case Direction.Forward:
                SetDirection(Vector3.forward);
                break;
            case Direction.Backwards:
                SetDirection(Vector3.back);
                break;
            default:
                SetDirection(Vector3.up);
                break;
        }
    }

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction.normalized;
        speed3 = this.direction * speed;
    }

    int FilteredRandomCardinal(bool lockX, bool lockY, bool lockZ)
    {
        Direction[] lockDirs = new Direction[0];
        if (lockX) lockDirs = lockDirs.Concat(new Direction[] { Direction.Left, Direction.Right }).ToArray();
        if (lockY) lockDirs = lockDirs.Concat(new Direction[] { Direction.Down, Direction.Up }).ToArray();
        if (lockZ) lockDirs = lockDirs.Concat(new Direction[] { Direction.Backwards, Direction.Forward }).ToArray();
        return FilteredRandomCardinal(lockDirs);
    }

    int FilteredRandomCardinal(params Direction[] directions)
    {
        int dir = Random.Range(0, 6 - directions.Length);
        directions = directions.OrderBy(x => (int)x).ToArray();
        for (int i = 0; i < directions.Length; i++)
            if (dir >= (int)directions[i]) dir++;
        return dir;
    }
}
