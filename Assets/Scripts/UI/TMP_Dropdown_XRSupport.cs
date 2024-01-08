using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace TMPro
{
    [AddComponentMenu("UI/XR Dropdown - TextMeshPro", 36)]
    [DisallowMultipleComponent]
    public class TMP_Dropdown_XRSupport : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
    {
        private bool _hovered = false;
                
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            _hovered = true;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            _hovered = false;
        }

        public void TryHideFromDeselect(InputAction.CallbackContext context)
        {
            if (_hovered)
            {
                return;
            }
            Hide();
        }
        [SerializeField]
        private RectTransform _template;
        // Template used to create the dropdown.
        [SerializeField]
        [FormerlySerializedAs("m_Template")]
        private RectTransform _templateInstance;
        private DropdownItemXRSupport _templateDropdownItem;

        /// <summary>
        /// The Rect Transform of the template for the dropdown list.
        /// </summary>
        public RectTransform template { get { return _templateInstance; } set { _templateInstance = value; RefreshShownValue(); } }

        // Text to be used as a caption for the current value. It's not required, but it's kept here for convenience.
        [SerializeField]
        private TMP_Text m_CaptionText;

        /// <summary>
        /// The Text component to hold the text of the currently selected option.
        /// </summary>
        public TMP_Text captionText { get { return m_CaptionText; } set { m_CaptionText = value; RefreshShownValue(); } }

        [SerializeField]
        private Image m_CaptionImage;

        /// <summary>
        /// The Image component to hold the image of the currently selected option.
        /// </summary>
        public Image captionImage { get { return m_CaptionImage; } set { m_CaptionImage = value; RefreshShownValue(); } }

        [SerializeField]
        private Graphic m_Placeholder;

        /// <summary>
        /// The placeholder Graphic component. Shown when no option is selected.
        /// </summary>
        public Graphic placeholder { get { return m_Placeholder; } set { m_Placeholder = value; RefreshShownValue(); } }

        [Space]

        [SerializeField]
        private TMP_Text m_ItemText;

        /// <summary>
        /// The Text component to hold the text of the item.
        /// </summary>
        public TMP_Text itemText { get { return m_ItemText; } set { m_ItemText = value; RefreshShownValue(); } }

        [SerializeField]
        private Image m_ItemImage;

        /// <summary>
        /// The Image component to hold the image of the item
        /// </summary>
        public Image itemImage { get { return m_ItemImage; } set { m_ItemImage = value; RefreshShownValue(); } }

        [Space]

        [SerializeField]
        private int m_Value;

        [Space]

        // Items that will be visible when the dropdown is shown.
        // We box this into its own class so we can use a Property Drawer for it.
        [SerializeField]
        private TMP_Dropdown.OptionDataList m_Options = new TMP_Dropdown.OptionDataList();

        /// <summary>
        /// The list of possible options. A text string and an image can be specified for each option.
        /// </summary>
        /// <remarks>
        /// This is the list of options within the Dropdown. Each option contains Text and/or image data that you can specify using UI.Dropdown.OptionData before adding to the Dropdown list.
        /// This also unlocks the ability to edit the Dropdown, including the insertion, removal, and finding of options, as well as other useful tools
        /// </remarks>
        /// /// <example>
        /// <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking Create>UI>Dropdown - TextMeshPro. Attach this script to the Dropdown GameObject.
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        /// using System.Collections.Generic;
        /// using TMPro;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     //Use these for adding options to the Dropdown List
        ///     TMP_Dropdown.OptionData m_NewData, m_NewData2;
        ///     //The list of messages for the Dropdown
        ///     List<TMP_Dropdown.OptionData> m_Messages = new List<TMP_Dropdown.OptionData>();
        ///
        ///
        ///     //This is the Dropdown
        ///     TMP_Dropdown m_Dropdown;
        ///     string m_MyString;
        ///     int m_Index;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Dropdown GameObject the script is attached to
        ///         m_Dropdown = GetComponent<TMP_Dropdown>();
        ///         //Clear the old options of the Dropdown menu
        ///         m_Dropdown.ClearOptions();
        ///
        ///         //Create a new option for the Dropdown menu which reads "Option 1" and add to messages List
        ///         m_NewData = new TMP_Dropdown.OptionData();
        ///         m_NewData.text = "Option 1";
        ///         m_Messages.Add(m_NewData);
        ///
        ///         //Create a new option for the Dropdown menu which reads "Option 2" and add to messages List
        ///         m_NewData2 = new TMP_Dropdown.OptionData();
        ///         m_NewData2.text = "Option 2";
        ///         m_Messages.Add(m_NewData2);
        ///
        ///         //Take each entry in the message List
        ///         foreach (TMP_Dropdown.OptionData message in m_Messages)
        ///         {
        ///             //Add each entry to the Dropdown
        ///             m_Dropdown.options.Add(message);
        ///             //Make the index equal to the total number of entries
        ///             m_Index = m_Messages.Count - 1;
        ///         }
        ///     }
        ///
        ///     //This OnGUI function is used here for a quick demonstration. See the [[wiki:UISystem|UI Section]] for more information about setting up your own UI.
        ///     void OnGUI()
        ///     {
        ///         //TextField for user to type new entry to add to Dropdown
        ///         m_MyString = GUI.TextField(new Rect(0, 40, 100, 40), m_MyString);
        ///
        ///         //Press the "Add" Button to add a new entry to the Dropdown
        ///         if (GUI.Button(new Rect(0, 0, 100, 40), "Add"))
        ///         {
        ///             //Make the index the last number of entries
        ///             m_Index = m_Messages.Count;
        ///             //Create a temporary option
        ///             TMP_Dropdown.OptionData temp = new TMP_Dropdown.OptionData();
        ///             //Make the option the data from the TextField
        ///             temp.text = m_MyString;
        ///
        ///             //Update the messages list with the TextField data
        ///             m_Messages.Add(temp);
        ///
        ///             //Add the Textfield data to the Dropdown
        ///             m_Dropdown.options.Insert(m_Index, temp);
        ///         }
        ///
        ///         //Press the "Remove" button to delete the selected option
        ///         if (GUI.Button(new Rect(110, 0, 100, 40), "Remove"))
        ///         {
        ///             //Remove the current selected item from the Dropdown from the messages List
        ///             m_Messages.RemoveAt(m_Dropdown.value);
        ///             //Remove the current selection from the Dropdown
        ///             m_Dropdown.options.RemoveAt(m_Dropdown.value);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public List<TMP_Dropdown.OptionData> options
        {
            get { return m_Options.options; }
            set { m_Options.options = value; RefreshShownValue(); }
        }

        [Space]

        // Notification triggered when the dropdown changes.
        [SerializeField]
        private TMP_Dropdown.DropdownEvent m_OnValueChanged = new TMP_Dropdown.DropdownEvent();

        /// <summary>
        /// A UnityEvent that is invoked when a user has clicked one of the options in the dropdown list.
        /// </summary>
        /// <remarks>
        /// Use this to detect when a user selects one or more options in the Dropdown. Add a listener to perform an action when this UnityEvent detects a selection by the user. See https://unity3d.com/learn/tutorials/topics/scripting/delegates for more information on delegates.
        /// </remarks>
        /// <example>
        ///  <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking Create>UI>Dropdown - TextMeshPro. Attach this script to the Dropdown GameObject.
        /// //Set your own Text in the Inspector window
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        /// using TMPro;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     TMP_Dropdown m_Dropdown;
        ///     public Text m_Text;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Dropdown GameObject
        ///         m_Dropdown = GetComponent<TMP_Dropdown>();
        ///         //Add listener for when the value of the Dropdown changes, to take action
        ///         m_Dropdown.onValueChanged.AddListener(delegate {
        ///                 DropdownValueChanged(m_Dropdown);
        ///             });
        ///
        ///         //Initialize the Text to say the first value of the Dropdown
        ///         m_Text.text = "First Value : " + m_Dropdown.value;
        ///     }
        ///
        ///     //Output the new value of the Dropdown into Text
        ///     void DropdownValueChanged(TMP_Dropdown change)
        ///     {
        ///         m_Text.text =  "New Value : " + change.value;
        ///     }
        /// }
        /// </code>
        /// </example>
        public TMP_Dropdown.DropdownEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        [SerializeField]
        private float m_AlphaFadeSpeed = 0.15f;

        /// <summary>
        /// The time interval at which a drop down will appear and disappear
        /// </summary>
        public float alphaFadeSpeed { get { return m_AlphaFadeSpeed; } set { m_AlphaFadeSpeed = value; } }

        private GameObject m_Dropdown;
        private GameObject m_Blocker;
        private List<DropdownItemXRSupport> m_Items = new List<DropdownItemXRSupport>();
        private TweenRunner<FloatTween> m_AlphaTweenRunner;
        private bool validTemplate = false;
        private Coroutine m_Coroutine = null;

        private static TMP_Dropdown.OptionData s_NoOptionData = new TMP_Dropdown.OptionData();

        /// <summary>
        /// The Value is the index number of the current selection in the Dropdown. 0 is the first option in the Dropdown, 1 is the second, and so on.
        /// </summary>
        /// <example>
        /// <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking Create>UI>Dropdown - TextMeshPro. Attach this script to the Dropdown GameObject.
        /// //Set your own Text in the Inspector window
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        /// using TMPro;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     //Attach this script to a Dropdown GameObject
        ///     TMP_Dropdown m_Dropdown;
        ///     //This is the string that stores the current selection m_Text of the Dropdown
        ///     string m_Message;
        ///     //This Text outputs the current selection to the screen
        ///     public Text m_Text;
        ///     //This is the index value of the Dropdown
        ///     int m_DropdownValue;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the DropDown component from the GameObject
        ///         m_Dropdown = GetComponent<TMP_Dropdown>();
        ///         //Output the first Dropdown index value
        ///         Debug.Log("Starting Dropdown Value : " + m_Dropdown.value);
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //Keep the current index of the Dropdown in a variable
        ///         m_DropdownValue = m_Dropdown.value;
        ///         //Change the message to say the name of the current Dropdown selection using the value
        ///         m_Message = m_Dropdown.options[m_DropdownValue].text;
        ///         //Change the on screen Text to reflect the current Dropdown selection
        ///         m_Text.text = m_Message;
        ///     }
        /// }
        /// </code>
        /// </example>
        public int value
        {
            get
            {
                return m_Value;
            }
            set
            {
                SetValue(value);
            }
        }

        /// <summary>
        /// Set index number of the current selection in the Dropdown without invoking onValueChanged callback.
        /// </summary>
        /// <param name="input">The new index for the current selection.</param>
        public void SetValueWithoutNotify(int input)
        {
            SetValue(input, false);
        }

        void SetValue(int value, bool sendCallback = true)
        {
            if (Application.isPlaying && (value == m_Value || options.Count == 0))
                return;

            m_Value = Mathf.Clamp(value, m_Placeholder ? -1 : 0, options.Count - 1);
            RefreshShownValue();

            if (sendCallback)
            {
                // Notify all listeners
                UISystemProfilerApi.AddMarker("Dropdown.value", this);
                m_OnValueChanged.Invoke(m_Value);
            }
        }

        public bool IsExpanded { get { return m_Dropdown != null; } }

        protected TMP_Dropdown_XRSupport() { }

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (m_CaptionImage)
                m_CaptionImage.enabled = (m_CaptionImage.sprite != null);

            if (_template != null)
            {
                _templateInstance = Instantiate(_template, transform);
            }

            if (_templateInstance != null)
            {
                _templateDropdownItem = _templateInstance.GetComponentInChildren<DropdownItemXRSupport>();
                if (_templateDropdownItem != null)
                {
                    itemText = _templateDropdownItem.Text;
                    itemImage = _templateDropdownItem.Image;
                    _templateDropdownItem.dropdown = this;
                }
                _templateInstance.gameObject.SetActive(false);
            }
        }

        protected override void Start()
        {
            m_AlphaTweenRunner = new TweenRunner<FloatTween>();
            m_AlphaTweenRunner.Init(this);
            base.Start();

            RefreshShownValue();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            RefreshShownValue();
        }
#endif

        protected override void OnDisable()
        {
            //Destroy dropdown and blocker in case user deactivates the dropdown when they click an option (case 935649)
            ImmediateDestroyDropdownList();

            if (m_Blocker != null)
                DestroyBlocker(m_Blocker);

            m_Blocker = null;

            base.OnDisable();
        }

        /// <summary>
        /// Refreshes the text and image (if available) of the currently selected option.
        /// </summary>
        /// <remarks>
        /// If you have modified the list of options, you should call this method afterwards to ensure that the visual state of the dropdown corresponds to the updated options.
        /// </remarks>
        public void RefreshShownValue()
        {
            TMP_Dropdown.OptionData data = s_NoOptionData;

            if (options.Count > 0 && m_Value >= 0)
                data = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];

            if (m_CaptionText)
            {
                if (data != null && data.text != null)
                    m_CaptionText.text = data.text;
                else
                    m_CaptionText.text = "";
            }

            if (m_CaptionImage)
            {
                if (data != null)
                    m_CaptionImage.sprite = data.image;
                else
                    m_CaptionImage.sprite = null;
                m_CaptionImage.enabled = (m_CaptionImage.sprite != null);
            }

            if (m_Placeholder)
            {
                m_Placeholder.enabled = options.Count == 0 || m_Value == -1;
            }
        }

        /// <summary>
        /// Add multiple options to the options of the Dropdown based on a list of OptionData objects.
        /// </summary>
        /// <param name="options">The list of OptionData to add.</param>
        /// /// <remarks>
        /// See AddOptions(List<string> options) for code example of usages.
        /// </remarks>
        public void AddOptions(List<TMP_Dropdown.OptionData> options)
        {
            this.options.AddRange(options);
            RefreshShownValue();
        }

        /// <summary>
        /// Add multiple text-only options to the options of the Dropdown based on a list of strings.
        /// </summary>
        /// <remarks>
        /// Add a List of string messages to the Dropdown. The Dropdown shows each member of the list as a separate option.
        /// </remarks>
        /// <param name="options">The list of text strings to add.</param>
        /// <example>
        /// <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking Create>UI>Dropdown - TextMeshPro. Attach this script to the Dropdown GameObject.
        ///
        /// using System.Collections.Generic;
        /// using UnityEngine;
        /// using UnityEngine.UI;
        /// using TMPro;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     //Create a List of new Dropdown options
        ///     List<string> m_DropOptions = new List<string> { "Option 1", "Option 2"};
        ///     //This is the Dropdown
        ///     TMP_Dropdown m_Dropdown;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Dropdown GameObject the script is attached to
        ///         m_Dropdown = GetComponent<TMP_Dropdown>();
        ///         //Clear the old options of the Dropdown menu
        ///         m_Dropdown.ClearOptions();
        ///         //Add the options created in the List above
        ///         m_Dropdown.AddOptions(m_DropOptions);
        ///     }
        /// }
        /// </code>
        /// </example>
        public void AddOptions(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
                this.options.Add(new TMP_Dropdown.OptionData(options[i]));

            RefreshShownValue();
        }

        /// <summary>
        /// Add multiple image-only options to the options of the Dropdown based on a list of Sprites.
        /// </summary>
        /// <param name="options">The list of Sprites to add.</param>
        /// <remarks>
        /// See AddOptions(List<string> options) for code example of usages.
        /// </remarks>
        public void AddOptions(List<Sprite> options)
        {
            for (int i = 0; i < options.Count; i++)
                this.options.Add(new TMP_Dropdown.OptionData(options[i]));

            RefreshShownValue();
        }

        /// <summary>
        /// Clear the list of options in the Dropdown.
        /// </summary>
        public void ClearOptions()
        {
            options.Clear();
            m_Value = m_Placeholder ? -1 : 0;
            RefreshShownValue();
        }

        private void SetupTemplate()
        {
            validTemplate = false;

            if (_templateInstance == null)
            {
                Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", this);
                return;
            }

            GameObject templateGo = _templateInstance.gameObject;
            templateGo.SetActive(true);
            Toggle itemToggle = _templateDropdownItem.Toggle;//_templateInstance.GetComponentInChildren<Toggle>();
            
            validTemplate = true;
            if (!itemToggle || itemToggle.transform == template)
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
            }
            else if (!(itemToggle.transform.parent is RectTransform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
            }
            else if (itemText != null && !itemText.transform.IsChildOf(itemToggle.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
            }
            else if (itemImage != null && !itemImage.transform.IsChildOf(itemToggle.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
            }

            if (!validTemplate)
            {
                templateGo.SetActive(false);
                return;
            }


            // Find the Canvas that this dropdown is a part of
            Canvas parentCanvas = null;
            Transform parentTransform = _templateInstance.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            Canvas popupCanvas = GetOrAddComponent<Canvas>(templateGo);
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 30000;

            // If we have a parent canvas, apply the same raycasters as the parent for consistency.
            if (parentCanvas != null)
            {
                Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    System.Type raycasterType = components[i].GetType();
                    if (templateGo.GetComponent(raycasterType) == null)
                    {
                        templateGo.AddComponent(raycasterType);
                    }
                }
            }
            else
            {
                GetOrAddComponent<GraphicRaycaster>(templateGo);
            }

            GetOrAddComponent<CanvasGroup>(templateGo);
            templateGo.SetActive(false);

            validTemplate = true;
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var hasComp = go.TryGetComponent<T>(out var comp);
            if (!hasComp)
                comp = go.AddComponent<T>();
            return comp;
        }

        /// <summary>
        /// Handling for when the dropdown is initially 'clicked'. Typically shows the dropdown
        /// </summary>
        /// <param name="eventData">The associated event data.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Show();
        }

        /// <summary>
        /// Handling for when the dropdown is selected and a submit event is processed. Typically shows the dropdown
        /// </summary>
        /// <param name="eventData">The associated event data.</param>
        public virtual void OnSubmit(BaseEventData eventData)
        {
            Show();
        }

        /// <summary>
        /// This will hide the dropdown list.
        /// </summary>
        /// <remarks>
        /// Called by a BaseInputModule when a Cancel event occurs.
        /// </remarks>
        /// <param name="eventData">The associated event data.</param>
        public virtual void OnCancel(BaseEventData eventData)
        {
            Hide();
        }

        /// <summary>
        /// Show the dropdown.
        ///
        /// Plan for dropdown scrolling to ensure dropdown is contained within screen.
        ///
        /// We assume the Canvas is the screen that the dropdown must be kept inside.
        /// This is always valid for screen space canvas modes.
        /// For world space canvases we don't know how it's used, but it could be e.g. for an in-game monitor.
        /// We consider it a fair constraint that the canvas must be big enough to contain dropdowns.
        /// </summary>
        public void Show()
        {
            InputManager.Instance.MainInput[InputManager.SelectedRight].performed += TryHideFromDeselect;
            InputManager.Instance.MainInput[InputManager.SelectedLeft].performed += TryHideFromDeselect;

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                ImmediateDestroyDropdownList();
            }

            if (!IsActive() || !IsInteractable() || m_Dropdown != null)
                return;

            // Get root Canvas.
            var list = ListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count == 0)
                return;

            Canvas rootCanvas = list[list.Count - 1];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].isRootCanvas)
                {
                    rootCanvas = list[i];
                    break;
                }
            }

            ListPool<Canvas>.Release(list);

            if (!validTemplate)
            {
                SetupTemplate();
                if (!validTemplate)
                    return;
            }

            _templateInstance.gameObject.SetActive(true);

            // popupCanvas used to assume the root canvas had the default sorting Layer, next line fixes (case 958281 - [UI] Dropdown list does not copy the parent canvas layer when the panel is opened)
            _templateInstance.GetComponent<Canvas>().sortingLayerID = rootCanvas.sortingLayerID;

            // Instantiate the drop-down template
            m_Dropdown = CreateDropdownList(_templateInstance.gameObject);
            m_Dropdown.name = "Dropdown List";
            m_Dropdown.SetActive(true);

            // Make drop-down RectTransform have same values as original.
            RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
            dropdownRectTransform.SetParent(_templateInstance.transform.parent, false);

            // Instantiate the drop-down list items

            // Find the dropdown item and disable it.
            DropdownItemXRSupport itemTemplate = m_Dropdown.GetComponentInChildren<DropdownItemXRSupport>();

            GameObject content = itemTemplate.RectTransform.parent.gameObject;
            RectTransform contentRectTransform = content.transform as RectTransform;
            itemTemplate.RectTransform.gameObject.SetActive(true);

            // Get the rects of the dropdown and item
            Rect dropdownContentRect = contentRectTransform.rect;
            Rect itemTemplateRect = itemTemplate.RectTransform.rect;

            // Calculate the visual offset between the item's edges and the background's edges
            Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.RectTransform.localPosition;
            Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.RectTransform.localPosition;
            Vector2 itemSize = itemTemplateRect.size;

            m_Items.Clear();

            Toggle prev = null;
            for (int i = 0; i < options.Count; ++i)
            {
                TMP_Dropdown.OptionData data = options[i];
                DropdownItemXRSupport item = AddItem(data, value == i, itemTemplate, m_Items);
                if (item == null)
                    continue;

                // Automatically set up a toggle state change listener
                item.Toggle.isOn = value == i;
                item.Toggle.onValueChanged.AddListener(x => OnSelectItem(item.Toggle));

                // Select current option
                if (item.Toggle.isOn)
                    item.Toggle.Select();

                // Automatically set up explicit navigation
                if (prev != null)
                {
                    Navigation prevNav = prev.navigation;
                    Navigation toggleNav = item.Toggle.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    toggleNav.mode = Navigation.Mode.Explicit;

                    prevNav.selectOnDown = item.Toggle;
                    prevNav.selectOnRight = item.Toggle;
                    toggleNav.selectOnLeft = prev;
                    toggleNav.selectOnUp = prev;

                    prev.navigation = prevNav;
                    item.Toggle.navigation = toggleNav;
                }
                prev = item.Toggle;
            }

            // Reposition all items now that all of them have been added
            Vector2 sizeDelta = contentRectTransform.sizeDelta;
            sizeDelta.y = itemSize.y * m_Items.Count + offsetMin.y - offsetMax.y;
            contentRectTransform.sizeDelta = sizeDelta;

            float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
            if (extraSpace > 0)
                dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

            // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
            // Typically this will have the effect of placing the dropdown above the button instead of below,
            // but it works as inversion regardless of initial setup.
            Vector3[] corners = new Vector3[4];
            dropdownRectTransform.GetWorldCorners(corners);

            RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
            Rect rootCanvasRect = rootCanvasRectTransform.rect;
            for (int axis = 0; axis < 2; axis++)
            {
                bool outside = false;
                for (int i = 0; i < 4; i++)
                {
                    Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                    if ((corner[axis] < rootCanvasRect.min[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis])) ||
                        (corner[axis] > rootCanvasRect.max[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
                    {
                        outside = true;
                        break;
                    }
                }
                if (outside)
                    RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
            }

            for (int i = 0; i < m_Items.Count; i++)
            {
                RectTransform itemRect = m_Items[i].RectTransform;
                itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
                itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
                itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
                itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
            }

            // Fade in the popup
            AlphaFadeList(m_AlphaFadeSpeed, 0f, 1f);

            // Make drop-down template and item template inactive
            _templateInstance.gameObject.SetActive(false);
            itemTemplate.gameObject.SetActive(false);

            m_Blocker = CreateBlocker(rootCanvas);
        }

        /// <summary>
        /// Create a blocker that blocks clicks to other controls while the dropdown list is open.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain a blocker GameObject.
        /// </remarks>
        /// <param name="rootCanvas">The root canvas the dropdown is under.</param>
        /// <returns>The created blocker object</returns>
        protected virtual GameObject CreateBlocker(Canvas rootCanvas)
        {
            // Create blocker GameObject.
            GameObject blocker = new GameObject("Blocker");

            // Setup blocker RectTransform to cover entire root canvas area.
            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            Canvas blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            Canvas dropdownCanvas = m_Dropdown.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            // Find the Canvas that this dropdown is a part of
            Canvas parentCanvas = null;
            Transform parentTransform = _templateInstance.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            // If we have a parent canvas, apply the same raycasters as the parent for consistency.
            if (parentCanvas != null)
            {
                Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    System.Type raycasterType = components[i].GetType();
                    if (blocker.GetComponent(raycasterType) == null)
                    {
                        blocker.AddComponent(raycasterType);
                    }
                }
            }
            else
            {
                // Add raycaster since it's needed to block.
                GetOrAddComponent<GraphicRaycaster>(blocker);
            }


            // Add image since it's needed to block, but make it clear.
            Image blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            Button blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }

        /// <summary>
        /// Convenience method to explicitly destroy the previously generated blocker object
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to dispose of a blocker GameObject that blocks clicks to other controls while the dropdown list is open.
        /// </remarks>
        /// <param name="blocker">The blocker object to destroy.</param>
        protected virtual void DestroyBlocker(GameObject blocker)
        {
            Destroy(blocker);
        }

        /// <summary>
        /// Create the dropdown list to be shown when the dropdown is clicked. The dropdown list should correspond to the provided template GameObject, equivalent to instantiating a copy of it.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain a dropdown list GameObject.
        /// </remarks>
        /// <param name="template">The template to create the dropdown list from.</param>
        /// <returns>The created drop down list gameobject.</returns>
        protected virtual GameObject CreateDropdownList(GameObject template)
        {
            return (GameObject)Instantiate(template);
        }

        /// <summary>
        /// Convenience method to explicitly destroy the previously generated dropdown list
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to dispose of a dropdown list GameObject.
        /// </remarks>
        /// <param name="dropdownList">The dropdown list GameObject to destroy</param>
        protected virtual void DestroyDropdownList(GameObject dropdownList)
        {
            Destroy(dropdownList);
        }

        /// <summary>
        /// Create a dropdown item based upon the item template.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain an option item.
        /// The option item should correspond to the provided template DropdownItem and its GameObject, equivalent to instantiating a copy of it.
        /// </remarks>
        /// <param name="itemTemplate">e template to create the option item from.</param>
        /// <returns>The created dropdown item component</returns>
        internal virtual DropdownItemXRSupport CreateItem(DropdownItemXRSupport itemTemplate)
        {
            return Instantiate(itemTemplate);
        }

        /// <summary>
        ///  Convenience method to explicitly destroy the previously generated Items.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to dispose of an option item.
        /// Likely no action needed since destroying the dropdown list destroys all contained items as well.
        /// </remarks>
        /// <param name="item">The Item to destroy.</param>
        internal virtual void DestroyItem(DropdownItemXRSupport item) { }

        // Add a new drop-down list item with the specified values.
        private DropdownItemXRSupport AddItem(TMP_Dropdown.OptionData data, bool selected, DropdownItemXRSupport itemTemplate, List<DropdownItemXRSupport> items)
        {
            // Add a new item to the dropdown.
            DropdownItemXRSupport item = CreateItem(itemTemplate);
            item.RectTransform.SetParent(itemTemplate.RectTransform.parent, false);

            item.gameObject.SetActive(true);
            item.gameObject.name = "Item " + items.Count + (data.text != null ? ": " + data.text : "");

            if (item.Toggle != null)
            {
                item.Toggle.isOn = false;
            }

            // Set the item's data
            if (item.Text)
                item.Text.text = data.text;
            if (item.Image)
            {
                item.Image.sprite = data.image;
                item.Image.enabled = (item.Image.sprite != null);
            }

            items.Add(item);
            return item;
        }

        private void AlphaFadeList(float duration, float alpha)
        {
            CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
            AlphaFadeList(duration, group.alpha, alpha);
        }

        private void AlphaFadeList(float duration, float start, float end)
        {
            if (end.Equals(start))
                return;

            FloatTween tween = new FloatTween { duration = duration, startValue = start, targetValue = end };
            tween.AddOnChangedCallback(SetAlpha);
            tween.ignoreTimeScale = true;
            m_AlphaTweenRunner.StartTween(tween);
        }

        private void SetAlpha(float alpha)
        {
            if (!m_Dropdown)
                return;

            CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
            group.alpha = alpha;
        }

        /// <summary>
        /// Hide the dropdown list. I.e. close it.
        /// </summary>
        public void Hide()
        {
            if (m_Coroutine == null)
            {
                if (m_Dropdown != null)
                {
                    AlphaFadeList(m_AlphaFadeSpeed, 0f);

                    // User could have disabled the dropdown during the OnValueChanged call.
                    if (IsActive())
                        m_Coroutine = StartCoroutine(DelayedDestroyDropdownList(m_AlphaFadeSpeed));
                }

                if (m_Blocker != null)
                    DestroyBlocker(m_Blocker);

                m_Blocker = null;
                Select();
            }

            InputManager.Instance.MainInput[InputManager.SelectedLeft].performed -= TryHideFromDeselect;
            InputManager.Instance.MainInput[InputManager.SelectedRight].performed -= TryHideFromDeselect;
        }

        private IEnumerator DelayedDestroyDropdownList(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ImmediateDestroyDropdownList();
        }

        private void ImmediateDestroyDropdownList()
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i] != null)
                    DestroyItem(m_Items[i]);
            }

            m_Items.Clear();

            if (m_Dropdown != null)
                DestroyDropdownList(m_Dropdown);

            if (m_AlphaTweenRunner != null)
                m_AlphaTweenRunner.StopTween();

            m_Dropdown = null;
            m_Coroutine = null;
        }

        // Change the value and hide the dropdown.
        private void OnSelectItem(Toggle toggle)
        {
            if (!toggle.isOn)
                toggle.isOn = true;

            int selectedIndex = -1;
            Transform tr = toggle.transform;
            Transform parent = tr.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) == tr)
                {
                    // Subtract one to account for template child.
                    selectedIndex = i - 1;
                    break;
                }
            }

            if (selectedIndex < 0)
                return;

            value = selectedIndex;
            Hide();
        }

        #region Internal TMPro Coroutine stuff

        // Base interface for tweeners,
        // using an interface instead of
        // an abstract class as we want the
        // tweens to be structs.
        internal interface ITweenValue
        {
            void TweenValue(float floatPercentage);
            bool ignoreTimeScale { get; }
            float duration { get; }
            bool ValidTarget();
        }

        // Color tween class, receives the
        // TweenValue callback and then sets
        // the value on the target.
        internal struct ColorTween : ITweenValue
        {
            public enum ColorTweenMode
            {
                All,
                RGB,
                Alpha
            }

            public class ColorTweenCallback : UnityEvent<Color> { }

            private ColorTweenCallback m_Target;
            private Color m_StartColor;
            private Color m_TargetColor;
            private ColorTweenMode m_TweenMode;

            private float m_Duration;
            private bool m_IgnoreTimeScale;

            public Color startColor
            {
                get { return m_StartColor; }
                set { m_StartColor = value; }
            }

            public Color targetColor
            {
                get { return m_TargetColor; }
                set { m_TargetColor = value; }
            }

            public ColorTweenMode tweenMode
            {
                get { return m_TweenMode; }
                set { m_TweenMode = value; }
            }

            public float duration
            {
                get { return m_Duration; }
                set { m_Duration = value; }
            }

            public bool ignoreTimeScale
            {
                get { return m_IgnoreTimeScale; }
                set { m_IgnoreTimeScale = value; }
            }

            public void TweenValue(float floatPercentage)
            {
                if (!ValidTarget())
                    return;

                var newColor = Color.Lerp(m_StartColor, m_TargetColor, floatPercentage);

                if (m_TweenMode == ColorTweenMode.Alpha)
                {
                    newColor.r = m_StartColor.r;
                    newColor.g = m_StartColor.g;
                    newColor.b = m_StartColor.b;
                }
                else if (m_TweenMode == ColorTweenMode.RGB)
                {
                    newColor.a = m_StartColor.a;
                }
                m_Target.Invoke(newColor);
            }

            public void AddOnChangedCallback(UnityAction<Color> callback)
            {
                if (m_Target == null)
                    m_Target = new ColorTweenCallback();

                m_Target.AddListener(callback);
            }

            public bool GetIgnoreTimescale()
            {
                return m_IgnoreTimeScale;
            }

            public float GetDuration()
            {
                return m_Duration;
            }

            public bool ValidTarget()
            {
                return m_Target != null;
            }
        }

        // Float tween class, receives the
        // TweenValue callback and then sets
        // the value on the target.
        internal struct FloatTween : ITweenValue
        {
            public class FloatTweenCallback : UnityEvent<float> { }

            private FloatTweenCallback m_Target;
            private float m_StartValue;
            private float m_TargetValue;

            private float m_Duration;
            private bool m_IgnoreTimeScale;

            public float startValue
            {
                get { return m_StartValue; }
                set { m_StartValue = value; }
            }

            public float targetValue
            {
                get { return m_TargetValue; }
                set { m_TargetValue = value; }
            }

            public float duration
            {
                get { return m_Duration; }
                set { m_Duration = value; }
            }

            public bool ignoreTimeScale
            {
                get { return m_IgnoreTimeScale; }
                set { m_IgnoreTimeScale = value; }
            }

            public void TweenValue(float floatPercentage)
            {
                if (!ValidTarget())
                    return;

                var newValue = Mathf.Lerp(m_StartValue, m_TargetValue, floatPercentage);
                m_Target.Invoke(newValue);
            }

            public void AddOnChangedCallback(UnityAction<float> callback)
            {
                if (m_Target == null)
                    m_Target = new FloatTweenCallback();

                m_Target.AddListener(callback);
            }

            public bool GetIgnoreTimescale()
            {
                return m_IgnoreTimeScale;
            }

            public float GetDuration()
            {
                return m_Duration;
            }

            public bool ValidTarget()
            {
                return m_Target != null;
            }
        }

        // Tween runner, executes the given tween.
        // The coroutine will live within the given
        // behaviour container.
        internal class TweenRunner<T> where T : struct, ITweenValue
        {
            protected MonoBehaviour m_CoroutineContainer;
            protected IEnumerator m_Tween;

            // utility function for starting the tween
            private static IEnumerator Start(T tweenInfo)
            {
                if (!tweenInfo.ValidTarget())
                    yield break;

                var elapsedTime = 0.0f;
                while (elapsedTime < tweenInfo.duration)
                {
                    elapsedTime += tweenInfo.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                    var percentage = Mathf.Clamp01(elapsedTime / tweenInfo.duration);
                    tweenInfo.TweenValue(percentage);
                    yield return null;
                }
                tweenInfo.TweenValue(1.0f);
            }

            public void Init(MonoBehaviour coroutineContainer)
            {
                m_CoroutineContainer = coroutineContainer;
            }

            public void StartTween(T info)
            {
                if (m_CoroutineContainer == null)
                {
                    Debug.LogWarning("Coroutine container not configured... did you forget to call Init?");
                    return;
                }

                StopTween();

                if (!m_CoroutineContainer.gameObject.activeInHierarchy)
                {
                    info.TweenValue(1.0f);
                    return;
                }

                m_Tween = Start(info);
                m_CoroutineContainer.StartCoroutine(m_Tween);
            }

            public void StopTween()
            {
                if (m_Tween != null)
                {
                    m_CoroutineContainer.StopCoroutine(m_Tween);
                    m_Tween = null;
                }
            }
        }
        #endregion
    }
}