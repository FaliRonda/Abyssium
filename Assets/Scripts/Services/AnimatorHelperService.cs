using System;
using DG.Tweening;
using Ju.Extensions;
using Ju.Services;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorHelperService : IAnimatorHelperService, IService
{
    public float GetAnimLenght(Animator animator, String animState)
    {
        //TODO obtener el nombre del estado a partir de la animación actual
        string animName = "";

        // Asegúrate de que el Animator y el runtimeAnimatorController no son nulos
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Obtén el AnimatorController asociado al Animator
            AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
            if (animatorController == null)
            {
                AnimatorOverrideController animatorOverrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
                animatorController = animatorOverrideController.runtimeAnimatorController as AnimatorController;
            }

            if (animatorController != null)
            {
                // Itera a través de todas las capas del AnimatorController
                for (int i = 0; i < animatorController.layers.Length; i++)
                {
                    // Obtén la capa actual
                    AnimatorControllerLayer layer = animatorController.layers[i];

                    // Obtén el estado del AnimatorStateMachine asociado a la capa
                    ChildAnimatorState[] states = layer.stateMachine.states;

                    foreach (ChildAnimatorState state in states)
                    {
                        // Verifica si el nombre del estado coincide
                        if (state.state.name == animState)
                        {
                            // Obtén el AnimationClip asociado al estado
                            AnimationClip animationClip = state.state.motion as AnimationClip;

                            // Imprime el nombre del AnimationClip
                            if (animationClip != null)
                            {
                                animName = animationClip.name;
                            }
                            else
                            {
                                Debug.LogError("No se encontró un AnimationClip asociado al estado " + animState);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Animator o runtimeAnimatorController es nulo");
        }
        
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