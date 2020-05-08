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
    Check m_CheckSample;
    [SerializeField]
    Transform m_CheckPool;
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

    PoolBase<Check> mCheckPool;
    PoolBase<Chess> mChessPool;

    Check[,] mCheckArray;
    List<Check> mEmptyCheck;

    event Action OnNextRound;
    event Action OnRoundEnd;
    event Action<int> OnChessSelect;

    int mRound = 0;
    bool mCurrentSide = false;
    string mCurrentSideName => (mCurrentSide) ? "Black" : "White";
    string mLastSideName => (!mCurrentSide) ? "Black" : "White";

    Check mCurSelectFrom;
    int mCurMaxMove;

    private void Awake()
    {
        ShowHint = false;

        mCheckPool = new PoolBase<Check>(m_CheckSample, m_CheckPool);
        mChessPool = new PoolBase<Chess>(m_ChessSample, m_ChessPool);
        mEmptyCheck = new List<Check>();

        var checkWidth = mBoardWidth / m_BoardSize;
        Debug.Log($"Board Width {mBoardWidth}, {checkWidth}");
        m_BoardLayout.cellSize = new Vector2(checkWidth, checkWidth);

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
        mEmptyCheck.Clear();
        mCheckPool.RecycleAll();
        mChessPool.RecycleAll();

        mCheckArray = new Check[m_BoardSize, m_BoardSize];

        InitBoard();

        StartRound();
    }

    void InitBoard()
    {
        for (var y = 0; y < m_BoardSize; y++)
        {
            for (var x = 0; x < m_BoardSize; x++)
            {
                var checkObj = mCheckPool.GetObj();
                var check = checkObj.GetComponent<Check>();
                check.transform.parent = m_BoardRoot;
                check.Init(x, y);
                mCheckArray[x, y] = check;

                var chessObj = mChessPool.GetObj();
                var chess = chessObj.GetComponent<Chess>();
                chess.transform.parent = check.transform;
                chess.Init(x, y);
                check.SetChess(chess);
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

        var emptyPos = mEmptyCheck[0].Pos;
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
            canMoveTo |= TraceMovablePath(side, check, Vector2Int.left, ref leftDepth, out var moveFromLeft);
            canMoveTo |= TraceMovablePath(side, check, Vector2Int.right, ref rightDepth, out var moveFromRight);

            var xDepth = leftDepth + rightDepth + 1;
            SetMaxMove(moveFromLeft, xDepth);
            SetMaxMove(moveFromRight, xDepth);
        }

        if (!check.YChecked)
        {
            var upDepth = 0;
            var downDepth = 0;
            canMoveTo |= TraceMovablePath(side, check, Vector2Int.up, ref upDepth, out var moveFromUp);
            canMoveTo |= TraceMovablePath(side, check, Vector2Int.down, ref downDepth, out var moveFromDown);

            var yDepth = upDepth + downDepth + 1;
            SetMaxMove(moveFromUp, yDepth);
            SetMaxMove(moveFromDown, yDepth);
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

    bool TraceMovablePath(bool side, Check check, Vector2Int direction, ref int depth, out Check moveFrom)
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
            SetCheckCanMoveFrom(nextCheck, ChessSelect);
            moveFrom = nextCheck;
            return true;
        }
        else
        {
            depth++;
            // 對面沒有棋，繼續追
            if (TraceMovablePath(side, nextCheck, direction, ref depth, out moveFrom))
            {
                //Debug.Log($"Highlight To {Check.YPos}, {Check.XPos}");
                SetCheckCanMoveTo(check, CheckSelect);
                check.SetDirChecked(direction);
                return true;
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
        return x >= 0 && x < m_BoardSize
            && y >= 0 && y < m_BoardSize;
    }

    void SetMaxMove(Check check, int depth)
    {
        if (check == null)
            return;

        check.SetChessMaxMove(depth);
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

        var check = mCheckArray[x, y];
        SetCheckCanMoveFrom(check, FirstRoundChessSelect);
    }

    void SetCheckCanMoveFrom(Check check, Action<Chess> selectEvent)
    {
        if (check.CanMoveFrom)
            return;

        var chess = check.CurrentChess;
        check.SetCanMoveFrom(true);
        check.RegisterMoveFromEvent(selectEvent);
        OnChessSelect += chess.OnSomeoneSelected;
        OnRoundEnd += check.ClearState;
        OnRoundEnd += chess.ClearState;
    }

    void SetCheckCanMoveTo(Check check, Action<Check> selectEvent)
    {
        if (check.CanMoveTo)
            return;

        var chess = check.CurrentChess;
        check.SetCanMoveTo(true);
        check.RegisterMoveToEvent(selectEvent);
        OnRoundEnd += check.ClearState;
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

        var check = mCheckArray[chess.XPos, chess.YPos];
        mCurSelectFrom = check;
    }

    void CheckSelect(Check check)
    {
        TryMoveChess(mCurSelectFrom, check);
    }

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
            //Debug.Log($"IsMoveValid MoveAndEndRound!");
            MoveChess(fromCheck, toCheck);
            RoundEnd();
            OnNextRound?.Invoke();
        }

        //Debug.Log($"IsMoveValid Can Jump Next? pos {toCheck.Pos}, dir {dir}");
        //             IsMoveDirectionValid(toCheck.Pos, dir, out var nextCheck) && nextCheck.CurrentChess == null)
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

        if (distance % 2 != 0 || distance <= 0)
            return false;

        //Debug.Log($"IsMoveValid IsPathValid? dir {normalizedDir}, distance {distance}");
        if (!IsPathValid(fromCheck.Pos, normalizedDir, distance))
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

        var crossCheck = mCheckArray[crossPos.x, crossPos.y];
        // 沒有棋可以吃
        if (crossCheck.CurrentChess == null)
            return false;

        var nextCheck = mCheckArray[nextPos.x, nextPos.y];
        // 沒有空位可以跳
        if (nextCheck.CurrentChess != null)
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
        toCheck.SetChess(moveChess);
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
        mChessPool.Recycle(chess);
        Debug.Log($"Empty {check.XPos}, {check.YPos}");
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

