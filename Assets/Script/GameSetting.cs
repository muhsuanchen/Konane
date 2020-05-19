namespace TrainingProject
{
    public enum Scenes
    {
        MenuScene,
        GameScene,
    }

    public class GameSetting : MonoSingleton<GameSetting>
    {
        public bool ShowHint { get; private set; } = false;
        public bool ResumeLastGame { get; private set; } = false;

        public void SwitchShowHint()
        {
            ShowHint = !ShowHint;
        }

        public void SetShowHint(bool show)
        {
            ShowHint = show;
        }

        public void SetStartWithRecord(bool active)
        {
            ResumeLastGame = active;
        }
    }
}