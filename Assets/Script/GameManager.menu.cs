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

    void InitMenu()
    {
        m_StartButton.onClick.AddListener(OnStartGame);
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
