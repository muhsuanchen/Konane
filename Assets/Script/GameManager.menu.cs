using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class GameManager
{
    [SerializeField]
    GameObject m_MenuPanel;
    [SerializeField]
    Button m_StartButton;
    [SerializeField]
    Text m_Version;

    void InitMenu()
    {
        m_Version.text = $"v{Version.VERSION}";
        m_StartButton.onClick.AddListener(ShowBoardSizeSelector);
    }

    void ShowMenu()
    {
        m_MenuPanel.SetActive(true);
    }

    void HideMenu()
    {
        m_MenuPanel.SetActive(false);
    }
}
