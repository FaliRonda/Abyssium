using System;
using UnityEngine;

public interface IAnimatorHelperService
{
    public float GetAnimLenght(Animator animator, String animName);

    public void DoOnAnimationFinish(float animLenght, Action<string> callback);
}