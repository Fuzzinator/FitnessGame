using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongInfoCellView : EnhancedScrollerCellView, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private TextMeshProUGUI _songDetails;

        private const string INVALIDINDICATOR = "<sprite index=1>";

        private const string SONGINFOFORMAT =
            "<align=center>{0}</style>\n<size=50%>{1}<line-indent=15%>{2}</size></align>";

        private bool _isDragging;
        private Canvas _draggingCanvas;
        private float _dragYOffset;
        private Vector2 _touchPos;

        private PlaylistSongInfoScrollerController _controller;
        private PlaylistItem _playlistItem;
        private int _songIndex;

        public void SetData(PlaylistSongInfoScrollerController controller, PlaylistItem item, int songIndex)
        {
            _controller = controller;
            _playlistItem = item;
            _songIndex = songIndex;
            SetDataAsync(item).Forget();
        }

        private async UniTaskVoid SetDataAsync(PlaylistItem playlistItem)
        {
            var isValid = await PlaylistValidator.IsValid(playlistItem);
            if (_songDetails == null)
            {
                return;
            }
            using (var sb = ZString.CreateStringBuilder(true))
            {
                if (!isValid)
                {
                    sb.Append(INVALIDINDICATOR);
                }
                sb.AppendFormat(SONGINFOFORMAT, playlistItem.SongName, playlistItem.Difficulty, playlistItem.TargetGameMode.Readable());

                _songDetails.SetText(sb);
            }
        }

        public void RemovePlaylistItem()
        {
            if (PlaylistMaker.Instance != null)
            {
                PlaylistMaker.Instance.RemovePlaylistItem(_playlistItem);
            }
        }

        public void SetActiveSongInfo()
        {
            _controller.SetActiveInfo(_playlistItem.SongInfo, _songIndex);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                _draggingCanvas = gameObject.AddComponent<Canvas>();
                _draggingCanvas.overrideSorting = true;
                _draggingCanvas.sortingOrder = 10000;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, eventData.position, Head.Instance.HeadCamera, out _touchPos);
            _dragYOffset = _touchPos.y - (transform as RectTransform).anchoredPosition.y;

            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var rectTransform = (transform as RectTransform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, eventData.position, Head.Instance.HeadCamera, out var currentPos);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, currentPos.y - _dragYOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            Destroy(_draggingCanvas);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, eventData.position, Head.Instance.HeadCamera, out var finalPos);
            var scale = (transform as RectTransform).rect.height;

            var offset = finalPos - _touchPos;
            var slotsToMove = (offset.y < 0 ? Mathf.CeilToInt(offset.y / scale) : Mathf.FloorToInt(offset.y / scale)) * -1;
            _controller.SetNewIndex(_songIndex, slotsToMove);
            _isDragging = false;
        }
    }
}