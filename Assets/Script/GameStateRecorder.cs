using System;
using UnityEngine;

namespace TrainingProject
{
    [Serializable]
    public class GameState
    {
        public bool side = false;
        public int round = 0;
        public int boardSize = 6;
        public CheckState[] checks;
    }

    [Serializable]
    public class CheckState
    {
        public int x = -1;
        public int y = -1;
        public bool haveChess = false;
    }

    public class GameStateRecorder
    {
        public static void ClearGameRecord()
        {
            Prefs.BoardState = string.Empty;
        }
        public static bool HaveGameRecord()
        {
            return !string.IsNullOrEmpty(Prefs.BoardState);
        }

        public static void RecordGameState(GameState state)
        {
            var json = JsonUtility.ToJson(state);
            Debug.Log($"[Recorder] record json: {json}");
            Prefs.BoardState = json;
        }

        public static bool TryLoadGameState(out GameState record)
        {
            record = null;

            if (!HaveGameRecord())
                return false;

            try
            {
                var json = Prefs.BoardState;
                record = JsonUtility.FromJson<GameState>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Recorder] 解析失敗! 請確認成對的訊息。Exception：{e.Message}");
                return false;
            }

            return true;
        }
    }
}