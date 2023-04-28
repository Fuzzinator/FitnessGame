using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class PoolManager
{
    public Transform poolParent { get; private set; }
    private IPoolable _poolableObj;
    private List<IPoolable> _pooledObjs = new List<IPoolable>();
    private const string FAILEDTOCAST = "Failed to cast instance of _poolableObj back to IPoolable";

    public IPoolable GetNewPoolable()
    {
        if (_pooledObjs.Count > 0)
        {
            var objToReturn = _pooledObjs[0];
            _pooledObjs.Remove(objToReturn);
            objToReturn.IsPooled = false;
            return objToReturn;
        }
        else
        {
            var obj = CreateNewPoolable();
            if (obj == null)
            {
                Debug.LogError(FAILEDTOCAST);
            }

            return obj;
        }
    }

    private IPoolable CreateNewPoolable()
    {
        var objToReturn = Object.Instantiate(_poolableObj as Object, poolParent) as IPoolable;
        if (objToReturn != null)
        {
            objToReturn.MyPoolManager = this;
            objToReturn.IsPooled = false;
            objToReturn.Initialize();
        }

        return objToReturn;
    }

    public void ReturnToPool(IPoolable poolable)
    {
        poolable.IsPooled = true;
        if(_pooledObjs.Contains(poolable))
        {
            return;
        }
        _pooledObjs.Add(poolable);
    }

    public PoolManager(IPoolable poolable, Transform poolParent, int initialSize = 10)
    {
        _poolableObj = poolable;
        this.poolParent = poolParent;

        for (var i = 0; i < initialSize; i++)
        {
            var tempPoolable = CreateNewPoolable();
            tempPoolable.ReturnToPool();
        }
    }
}