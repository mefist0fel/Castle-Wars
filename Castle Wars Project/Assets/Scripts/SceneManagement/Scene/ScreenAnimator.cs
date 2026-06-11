using System;
using UnityEngine;

/// <summary>
/// Abstract component that drives the show/hide animation of a scene or window.
/// Attach a concrete subclass to the scene root and assign it on
/// <see cref="AnimatedBaseScene{TToken}._animator"/>, or leave the field empty to let
/// the scene create a <see cref="DefaultScreenAnimator"/> automatically.
///
/// To create a custom animator, inherit from this class and override
/// <see cref="PlayShow"/> and <see cref="PlayHide"/>.
/// </summary>
public abstract class ScreenAnimator : MonoBehaviour
{
    /// <summary>Start the show animation. Invoke <paramref name="onComplete"/> when finished.</summary>
    public abstract void PlayShow(Action onComplete);

    /// <summary>Start the hide animation. Invoke <paramref name="onComplete"/> when finished.</summary>
    public abstract void PlayHide(Action onComplete);
}
