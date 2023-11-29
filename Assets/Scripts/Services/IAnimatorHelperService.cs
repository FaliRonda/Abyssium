using System;
using UnityEditor.Animations;
using UnityEngine;

public interface IAnimatorHelperService
{
    public float GetAnimLenght(Animator animator, String animName);

    public void DoOnAnimationFinish(float animLenght, Action callback);
}