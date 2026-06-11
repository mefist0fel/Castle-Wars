using System;
using UnityEngine;

public abstract class AnimatedBaseScene<TToken> : BaseScene<TToken>
{
    [SerializeField] private ScreenAnimator _animator;

    protected virtual void OnEnterEnd(TToken token) { }

    protected virtual void OnExitEnd() { }

    protected override void DoEnter(TToken token)
    {
        OnEnter(token);
        EnsureAnimator().PlayShow(() => OnEnterEnd(token));
    }

    protected override void DoExit(Action deactivate)
    {
        OnExit();
        EnsureAnimator().PlayHide(() => { OnExitEnd(); deactivate(); });
    }


    private ScreenAnimator EnsureAnimator()
    {
        if (_animator == null)
            _animator = gameObject.AddComponent<DefaultScreenAnimator>();
        return _animator;
    }
}
