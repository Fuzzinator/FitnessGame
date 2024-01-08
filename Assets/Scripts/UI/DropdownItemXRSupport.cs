using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TMPro
{    internal class DropdownItemXRSupport : MonoBehaviour, IPointerEnterHandler, ICancelHandler
    {
        [field: SerializeField]
        [field: FormerlySerializedAs("m_Text")]
        public TMP_Text Text { get; set; }
        [field: SerializeField]
        [field: FormerlySerializedAs("m_Image")]
        public Image Image { get; set; }
        [field: SerializeField]
        [field: FormerlySerializedAs("m_RectTransform")]
        public RectTransform RectTransform { get; set; }
        [field: SerializeField]
        [field: FormerlySerializedAs("m_Toggle")]
        public Toggle Toggle { get; set; }

        public TMP_Dropdown_XRSupport dropdown;

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            if (dropdown != null)
            {
                dropdown.Hide();
            }
        }
    }
}