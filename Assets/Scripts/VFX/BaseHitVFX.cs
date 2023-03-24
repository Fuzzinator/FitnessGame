using UnityEngine;

public class BaseHitVFX : MonoBehaviour, IPoolable
{
    [SerializeField]
    private ParticleSystemControls[] _particleSystemControls;

    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }

    private void OnValidate()
    {
        if (_particleSystemControls == null || _particleSystemControls.Length == 0)
        {
            /*_particleSystemControls = new ParticleSystemControls[_particleSystems.Length];
            for (var i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystemControls[i] = new ParticleSystemControls(_particleSystems[i]);
            }*/
        }
    }

    public void Initialize()
    {
    }

    public void SetHitQuality(HitInfo info)
    {
        transform.localScale = Vector3.one + (Vector3.one * info.HitQuality);
        foreach (var control in _particleSystemControls)
        {
            var emission = control.System.emission;
            var burst = control.BurstCount;
            burst = (burst.constant + burst.constant*Mathf.Clamp(info.MagnitudeBonus, 0, 2));
            emission.SetBurst(0, new ParticleSystem.Burst(0, burst));
            var main = control.System.main;

            main.startSpeedMultiplier = (info.MagnitudeBonus*.25f);
        }
    }

    public void SetParticleColor(Color color)
    {
        foreach (var ps in _particleSystemControls)
        {
            var main = ps.System.main;

            main.startColor = color;
        }
    }

    public void PlayParticles()
    {
        gameObject.SetActive(true);

        foreach (var control in _particleSystemControls)
        {
            control.System.Play(true);
        }
    }

    private void OnParticleSystemStopped()
    {
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        foreach (var control in _particleSystemControls)
        {
            control.System.Stop(true);
        }

        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }

    [System.Serializable]
    private struct ParticleSystemControls
    {
        [field: SerializeField]
        public ParticleSystem.MinMaxCurve BurstCount { get; private set; }
        [field: SerializeField]
        public ParticleSystem System {get; private set;}

        public ParticleSystemControls(ParticleSystem sourceSystem)
        {
            BurstCount = sourceSystem.emission.GetBurst(0).count;
            System = sourceSystem;
        }
    }
}