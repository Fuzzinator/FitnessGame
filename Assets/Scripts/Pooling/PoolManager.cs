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
    public List<IPoolable> ActiveObjs { get; private set; } = new List<IPoolable>();
    private const string FAILEDTOCAST = "Failed to cast instance of _poolableObj back to IPoolable";

    public IPoolable GetNewPoolable(bool forceCreateNew = false)
    {
        if (!forceCreateNew && _pooledObjs.Count > 0)
        {
            var objToReturn = _pooledObjs[0];
            _pooledObjs.Remove(objToReturn);
            objToReturn.IsPooled = false;
            if (!ActiveObjs.Contains(objToReturn))
            {
                ActiveObjs.Add(objToReturn);
            }
            return objToReturn;
        }
        else
        {
            var objToReturn = CreateNewPoolable();

            if (objToReturn == null)
            {
                Debug.LogError(FAILEDTOCAST);
            }
            if (!ActiveObjs.Contains(objToReturn))
            {
                ActiveObjs.Add(objToReturn);
            }
            return objToReturn;
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
        ActiveObjs.Remove(poolable);
        if (_pooledObjs.Contains(poolable))
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
            var tempPoolable = GetNewPoolable(true);
            tempPoolable.ReturnToPool();
        }
    }
}