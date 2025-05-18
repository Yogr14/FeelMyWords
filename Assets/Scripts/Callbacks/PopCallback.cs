using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PopCallback", menuName = "ScriptableObjects/Callbacks/Pop", order = 1)]
public class PopCallback : CallbackBase
{
    public override void OnLetterBoxHovered(GameObject letterBox, Vector3 position)
    {
        OwnerlessCroutineRunner.Run(Pop(letterBox));
    }

    private IEnumerator Pop(GameObject letterBox)
    {
        // float t = 1f;

        // while (t > 0f)
        // {
        //     t -= Time.deltaTime * 2f;
        //     if (letterBox == null) yield break;
        //     letterBox.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, Mathf.Sin(Mathf.PI * (1 - t)));
        //     yield return null;
        // }
        yield return null;
    }
}
