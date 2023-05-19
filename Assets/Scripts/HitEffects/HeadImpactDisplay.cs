using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadImpactDisplay : MonoBehaviour
{
    [SerializeField]
    private string _effectName;
    [SerializeField]
    private Material _displayMaterial;
    [SerializeField]
    private MaterialValueLerper _valueLerper;

    public void TriggerHitEffect(Collider _obstacle)
    {
        _displayMaterial.color = ColorsManager.Instance.GetAppropriateColor(HitSideType.Unused, false);
        _valueLerper.TriggerValueChangeAsync(_effectName).Forget();
    }
}
