using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuController : MonoBehaviour
{
    [SerializeField]
    private List<Toggle> _toggles;

    [SerializeField]
    private CanvasGroup[] _settingsPages;
   
    private CanvasGroup _activePage;
    protected virtual void Start()
    {
        if (_settingsPages == null || _settingsPages.Length < 1)
        {
            return;
        }
        
        _activePage = _settingsPages[0];
        _activePage.SetGroupState(1, true);
        _activePage.gameObject.SetActive(true);
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = Head.Instance.HeadCamera;
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
    
    public void SetActivePage(int pageNumber)
    {
        if (pageNumber >= _settingsPages.Length)
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
