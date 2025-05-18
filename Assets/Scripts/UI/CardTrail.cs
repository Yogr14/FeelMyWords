using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class CardTrail : MonoBehaviour
{
    [SerializeField] private GameObject _trailPrefab;
    [SerializeField] private float _baseTrailInterval = 0.05f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField, CurveRange(0, 0, 1, 1)] private AnimationCurve _fadeCurve;
    [SerializeField, CurveRange(0, 0, 1000, 1)] private AnimationCurve _speedToFrequencyCurve;
    [SerializeField] private bool _rotateTrailWithTarget = false;

    private RectTransform _rectTransform;
    private Vector3 _lastPosition;
    private float _lastSpeed;
    private Coroutine _trailCoroutine;
    private bool _isMoving = false;

    private List<GameObject> _activeTrails = new List<GameObject>();

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _lastPosition = _rectTransform.position;
    }

    void Update()
    {
        Vector3 currentPosition = _rectTransform.position;
        float speed = (currentPosition - _lastPosition).magnitude / Time.deltaTime;
        _lastSpeed = speed;

        if (currentPosition != _lastPosition)
        {
            if (!_isMoving)
            {
                _isMoving = true;
                _trailCoroutine = StartCoroutine(SpawnTrail());
            }
        }
        else
        {
            if (_isMoving)
            {
                _isMoving = false;
                if (_trailCoroutine != null)
                    StopCoroutine(_trailCoroutine);
            }
        }

        _lastPosition = currentPosition;
    }

    IEnumerator SpawnTrail()
    {
        while (true)
        {
            CreateTrailInstance();

            float frequencyMultiplier = _speedToFrequencyCurve.Evaluate(_lastSpeed);
            float dynamicInterval = Mathf.Max(0.005f, _baseTrailInterval * (1f - frequencyMultiplier));
            yield return new WaitForSeconds(dynamicInterval);
        }
    }

    void CreateTrailInstance()
    {
        GameObject ghost = Instantiate(_trailPrefab, _rectTransform.position, _rectTransform.rotation, _rectTransform.parent);
        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
        ghostRect.sizeDelta = _rectTransform.sizeDelta;

        if (!_rotateTrailWithTarget)
            ghostRect.rotation = Quaternion.identity;

        int targetIndex = _rectTransform.GetSiblingIndex();
        ghost.transform.SetSiblingIndex(targetIndex);

        Image ghostImage = ghost.GetComponent<Image>();
        if (ghostImage != null)
        {
            Color c = ghostImage.color;
            c.a = 0.5f;
            ghostImage.color = c;

            _activeTrails.Add(ghost); // Добавляем в список активных
            StartCoroutine(FadeAndDestroy(ghostImage, ghost));
        }
    }

    IEnumerator FadeAndDestroy(Image image, GameObject ghostObject)
    {
        float elapsed = 0f;
        Color original = image.color;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _fadeDuration;
            float alpha = original.a * _fadeCurve.Evaluate(t);
            image.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        _activeTrails.Remove(ghostObject); // Удаляем из списка
        Destroy(ghostObject);
    }

    /// <summary>
    /// Удаляет все текущие трейлы мгновенно.
    /// </summary>
    public void ClearAllTrails()
    {
        foreach (GameObject trail in _activeTrails)
        {
            if (trail != null)
                Destroy(trail);
        }

        _activeTrails.Clear();
    }

    private void OnDestroy()
    {
        ClearAllTrails();
    }
}
