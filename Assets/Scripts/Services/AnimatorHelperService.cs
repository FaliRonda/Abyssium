using System;
using System.Collections.Generic;
using DG.Tweening;
using Ju.Extensions;
using Ju.Services;
using UnityEngine;

public class AnimatorHelperService : IAnimatorHelperService, IService
{
    public float GetAnimLength(Animator animator, String animState)
    {
        //TODO obtener el nombre del estado a partir de la animación actual
        string animName = "";

        // Asegúrate de que el Animator y el runtimeAnimatorController no son nulos
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimatorOverrideController animatorOverrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            animName = GetAnimNameFromOverrideAnimator(animatorOverrideController, animState);
        }
        else
        {
            Debug.LogError("Animator o runtimeAnimatorController es nulo");
        }
        
        return animator.runtimeAnimatorController.animationClips.Find(element => element.name == animName).length;
    }

    private string GetAnimNameFromOverrideAnimator(AnimatorOverrideController animatorOverrideController, string animState)
    {
        string overrideAnimName = "";

        string animType = animState.Split("_")[1];
        
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        animatorOverrideController.GetOverrides(overrides);
        
        foreach (KeyValuePair<AnimationClip,AnimationClip> keyValuePair in overrides)
        {
            if (keyValuePair.Key.name.Split("_")[1] == animType)
            {
                overrideAnimName = keyValuePair.Value.name;
            }
        }

        return overrideAnimName;
    }

    public void DoOnAnimationFinish(float animLenght, Action callback)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(animLenght)
            .AppendCallback(()=>
            {
                callback();
            });
    }
}