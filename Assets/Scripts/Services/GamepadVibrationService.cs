using DG.Tweening;
using Ju.Services;
using UnityEngine.InputSystem;

public class GamepadVibrationService : IService
{
    private Gamepad gamepad;

    public void Initialize() {
        gamepad = Gamepad.current;
    }

    public void SetControllerVibration(float intensity, float duration) {
        if (gamepad != null) {
            gamepad.SetMotorSpeeds(intensity, intensity);
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(duration).AppendCallback(() => gamepad.SetMotorSpeeds(0, 0));
        }
    }
}