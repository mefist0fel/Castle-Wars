using System;
using UnityEngine;

public class SceneAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator      = null;
    [SerializeField] private string   _enterTrigger  = "Enter";
    [SerializeField] private string   _exitTrigger   = "Exit";
    [SerializeField] private float    _enterDuration = 0.3f;
    [SerializeField] private float    _exitDuration  = 0.3f;

    // ── Public API ────────────────────────────────────────────────────────────

    public void PlayEnter(Action onComplete) => StartCoroutine(PlayRoutine(_enterTrigger, _enterDuration, onComplete));
    public void PlayExit(Action onComplete)  => StartCoroutine(PlayRoutine(_exitTrigger,  _exitDuration,  onComplete));

    // ── Override hooks ────────────────────────────────────────────────────────

    protected virtual void OnPlayEnter(Action onComplete) => StartCoroutine(PlayRoutine(_enterTrigger, _enterDuration, onComplete));
    protected virtual void OnPlayExit(Action onComplete)  => StartCoroutine(PlayRoutine(_exitTrigger,  _exitDuration,  onComplete));

    // ── Internals ─────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator PlayRoutine(string trigger, float duration, Action onComplete)
    {
        if (_animator != null && !string.IsNullOrEmpty(trigger))
            _animator.SetTrigger(trigger);

        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }
}
