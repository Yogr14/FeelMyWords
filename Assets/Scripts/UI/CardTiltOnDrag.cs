using UnityEngine;

public class CardTiltOnDrag : MonoBehaviour
{
    [SerializeField] private float _maxTiltAngle = 15f; // Максимальный угол наклона
    [SerializeField] private float _tiltSpeed = 10f;    // Скорость "догонки" к целевому наклону
    // [SerializeField] private float _deltaMultiplyer = 2f;

    private Vector3 _lastPosition;
    private float _currentTilt = 0f;

    private void Start()
    {
        _lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        float deltaX = currentPosition.x - _lastPosition.x;

        // Чем быстрее двигается мышь по X, тем больше угол наклона
        float targetTilt = Mathf.Clamp(-deltaX * 15f, -_maxTiltAngle, _maxTiltAngle);

        // Плавное сглаживание перехода
        _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * _tiltSpeed);

        // Применяем вращение относительно верхней центральной точки
        ApplyTilt(_currentTilt);

        _lastPosition = currentPosition;
    }

    private void ApplyTilt(float angle)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // Смещаем pivot к верхней центральной точке
        // rectTransform.pivot = new Vector2(0.5f, 1f);

        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
