using DG.Tweening;
using Ju.Services;
using UnityEngine.InputSystem;

public class GamepadVibrationService : IService
{
    private Gamepad gamepad;
    private float currentIntensity;
    public void Initialize() {
        gamepad = Gamepad.current;
    }

    public void SetControllerVibration(float intensity, float duration)
    {
        currentIntensity = 0;
        
        if (gamepad != null)
        {
            DOTween.To(() => currentIntensity, x => currentIntensity = x, intensity, duration)
                .OnUpdate(() => {
                    gamepad.SetMotorSpeeds(currentIntensity, currentIntensity);
                })
                .SetEase(Ease.OutBack)
                .OnComplete(() => { gamepad.SetMotorSpeeds(0, 0); });
        }
    }
}