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

        [SerializeField]
        Button m_StartLastGameButton;

        protected override void Awake()
        {
            m_Version.text = $"v{Version.VERSION}";

            m_StartButton.onClick.AddListener(SwitchToGameScene);
            m_StartLastGameButton.onClick.AddListener(StartLastGameWithSize);

            DontDestroyOnLoad(m_Undead);
        }

        void Start()
        {
            m_StartLastGameButton.interactable = GameStateRecorder.HaveGameRecord();
        }

        void StartLastGameWithSize()
        {
            GameSetting.Instance.SetStartWithRecord(true);

            SwitchToGameScene();
        }

        void SwitchToGameScene()
        {
            SceneManager.LoadScene(Scenes.GameScene.ToString(), LoadSceneMode.Single);
        }
    }
}