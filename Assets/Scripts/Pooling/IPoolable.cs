using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IPoolable
{
    PoolManager MyPoolManager { get; set; }
    bool IsPooled { get; set; }
    void ReturnToPool();
}
