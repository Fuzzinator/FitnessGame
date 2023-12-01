using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnvAssetScroller
{
    void EnableOptionsDisplay();
    void DisableOptionsDisplay();
    int GetAvailableAssetCount();
    EnvAssetRef GetAssetRef(int assetIndex);
    void SetAssetIndex(int index);
}
