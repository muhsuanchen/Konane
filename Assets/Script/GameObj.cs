using UnityEngine;

namespace TrainingProject
{
    public class GameObj : MonoBehaviour
    {
        [SerializeField]
        GameSide mGameSide;

        public bool Side => mGameSide == GameSide.BLACK;
        public int XPos { get; private set; }
        public int YPos { get; private set; }

        Vector2Int mPos;
        public Vector2Int Pos => mPos;

        public virtual void Init(int x, int y)
        {
            SetPos(x, y);

            mGameSide = GameManager.GetGameSideFromXY(x, y);
            InitColorBySide();

            gameObject.SetActive(true);
        }

        public void SetPos(int x, int y)
        {
            XPos = x;
            YPos = y;
            mPos = new Vector2Int(XPos, YPos);
        }

        protected virtual void InitColorBySide()
        {
        }

        public virtual void Recycle()
        {
            gameObject.SetActive(false);
        }
    }
}