using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnvAssetScroller
{
    void EnableOptionsDisplay();
    void DisableOptionsDisplay();
    int GetAvailableAssetCount();
    EnvAssetReference GetAssetRef(int assetIndex);
    void SetAssetIndex(int index);
}
