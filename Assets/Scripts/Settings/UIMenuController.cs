using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuController : MonoBehaviour
{
    [SerializeField]
    protected List<Toggle> _toggles;

    [SerializeField]
    protected CanvasGroup[] _settingsPages;

    protected CanvasGroup _activePage;

    protected bool _initialized = false;
    protected int _activePageIndex;


    protected virtual void Start()
    {
        if (_settingsPages == null || _settingsPages.Length < 1)
        {
            return;
        }

        if (TryGetComponent(out Canvas canvas))
        {
            canvas.worldCamera = Head.Instance.HeadCamera;
        }
    }

    protected virtual void OnEnable()
    {
        if (!_initialized)
        {
            _initialized = true;
            return;
        }

        Activate();
    }

    protected virtual void OnDisable()
    {
        foreach( var page in _settingsPages )
        {
            page.SetGroupState(0, false);
        }
    }

    public virtual void Activate()
    {
        gameObject.SetActive(true);
        for (var i = 0; i < _toggles.Count; i++)
        {
            _toggles[i].isOn = i == 0;
        }
        SetActivePage(0);
    }

    public void NextPage()
    {
        SetActivePage(_activePageIndex + 1);
    }

    public void PreviousPage()
    {
        SetActivePage(_activePageIndex - 1);
    }

    public void SetActivePage(int pageNumber)
    {
        if (pageNumber >= _settingsPages.Length || pageNumber < 0 ||
            _activePage == _settingsPages[pageNumber])
        {
            return;
        }

        if (_activePage != null)
        {
            _activePage.SetGroupState(0, false);
            _activePage.gameObject.SetActive(false);
        }

        _activePage = _settingsPages[pageNumber];
        _activePage.SetGroupState(1, true);
        _activePage.gameObject.SetActive(true);

        _activePageIndex = pageNumber;
        DelayAndUpdateToggle(pageNumber).Forget();
    }

    private async UniTaskVoid DelayAndUpdateToggle(int pageNumber)
    {
        await UniTask.DelayFrame(1);
        if(_toggles.Count > 0 || pageNumber >= _toggles.Count) 
        {
            return;
        }
        _toggles[pageNumber].SetIsOnWithoutNotify(true);
    }


    public void PageToggleChanged(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            return;
        }

        SetActivePage(_toggles.IndexOf(toggle));
    }
}
