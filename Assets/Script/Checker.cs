using System;
using UnityEngine;
using UnityEngine.UI;

public class Checker : GameObj
{
    [SerializeField]
    Image m_CheckerBG;
    [SerializeField]
    Image m_Highlight;  // Can Move To
    [SerializeField]
    Button m_Button;

    RectTransform mRectTrans;
    Action<Checker> OnMoveToEvent;

    public Chess CurrentChess { get; private set; }
    public bool CanMoveFrom => (CurrentChess == null) ? false : CurrentChess.Selectable;
    public bool CanMoveTo { get; private set; }
    public bool XChecked { get; private set; }
    public bool YChecked { get; private set; }

    public override void Init(int x, int y)
    {
        base.Init(x, y);

        gameObject.name = $"Checker ({x}-{y})";

        InitCheckImage();

        m_Button.onClick.AddListener(OnSelect);

        mRectTrans = gameObject.GetComponent<RectTransform>();
        mRectTrans.localScale = Vector3.one;

        ClearState();
        RemoveChess();
    }

    void InitCheckImage()
    {
        var image = ImageManager.Instance.GetCheckImageBySide(Side);
        m_CheckerBG.sprite = image;
    }

    public void RegisterMoveFromEvent(Action<Chess> callback)
    {
        CurrentChess.RegisterSelectEvent(callback);
    }

    public void RegisterMoveToEvent(Action<Checker> callback)
    {
        OnMoveToEvent += callback;
    }

    private void OnSelect()
    {
        if (!CanMoveTo)
            return;

        OnMoveToEvent?.Invoke(this);
    }

    public void SetCanMoveFrom(bool active)
    {
        if (CurrentChess == null)
            return;

        CurrentChess.SetSelectable(active);
    }

    public void SetCanMoveTo(bool active)
    {
        CanMoveTo = active;
        m_Highlight.enabled = CanMoveTo;
    }

    public void ClearState()
    {
        SetCanMoveFrom(false);
        SetCanMoveTo(false);
        SetAllChecked(false);

        if (CurrentChess != null)
            CurrentChess.ClearState();
    }

    public void SetChess(Chess chess)
    {
        chess.transform.parent = transform;
        chess.ResetPos();
        chess.SetPos(XPos, YPos);
        CurrentChess = chess;
    }

    public Chess RemoveChess()
    {
        if (CurrentChess == null)
            return null;

        var chess = CurrentChess;
        CurrentChess = null;
        return chess;
    }

    public void SetAllChecked(bool check)
    {
        SetXChecked(check);
        SetYChecked(check);
    }

    public void SetDirChecked(Vector2Int dir)
    {
        if (dir == Vector2Int.left || dir == Vector2Int.right)
            SetXChecked(true);
        else if (dir == Vector2Int.up || dir == Vector2Int.down)
            SetYChecked(true);
    }

    public void SetXChecked(bool check)
    {
        XChecked = check;
    }

    public void SetYChecked(bool check)
    {
        YChecked = check;
    }

    public bool SetChessMaxMove(int maxMove)
    {
        if (CurrentChess == null)
            return false;

        return CurrentChess.SetMaxMove(maxMove);
    }
}
