using System;
using UnityEngine;
using UnityEngine.UI;

public class Chess : GameObj
{
    [SerializeField]
    Image m_Chess;
    [SerializeField]
    Image m_Selecting;
    [SerializeField]
    Button m_Button;
    [SerializeField]
    Text m_MaxMoveText;

    RectTransform mRectTrans;
    Action<Chess> OnSelectEvent;
    public bool Selectable { get; private set; }
    int mCurMaxMove = 0;

    public override void Init(int x, int y)
    {
        base.Init(x, y);

        gameObject.name = $"Chess ({x}-{y})";

        var color = Side ? Color.black : Color.white;
        m_Chess.color = color;

        m_Button.onClick.AddListener(OnSelect);

        mRectTrans = gameObject.GetComponent<RectTransform>();
        mRectTrans.localScale = Vector3.one;

        UpdateHintVisible();

        ResetPos();
    }

    public void UpdateHintVisible()
    {
        m_MaxMoveText.enabled = GameManager.ShowHint;
    }

    public void ResetPos()
    {
        mRectTrans.offsetMin = new Vector2(2, 2); // new Vector2(left, bottom);
        mRectTrans.offsetMax = new Vector2(-2, -2); // new Vector2(-right, -top);    
    }

    private void OnSelect()
    {
        if (!Selectable)
            return;

        Selecting(true);
        OnSelectEvent?.Invoke(this);
    }

    public void RegisterSelectEvent(Action<Chess> callback)
    {
        OnSelectEvent += callback;
    }

    public void OnSomeoneSelected(int index)
    {
        if (index == Index)
            return;

        Selecting(false);
    }

    public override void Recycle()
    {
        m_Button.onClick.RemoveAllListeners();
        ClearState();

        SetPos(-1, -1);
        Index = -1;

        base.Recycle();
    }

    public void ClearState()
    {
        ClearMaxMove();
        Selecting(false);
        OnSelectEvent = null;
    }

    public void SetSelectable(bool active)
    {
        Selectable = active;
    }

    public void Selecting(bool active)
    {
        m_Selecting.enabled = active;
    }

    public void ClearMaxMove()
    {
        mCurMaxMove = 0;
        UpdateMaxMoveText();
    }

    public bool SetMaxMove(int maxMove)
    {
        if (maxMove <= mCurMaxMove)
            return false;

        mCurMaxMove = maxMove;
        UpdateMaxMoveText();
        return true;
    }

    void UpdateMaxMoveText()
    {
        if (mCurMaxMove <= 0)
            m_MaxMoveText.text = string.Empty;
        else
            m_MaxMoveText.text = $"{mCurMaxMove}";
    }
}
