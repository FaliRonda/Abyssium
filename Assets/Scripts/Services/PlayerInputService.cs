using Ju.Services;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputService : IService
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction rollAction;
    private InputAction interactAction;
    private InputAction cameraRotationAction;
    private InputAction cameraRotationMouseAction;
    private InputAction closeAction;
    private InputAction restartAction;
    
    private Vector2 inputDirection = Vector2.zero;
    private PJ pj;

    public struct ControlInputData
    {
        public Vector3 movementDirection;
        public Vector3 inputDirection;
        public Vector2 cameraRotation;
        public Vector2 cameraMouseRotation;

        public ControlInputData(Vector3 movementDirection, Vector3 inputDirection, Vector2 cameraRotation, Vector2 cameraMouseRotation)
        {
            this.movementDirection = movementDirection;
            this.inputDirection = inputDirection;
            this.cameraRotation = cameraRotation;
            this.cameraMouseRotation = cameraMouseRotation;
        }
    }

    public enum ACTION_TYPE
    {
        INTERACT,
        CLOSE,
        RESTART,
        ROLL,
    }

    public void Initialize(PJ pj, PlayerInput playerInput)
    {
        this.pj = pj;
        this.playerInput = playerInput;
        
        if (this.playerInput)
        {
            moveAction = playerInput.actions["Move"];
            rollAction = playerInput.actions["Roll"];
            interactAction = playerInput.actions["Action"];
            cameraRotationAction = playerInput.actions["CameraRotation"];
            cameraRotationMouseAction = playerInput.actions["CameraRotationMouse"];
            closeAction = playerInput.actions["Close"];
            restartAction = playerInput.actions["Restart"];
        }
    }
    
    public ControlInputData GetControlInputDataValues()
    {
        inputDirection = moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3();
        
        if (inputDirection.x > 0)
        {
            direction += pj.transform.right;
        } else if (inputDirection.x < 0)
        {
            direction += -pj.transform.right;
        }
            
        if (inputDirection.y > 0)
        {
            direction += pj.transform.forward;
        } else if (inputDirection.y < 0)
        {
            direction += -pj.transform.forward;
        }

        return new ControlInputData(direction, inputDirection, cameraRotationAction.ReadValue<Vector2>(), cameraRotationMouseAction.ReadValue<Vector2>());
    }

    public bool ActionTriggered(ACTION_TYPE action)
    {
        bool actionTriggered = false;

        if (playerInput)
        {
            switch (action)
            {
                case ACTION_TYPE.INTERACT:
                    actionTriggered = interactAction.triggered;
                    break;
                case ACTION_TYPE.ROLL:
                    actionTriggered = rollAction.triggered;
                    break;
                case ACTION_TYPE.RESTART:
                    actionTriggered = restartAction.triggered;
                    break;
                case ACTION_TYPE.CLOSE:
                    actionTriggered = closeAction.triggered;
                    break;
                default:
                    break;
            }
        }

        return actionTriggered;
    }
}