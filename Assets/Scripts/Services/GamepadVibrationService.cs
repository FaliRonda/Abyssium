using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Services;
using UnityEngine.InputSystem;

public class GamepadVibrationService : IService
{
    private Gamepad gamepad;
    private float currentIntensity;
    private TweenerCore<float,float,FloatOptions> vibrationSequence;

    public void Initialize() {
        gamepad = Gamepad.current;
    }

    public void SetControllerVibration(float intensity, float duration)
    {
        currentIntensity = 0;
        
        if (gamepad != null)
        {
            vibrationSequence = DOTween.To(() => currentIntensity, x => currentIntensity = x, intensity, duration)
                .OnUpdate(() => {
                    gamepad.SetMotorSpeeds(currentIntensity, currentIntensity);
                })
                .SetEase(Ease.OutBack)
                .OnComplete(() => { gamepad.SetMotorSpeeds(0, 0); });
        }
    }

    public void StopVibration()
    {
        if (gamepad != null)
        {
            vibrationSequence.Kill();
            gamepad.SetMotorSpeeds(0, 0);
        }
    }
}