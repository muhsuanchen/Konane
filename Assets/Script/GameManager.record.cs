namespace TrainingProject
{
    public partial class GameManager
    {
        void ClearRecord()
        {
            GameStateRecorder.ClearGameRecord();
        }

        void RecordCurrentGameToPref()
        {
            var gameState = new GameState();
            gameState.side = CurrentSide;
            gameState.round = Round;
            gameState.boardSize = mBoardSize;
            gameState.checks = new CheckState[mBoardSize * mBoardSize];
            for (int x = 0; x < mBoardSize; x++)
            {
                for (int y = 0; y < mBoardSize; y++)
                {
                    var check = mCheckArray[x, y];
                    var checkState = new CheckState()
                    {
                        x = check.XPos,
                        y = check.YPos,
                        haveChess = check.HaveChess,
                    };

                    var index = GetIndexFromXY(mBoardSize, x, y);
                    gameState.checks[index] = checkState;
                }
            }

            GameStateRecorder.RecordGameState(gameState);
        }

        bool TryLoadGameFromPref()
        {
            if (!GameStateRecorder.TryLoadGameState(out var gameState))
                return false;

            CurrentSide = gameState.side;
            Round = gameState.round;
            mBoardSize = gameState.boardSize;
            mCheckArray = new Check[mBoardSize, mBoardSize];

            OnRoundEnd = null;
            OnChessSelect = null;

            UpdateBoardSize();

            for (var x = 0; x < mBoardSize; x++)
            {
                for (var y = 0; y < mBoardSize; y++)
                {
                    var index = GetIndexFromXY(mBoardSize, x, y);
                    var checkState = gameState.checks[index];

                    // true = (xy相加)偶數格, false = (xy相加)奇數格
                    var side = GetSideFromXY(x, y);

                    var checkObj = GetCheck();
                    var check = checkObj.GetComponent<Check>();
                    check.transform.parent = m_BoardRoot;
                    check.Init(x, y);
                    mCheckArray[x, y] = check;

                    if (checkState.haveChess)
                    {
                        var chessObj = GetChess();
                        var chess = chessObj.GetComponent<Chess>();
                        chess.transform.parent = check.transform;
                        chess.Init(x, y);

                        check.SetChess(chess, ChessSelect, ChessRemove);
                        OnChessSelect += check.OnSomeChessSelected;
                    }
                    else
                    {
                        mEmptyCheck.Add(check);
                    }

                    OnRoundEnd += check.ClearState;
                }
            }

            return true;
        }

        int GetIndexFromXY(int size, int x, int y)
        {
            return x * size + y;
        }

        public static bool GetSideFromXY(int x, int y)
        {
            // true = (xy相加)偶數格, false = (xy相加)奇數格
            return (x + y) % 2 == 0;
        }

        public static GameSide GetGameSideFromXY(int x, int y)
        {
            return GetSideFromXY(x, y) ? GameSide.BLACK : GameSide.WHITE;
        }
    }
}