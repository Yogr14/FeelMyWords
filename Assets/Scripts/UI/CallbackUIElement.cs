using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CallbackUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _callbackNameText;
    [SerializeField] private Toggle _callbackToggle;
    public Action<bool> OnCallbackToggleChanged;
    private void Awake()
    {
        _callbackToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }
    public void Setup(string callbackName, bool isOn)
    {
        _callbackNameText.text = callbackName;
        _callbackToggle.SetIsOnWithoutNotify(isOn);
    }
    private void OnToggleValueChanged(bool value)
    {
        OnCallbackToggleChanged?.Invoke(value);
    }
}
