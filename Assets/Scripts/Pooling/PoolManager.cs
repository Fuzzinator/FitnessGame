using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            var objToReturn = Object.Instantiate(_poolableObj as Object) as IPoolable;
            if (objToReturn != null)
            {
                objToReturn.MyPoolManager = this;
                objToReturn.IsPooled = false;
                return objToReturn;
            }

            Debug.LogError(FAILEDTOCAST);
            return null;
        }
    }

    public void ReturnToPool(IPoolable poolable)
    {
        poolable.IsPooled = true;
        _pooledObjs.Add(poolable);
    }

    public PoolManager(IPoolable poolable, Transform poolParent)
    {
        _poolableObj = poolable;
        this.poolParent = poolParent;
    }
}