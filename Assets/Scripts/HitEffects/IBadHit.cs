using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBadHit
{
    public void TriggerBadHitEffect(HitInfo info, ValidHit validHit);
}
