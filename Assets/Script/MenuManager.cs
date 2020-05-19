using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TrainingProject
{
    public class MenuManager : MonoSingleton<MenuManager>
    {
        [SerializeField]
        GameObject m_Undead = null;

        [SerializeField]
        Text m_Version;

        [SerializeField]
        Button m_StartButton;

        protected override void Awake()
        {
            m_Version.text = $"v{Version.VERSION}";

            m_StartButton.onClick.AddListener(ShowBoardSizeSelector);

            DontDestroyOnLoad(m_Undead);
        }

        void ShowBoardSizeSelector()
        {
            Notify.Instance.InitNotify(new NotifyData
            {
                Content = "Please choose a board size.",
                ConfirmText = "6x6",
                ConfirmEvent = () => StartGameWithSize(6),
                CancelText = "8x8",
                CancelEvent = () => StartGameWithSize(8),
            });

            Notify.Instance.Show();
        }

        void StartGameWithSize(int size)
        {
            GameSetting.Instance.SetBoardSize(size);
            SceneManager.LoadScene(Scenes.GameScene.ToString(), LoadSceneMode.Single);
        }
    }
}