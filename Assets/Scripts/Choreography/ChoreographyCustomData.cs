using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChoreographyCustomData
{
    public ChoreographyBookmark[] Bookmarks => _bookmarks;

    [SerializeField]
    private ChoreographyBookmark[] _bookmarks;
}
