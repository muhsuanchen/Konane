using System;
using UnityEngine;
using UnityEngine.UI;

public class Chess : GameObj
{
    [SerializeField]
    Image m_Chess;
    [SerializeField]
    Image m_Highlight;  // Can Move
    [SerializeField]
    Image m_Selecting;
    [SerializeField]
    Image m_Remove;
    [SerializeField]
    Button m_Button;
    [SerializeField]
    Text m_MaxMoveText;

    RectTransform mRectTrans;
    Action SelectHintEvent;
    Action<Chess> OnSelectEvent;
    Action<Chess> OnRemoveEvent;
    public bool Selectable { get; private set; }
    public bool Removable { get; private set; }
    int mCurMaxMove = 0;

    public override void Init(int x, int y)
    {
        base.Init(x, y);

        gameObject.name = $"Chess ({x}-{y})";

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

    public override void Recycle()
    {
        ClearState();
        ClearEvent();
        SetPos(-1, -1);

        m_Button.onClick.RemoveAllListeners();

        base.Recycle();
    }

    public void ClearEvent()
    {
        SelectHintEvent = null;
        OnSelectEvent = null;
        OnRemoveEvent = null;
    }

    public void ClearState()
    {
        ClearMaxMove();
        Selecting(false);
        SetRemovable(false);
        SetSelectable(false);
    }

    #region Event
    public void RegisterSelectHintEvent(Action callback)
    {
        SelectHintEvent += callback;
    }

    public void RegisterSelectEvent(Action<Chess> callback)
    {
        OnSelectEvent += callback;
    }

    public void RegisterRemoveEvent(Action<Chess> callback)
    {
        OnRemoveEvent += callback;
    }

    private void OnSelect()
    {
        //if (!Selectable)
        //    return;

        if (GameManager.Instance.CurrentSide != Side)
            return;

        Debug.Log($"OnSelect {Pos} **********");
        Selecting(true);
    }

    public void OnOtherChessSelected()
    {
        Selecting(false);
    }
    #endregion

    #region State
    public void SetRemovable(bool active)
    {
        Removable = active;
        m_Highlight.enabled = active;
    }

    public void SetSelectable(bool active)
    {
        Selectable = active;
        m_Highlight.enabled = active;
    }

    public void Selecting(bool active)
    {
        if (GameManager.Instance.Round == 1)
        {
            //Debug.Log($"Selecting {Pos} = {active} && {Removable}");
            var remove = Removable && active;
            m_Remove.enabled = remove;
            if (remove) OnRemoveEvent?.Invoke(this);
        }
        else
        {
            m_Selecting.enabled = active;
            if (active)
            {
                OnSelectEvent?.Invoke(this);
                SelectHintEvent?.Invoke();
            }
        }
    }
    #endregion

    #region Path
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

    public void ClearMaxMove()
    {
        mCurMaxMove = 0;
        UpdateMaxMoveText();
    }
    #endregion
}
