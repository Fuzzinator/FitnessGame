using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class TwoDSlider : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
{
    #region UNITY

    [System.Serializable]
    /// <summary>
    /// Event type used by the UI.Slider.
    /// </summary>
    public class SliderEvent : UnityEvent<Vector2>
    {
    }

    [SerializeField]
    private RectTransform m_FillRect;

    public RectTransform fillRect
    {
        get { return m_FillRect; }
        set
        {
            if (SetClass(ref m_FillRect, value))
            {
                UpdateCachedReferences();
                UpdateVisuals();
            }
        }
    }

    [SerializeField]
    private RectTransform m_HandleRect;

    [SerializeField]
    private Vector2 m_MinValue = Vector2.zero;

    public Vector2 minValue
    {
        get { return m_MinValue; }
        set
        {
            if (SetStruct(ref m_MinValue, value))
            {
                Set(m_Value);
                UpdateVisuals();
            }
        }
    }

    [SerializeField]
    private Vector2 m_MaxValue = Vector2.one;

    public Vector2 maxValue
    {
        get { return m_MaxValue; }
        set
        {
            if (SetStruct(ref m_MaxValue, value))
            {
                Set(m_Value);
                UpdateVisuals();
            }
        }
    }

    [SerializeField]
    private bool m_WholeNumbers = false;

    public bool wholeNumbers
    {
        get { return m_WholeNumbers; }
        set
        {
            if (SetStruct(ref m_WholeNumbers, value))
            {
                Set(m_Value);
                UpdateVisuals();
            }
        }
    }

    [SerializeField]
    protected Vector2 m_Value;

    public virtual Vector2 value
    {
        get { return wholeNumbers ? new Vector2(Mathf.Round(m_Value.x), Mathf.Round(m_Value.y)) : m_Value; }
        set { Set(value); }
    }

    public virtual void SetValueWithoutNotify(Vector2 input)
    {
        Set(input, false);
    }

    public Vector2 normalizedValue
    {
        get
        {
            if (Mathf.Approximately(minValue.x, maxValue.x) && Mathf.Approximately(minValue.y, maxValue.y))
            {
                return Vector2.zero;
            }

            return InverseLerp(minValue, maxValue, value);
        }
        set { this.value = Lerp(minValue, maxValue, value); }
    }

    [Space]
    [SerializeField]
    private SliderEvent m_OnValueChanged = new SliderEvent();

    public SliderEvent onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }


    private Image m_FillImage;
    private Transform m_FillTransform;
    private RectTransform m_FillContainerRect;
    private Transform m_HandleTransform;
    private RectTransform m_HandleContainerRect;

    // The offset from handle position to mouse down position
    private Vector2 m_Offset = Vector2.zero;

    // field is never assigned warning
#pragma warning disable 649
    private DrivenRectTransformTracker m_Tracker;
#pragma warning restore 649

    // This "delayed" mechanism is required for case 1037681.
    private bool m_DelayedUpdateVisuals = false;

    // Size of each step.
    Vector2 stepSize
    {
        get { return wholeNumbers ? Vector2.one : (maxValue - minValue) * 0.1f; }
    }

    protected TwoDSlider()
    {
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (wholeNumbers)
        {
            m_MinValue = new Vector2(Mathf.Round(m_MinValue.x), Mathf.Round(m_MinValue.y));
            m_MaxValue = new Vector2(Mathf.Round(m_MaxValue.x), Mathf.Round(m_MaxValue.y));
        }

        //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
        if (IsActive())
        {
            UpdateCachedReferences();
            // Update rects in next update since other things might affect them even if value didn't change.
            m_DelayedUpdateVisuals = true;
        }

        if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

#endif // if UNITY_EDITOR

    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
        if (executing == CanvasUpdate.Prelayout)
            onValueChanged.Invoke(value);
#endif
    }

    public virtual void LayoutComplete()
    {
    }

    public virtual void GraphicUpdateComplete()
    {
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateCachedReferences();
        Set(m_Value, false);
        // Update rects since they need to be initialized correctly.
        UpdateVisuals();
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
        base.OnDisable();
    }

    protected virtual void Update()
    {
        if (m_DelayedUpdateVisuals)
        {
            m_DelayedUpdateVisuals = false;
            Set(m_Value, false);
            UpdateVisuals();
        }
    }

    protected override void OnDidApplyAnimationProperties()
    {
        // Has value changed? Various elements of the slider have the old normalisedValue assigned, we can use this to perform a comparison.
        // We also need to ensure the value stays within min/max.
        m_Value = ClampValue(m_Value);
        var oldNormalizedValue = normalizedValue;
        if (m_FillContainerRect != null)
        {
            oldNormalizedValue =
                (_reverseValue ? Vector2.one - m_FillRect.anchorMin : m_FillRect.anchorMax);
        }
        else if (m_HandleContainerRect != null)
            oldNormalizedValue =
                (_reverseValue ? Vector2.one - m_HandleRect.anchorMin : m_HandleRect.anchorMin);

        UpdateVisuals();

        if (oldNormalizedValue != normalizedValue)
        {
            UISystemProfilerApi.AddMarker("Slider.value", this);
            onValueChanged.Invoke(m_Value);
        }
    }

    void UpdateCachedReferences()
    {
        if (m_FillRect && m_FillRect != (RectTransform) transform)
        {
            m_FillTransform = m_FillRect.transform;
            m_FillImage = m_FillRect.GetComponent<Image>();
            if (m_FillTransform.parent != null)
                m_FillContainerRect = m_FillTransform.parent.GetComponent<RectTransform>();
        }
        else
        {
            m_FillRect = null;
            m_FillContainerRect = null;
            m_FillImage = null;
        }

        if (m_HandleRect && m_HandleRect != (RectTransform) transform)
        {
            m_HandleTransform = m_HandleRect.transform;
            if (m_HandleTransform.parent != null)
                m_HandleContainerRect = m_HandleTransform.parent.GetComponent<RectTransform>();
        }
        else
        {
            m_HandleRect = null;
            m_HandleContainerRect = null;
        }
    }

    Vector2 ClampValue(Vector2 input)
    {
        var newValue = new Vector2(Mathf.Clamp(input.x, minValue.x, maxValue.x),
            Mathf.Clamp(input.y, minValue.y, maxValue.y));
        if (wholeNumbers)
            newValue = Round(newValue);
        return newValue;
    }

    protected virtual void Set(Vector2 input, bool sendCallback = true)
    {
        // Clamp the input
        var newValue = ClampValue(input);

        // If the stepped value doesn't match the last one, it's time to update
        if (m_Value == newValue)
            return;

        m_Value = newValue;
        UpdateVisuals();
        if (sendCallback)
        {
            UISystemProfilerApi.AddMarker("Slider.value", this);
            m_OnValueChanged.Invoke(newValue);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
        if (!IsActive())
            return;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UpdateCachedReferences();
#endif

        m_Tracker.Clear();

        if (m_FillContainerRect != null)
        {
            m_Tracker.Add(this, m_FillRect, DrivenTransformProperties.Anchors);
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;


            if (_reverseValue)
                anchorMin = Vector2.one - normalizedValue;
            else
                anchorMax = normalizedValue;

            m_FillRect.anchorMin = anchorMin;
            m_FillRect.anchorMax = anchorMax;
        }

        if (m_HandleContainerRect != null)
        {
            m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;
            anchorMin = anchorMax = (_reverseValue ? (Vector2.one - normalizedValue) : normalizedValue);
            m_HandleRect.anchorMin = anchorMin;
            m_HandleRect.anchorMax = anchorMax;
        }
    }

    [SerializeField]
    private bool _reverseValue = false;


    void UpdateDrag(PointerEventData eventData, Camera cam)
    {
        RectTransform clickRect = m_HandleContainerRect ?? m_FillContainerRect;
        if (clickRect != null && clickRect.rect.size.x > 0 && clickRect.rect.size.y > 0)
        {
            Vector2 position = Vector2.zero;
            if (!GetRelativeMousePositionForDrag(eventData, ref position))
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, position, cam, out localCursor))
                return;
            localCursor -= clickRect.rect.position;

            var valX = Mathf.Clamp01((localCursor.x - m_Offset.x) / clickRect.rect.size.x);
            var valY = Mathf.Clamp01((localCursor.y - m_Offset.y) / clickRect.rect.size.y);
            var val = new Vector2(valX, valY);
            normalizedValue = (_reverseValue ? Vector2.one - val : val);
        }
    }

    private bool MayDrag(PointerEventData eventData)
    {
        return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!MayDrag(eventData))
            return;

        base.OnPointerDown(eventData);

        m_Offset = Vector2.zero;
        if (m_HandleContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect,
                eventData.pointerPressRaycast.screenPosition, eventData.enterEventCamera))
        {
            Vector2 localMousePos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect,
                    eventData.pointerPressRaycast.screenPosition, eventData.pressEventCamera, out localMousePos))
                m_Offset = localMousePos;
        }
        else
        {
            // Outside the slider handle - jump to this point instead
            UpdateDrag(eventData, eventData.pressEventCamera);
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!MayDrag(eventData))
            return;
        UpdateDrag(eventData, eventData.pressEventCamera);
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
        {
            base.OnMove(eventData);
            return;
        }

        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                Set(_reverseValue ? value + stepSize : value - stepSize);
                break;
            case MoveDirection.Right:
                Set(_reverseValue ? value - stepSize : value + stepSize);
                break;
            case MoveDirection.Up:
                Set(_reverseValue ? value - stepSize : value + stepSize);
                break;
            case MoveDirection.Down:
                Set(_reverseValue ? value + stepSize : value - stepSize);
                break;
        }
    }

    /// <summary>
    /// See Selectable.FindSelectableOnLeft
    /// </summary>
    public override Selectable FindSelectableOnLeft()
    {
        return null;
    }

    /// <summary>
    /// See Selectable.FindSelectableOnRight
    /// </summary>
    public override Selectable FindSelectableOnRight()
    {
        return null;
    }

    /// <summary>
    /// See Selectable.FindSelectableOnUp
    /// </summary>
    public override Selectable FindSelectableOnUp()
    {
        return null;
    }

    /// <summary>
    /// See Selectable.FindSelectableOnDown
    /// </summary>
    public override Selectable FindSelectableOnDown()
    {
        return null;
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }


    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool GetRelativeMousePositionForDrag(PointerEventData eventData, ref Vector2 position)
    {
#if UNITY_EDITOR
        position = eventData.position;
#else
            int pressDisplayIndex = eventData.pointerPressRaycast.displayIndex;
            var relativePosition = Display.RelativeMouseAt(eventData.position);
            int currentDisplayIndex = (int)relativePosition.z;

            // Discard events on a different display.
            if (currentDisplayIndex != pressDisplayIndex)
                return false;

            // If we are not on the main display then we must use the relative position.
            position = pressDisplayIndex != 0 ? (Vector2)relativePosition : eventData.position;
#endif
        return true;
    }
    
    #endregion

    #region Extentions

    public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 val)
    {
        return new Vector2(a.x + (b.x - a.x) * Mathf.Clamp01(val.x), a.y + (b.y - a.y) * Mathf.Clamp01(val.y));
    }

    public static Vector2 InverseLerp(Vector2 minValue, Vector2 maxValue, Vector2 value)
    {
        var lerpedValue = Vector2.zero;
        if (!Mathf.Approximately(minValue.x, maxValue.x))
        {
            lerpedValue.x = Mathf.Clamp01((value.x - minValue.x) / (maxValue.x - minValue.x));
        }

        if (!Mathf.Approximately(minValue.y, maxValue.y))
        {
            lerpedValue.y = Mathf.Clamp01((value.y - minValue.y) / (maxValue.y - minValue.y));
        }

        return lerpedValue;
    }

    public static Vector2 Round(Vector2 value)
    {
        value.x = Mathf.Round(value.x);
        value.y = Mathf.Round(value.y);
        return value;
    }

    #endregion
}