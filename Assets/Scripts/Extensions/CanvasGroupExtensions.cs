using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CanvasGroupExtensions
{
    public static void SetGroupState(this CanvasGroup group, float alpha, bool enabled)
    {
        group.alpha = alpha;
        group.interactable = enabled;
        group.blocksRaycasts = enabled;
    }

    public static void SetGroupState(this CanvasGroup group, bool enabled)
    {
        group.alpha = enabled ? 1 : 0;
        group.interactable = enabled;
        group.blocksRaycasts = enabled;
    }
}