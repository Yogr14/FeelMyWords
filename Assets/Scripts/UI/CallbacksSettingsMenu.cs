using System.Collections.Generic;
using UnityEngine;

public class CallbacksSettingsMenu : MonoBehaviour
{
    [SerializeField] private CallbacksPreset _preset;
    [SerializeField] private CallbackUIElement _callbackPrefab;
    [SerializeField] private Transform _callbackListParent;
    private List<CallbackUIElement> _callbackUIElements = new List<CallbackUIElement>();

    public CallbacksPreset GetCurrentPreset() => _preset;
    private void Awake()
    {
        SetupCallbacksUIElements();
        gameObject.SetActive(false);
    }
    private void SetupCallbacksUIElements()
    {
        foreach (var callback in _preset.Callbacks)
        {
            var callbackUIElement = Instantiate(_callbackPrefab, _callbackListParent);
            callbackUIElement.Setup(callback.Name, callback.IsEnabled);
            callbackUIElement.OnCallbackToggleChanged += (value) => { callback.IsEnabled = value; };
            _callbackUIElements.Add(callbackUIElement);
        }
    }
}
