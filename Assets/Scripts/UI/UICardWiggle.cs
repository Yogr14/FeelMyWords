using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UICardWiggleCoroutine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
{
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isHovering = false;
    private int originalSiblingIndex;

    public float hoverScale = 1.05f;
    public float rotationAngle = 5f;
    public float effectDuration = 0.1f;

    private void Awake()
    {
        enabled = RandomEnabled(GameManager.RunRundomSeed);
        Debug.LogFormat("{0} enabled: {1}", GetType().Name, enabled);
    }
    public static bool RandomEnabled(int runSeed)
    {
        int typeHash = typeof(UICardWiggleCoroutine).Name.GetHashCode();
        int randomValue = new System.Random(runSeed + typeHash).Next(0, 100);
        return randomValue < 50; // 50% шанс включить эффект
    }
    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHovering)
        {
            isHovering = true;

            // Запоминаем позицию и поднимаем карточку вверх по иерархии
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();

            StopAllCoroutines();
            StartCoroutine(WiggleOnly());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Возвращаем карточку обратно
        transform.SetSiblingIndex(originalSiblingIndex);

        StopAllCoroutines();
        StartCoroutine(ResetAll());
    }

    IEnumerator WiggleOnly()
    {
        Vector3 targetScale = originalScale * hoverScale;
        transform.localScale = targetScale;

        Quaternion wiggleRotation = Quaternion.Euler(0, 0, rotationAngle + Random.Range(-3, 3));
        float timer = 0f;
        float duration = effectDuration * 0.4f;

        while (timer < duration)
        {
            float t = timer / duration;
            transform.rotation = Quaternion.Lerp(originalRotation, wiggleRotation, t);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            transform.rotation = Quaternion.Lerp(wiggleRotation, originalRotation, t);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
    }

    IEnumerator ResetAll()
    {
        Vector3 currentScale = transform.localScale;
        Quaternion currentRotation = transform.rotation;

        float timer = 0f;
        float duration = effectDuration * 0.6f;

        while (timer < duration)
        {
            float t = timer / duration;
            transform.localScale = Vector3.Lerp(currentScale, originalScale, t);
            transform.rotation = Quaternion.Lerp(currentRotation, originalRotation, t);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        transform.rotation = originalRotation;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(ResetAll());
    }
}
