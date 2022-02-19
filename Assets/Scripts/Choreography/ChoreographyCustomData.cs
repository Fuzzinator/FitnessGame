using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[Serializable]
[BurstCompile]
public struct ChoreographyCustomData
{
    public ChoreographyBookmark[] Bookmarks => _bookmarks;

    [SerializeField]
    private ChoreographyBookmark[] _bookmarks;
}
