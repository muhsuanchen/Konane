using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    [SerializeField]
    Check m_BlackCheckSample;
    [SerializeField]
    Transform m_BlackCheckPool;
    [SerializeField]
    Chess m_BlackChessSample;
    [SerializeField]
    Transform m_BlackChessPool;
    [SerializeField]
    Check m_WhiteCheckSample;
    [SerializeField]
    Transform m_WhiteCheckPool;
    [SerializeField]
    Chess m_WhiteChessSample;
    [SerializeField]
    Transform m_WhiteChessPool;

    PoolBase<Check> mBlackCheckPool;
    PoolBase<Chess> mBlackChessPool;
    PoolBase<Check> mWhiteCheckPool;
    PoolBase<Chess> mWhiteChessPool;

    void InitPool()
    {
        mBlackCheckPool = new PoolBase<Check>(m_BlackCheckSample, m_BlackCheckPool);
        mBlackChessPool = new PoolBase<Chess>(m_BlackChessSample, m_BlackChessPool);
        mWhiteCheckPool = new PoolBase<Check>(m_WhiteCheckSample, m_WhiteCheckPool);
        mWhiteChessPool = new PoolBase<Chess>(m_WhiteChessSample, m_WhiteChessPool);
    }

    void RecycleAllToPool()
    {
        mBlackCheckPool.RecycleAll();
        mBlackChessPool.RecycleAll();
        mWhiteCheckPool.RecycleAll();
        mWhiteChessPool.RecycleAll();
    }

    void RecycleChess(Chess chess)
    {
        if (chess.Side)
            mBlackChessPool.Recycle(chess);
        else
            mWhiteChessPool.Recycle(chess);
    }

    Chess GetChess(bool side)
    {
        return (side) ? mBlackChessPool.GetObj() : mWhiteChessPool.GetObj();
    }

    Check GetCheck(bool side)
    {
        return (side) ? mBlackCheckPool.GetObj() : mWhiteCheckPool.GetObj();
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
