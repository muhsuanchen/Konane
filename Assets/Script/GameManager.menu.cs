using UnityEngine;
using UnityEngine.UI;

namespace TrainingProject
{
    public partial class GameManager
    {
        [SerializeField]
        GameObject m_MenuPanel;
        [SerializeField]
        Button m_StartButton;

        void InitMenu()
        {
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
}