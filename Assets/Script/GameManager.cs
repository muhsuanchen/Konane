using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static bool ShowHint = false;

    [SerializeField]
    GameObject m_MenuPanel;
    [SerializeField]
    Button m_StartButton;

    [SerializeField]
    GameObject m_GamePanel;
    [SerializeField]
    Checker m_CheckerSample;
    [SerializeField]
    Transform m_CheckerPool;
    [SerializeField]
    Chess m_ChessSample;
    [SerializeField]
    Transform m_ChessPool;
    [SerializeField]
    Transform m_BoardRoot;
    [SerializeField]
    GridLayoutGroup m_BoardLayout;
    [SerializeField]
    Button m_ShowHintButton;
    [SerializeField]
    Button m_BackToMenuButton;

    [SerializeField]
    Notify m_Notify;

    [SerializeField]
    int m_BoardSize = 8;    // 須為偶數

    float mBoardWidth = 900;

    PoolBase<Checker> mCheckerPool;
    PoolBase<Chess> mChessPool;

    Checker[,] mCheckerArray;
    List<Checker> mEmptyChecker;

    event Action OnNextRound;
    event Action OnRoundEnd;
    event Action<int> OnChessSelect;

    int mRound = 0;
    bool mCurrentSide = false;
    string mCurrentSideName => (mCurrentSide) ? "Black" : "White";
    string mLastSideName => (!mCurrentSide) ? "Black" : "White";

    Checker mCurSelectFrom;
    int mCurMaxMove;

    private void Awake()
    {
        ShowHint = false;

        mCheckerPool = new PoolBase<Checker>(m_CheckerSample, m_CheckerPool);
        mChessPool = new PoolBase<Chess>(m_ChessSample, m_ChessPool);
        mEmptyChecker = new List<Checker>();

        var checkerWidth = mBoardWidth / m_BoardSize;
        Debug.Log($"Board Width {mBoardWidth}, {checkerWidth}");
        m_BoardLayout.cellSize = new Vector2(checkerWidth, checkerWidth);

        m_StartButton.onClick.AddListener(OnStartGame);
        m_BackToMenuButton.onClick.AddListener(OnBackToMenu);
        m_ShowHintButton.onClick.AddListener(OnShowHint);
    }

    void Start()
    {
        m_MenuPanel.SetActive(true);
        m_GamePanel.SetActive(false);
        m_Notify.Hide();
    }

    void OnShowHint()
    {
        ShowHint = !ShowHint;
        var allUsing = mChessPool.GetAllUsing();
        foreach (var chess in allUsing)
        {
            chess.UpdateHintVisible();
        }
    }

    void OnBackToMenu()
    {
        m_MenuPanel.SetActive(true);
        m_GamePanel.SetActive(false);
    }

    void OnStartGame()
    {
        m_MenuPanel.SetActive(false);
        m_GamePanel.SetActive(true);
        InitGame();
    }

    void InitGame()
    {
        mEmptyChecker.Clear();
        mCheckerPool.RecycleAll();
        mChessPool.RecycleAll();

        mCheckerArray = new Checker[m_BoardSize, m_BoardSize];

        InitBoard();

        StartRound();
    }

    void InitBoard()
    {
        for (var y = 0; y < m_BoardSize; y++)
        {
            for (var x = 0; x < m_BoardSize; x++)
            {
                var checkerObj = mCheckerPool.GetObj();
                var checker = checkerObj.GetComponent<Checker>();
                checker.transform.parent = m_BoardRoot;
                checker.Init(x, y);
                mCheckerArray[x, y] = checker;

                var chessObj = mChessPool.GetObj();
                var chess = chessObj.GetComponent<Chess>();
                chess.transform.parent = checker.transform;
                chess.Init(x, y);
                checker.SetChess(chess);
            }
        }
    }

    #region Round
    void StartRound()
    {
        BlackFirstRound();
    }

    void BlackFirstRound()
    {
        mRound = 1;
        mCurrentSide = true;
        Debug.Log($"------- {mCurrentSideName} Round {mRound} -------");

        var firstPos = m_BoardSize - 1;
        var secondPos = m_BoardSize / 2;
        var thirdPos = secondPos - 1;
        SetFirstRoundMovableChess(firstPos, firstPos);
        SetFirstRoundMovableChess(secondPos, secondPos);
        SetFirstRoundMovableChess(thirdPos, thirdPos);
        SetFirstRoundMovableChess(0, 0);

        OnNextRound = WhiteFirstRound;
    }

    void WhiteFirstRound()
    {
        mRound = 1;
        mCurrentSide = false;
        Debug.Log($"------- {mCurrentSideName} Round {mRound} -------");

        var emptyPos = mEmptyChecker[0].Pos;
        var upPos = emptyPos + Vector2Int.up;
        var downPos = emptyPos + Vector2Int.down;
        var leftPos = emptyPos + Vector2Int.left;
        var rightPos = emptyPos + Vector2Int.right;
        SetFirstRoundMovableChess(upPos);
        SetFirstRoundMovableChess(downPos);
        SetFirstRoundMovableChess(leftPos);
        SetFirstRoundMovableChess(rightPos);

        OnNextRound = NextRound;
    }

    void NextRound()
    {
        mCurrentSide = !mCurrentSide;
        mRound += (mCurrentSide) ? 1 : 0;
        Debug.Log($"------- {mCurrentSideName} Round {mRound} -------");

        mCurMaxMove = 0;
        if (!CheckMovablePath(mCurrentSide))
        {
            GameEnd();
            return;
        }

        OnNextRound = NextRound;
    }

    void RoundEnd()
    {
        OnRoundEnd?.Invoke();
        OnRoundEnd = null;
        mCurSelectFrom = null;
    }

    void GameEnd()
    {
        var content = Notify.kGameEnd + $"\n Winner is {mLastSideName}!";
        m_Notify.InitNotify(new NotifyData
        {
            Content = content,
            ConfirmText = "Again!",
            ConfirmEvent = InitGame,
            CancelText = "Fine...",
            CancelEvent = null,
        });

        m_Notify.Show();
    }

    #endregion Round

    #region Path Check
    bool CheckMovablePath(bool side)
    {
        foreach (var emptyChecker in mEmptyChecker)
        {
            emptyChecker.ClearState();
        }

        IsMoveValidCheckTime = 0;
        bool haveValidPath = false;
        foreach (var emptyChecker in mEmptyChecker)
        {
            //Debug.Log($"Check emptyChecker {emptyChecker.Pos}");
            haveValidPath |= TraceAllDirection(side, emptyChecker);
        }

        Debug.Log($"CurMaxMove {mCurMaxMove}, IsMoveValidCheckTime {IsMoveValidCheckTime}");
        return haveValidPath;
    }

    bool TraceAllDirection(bool side, Checker checker)
    {
        //Debug.Log($"Check Same Side? {checker.Side == side}");
        // 只能採在同隊的格子上
        if (checker.Side != side)
            return false;

        var canMoveTo = false;

        // 對面沒有棋，繼續追
        if (!checker.XChecked)
        {
            var leftDepth = 0;
            var rightDepth = 0;
            canMoveTo |= TraceMovablePath(side, checker, Vector2Int.left, ref leftDepth, out var moveFromLeft);
            canMoveTo |= TraceMovablePath(side, checker, Vector2Int.right, ref rightDepth, out var moveFromRight);

            var xDepth = leftDepth + rightDepth + 1;
            SetMaxMove(moveFromLeft, xDepth);
            SetMaxMove(moveFromRight, xDepth);
        }

        if (!checker.YChecked)
        {
            var upDepth = 0;
            var downDepth = 0;
            canMoveTo |= TraceMovablePath(side, checker, Vector2Int.up, ref upDepth, out var moveFromUp);
            canMoveTo |= TraceMovablePath(side, checker, Vector2Int.down, ref downDepth, out var moveFromDown);

            var yDepth = upDepth + downDepth + 1;
            SetMaxMove(moveFromUp, yDepth);
            SetMaxMove(moveFromDown, yDepth);
        }

        if (canMoveTo)
        {
            //Debug.Log($"Highlight To {checker.YPos}, {checker.XPos}");
            SetCheckerCanMoveTo(checker, CheckerSelect);
        }

        checker.SetAllChecked(true);

        return canMoveTo;
    }

    int IsMoveValidCheckTime = 0;

    bool TraceMovablePath(bool side, Checker checker, Vector2Int direction, ref int depth, out Checker moveFrom)
    {
        IsMoveValidCheckTime++;
        //Debug.Log($"Check {checker.Pos}, {direction}");
        moveFrom = null;

        var isMoveDirectionValid = IsMoveDirectionValid(checker.Pos, direction, out var nextChecker);
        //Debug.Log($"Check IsMoveValid? {isMoveDirectionValid}");
        if (!isMoveDirectionValid)
            return false;

        var nextCheckerHaveChess = nextChecker.CurrentChess != null;
        //Debug.Log($"Check nextCheckerHaveChess {nextCheckerHaveChess}");
        if (nextCheckerHaveChess)
        {
            // 對面有棋了，此條路線成立
            //Debug.Log($"Highlight From {nextChecker.Pos}");
            SetCheckerCanMoveFrom(nextChecker, ChessSelect);
            moveFrom = nextChecker;
            return true;
        }
        else
        {
            depth++;
            // 對面沒有棋，繼續追
            if (TraceMovablePath(side, nextChecker, direction, ref depth, out moveFrom))
            {
                //Debug.Log($"Highlight To {checker.YPos}, {checker.XPos}");
                SetCheckerCanMoveTo(checker, CheckerSelect);
                checker.SetDirChecked(direction);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 前方有棋子可以跳，且目標格子在棋盤範圍內
    /// </summary>
    bool IsMoveDirectionValid(Vector2Int pos, Vector2Int direction, out Checker nextChecker)
    {
        nextChecker = null;
        var nextPos = pos + direction * 2;
        var isNextPosValid = IsPosValid(nextPos.x, nextPos.y);
        if (!isNextPosValid)
            return false;

        //Debug.Log($"IsMoveValid crossPos {crossPos}, nextPos {nextPos}");
        nextChecker = mCheckerArray[nextPos.x, nextPos.y];

        // 是否有可以跨過去的棋
        var crossPos = pos + direction;
        return mCheckerArray[crossPos.x, crossPos.y].CurrentChess != null;
    }

    /// <summary>
    /// 位置是否在棋盤內
    /// </summary>
    bool IsPosValid(int x, int y)
    {
        return x >= 0 && x < m_BoardSize
            && y >= 0 && y < m_BoardSize;
    }

    void SetMaxMove(Checker checker, int depth)
    {
        if (checker == null)
            return;

        checker.SetChessMaxMove(depth);
        UpdateMaxMove(depth);
    }

    void UpdateMaxMove(int newDepth)
    {
        if (newDepth > mCurMaxMove)
            mCurMaxMove = newDepth;
    }
    #endregion Path Check

    #region Select Event
    void SetFirstRoundMovableChess(Vector2Int pos)
    {
        SetFirstRoundMovableChess(pos.x, pos.y);
    }

    void SetFirstRoundMovableChess(int x, int y)
    {
        if (!IsPosValid(x, y))
            return;

        var checker = mCheckerArray[x, y];
        SetCheckerCanMoveFrom(checker, FirstRoundChessSelect);
    }

    void SetCheckerCanMoveFrom(Checker checker, Action<Chess> selectEvent)
    {
        if (checker.CanMoveFrom)
            return;

        var chess = checker.CurrentChess;
        checker.SetCanMoveFrom(true);
        checker.RegisterMoveFromEvent(selectEvent);
        OnChessSelect += chess.OnSomeoneSelected;
        OnRoundEnd += checker.ClearState;
        OnRoundEnd += chess.ClearState;
    }

    void SetCheckerCanMoveTo(Checker checker, Action<Checker> selectEvent)
    {
        if (checker.CanMoveTo)
            return;

        var chess = checker.CurrentChess;
        checker.SetCanMoveTo(true);
        checker.RegisterMoveToEvent(selectEvent);
        OnRoundEnd += checker.ClearState;
        if (chess != null)
        {
            OnRoundEnd += chess.ClearState;
        }
    }

    void FirstRoundChessSelect(Chess chess)
    {
        // Remove first chess
        RemoveChess(chess.Pos);
        OnChessSelect?.Invoke(chess.Index);

        RoundEnd();
        OnNextRound?.Invoke();
    }

    void ChessSelect(Chess chess)
    {
        OnChessSelect?.Invoke(chess.Index);

        var checker = mCheckerArray[chess.XPos, chess.YPos];
        mCurSelectFrom = checker;
    }

    void CheckerSelect(Checker checker)
    {
        TryMoveChess(mCurSelectFrom, checker);
    }

    void TryMoveChess(Checker fromChecker, Checker toChecker)
    {
        //Debug.Log($"TryMoveChess {fromChecker.Pos}, {toChecker.Pos}");
        if (fromChecker == null || toChecker == null)
            return;

        // 沒有可以移動的棋，或者對面有棋導致無法移動過去
        if (fromChecker.CurrentChess == null || toChecker.CurrentChess != null)
            return;

        if (!IsMoveValid(fromChecker, toChecker, out var moveTimes))
            return;

        void MoveAndEndRound()
        {
            //Debug.Log($"IsMoveValid MoveAndEndRound!");
            MoveChess(fromChecker, toChecker);
            RoundEnd();
            OnNextRound?.Invoke();
        }

        //Debug.Log($"IsMoveValid Can Jump Next? pos {toChecker.Pos}, dir {dir}");
        //             IsMoveDirectionValid(toChecker.Pos, dir, out var nextChecker) && nextChecker.CurrentChess == null)
        // 可以連跳
        if (mCurMaxMove > moveTimes)
        {
            m_Notify.InitNotify(new NotifyData
            {
                Content = Notify.kBetterChoice,
                ConfirmText = "Yes!",
                ConfirmEvent = MoveAndEndRound,
                CancelText = "No!",
                CancelEvent = null,
            });
            m_Notify.Show();
            Debug.Log($"IsMoveValid Can Jump Next? Yes!");
            return;
        }

        MoveAndEndRound();
        return;
    }

    bool IsMoveValid(Checker fromChecker, Checker toChecker, out int moveTimes)
    {
        moveTimes = 0;

        //Debug.Log($"IsMoveValid CanMoveFrom {fromChecker.CanMoveFrom}, CanMoveTo {toChecker.CanMoveTo}");
        if (!fromChecker.CanMoveFrom)
            return false;

        if (!toChecker.CanMoveTo)
            return false;

        var direction = toChecker.Pos - fromChecker.Pos;
        var distance = (int)direction.magnitude;
        var normalizedDir = direction / distance;
        //Debug.Log($"IsMoveValid IsMoveDirectionValid? dir {normalizedDir}, distance {distance}");
        if (!IsMoveDirectionValid(normalizedDir))
            return false;

        if (distance % 2 != 0 || distance <= 0)
            return false;

        //Debug.Log($"IsMoveValid IsPathValid? dir {normalizedDir}, distance {distance}");
        if (!IsPathValid(fromChecker.Pos, normalizedDir, distance))
            return false;

        moveTimes = distance / 2;
        //Debug.Log($"IsMoveValid IsPathValid? Yes!");
        return true;
    }

    bool IsPathValid(Vector2Int from, Vector2Int direction, int distance)
    {
        //Debug.Log($"IsPathValid from {from}, direction {direction}, distance {distance}");
        // 順利走完了，回家ㄅ
        if (distance <= 0)
            return true;

        var crossPos = from + direction;
        var nextPos = from + direction * 2;
        //Debug.Log($"IsPathValid crossPos {crossPos}, nextPos {nextPos}");

        var crossChecker = mCheckerArray[crossPos.x, crossPos.y];
        // 沒有棋可以吃
        if (crossChecker.CurrentChess == null)
            return false;

        var nextChecker = mCheckerArray[nextPos.x, nextPos.y];
        // 沒有空位可以跳
        if (nextChecker.CurrentChess != null)
            return false;

        distance -= 2;
        return IsPathValid(nextPos, direction, distance);
    }

    bool IsMoveDirectionValid(Vector2Int direction)
    {
        return direction == Vector2Int.up
            || direction == Vector2Int.down
            || direction == Vector2Int.left
            || direction == Vector2Int.right;
    }

    void MoveChess(Checker fromChecker, Checker toChecker)
    {
        var direction = toChecker.Pos - fromChecker.Pos;
        var distance = (int)direction.magnitude;
        var normalizedDir = direction / distance;

        while (distance > 0)
        {
            // Remove Cross Chess
            var crossPos = fromChecker.Pos + normalizedDir * (distance - 1);
            RemoveChess(crossPos);
            distance -= 2;
        }

        // Move My Chess
        var moveChess = fromChecker.RemoveChess();
        toChecker.SetChess(moveChess);
        mEmptyChecker.Remove(toChecker);
        mEmptyChecker.Add(fromChecker);
    }

    void RemoveChess(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
            return;

        var checker = mCheckerArray[pos.x, pos.y];
        var chess = checker.RemoveChess();
        mEmptyChecker.Add(checker);
        mChessPool.Recycle(chess);
        Debug.Log($"Empty {checker.XPos}, {checker.YPos}");
    }
    #endregion Select Event

    class PoolBase<T> where T : GameObj
    {
        Queue<T> mPool = new Queue<T>();
        List<T> mUsing = new List<T>();
        Transform mPoolRoot;
        T mSample;

        public PoolBase(T sample, Transform root)
        {
            mSample = sample;
            mPoolRoot = root;
        }

        public T GetObj()
        {
            T obj;
            if (mPool.Count == 0)
            {
                obj = Instantiate(mSample);
            }
            else
            {
                obj = mPool.Dequeue();
            }

            mUsing.Add(obj);
            return obj;
        }

        public List<T> GetAllUsing()
        {
            return mUsing;
        }

        public void Recycle(T obj)
        {
            if (!mUsing.Contains(obj))
                return;

            obj.Recycle();
            obj.transform.parent = mPoolRoot;
            mPool.Enqueue(obj);
            mUsing.Remove(obj);
        }

        public void RecycleAll()
        {
            foreach (var obj in mUsing)
            {
                obj.Recycle();
                obj.transform.parent = mPoolRoot;
                mPool.Enqueue(obj);
            }
            mUsing.Clear();
        }

        public void Clear()
        {
            foreach (var obj in mUsing)
            {
                obj.Recycle();
                Destroy(obj);
            }
            mUsing.Clear();

            foreach (var obj in mPool)
            {
                obj.Recycle();
                Destroy(obj);
            }
            mPool.Clear();
        }
    }
}

