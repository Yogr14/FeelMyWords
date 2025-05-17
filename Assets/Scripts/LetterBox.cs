using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;

public class LetterBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI _letterText;
    [SerializeField] private CanvasGroup _canvasGroup;
    private int _ownerSlotID = -1;
    private bool _isInWord = false;
    private Vector3 _mouseOffset;
    private Transform _parentTransform;
    private Vector3 _startDragPosition;
    private CallbacksPreset _callbacksPreset;
    [ShowNativeProperty] public int OwnerSlotID => _ownerSlotID;
    public string Letter => _letterText.text;
    [ShowNativeProperty] public bool IsInWord => _isInWord;

    public Action<LetterBox> OnEndDragAction;

    public void Setup(string letter, CallbacksPreset callbacksPreset)
    {
        _letterText.text = letter;
        _callbacksPreset = callbacksPreset;
    }
    public void OnAddedToWord(int slotID)
    {
        _isInWord = true;
        _ownerSlotID = slotID;
    }
    public void OnRemovedFromWord()
    {
        _isInWord = false;
        _ownerSlotID = -1;
    }
    public void FlyBack()
    {
        return; // fly back removed
        //fly back to position of start dragging process
        StartCoroutine(FlyBackRoutine());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _mouseOffset = (Vector3)eventData.position - transform.position;
        _startDragPosition = transform.position;
        _parentTransform = transform.parent;
        if (_parentTransform.parent != null)
            transform.SetParent(_parentTransform.parent.parent);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = (Vector3)eventData.position - _mouseOffset;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _mouseOffset = Vector3.zero;
        transform.SetParent(_parentTransform);
        OnEndDragAction?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_callbacksPreset == null) return;
        foreach (var callback in _callbacksPreset.Callbacks)
        {
            if (callback.IsEnabled)
            {
                callback.OnLetterBoxHovered(gameObject, eventData.position);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_callbacksPreset == null) return;
        foreach (var callback in _callbacksPreset.Callbacks)
        {
            if (callback.IsEnabled)
            {
                callback.OnLetterBoxUnhovered(gameObject, eventData.position);
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_callbacksPreset == null) return;
        foreach (var callback in _callbacksPreset.Callbacks)
        {
            if (callback.IsEnabled)
            {
                callback.OnLetterBoxPressed(gameObject, eventData.position, _isInWord, _ownerSlotID);
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_callbacksPreset == null) return;
        foreach (var callback in _callbacksPreset.Callbacks)
        {
            if (callback.IsEnabled)
            {
                callback.OnLetterBoxReleased(gameObject, eventData.position, _isInWord, _ownerSlotID);
            }
        }
    }
    private IEnumerator FlyBackRoutine()
    {
        _canvasGroup.blocksRaycasts = false;
        Vector3 start = transform.position;
        Vector3 end = _startDragPosition;
        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(start, end, 1 - t);
            yield return null;
        }
        _canvasGroup.blocksRaycasts = true;
        transform.SetParent(_parentTransform);
    }
}
