using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Default screen animator: fades a <see cref="CanvasGroup"/> in and out along an animation curve.
/// A CanvasGroup is created automatically on the same GameObject if one is not already present.
/// Created automatically by <see cref="AnimatedBaseScene{TToken}"/> when no animator is assigned.
/// </summary>
public sealed class DefaultScreenAnimator : ScreenAnimator
{
    [SerializeField] private float          _duration = 0.3f;
    [SerializeField] private AnimationCurve _curve    = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup _group;

    private void Awake()
    {
        if (!TryGetComponent(out _group))
            _group = gameObject.AddComponent<CanvasGroup>();
    }

    public override void PlayShow(Action onComplete) => StartCoroutine(FadeTo(1f, onComplete));
    public override void PlayHide(Action onComplete) => StartCoroutine(FadeTo(0f, onComplete));

    private IEnumerator FadeTo(float target, Action onComplete)
    {
        if (_group == null) {
            onComplete?.Invoke();
            yield break;
        }

        float start   = _group.alpha;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed      += Time.unscaledDeltaTime;
            _group.alpha  = Mathf.Lerp(start, target, _curve.Evaluate(Mathf.Clamp01(elapsed / _duration)));
            yield return null;
        }

        _group.alpha = target;
        onComplete?.Invoke();
    }
}
