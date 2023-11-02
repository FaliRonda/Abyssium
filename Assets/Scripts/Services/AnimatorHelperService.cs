using System;
using DG.Tweening;
using Ju.Extensions;
using Ju.Services;
using UnityEngine;

public class AnimatorHelperService : IAnimatorHelperService, IService
{
    public float GetAnimLenght(Animator animator, String animName)
    {
        return animator.runtimeAnimatorController.animationClips.Find(element => element.name == animName).length;
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