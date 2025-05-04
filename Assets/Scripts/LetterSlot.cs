using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class LetterSlot : MonoBehaviour
{
    [SerializeField] private RectTransform _slotRectTransform;
    private int _slotID = -1;
    [ShowNativeProperty] public int SlotID => _slotID;
    private LetterBox _storedLetterBox;
    public LetterBox StoredLetterBox => _storedLetterBox;
    [ShowNativeProperty] public bool IsEmpty => _storedLetterBox == null;
    public bool IsInRect(Vector2 position)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_slotRectTransform, position);
    }
    public void Setup(int slotID)
    {
        _slotID = slotID;
    }
    public bool TryEnqueueLetterBox(LetterBox letterBox)
    {
        if(!IsEmpty) return false;
        if(letterBox.IsInWord) letterBox.OnRemovedFromWord();

        letterBox.transform.SetParent(transform);
        letterBox.transform.localPosition = Vector3.zero;
        _storedLetterBox = letterBox;
        letterBox.OnAddedToWord(_slotID);

        return true;
    }
    public void ClearSlot()
    {
        if(_storedLetterBox != null)
        {
            _storedLetterBox = null;
        }
    }
}
