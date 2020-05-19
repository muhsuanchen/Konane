using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrainingProject
{
    public class Prefs : MonoSingleton<MenuManager>
    {
        const string kBoardSize = "BoardSize";
        const string kBoardState = "BoardState";

        public static int BoardSize
        {
            get => PlayerPrefs.GetInt(kBoardSize, -1);
            set => PlayerPrefs.SetInt(kBoardSize, value);
        }

        public static string BoardState
        {
            get => PlayerPrefs.GetString(kBoardState, string.Empty);
            set => PlayerPrefs.SetString(kBoardState, value);
        }
    }
}