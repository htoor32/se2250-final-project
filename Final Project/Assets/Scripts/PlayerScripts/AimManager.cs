using Cinemachine;
using UnityEngine;

/* This class is responsible for handling aim input switching between the aim camera and 
 * free look camera
 */
public class AimManager : MonoBehaviour {
    // Instance of Input class
    private Input _input;

    #region Aim Fields
    [Header("Aim Settings")]
    [SerializeField] Transform _cameraFollowTransform;

    [Tooltip("How far down the camera is allowed to look")]
    [SerializeField, Range(-90, 0f)] private float _bottomLookClamp = -30f;

    [Tooltip("How far up the camera is allowed to look")]
    [SerializeField, Range(0f, 90)] private float _topLookClamp = 70f;

    [Tooltip("Sensitivity in the x-axis")]
    [SerializeField, Range(50f, 500f)] private float _xSensitivity = 300f;

    [Tooltip("Sensitivity in the y-axis")]
    [SerializeField, Range(50f, 500f)] private float _ySensitivity = 300f;

    [SerializeField] private Transform _weaponHolderTransform;

    private Vector2 _mousePosition;
    private float _targetYaw;
    private float _targetPitch;
    #endregion

    [Space(10f)]

    #region ADS Fields
    [Header("ADS Settings")]
    [Tooltip("Reference to the camera active when aiming")]
    [SerializeField] private CinemachineVirtualCamera _adsCamera;

    [Tooltip("How X & Y sensitivity is reduced when aiming, lower values allow for more precise tracking")]
    [SerializeField] private float _aimSlowdownFactor = 0.5f;


    private bool _isADS = false;
    #endregion

    #region Properties
    /* The movement manager requires the isADS boolean
     * so a read only property is created */
    public bool IsADS { get { return _isADS; } }
    #endregion


    private void Awake() {
        // Lock cursor to screen center and make invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start() {
        _input = new Input();
        _input.player.Enable();

        #region Find Objects
        if (_cameraFollowTransform == null) {
            FindCameraFollowTarget();
        }

        if (_adsCamera == null) {
            FindADSCamera();
        }


        if (_weaponHolderTransform == null) {
            FindWeaponHolder();
        }

        #endregion


        #region Subscribe to Events
        _input.player.ads.performed += Ads_performed;
        _input.player.ads.canceled += Ads_canceled;
        #endregion
    }

    private void Update() {
        #region Aiming
        // Read mouse delta
        _mousePosition = _input.player.aim.ReadValue<Vector2>();

        // Set target pitch and target yaw for camera
        _targetYaw += _mousePosition.x * _xSensitivity * Time.deltaTime * (_isADS ? _aimSlowdownFactor : 1f); // If ADS, slow down aim movement
        _targetPitch += _mousePosition.y * _ySensitivity * Time.deltaTime * (_isADS ? _aimSlowdownFactor : 1f);

        // Clamp pitch and yaw
        _targetYaw = Mathf.Clamp(_targetYaw, float.MinValue, float.MaxValue);
        _targetPitch = Mathf.Clamp(_targetPitch, _bottomLookClamp, _topLookClamp);
        #endregion

        #region ADSing
        // Transition to aim camera if player is ADSing
        _adsCamera.gameObject.SetActive(_isADS);
        #endregion
    }

    // Updates camera position, this is done in late update so all camera movement happens
    // AFTER player movement has finished updating; Ensures smooth camera movement.
    private void LateUpdate() {
        // Update camera follow target rotation
        _cameraFollowTransform.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
        _cameraFollowTransform.Rotate(Vector3.up * _mousePosition.x * Time.deltaTime);

        // Update x-rotation of weapon holder
        Vector3 weaponHolderEulerAngles = _weaponHolderTransform.rotation.eulerAngles;
        weaponHolderEulerAngles.x = _cameraFollowTransform.rotation.eulerAngles.x;
        _weaponHolderTransform.rotation = Quaternion.Euler(weaponHolderEulerAngles);
    }

    #region ADS Events
    private void Ads_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        _isADS = true;
    }

    private void Ads_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        _isADS = false;
    }
    #endregion

    #region Object Find Methods
    private void FindWeaponHolder() { 
        foreach (Transform transform in this.gameObject.transform) {
            if (transform.CompareTag("WeaponHolder")) {
                _weaponHolderTransform = transform;
                return;
            }
        }
    }

    private void FindADSCamera() {
        foreach (CinemachineVirtualCamera VirtualCamera in FindObjectsOfType<CinemachineVirtualCamera>(true)) {
            if (VirtualCamera.CompareTag("AimCamera")) {
                _adsCamera = VirtualCamera;
                _adsCamera.gameObject.SetActive(false);
                return;
            }
        }
    }

    private void FindCameraFollowTarget() {
        _cameraFollowTransform = GameObject.FindWithTag("CameraFollow").transform;
    }
    #endregion
}
