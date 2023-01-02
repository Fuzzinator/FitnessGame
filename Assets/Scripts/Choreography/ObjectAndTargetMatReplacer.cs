using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectAndTargetMatReplacer : BaseGameStateListener
{
    [SerializeField]
    private Material _replacementMat;

    private Dictionary<Renderer, Material> _rendererMatSet = new Dictionary<Renderer, Material>();
    private List<Renderer> _renderers = new List<Renderer>();
    private LocalKeyword _keyword;

    protected void Start()
    {
        _keyword = new LocalKeyword(_replacementMat.shader, "_SHOWNOTES");
    }

    protected override void AddListener()
    {
        base.AddListener();
        ActiveTargetManager.Instance.newActiveTarget.AddListener(AddRendererSet);
        ActiveTargetManager.Instance.newActiveObstacle.AddListener(AddRendererSet);
        ActiveTargetManager.Instance.targetDeactivated.AddListener(RemoveRendererSet);
        ActiveTargetManager.Instance.obstacleDeactivated.AddListener(RemoveRendererSet);
    }

    protected override void RemoveListener()
    {
        base.RemoveListener();
        ActiveTargetManager.Instance.newActiveTarget.RemoveListener(AddRendererSet);
        ActiveTargetManager.Instance.newActiveObstacle.RemoveListener(AddRendererSet);
        ActiveTargetManager.Instance.targetDeactivated.RemoveListener(RemoveRendererSet);
        ActiveTargetManager.Instance.obstacleDeactivated.RemoveListener(RemoveRendererSet);

        ResetMaterials();
        _renderers.Clear();
        _rendererMatSet.Clear();
    }
    
    private void AddRendererSet(BaseTarget target)
    {
        AddRenderers(target?.RendererSetter?.Renderers);
    }
    
    private void AddRendererSet(BaseObstacle obstacle)
    {
        AddRenderers(obstacle?.RendererSetter?.Renderers);
    }
    
    private void AddRenderers(Renderer[] renderers)
    {
        if (renderers != null)
        {
            foreach (var rend in renderers)
            {
                _rendererMatSet[rend] = rend.sharedMaterial;
            }
            _renderers.AddRange(renderers);
        }
    }
    
    private void RemoveRendererSet(BaseTarget target)
    {
        RemoveRenderers(target?.RendererSetter?.Renderers);
    }
    
    private void RemoveRendererSet(BaseObstacle obstacle)
    {
       RemoveRenderers(obstacle?.RendererSetter?.Renderers);
    }

    private void RemoveRenderers(Renderer[] renderers)
    {
        if (renderers != null)
        {
            foreach (var rend in renderers)
            {
                if (_rendererMatSet.TryGetValue(rend, out var material))
                {
                    rend.sharedMaterial = material;
                    _rendererMatSet.Remove(rend);
                }

                _renderers.Remove(rend);
            }
        }
    }
    
    protected override void GameStateListener(GameState oldState, GameState newState)
    {
       switch(newState)
       {
           case GameState.InMainMenu:
           case GameState.Playing:
               ResetMaterials();
               break;
           
           case GameState.Entry:
           case GameState.Paused:
           case GameState.Unfocused:
               ReplaceMaterials();
               break;
           default:
               throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
       }
    }

    private void ResetMaterials()
    {
        foreach (var rend in _renderers)
        {
            if (_rendererMatSet.TryGetValue(rend, out var material))
            {
                rend.sharedMaterial = material;
                //rend.sharedMaterial.SetKeyword(_keyword, true);
            }
        }
    }

    private void ReplaceMaterials()
    {
        foreach (var rend in _renderers)
        {
            rend.sharedMaterial = _replacementMat;
            //rend.sharedMaterial.SetKeyword(_keyword, false);
        }
    }
}