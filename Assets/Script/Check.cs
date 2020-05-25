using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TrainingProject
{
    public class Check : GameObj
    {
        [SerializeField]
        Image m_CheckBG;
        [SerializeField]
        Image m_Highlight;  // Can Move To
        [SerializeField]
        Button m_Button;

        RectTransform mRectTrans;
        Action<Check> OnMoveToEvent;

        Color kBlackSideColor = new Color(188f / 255f, 133f / 255f, 100f / 255f);
        Color kWhiteSideColor = new Color(87f / 255f, 38f / 255f, 32f / 255f);

        public bool HaveChess => CurrentChess != null;
        public Chess CurrentChess { get; private set; }
        public bool CanRemove => (CurrentChess == null) ? false : CurrentChess.Removable;
        public bool CanMoveFrom => (CurrentChess == null) ? false : CurrentChess.Selectable;
        public bool CanMoveTo { get; private set; }
        public bool XChecked { get; private set; }
        public bool YChecked { get; private set; }

        LinkedList<Check> mCurMaxXPath = new LinkedList<Check>();
        LinkedList<Check> mCurMaxYPath = new LinkedList<Check>();

        public override void Init(int x, int y)
        {
            base.Init(x, y);

            gameObject.name = $"Check ({x}-{y})";

            m_Button.onClick.AddListener(OnSelect);

            mRectTrans = gameObject.GetComponent<RectTransform>();
            mRectTrans.localScale = Vector3.one;

            ClearState();
            RemoveChess();
        }

        protected override void InitColorBySide()
        {
            m_CheckBG.color = (Side) ? kBlackSideColor : kWhiteSideColor;
        }

        public override void Recycle()
        {
            ClearState();

            SetPos(-1, -1);

            m_Button.onClick.RemoveAllListeners();

            base.Recycle();
        }

        public void ClearState()
        {
            SetCanMoveTo(false);
            SetAllChecked(false);
            SetHighlight(false);

            mCurMaxXPath.Clear();
            mCurMaxYPath.Clear();

            if (CurrentChess != null)
                CurrentChess.ClearState();
        }

        #region Event
        public void RegisterMoveToEvent(Action<Check> callback)
        {
            OnMoveToEvent += callback;
        }

        private void OnSelect()
        {
            if (!CanMoveTo)
                return;

            OnMoveToEvent?.Invoke(this);
        }

        void ShowChessPathHint()
        {
            if (!CanMoveFrom)
                return;

            SetPathHighlight(true);
        }

        public void OnSomeChessSelected(Vector2Int selectPos)
        {
            if (selectPos == Pos)
                return;

            if (CurrentChess == null)
                return;

            CurrentChess.OnOtherChessSelected();
            SetPathHighlight(false);
        }
        #endregion

        #region State
        void SetPathHighlight(bool highlight)
        {
            //Debug.Log($"Path {Pos} X {mCurMaxXPath.Count}, Y {mCurMaxYPath.Count}");
            foreach (var check in mCurMaxXPath)
            {
                check.SetHighlight(highlight);
            }
            foreach (var check in mCurMaxYPath)
            {
                check.SetHighlight(highlight);
            }
        }

        public void SetHighlight(bool highlight)
        {
            //Debug.Log($"[Game] SetHighlight {Pos}, {CanMoveTo}, {highlight}");
            m_Highlight.enabled = CanMoveTo && highlight;
        }

        public void SetCanRemove(bool active)
        {
            if (CurrentChess == null)
                return;

            CurrentChess.SetRemovable(active);
        }

        public void SetCanMoveFrom(bool active)
        {
            if (CurrentChess == null)
                return;

            CurrentChess.SetSelectable(active);
        }

        public void SetCanMoveTo(bool active)
        {
            CanMoveTo = active;
        }
        #endregion

        #region Chess
        public void SetChess(Chess chess, Action<Chess> selectEvent, Action<Chess> removeEvent)
        {
            CurrentChess = chess;
            CurrentChess.transform.parent = transform;
            CurrentChess.ResetPos();
            CurrentChess.SetPos(XPos, YPos);
            CurrentChess.RegisterSelectHintEvent(ShowChessPathHint);
            CurrentChess.RegisterSelectEvent(selectEvent);
            CurrentChess.RegisterRemoveEvent(removeEvent);
        }

        public Chess RemoveChess()
        {
            if (CurrentChess == null)
                return null;

            var chess = CurrentChess;
            CurrentChess.ClearEvent();
            CurrentChess = null;
            return chess;
        }

        public bool SetChessMaxMove(int maxMove)
        {
            if (CurrentChess == null)
                return false;

            return CurrentChess.SetMaxMove(maxMove);
        }
        #endregion

        #region Path
        public void SetDirPath(Vector2Int dir, Check nextCheck)
        {
            if (dir == Vector2Int.left || dir == Vector2Int.right)
                SetXDirPath(nextCheck);
            else if (dir == Vector2Int.up || dir == Vector2Int.down)
                SetYDirPath(nextCheck);
        }

        public void SetDirPath(Vector2Int dir, LinkedList<Check> path)
        {
            //Debug.Log($"[Game] SetDirPath to {Pos}, {dir}, {path.Count}");
            if (dir == Vector2Int.left || dir == Vector2Int.right)
            {
                foreach (var check in path)
                {
                    SetXDirPath(check);
                }
            }
            else if (dir == Vector2Int.up || dir == Vector2Int.down)
            {
                foreach (var check in path)
                {
                    SetYDirPath(check);
                }
            }
        }

        void SetXDirPath(Check nextCheck)
        {
            //Debug.Log($"[Game] SetXDirPath to {Pos}, {nextCheck.Pos}");
            if (!mCurMaxXPath.Contains(nextCheck))
                mCurMaxXPath.AddLast(nextCheck);
        }

        void SetYDirPath(Check nextCheck)
        {
            //Debug.Log($"[Game] SetYDirPath to {Pos}, {nextCheck.Pos}");
            if (!mCurMaxYPath.Contains(nextCheck))
                mCurMaxYPath.AddLast(nextCheck);
        }

        public void SetAllChecked(bool check)
        {
            SetXChecked(check);
            SetYChecked(check);
        }

        public void SetDirChecked(Vector2Int dir)
        {
            if (dir == Vector2Int.left || dir == Vector2Int.right)
                SetXChecked(true);
            else if (dir == Vector2Int.up || dir == Vector2Int.down)
                SetYChecked(true);
        }

        public void SetXChecked(bool check)
        {
            XChecked = check;
        }

        public void SetYChecked(bool check)
        {
            YChecked = check;
        }
        #endregion
    }

}