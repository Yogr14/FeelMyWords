using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CallbacksPreset", menuName = "ScriptableObjects/CallbacksPreset", order = 1)]
public class CallbacksPreset : ScriptableObject
{
    [SerializeField] private List<CallbackBase> _callbacks = new List<CallbackBase>();
    public List<CallbackBase> Callbacks => _callbacks;
    public void SetCallbacks(List<CallbackBase> callbacks)
    {
        _callbacks = callbacks;
    }
    public CallbacksPreset GetRandomPreset(int seed)
    {
        CallbacksPreset randomPreset = ScriptableObject.CreateInstance<CallbacksPreset>();
        System.Random random = new System.Random(seed);
        randomPreset.SetCallbacks(new List<CallbackBase>(_callbacks));
        for (int i = 0; i < randomPreset.Callbacks.Count; i++)
        {
            randomPreset.Callbacks[i].IsEnabled = random.Next(0, 2) == 1;
        }
        return randomPreset;
    }
}
