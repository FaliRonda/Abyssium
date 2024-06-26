﻿using System;
using UnityEngine;

public interface IAnimatorHelperService
{
    public float GetAnimLength(Animator animator, String animState);

    public void DoOnAnimationFinish(float animLength, Action callback);
}