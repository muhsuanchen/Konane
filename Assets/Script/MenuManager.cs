using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TrainingProject
{
    public class MenuManager : MonoSingleton<MenuManager>
    {
        [SerializeField]
        Text m_Version;

        [SerializeField]
        Button m_StartButton;

        [SerializeField]
        Button m_StartLastGameButton;

        [SerializeField]
        Button m_ExitButton;

        protected override void Awake()
        {
            m_Version.text = $"v{Version.VERSION}";

            m_StartButton.onClick.AddListener(StartNewGame);
            m_StartLastGameButton.onClick.AddListener(ResumeLastGame);
            m_ExitButton.onClick.AddListener(ShowExitGameNotify);
        }

        void Start()
        {
            m_StartLastGameButton.interactable = GameStateRecorder.HaveGameRecord();
        }

        void StartNewGame()
        {
            GameSetting.Instance.SetStartWithRecord(false);
            SwitchToGameScene();
        }

        void ResumeLastGame()
        {
            GameSetting.Instance.SetStartWithRecord(true);
            SwitchToGameScene();
        }

        void SwitchToGameScene()
        {
            SceneManager.LoadScene(Scenes.GameScene.ToString(), LoadSceneMode.Single);
        }

        void ShowExitGameNotify()
        {
            Notify.Instance.InitNotify(new NotifyData
            {
                Content = "Sure to close app?",
                ConfirmText = "Yes!",
                ConfirmEvent = ExitGame,
                CancelText = "Noooooooo",
                CancelEvent = null,
            });

            Notify.Instance.Show();
        }

        void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}