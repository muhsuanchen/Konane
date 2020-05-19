using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrainingProject
{
    public class AppInitialization : MonoBehaviour
    {
        [SerializeField]
        GameObject m_Undead = null;

        private void Awake()
        {
            DontDestroyOnLoad(m_Undead);
        }

        void Start()
        {
            SceneManager.LoadScene(Scenes.MenuScene.ToString(), LoadSceneMode.Single);
        }
    }
}