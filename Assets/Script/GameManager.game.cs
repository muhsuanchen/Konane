using UnityEngine;
using UnityEngine.UI;

public partial class GameManager
{
    public static bool ShowHint = false;

    [SerializeField]
    GameObject m_GamePanel;
    [SerializeField]
    Transform m_BoardRoot;
    [SerializeField]
    GridLayoutGroup m_BoardLayout;

    [SerializeField]
    Image m_ShowHintImage;
    [SerializeField]
    Button m_ShowHintButton;
    [SerializeField]
    Button m_BackToMenuButton;

    int m_BoardSize = 8;    // 須為偶數
    float mBoardWidth = 900;
    Color kNormalBtnColor = Color.white;
    Color kSelectBtnColor = Color.red;

    void InitGame()
    {
        var checkWidth = mBoardWidth / m_BoardSize;
        Debug.Log($"Board Width {mBoardWidth}, {checkWidth}");
        m_BoardLayout.cellSize = new Vector2(checkWidth, checkWidth);

        m_BackToMenuButton.onClick.AddListener(OnBackToMenu);
        m_ShowHintButton.onClick.AddListener(OnShowHint);
        UpdateBtnColor();
    }

    void ShowGame()
    {
        m_GamePanel.SetActive(true);
    }

    void HideGame()
    {
        m_GamePanel.SetActive(false);
    }

    void OnShowHint()
    {
        ShowHint = !ShowHint;
        UpdateBtnColor();

        var allBlackUsing = mBlackChessPool.GetAllUsing();
        foreach (var chess in allBlackUsing)
        {
            chess.UpdateHintVisible();
        }

        var allWhiteUsing = mWhiteChessPool.GetAllUsing();
        foreach (var chess in allWhiteUsing)
        {
            chess.UpdateHintVisible();
        }
    }

    void UpdateBtnColor()
    {
        m_ShowHintImage.color = (ShowHint) ? kSelectBtnColor : kNormalBtnColor;
    }
}
