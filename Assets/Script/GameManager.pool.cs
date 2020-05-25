using System.Collections.Generic;
using UnityEngine;

namespace TrainingProject
{
    public partial class GameManager
    {
        [SerializeField]
        Check m_CheckSample;
        [SerializeField]
        Transform m_CheckPool;
        [SerializeField]
        Chess m_ChessSample;
        [SerializeField]
        Transform m_ChessPool;

        PoolBase<Check> mCheckPool;
        PoolBase<Chess> mChessPool;

        void InitPool()
        {
            mCheckPool = new PoolBase<Check>(m_CheckSample, m_CheckPool);
            mChessPool = new PoolBase<Chess>(m_ChessSample, m_ChessPool);
        }

        void RecycleAllToPool()
        {
            mCheckPool.RecycleAll();
            mChessPool.RecycleAll();
        }

        void RecycleChess(Chess chess)
        {
            mChessPool.Recycle(chess);
        }

        Chess GetChess()
        {
            return mChessPool.GetObj();
        }

        Check GetCheck()
        {
            return mCheckPool.GetObj();
        }

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
}