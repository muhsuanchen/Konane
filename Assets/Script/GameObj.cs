using UnityEngine;

public class GameObj : MonoBehaviour
{
    [SerializeField]
    GameSide mGameSide;  // true = (xy相加)偶數格, false = (xy相加)奇數格

    public bool Side => mGameSide == GameSide.BLACK;
    public int XPos { get; private set; }
    public int YPos { get; private set; }

    Vector2Int mPos;
    public Vector2Int Pos => mPos;

    public virtual void Init(int x, int y)
    {
        SetPos(x, y);
        gameObject.SetActive(true);
    }

    public void SetPos(int x, int y)
    {
        XPos = x;
        YPos = y;
        mPos = new Vector2Int(XPos, YPos);
    }

    public virtual void Recycle()
    {
        gameObject.SetActive(false);
    }
}
