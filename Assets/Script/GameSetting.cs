using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrainingProject
{
    public enum Scenes
    {
        MenuScene,
        GameScene,
    }

    public class GameSetting : MonoSingleton<GameSetting>
    {
        public int BoardSize { get; private set; } = 6;
        public bool ShowHint { get; private set; } = false;

        public void SetBoardSize(int size)
        {
            if (size != 6 && size != 8)
                return;

            BoardSize = size;
        }

        public void SwitchShowHint()
        {
            ShowHint = !ShowHint;
        }

        public void SetShowHint(bool show)
        {
            ShowHint = show;
        }
    }
}