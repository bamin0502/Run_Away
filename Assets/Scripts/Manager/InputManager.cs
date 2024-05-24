using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    #region Events
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;
    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;
    
    public delegate void BackButtonPressed();
    public event BackButtonPressed OnBackButtonPressed;
    #endregion
    
    private PlayerControls playerControls;
    private Camera mainCamera;
    private UiManager uiManager;
    private new void Awake()
    {
        playerControls = new PlayerControls();
        mainCamera = Camera.main;
        uiManager = GameObject.FindGameObjectWithTag("UiManager").gameObject.GetComponent<UiManager>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Start()
    {
        playerControls.Touch.PrimaryContact.started += StartTouchPrimary;
        playerControls.Touch.PrimaryContact.canceled += EndTouchPrimary;
        
        playerControls.UI.Back.started += OnBackAction;
    }
    
    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        OnEndTouch?.Invoke(PrimaryPosition(),(float)context.time);
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        OnStartTouch?.Invoke(PrimaryPosition(),(float)context.startTime);
    }

    private Vector2 PrimaryPosition()
    {
        return Utils.ScreenToWorld(mainCamera,playerControls.Touch.PrimaryPosition.ReadValue<Vector2>());
    }
    
    private void OnBackAction(InputAction.CallbackContext context)
    {
        OnBackButtonPressed?.Invoke();
        Debug.Log("Back button pressed");
        uiManager.ShowPausePanel();
    }

}
