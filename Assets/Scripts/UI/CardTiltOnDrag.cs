using UnityEngine;

public class CardTiltOnDrag : MonoBehaviour
{
    [SerializeField] private float _maxTiltAngle = 15f; // Максимальный угол наклона
    [SerializeField] private float _tiltSpeed = 10f;    // Скорость "догонки" к целевому наклону
    // [SerializeField] private float _deltaMultiplyer = 2f;

    private Vector3 _lastPosition;
    private float _currentTilt = 0f;
    private void Awake()
    {
        enabled = RandomEnabled(GameManager.RunRundomSeed);
        Debug.LogFormat("{0} enabled: {1}", GetType().Name, enabled);
    }
    public static bool RandomEnabled(int runSeed)
    {
        int typeHash = typeof(CardTiltOnDrag).Name.GetHashCode();
        int randomValue = new System.Random(runSeed + typeHash).Next(0, 100);
        return randomValue < 50; // 50% шанс включить эффект
    }

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
