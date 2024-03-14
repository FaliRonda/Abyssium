using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Services;
using UnityEngine.InputSystem;

public class GamepadVibrationService : IService
{
    private Gamepad[] gamepads;
    private float currentIntensity;
    private TweenerCore<float,float,FloatOptions> vibrationSequence;

    public void Initialize()
    {
        gamepads = Gamepad.all.ToArray();
    }

    public void SetControllerVibration(float intensity, float duration)
    {
        foreach (Gamepad gamepad in gamepads)
        {
            currentIntensity = 0;
            
            if (gamepads != null)
            {
                vibrationSequence = DOTween.To(() => currentIntensity, x => currentIntensity = x, intensity, duration)
                    .OnUpdate(() => {
                        gamepad.SetMotorSpeeds(currentIntensity, currentIntensity);
                    })
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { gamepad.SetMotorSpeeds(0, 0); });
            }
        }
    }

    public void StopVibration()
    {
        foreach (Gamepad gamepad in gamepads)
        {
            if (gamepads != null)
            {
                vibrationSequence.Kill();
                gamepad.SetMotorSpeeds(0, 0);
            }
        }
    }
}