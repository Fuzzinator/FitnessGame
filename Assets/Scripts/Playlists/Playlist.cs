using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Playlist
{
    [SerializeField]
    private PlaylistItem[] items;
    public PlaylistItem[] Items => items;
}
