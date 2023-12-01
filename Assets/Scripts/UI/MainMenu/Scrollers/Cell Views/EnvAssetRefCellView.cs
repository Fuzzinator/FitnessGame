using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class EnvAssetRefCellView : EnhancedScrollerCellView
{
    [SerializeField]
    private Image _thumbnail;
    [SerializeField]
    private TextMeshProUGUI _title;

    private EnvAssetScrollerController _scroller;

    private EnvAssetRef _activeAssetRef;
    private int _activeAssetRefIndex;

    public void SetData(EnvAssetRef assetRef, int index, EnvAssetScrollerController scroller)
    {
        _activeAssetRef = assetRef;
        _activeAssetRefIndex = index;
        _scroller = scroller;

        _title.SetTextZeroAlloc(_activeAssetRef.AssetName, true);
        SetSprite().Forget();
    }

    private async UniTaskVoid SetSprite()
    {
        await UniTask.DelayFrame(1);
        if (_activeAssetRef.SpriteThumbnail == null || !_activeAssetRef.SpriteThumbnail.RuntimeKeyIsValid())
         {
            return;
        }
        var sprite = await Addressables.LoadAssetAsync<Sprite>(_activeAssetRef.SpriteThumbnail);
        _thumbnail.sprite = sprite;
    }

    public void SetActiveAssetIndex()
    {
        _scroller.SetActiveAsset(_activeAssetRefIndex);
    }

    private void OnDisable()
    {
        if (_activeAssetRef.SpriteThumbnail.Asset == null)
        {
            return;
        }
        _activeAssetRef.SpriteThumbnail.ReleaseAsset();
    }
}
