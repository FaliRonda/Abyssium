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

    public void SetControllerVibration(float intensity, float duration)
    {
        gamepads = Gamepad.all.ToArray();
        foreach (Gamepad gamepad in gamepads)
        {
            if (gamepad.name.Contains("DualShock5"))
            {
                intensity *= 0.5f;
            }
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
        gamepads = Gamepad.all.ToArray();
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