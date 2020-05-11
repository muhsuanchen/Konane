using UnityEngine;

public class GameObj : MonoBehaviour
{
    public int XPos { get; private set; }
    public int YPos { get; private set; }
    public bool Side { get; private set; }  // true = (xy相加)偶數格, false = (xy相加)奇數格

    Vector2Int mPos;
    public Vector2Int Pos => mPos;

    public virtual void Init(int x, int y)
    {
        SetPos(x, y);
        Side = (XPos + YPos) % 2 == 0;

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
