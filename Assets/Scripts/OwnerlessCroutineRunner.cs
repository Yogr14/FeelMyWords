using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnerlessCroutineRunner : MonoBehaviour
{
    private static OwnerlessCroutineRunner _instance;
    private static OwnerlessCroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("OwnerlessCroutineRunner");
                _instance = obj.AddComponent<OwnerlessCroutineRunner>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public static Coroutine Run(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }

    public static void Stop(IEnumerator coroutine)
    {
        Instance.StopCoroutine(coroutine);
    }
}
