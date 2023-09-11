using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DontDestroy : MonoBehaviour
{
    [SerializeField]
    private RawImage _image;
    [SerializeField]
    private Renderer _renderer;
    private Texture2D _texture;
    private bool requested = false;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        _texture = EnvironmentControlManager.Instance.Skybox;
        if(_texture == null)
        {
            return;
        }
        if (_image != null && _image.texture == null)
        {
            _image.texture = _texture;
        }
        if(_renderer != null && !requested)
        {
            //if(!_renderer.sharedMaterial.HasTexture("_SkyboxColor"))
            {
                NotificationManager.RequestNotification(new Notification.NotificationVisuals($"why not!? {_renderer.sharedMaterial} has {_renderer.sharedMaterial.shader}", "why no work", autoTimeOutTime: 10));
            }
            _renderer.sharedMaterial.SetTexture("_SkyboxColor", _texture);
                requested = true;
        }
    }
}
