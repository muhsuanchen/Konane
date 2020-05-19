using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TrainingProject
{
    public enum GameSide
    {
        BLACK = 1,  // true
        WHITE = 0,  // false
    }

    public partial class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField]
        Text m_Version;

        [SerializeField]
        GameObject m_GamePanel;
        [SerializeField]
        Transform m_BoardRoot;
        [SerializeField]
        GridLayoutGroup m_BoardLayout;

        [SerializeField]
        Text m_Title;
        [SerializeField]
        Image m_ShowHintImage;
        [SerializeField]
        Button m_ShowHintButton;
        [SerializeField]
        Button m_BackToMenuButton;

        Check[,] mCheckArray;
        List<Check> mEmptyCheck;

        int mBoardSize = 6;
        float mBoardWidth = 900;

        event Action OnNextRound;
        event Action OnRoundEnd;
        event Action<Vector2Int> OnChessSelect;

        public int Round { get; private set; } = 0;

        public bool CurrentSide { get; private set; } = false;
        string mCurrentSideName => (CurrentSide) ? GameSide.BLACK.ToString() : GameSide.WHITE.ToString();
        string mLastSideName => (!CurrentSide) ? GameSide.BLACK.ToString() : GameSide.WHITE.ToString();

        List<Check> mAllMovableCheck = new List<Check>();

        Check mCurSelectFrom;
        int mCurMaxMove = 0;
        bool mIsGameEnd = false;

        Color mNormalBtnColor = Color.white;
        Color mSelectBtnColor = Color.red;

        protected override void Awake()
        {
            mEmptyCheck = new List<Check>();

            m_Version.text = $"v{Version.VERSION}";

            InitPool();
            InitGame();
        }

        private void Start()
        {
            if (GameSetting.Instance.ResumeLastGame)
                ResumeLastGame();
            else
                ShowBoardSizeSelectorAndStart();
        }

        #region Btn Event
        void OnShowHint()
        {
            GameSetting.Instance.SwitchShowHint();
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

        void OnBackToMenu()
        {
            if (mIsGameEnd)
            {
                Notify.Instance.InitNotify(new NotifyData
                {
                    Content = "Sure to back to menu?",
                    ConfirmText = "Yah!",
                    ConfirmEvent = LeaveGame,
                    CancelText = "Nah, wait...",
                    CancelEvent = null,
                });
            }
            else
            {
                Notify.Instance.InitNotify(new NotifyData
                {
                    Content = "Sure to leave the game?",
                    ConfirmText = "Let me go!",
                    ConfirmEvent = LeaveGame,
                    CancelText = "Nah, wait...",
                    CancelEvent = null,
                });
            }

            Notify.Instance.Show();
        }
        #endregion

        void InitGame()
        {
            m_Title.text = string.Empty;
            m_BackToMenuButton.onClick.AddListener(OnBackToMenu);
            m_ShowHintButton.onClick.AddListener(OnShowHint);
            UpdateBtnColor();
        }

        void LeaveGame()
        {
            ClearGame();

            SceneManager.LoadScene(Scenes.MenuScene.ToString(), LoadSceneMode.Single);
        }

        void ClearGame()
        {
            RecycleAllToPool();

            mCurMaxMove = 0;
            mEmptyCheck.Clear();
            mAllMovableCheck.Clear();
            mCurSelectFrom = null;
            OnNextRound = null;
        }

        void ResumeLastGame()
        {
            mIsGameEnd = false;

            if (!TryLoadGameFromPref())
            {
                Notify.Instance.InitNotify(new NotifyData
                {
                    Content = "Load Failed.\nStart a new game?",
                    ConfirmText = "OK!",
                    ConfirmEvent = ShowBoardSizeSelectorAndStart,
                    CancelText = "Back to menu.",
                    CancelEvent = LeaveGame,
                });

                Notify.Instance.Show();
            }

            if (Round == 0)
                StartRound();
            else if (Round == 1 && CurrentSide)
                WhiteFirstRound();
            else
                NextRound();
        }

        void ShowBoardSizeSelectorAndStart()
        {
            GameSetting.Instance.SetStartWithRecord(false);

            Notify.Instance.InitNotify(new NotifyData
            {
                Content = "Please choose a board size.",
                ConfirmText = "6x6",
                ConfirmEvent = () => StartGame(6),
                CancelText = "8x8",
                CancelEvent = () => StartGame(8),
            });

            Notify.Instance.Show();
        }

        void StartGame(int size)
        {
            mIsGameEnd = false;

            ClearRecord();

            mBoardSize = size;
            mCheckArray = new Check[mBoardSize, mBoardSize];

            InitBoard();

            StartRound();
        }

        void InitBoard()
        {
            OnRoundEnd = null;
            OnChessSelect = null;

            UpdateBoardSize();

            for (var x = 0; x < mBoardSize; x++)
            {
                for (var y = 0; y < mBoardSize; y++)
                {
                    // true = (xy相加)偶數格, false = (xy相加)奇數格
                    var side = GetSideFromXY(x, y);

                    var checkObj = GetCheck(side);
                    var check = checkObj.GetComponent<Check>();
                    check.transform.parent = m_BoardRoot;
                    check.Init(x, y);
                    mCheckArray[x, y] = check;

                    var chessObj = GetChess(side);
                    var chess = chessObj.GetComponent<Chess>();
                    chess.transform.parent = check.transform;
                    chess.Init(x, y);
                    check.SetChess(chess, ChessSelect, ChessRemove);
                    OnChessSelect += check.OnSomeChessSelected;

                    OnRoundEnd += check.ClearState;
                }
            }
        }

        void UpdateBoardSize()
        {
            var checkWidth = mBoardWidth / mBoardSize;
            Debug.Log($"Board Width {mBoardWidth}, {checkWidth}");
            m_BoardLayout.cellSize = new Vector2(checkWidth, checkWidth);
        }

        void UpdateBtnColor()
        {
            m_ShowHintImage.color = (GameSetting.Instance.ShowHint)
                                    ? mSelectBtnColor
                                    : mNormalBtnColor;
        }

        #region Round
        void StartRound()
        {
            BlackFirstRound();
        }

        void BlackFirstRound()
        {
            Round = 1;
            CurrentSide = true;
            m_Title.text = $"Round {Round} {mCurrentSideName}";

            var firstPos = mBoardSize - 1;
            var secondPos = mBoardSize / 2;
            var thirdPos = secondPos - 1;
            SetCheckCanRemove(firstPos, firstPos);
            SetCheckCanRemove(secondPos, secondPos);
            SetCheckCanRemove(thirdPos, thirdPos);
            SetCheckCanRemove(0, 0);

            OnNextRound = WhiteFirstRound;
        }

        void WhiteFirstRound()
        {
            Round = 1;
            CurrentSide = false;
            m_Title.text = $"Round {Round} {mCurrentSideName}";

            var emptyPos = mEmptyCheck[0].Pos;
            var upPos = emptyPos + Vector2Int.up;
            var downPos = emptyPos + Vector2Int.down;
            var leftPos = emptyPos + Vector2Int.left;
            var rightPos = emptyPos + Vector2Int.right;
            SetCheckCanRemove(upPos);
            SetCheckCanRemove(downPos);
            SetCheckCanRemove(leftPos);
            SetCheckCanRemove(rightPos);

            OnNextRound = NextRound;
        }

        void NextRound()
        {
            CurrentSide = !CurrentSide;
            Round += (CurrentSide) ? 1 : 0;
            m_Title.text = $"Round {Round} {mCurrentSideName}";

            if (!CheckMovablePath(CurrentSide))
            {
                GameEnd();
                return;
            }

            OnNextRound = NextRound;
        }

        void RoundEnd()
        {
            OnRoundEnd?.Invoke();

            mCurMaxMove = 0;
            mAllMovableCheck.Clear();
            mCurSelectFrom = null;

            RecordCurrentGameToPref();
        }

        void GameEnd()
        {
            mIsGameEnd = true;

            ClearRecord();

            var winnerText = $"Winner is {mLastSideName}!";
            m_Title.text = winnerText;

            var notifyText = $"{Notify.kGameEnd}\n{winnerText}";
            Notify.Instance.InitNotify(new NotifyData
            {
                Content = notifyText,
                ConfirmText = "OK",
                ConfirmEvent = LeaveGame,
                CancelText = "See Board",
                CancelEvent = null,
            });

            Notify.Instance.Show();
        }

        #endregion Round

        #region Path Check
        bool CheckMovablePath(bool side)
        {
            foreach (var emptyCheck in mEmptyCheck)
            {
                emptyCheck.ClearState();
            }

            IsMoveValidCheckTime = 0;
            bool haveValidPath = false;
            foreach (var emptyCheck in mEmptyCheck)
            {
                //Debug.Log($"Check emptyCheck {emptyCheck.Pos}");
                haveValidPath |= TraceAllDirection(side, emptyCheck);
            }

            Debug.Log($"CurMaxMove {mCurMaxMove}, IsMoveValidCheckTime {IsMoveValidCheckTime}");
            return haveValidPath;
        }

        bool TraceAllDirection(bool side, Check check)
        {
            //Debug.Log($"Check Same Side? {Check.Side == side}");
            // 只能採在同隊的格子上
            if (check.Side != side)
                return false;

            var canMoveTo = false;

            // 對面沒有棋，繼續追
            if (!check.XChecked)
            {
                var leftDepth = 0;
                var rightDepth = 0;
                var leftMovePath = new LinkedList<Check>();
                var rightMovePath = new LinkedList<Check>();
                canMoveTo |= TraceMovablePath(side, check, Vector2Int.left, ref leftDepth, ref leftMovePath, out var moveFromLeft);
                canMoveTo |= TraceMovablePath(side, check, Vector2Int.right, ref rightDepth, ref rightMovePath, out var moveFromRight);

                leftMovePath.AddFirst(check);
                rightMovePath.AddFirst(check);

                var xDepth = leftDepth + rightDepth + 1;
                SetMovePathInfo(moveFromLeft, Vector2Int.left, rightMovePath, xDepth);  // 補上對面方向的 path
                SetMovePathInfo(moveFromRight, Vector2Int.right, leftMovePath, xDepth); // 補上對面方向的 path
            }

            if (!check.YChecked)
            {
                var upDepth = 0;
                var downDepth = 0;
                var upMovePath = new LinkedList<Check>();
                var downMovePath = new LinkedList<Check>();
                canMoveTo |= TraceMovablePath(side, check, Vector2Int.up, ref upDepth, ref upMovePath, out var moveFromUp);
                canMoveTo |= TraceMovablePath(side, check, Vector2Int.down, ref downDepth, ref downMovePath, out var moveFromDown);

                upMovePath.AddFirst(check);
                downMovePath.AddFirst(check);

                var yDepth = upDepth + downDepth + 1;
                SetMovePathInfo(moveFromUp, Vector2Int.up, downMovePath, yDepth);       // 補上對面方向的 path
                SetMovePathInfo(moveFromDown, Vector2Int.down, upMovePath, yDepth);     // 補上對面方向的 path
            }

            if (canMoveTo)
            {
                //Debug.Log($"Highlight To {Check.YPos}, {Check.XPos}");
                SetCheckCanMoveTo(check, CheckSelect);
            }

            check.SetAllChecked(true);

            return canMoveTo;
        }

        int IsMoveValidCheckTime = 0;

        bool TraceMovablePath(bool side, Check check, Vector2Int direction, ref int depth, ref LinkedList<Check> movePath, out Check moveFrom)
        {
            IsMoveValidCheckTime++;
            //Debug.Log($"Check {Check.Pos}, {direction}");
            moveFrom = null;

            var isMoveDirectionValid = IsMoveDirectionValid(check.Pos, direction, out var nextCheck);
            //Debug.Log($"Check IsMoveValid? {isMoveDirectionValid}");
            if (!isMoveDirectionValid)
                return false;

            var nextCheckHaveChess = nextCheck.CurrentChess != null;
            //Debug.Log($"Check nextCheckHaveChess {nextCheckHaveChess}");
            if (nextCheckHaveChess)
            {
                // 對面有棋了，此條路線成立
                //Debug.Log($"Highlight From {nextCheck.Pos}");
                SetCheckCanMoveFrom(nextCheck);
                moveFrom = nextCheck;
                return true;
            }
            else
            {
                depth++;
                // 對面沒有棋，繼續追
                if (TraceMovablePath(side, nextCheck, direction, ref depth, ref movePath, out moveFrom))
                {
                    movePath.AddFirst(check);    // 統計可移動的空格，方便之後 Apply 給可移動的旗子 □ → □ → □ → ●
                    if (moveFrom != null)
                        moveFrom.SetDirPath(direction, check);

                    //Debug.Log($"Highlight To {Check.YPos}, {Check.XPos}");
                    SetCheckCanMoveTo(check, CheckSelect);
                    check.SetDirChecked(direction);
                    return true;
                }
                else
                {
                    movePath.AddFirst(check);    // 統計可移動的空格，方便之後 Apply 給可移動的旗子 □ → □ → □ → ●
                }
            }

            return false;
        }

        /// <summary>
        /// 前方有棋子可以跳，且目標格子在棋盤範圍內
        /// </summary>
        bool IsMoveDirectionValid(Vector2Int pos, Vector2Int direction, out Check nextCheck)
        {
            nextCheck = null;
            var nextPos = pos + direction * 2;
            var isNextPosValid = IsPosValid(nextPos.x, nextPos.y);
            if (!isNextPosValid)
                return false;

            //Debug.Log($"IsMoveValid crossPos {crossPos}, nextPos {nextPos}");
            nextCheck = mCheckArray[nextPos.x, nextPos.y];

            // 是否有可以跨過去的棋
            var crossPos = pos + direction;
            return mCheckArray[crossPos.x, crossPos.y].CurrentChess != null;
        }

        /// <summary>
        /// 位置是否在棋盤內
        /// </summary>
        bool IsPosValid(int x, int y)
        {
            return x >= 0 && x < mBoardSize
                && y >= 0 && y < mBoardSize;
        }

        void SetMovePathInfo(Check check, Vector2Int direction, LinkedList<Check> movePath, int depth)
        {
            if (check == null)
                return;

            check.SetChessMaxMove(depth);
            check.SetDirPath(direction, movePath);
            UpdateMaxMove(depth);

            if (!mAllMovableCheck.Contains(check))
                mAllMovableCheck.Add(check);
        }

        void UpdateMaxMove(int newDepth)
        {
            if (newDepth > mCurMaxMove)
                mCurMaxMove = newDepth;
        }
        #endregion Path Check

        #region Remove Event
        void SetCheckCanRemove(Vector2Int pos)
        {
            SetCheckCanRemove(pos.x, pos.y);
        }

        void SetCheckCanRemove(int x, int y)
        {
            if (!IsPosValid(x, y))
                return;

            var check = mCheckArray[x, y];
            SetCheckCanRemove(check);
        }

        void SetCheckCanRemove(Check check)
        {
            if (check.CanRemove)
                return;

            check.SetCanRemove(true);
        }

        void ChessRemove(Chess chess)
        {
            Debug.Log($"ChessRemove {chess.Pos}");
            OnChessSelect?.Invoke(chess.Pos);

            if (mCurSelectFrom == null
                || mCurSelectFrom.Pos != chess.Pos)
            {
                var check = mCheckArray[chess.XPos, chess.YPos];
                mCurSelectFrom = check;
                return;
            }

            // Remove first chess
            RemoveChess(chess.Pos);

            RoundEnd();
            OnNextRound?.Invoke();
        }
        #endregion

        #region Move Event
        void SetCheckCanMoveFrom(Check check)
        {
            if (check.CanMoveFrom)
                return;

            check.SetCanMoveFrom(true);
        }

        void SetCheckCanMoveTo(Check check, Action<Check> selectEvent)
        {
            if (check.CanMoveTo)
                return;

            var chess = check.CurrentChess;
            check.SetCanMoveTo(true);
            check.RegisterMoveToEvent(selectEvent);
            OnRoundEnd += check.ClearState;
        }

        /// <summary>
        /// 選擇要操作的棋子
        /// </summary>
        void ChessSelect(Chess chess)
        {
            OnChessSelect?.Invoke(chess.Pos);

            var check = mCheckArray[chess.XPos, chess.YPos];
            mCurSelectFrom = check;
        }

        /// <summary>
        /// 選擇要跳往的棋格
        /// </summary>
        void CheckSelect(Check check)
        {
            TryMoveChess(mCurSelectFrom, check);
        }
        #endregion

        #region Move Chess
        void TryMoveChess(Check fromCheck, Check toCheck)
        {
            //Debug.Log($"TryMoveChess {fromCheck.Pos}, {toCheck.Pos}");
            if (fromCheck == null || toCheck == null)
                return;

            // 沒有可以移動的棋，或者對面有棋導致無法移動過去
            if (fromCheck.CurrentChess == null || toCheck.CurrentChess != null)
                return;

            if (!IsMoveValid(fromCheck, toCheck, out var moveTimes))
                return;

            void MoveAndEndRound()
            {
                MoveChess(fromCheck, toCheck);
                RoundEnd();
                OnNextRound?.Invoke();
            }

            // 可以連跳
            if (GameSetting.Instance.ShowHint && mCurMaxMove > moveTimes)
            {
                Notify.Instance.InitNotify(new NotifyData
                {
                    Content = Notify.kBetterChoice,
                    ConfirmText = "I'm sure.",
                    ConfirmEvent = MoveAndEndRound,
                    CancelText = "Wait!",
                    CancelEvent = null,
                });
                Notify.Instance.Show();
                return;
            }

            MoveAndEndRound();
            return;
        }

        bool IsMoveValid(Check fromCheck, Check toCheck, out int moveTimes)
        {
            moveTimes = 0;

            //Debug.Log($"IsMoveValid CanMoveFrom {fromCheck.CanMoveFrom}, CanMoveTo {toCheck.CanMoveTo}");
            if (!fromCheck.CanMoveFrom)
                return false;

            if (!toCheck.CanMoveTo)
                return false;

            var direction = toCheck.Pos - fromCheck.Pos;
            var distance = (int)direction.magnitude;
            var normalizedDir = direction / distance;
            //Debug.Log($"IsMoveValid IsMoveDirectionValid? dir {normalizedDir}, distance {distance}");
            if (!IsMoveDirectionValid(normalizedDir))
                return false;

            if (!IsMoveDistanceValid(distance))
                return false;

            if (!IsMovePathValid(fromCheck.Pos, normalizedDir, distance))
                return false;

            moveTimes = distance / 2;
            return true;
        }

        bool IsMoveDirectionValid(Vector2Int direction)
        {
            return direction == Vector2Int.up
                || direction == Vector2Int.down
                || direction == Vector2Int.left
                || direction == Vector2Int.right;
        }

        bool IsMoveDistanceValid(int distance)
        {
            return distance > 0 && distance % 2 == 0;
        }

        bool IsMovePathValid(Vector2Int from, Vector2Int direction, int distance)
        {
            //Debug.Log($"IsPathValid from {from}, direction {direction}, distance {distance}");
            // 順利走完了，回家ㄅ
            if (distance <= 0)
                return true;

            var crossPos = from + direction;
            var nextPos = from + direction * 2;
            //Debug.Log($"IsPathValid crossPos {crossPos}, nextPos {nextPos}");

            var crossCheck = mCheckArray[crossPos.x, crossPos.y];
            // 沒有棋可以吃
            if (crossCheck.CurrentChess == null)
                return false;

            var nextCheck = mCheckArray[nextPos.x, nextPos.y];
            // 沒有空位可以跳
            if (nextCheck.CurrentChess != null)
                return false;

            distance -= 2;
            return IsMovePathValid(nextPos, direction, distance);
        }

        void MoveChess(Check fromCheck, Check toCheck)
        {
            var direction = toCheck.Pos - fromCheck.Pos;
            var distance = (int)direction.magnitude;
            var normalizedDir = direction / distance;

            while (distance > 0)
            {
                // Remove Cross Chess
                var crossPos = fromCheck.Pos + normalizedDir * (distance - 1);
                RemoveChess(crossPos);
                distance -= 2;
            }

            // Move My Chess
            var moveChess = fromCheck.RemoveChess();
            toCheck.SetChess(moveChess, ChessSelect, ChessRemove);
            mEmptyCheck.Remove(toCheck);
            mEmptyCheck.Add(fromCheck);
        }

        void RemoveChess(Vector2Int pos)
        {
            if (pos.x < 0 || pos.y < 0)
                return;

            var check = mCheckArray[pos.x, pos.y];
            var chess = check.RemoveChess();
            mEmptyCheck.Add(check);
            RecycleChess(chess);
            Debug.Log($"Empty {check.XPos}, {check.YPos}");
        }
        #endregion Select Event
    }
}