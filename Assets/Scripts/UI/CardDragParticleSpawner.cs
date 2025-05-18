using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragParticleSpawner : MonoBehaviour, IEndDragHandler
{
    public GameObject particlePrefab; // Префаб частицы (UI Image)
    public int particleCount = 10;
    public float explosionRadius = 100f;
    public Vector2 _particleLifetimeRange = new Vector2(0.5f, 1f);
    public float gravity = -500f; // Отрицательное значение для движения вниз
    public float _yOffset = 0f;

    public void OnEndDrag(PointerEventData eventData)
    {
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = Instantiate(particlePrefab, transform.parent);
            RectTransform rt = particle.GetComponent<RectTransform>();
            rt.position = transform.position;

            // Случайное начальное направление, но вверх/в стороны
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector2 initialVelocity = dir * (explosionRadius / _particleLifetimeRange.y); // Скорость зависит от радиуса и времени

            StartCoroutine(ExplodeParticle(rt, initialVelocity));
        }
    }

    private System.Collections.IEnumerator ExplodeParticle(RectTransform rt, Vector2 initialVelocity)
    {
        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;
        startPos.y += _yOffset;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;
        float lifetime = Random.Range(_particleLifetimeRange.x, _particleLifetimeRange.y);

        while (elapsed < lifetime)
        {
            float t = elapsed;

            // Параболическое движение
            float x = startPos.x + initialVelocity.x * t;
            float y = startPos.y + initialVelocity.y * t + 0.5f * gravity * t * t;

            rt.anchoredPosition = new Vector2(x, y);
            rt.localScale = Vector3.Lerp(startScale, endScale, elapsed / lifetime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(rt.gameObject);
    }
}
