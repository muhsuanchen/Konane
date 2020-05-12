using System;
using UnityEngine;
using UnityEngine.UI;

public class NotifyData
{
    public string Content;
    public string ConfirmText;
    public Action ConfirmEvent;
    public string CancelText;
    public Action CancelEvent;
}

public class Notify : MonoBehaviour
{
    public static string kBetterChoice = "Are you sure?\nYou have a better choice!";
    public static string kGameEnd = "Game Over!";

    [SerializeField]
    Text m_Content;
    [SerializeField]
    Text m_ConfirmText;
    [SerializeField]
    Button m_ConfirmButton;
    [SerializeField]
    Text m_CancelText;
    [SerializeField]
    Button m_CancelButton;

    Action mConfirmEvent;
    Action mCancelEvent;

    // Start is called before the first frame update
    void Start()
    {
        m_ConfirmButton.onClick.AddListener(OnConfirm);
        m_CancelButton.onClick.AddListener(OnCancel);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void InitNotify(NotifyData data)
    {
        m_Content.text = data.Content;
        m_ConfirmText.text = data.ConfirmText;
        mConfirmEvent = data.ConfirmEvent;
        m_CancelText.text = data.CancelText;
        mCancelEvent = data.CancelEvent;
    }

    void OnConfirm()
    {
        mConfirmEvent?.Invoke();
        Hide();
    }

    void OnCancel()
    {
        mCancelEvent?.Invoke();
        Hide();
    }
}
