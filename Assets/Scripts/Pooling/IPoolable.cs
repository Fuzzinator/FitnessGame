using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


public interface IPoolable
{
    PoolManager MyPoolManager { get; set; }
    bool IsPooled { get; set; }
    void Initialize();
    void ReturnToPool();
}