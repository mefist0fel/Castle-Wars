using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class LoadingFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;
    private Coroutine   _current;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(Action onComplete = null)
    {
        gameObject.SetActive(true);
        Restart(FadeRoutine(0f, 1f, onComplete));
    }

    public void Hide(Action onComplete = null) => Restart(FadeRoutine(1f, 0f, onComplete));

    private void Restart(IEnumerator routine)
    {
        if (_current != null)
            StopCoroutine(_current);
        _current = StartCoroutine(routine);
    }

    private IEnumerator FadeRoutine(float from, float to, Action onComplete)
    {
        yield return null; // wait for one frame to avoid laggy first frame
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
        gameObject.SetActive(to > 0f);
        onComplete?.Invoke();
    }
}
