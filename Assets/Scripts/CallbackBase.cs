using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CallbackBase : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private bool _isEnabled = true;
    public string Name {
        get
        {
            if (string.IsNullOrEmpty(_name))
            {
                return GetType().Name;
            }
            return _name;
        }
    }
    public bool IsEnabled 
    {
        get => _isEnabled; 
        set => _isEnabled = value; 
    }
    /// <summary>
    /// Called on mouse down event above letter.
    /// </summary>
    /// <param name="letterBox">letter box object</param>
    /// <param name="position">mouse click pos</param>
    /// <param name="letterIsFree">letter is in free zone or in word</param>
    /// <param name="slotID">if letter is in word zone pass the id of letter in word</param>
    public virtual void OnLetterBoxPressed(GameObject letterBox, Vector3 position, bool letterIsFree, int slotID){}
    /// <summary>
    /// Called on mouse up event above letter.
    /// </summary>
    /// <param name="letterBox">letter box object</param>
    /// <param name="position">mouse click pos</param>
    /// <param name="letterIsFree">letter is in free zone or in word</param>
    /// <param name="slotID">if letter is in word zone pass the id of letter in word</param>
    public virtual void OnLetterBoxReleased(GameObject letterBox, Vector3 position, bool letterIsFree, int slotID){}
    /// <summary>
    /// Called when letter is added to word.
    /// </summary>
    /// <param name="letterBox">letter box object</param>
    /// <param name="slotID">id of letter in word</param>
    public virtual void OnLetterAddToWord(GameObject letterBox, int slotID){}
    /// <summary>
    /// Called when letter is removed from word.
    /// </summary>
    /// <param name="letterBox">letter box object</param>
    /// <param name="slotID">id of letter in word</param>
    public virtual void OnLetterRemovedFromWord(GameObject letterBox, int slotID){}
    public virtual void OnLetterBoxHovered(GameObject letterBox, Vector3 position){}
    public virtual void OnLetterBoxUnhovered(GameObject letterBox, Vector3 position){}
}
