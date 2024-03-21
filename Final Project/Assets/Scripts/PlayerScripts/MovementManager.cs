using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovementManager : MonoBehaviour
{
    // Create instance of generated C# script for input
    private Input _input;
    private AimManager _aimManager;
    private FirearmManager _firearmManager;
    private GameObject _weaponHolder;

    #region WASD Movement Fields
    [Header("WASD Movement Settings")]

    [Tooltip("Controls how fast the character will move")]
    [SerializeField, Range(1f, 10f)] private float _movementSpeed = 5f;

    [Tooltip("When moving, this value affects how fast the player will turn to face the movement direction, lower it for snappier movement")]
    [SerializeField, Range(0.1f, 1f)] private float _turnSmoothing = 0.1f;

    [Tooltip("Reference to main camera")]
    [SerializeField] private Transform _cameraTransform;

    [Tooltip("Reference to main camera follow target")]
    [SerializeField] private Transform _cameraFollowTransform;

    
    #region Private Fields for Movement
    private CharacterController _controller;
    private bool _isMoving;
    private Vector2 _inputRaw;
    private Vector3 _moveDirection, _moveDirectionSmoothed;
    private float _targetAngle, _smoothedAngle, _turnRef, _movementSpeedInternal;
    #endregion

    #endregion

    [Space(10f)]

    #region Sprint & Stamina Fields
    [Header("Sprint Settings")]
    [Tooltip("Movement speed is multiplied by this float when sprinting, increase for faster sprinting")]
    [SerializeField, Range(1f, 10f)] private float _sprintMultiplier = 2f;
    [Space(5f)]
    [Header("Stamina Settings")]
    [SerializeField, Range(50f, 150f)] private float _staminaMax = 100f;
    [SerializeField, Range(5f, 15f)] private float _staminaDrain = 10f;
    [SerializeField, Range(5f, 15f)] private float _staminaRegen = 5f;
    [SerializeField] private Image _staminaBar;

    private bool _isSprinting;
    private float _staminaCurrent = 100f;
    #endregion

    [Space(10f)]

    #region Jump & Gravity Fields
    [Header("Jump & Gravity Settings")]
    [SerializeField] private float _jumpHeight = 3f;
    [SerializeField] private float _gravityAcceleration = -25f;
    [SerializeField] private Transform _groundCheckTransform;

    [Tooltip("How big of a sphere the ground check should project when updating the isGrounded field")]
    [SerializeField, Range(0.01f, 0.1f)] private float _groundCheckRadius = 0.01f;
    [Tooltip("Layer(s) the ground check should check for when determining if on ground or not")]
    [SerializeField] private LayerMask _groundCheckLayerMask;

    private Vector3 _velocity;
    private bool _isJumping;
    private bool _isGrounded;
    #endregion

    public float StaminaMax {  
        get { return _staminaMax; }
        set { _staminaMax = value; } 
    }

    
    private void Awake() {
        _controller = GetComponent<CharacterController>();
        _aimManager = GetComponent<AimManager>();

        #region Find Objects
        if (_cameraTransform == null) {
            _cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        if (_cameraFollowTransform == null) {
            _cameraFollowTransform = GameObject.FindGameObjectWithTag("CameraFollow").transform;
        }

        if (_groundCheckTransform == null) {
            _groundCheckTransform = GameObject.FindGameObjectWithTag("GroundCheck").transform;
        }

        if (_groundCheckLayerMask == 0) {
            _groundCheckLayerMask = LayerMask.GetMask("Ground", "Terrain");
        }

        _weaponHolder = FindChildWithTag(this.gameObject, "WeaponHolder");
        #endregion
    }

    private void Start() {
        _firearmManager = _weaponHolder.GetComponentInChildren<FirearmManager>();
        _input = new Input();
        _input.player.Enable(); // Enable the player action map for input

        #region Method Subscribtions
        // Subscribe to the necessary performed and cancelled methods
        _input.player.movement.performed += Movement_performed;
        _input.player.movement.canceled += Movement_canceled;

        _input.player.sprint.performed += Sprint_performed;
        _input.player.sprint.canceled += Sprint_canceled;
        #endregion

        
    }

    private void Update() {
        #region Movement & Smoothing
        _inputRaw = _input.player.movement.ReadValue<Vector2>();
        _moveDirection = _moveDirectionSmoothed = new Vector3(_inputRaw.x, 0f, _inputRaw.y).normalized;

        if (_moveDirection.magnitude >= 0.1f && !_aimManager.IsADS && !_firearmManager.IsShooting) {
            _targetAngle = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y ;
            _smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref _turnRef, _turnSmoothing);

            transform.rotation = Quaternion.Euler(0f, _smoothedAngle, 0f);
            

            _moveDirectionSmoothed = Quaternion.Euler(0f, _targetAngle, 0f) * Vector3.forward;
        } else if (_aimManager.IsADS || _firearmManager.IsShooting) {
            _targetAngle = _cameraFollowTransform.eulerAngles.y;
            _smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref _turnRef, _turnSmoothing);

            transform.rotation = Quaternion.Euler(0f, _smoothedAngle, 0f);



            _moveDirectionSmoothed = Quaternion.Euler(0f, _targetAngle, 0f) * _moveDirection;
        }

        // Move character according to smoothing and input
        _controller.Move(_moveDirectionSmoothed * _movementSpeedInternal * Time.deltaTime);
        #endregion

        
    }

    private void LateUpdate() {
        // Ground Check 
        _isGrounded = Physics.CheckSphere(_groundCheckTransform.position, _groundCheckRadius, _groundCheckLayerMask);

        #region Update Stats
        if (_isGrounded && _velocity.y < 0f) {
            _velocity.y = -2f;
        }

        if (_isGrounded && _isJumping) {
            _isJumping = false;
        }

        if ((!_isSprinting || !_isMoving) && _staminaCurrent < _staminaMax) {
            _staminaCurrent += _staminaRegen * Time.deltaTime;
            _staminaCurrent = Mathf.Clamp(_staminaCurrent, 0f, _staminaMax);

            _staminaBar.fillAmount = _staminaCurrent / _staminaMax;
        }

        if (_isSprinting && _isMoving) {
            _staminaCurrent -= _staminaDrain * Time.deltaTime;
            _staminaCurrent = Mathf.Clamp(_staminaCurrent, 0f, _staminaMax);

            _staminaBar.fillAmount = _staminaCurrent / _staminaMax;
        }

        if (_staminaCurrent == 0f) {
            _movementSpeedInternal = _movementSpeed;
            _isSprinting = false;
        }
        _movementSpeedInternal = Mathf.Clamp(_movementSpeedInternal, _movementSpeed, _movementSpeed * _sprintMultiplier);
        #endregion

        #region Jump & Gravity
        if (_input.player.jump.ReadValue<float>() > 0f && _isGrounded) {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravityAcceleration);
        }

        _velocity.y += _gravityAcceleration * Time.deltaTime;

        // Apply Gravity and Jump
        _controller.Move(_velocity * Time.deltaTime);
        #endregion
    }

    #region Sprint Events
    private void Sprint_canceled(InputAction.CallbackContext obj) {
        if (_staminaCurrent > 0) {
            _movementSpeedInternal /= _sprintMultiplier;
            _isSprinting = false;
            _movementSpeedInternal = Mathf.Clamp(_movementSpeedInternal, _movementSpeed, _movementSpeed * _sprintMultiplier);
        }
    }

    private void Sprint_performed(InputAction.CallbackContext obj) {
        if (_staminaCurrent > 0) {
            _movementSpeedInternal *= _sprintMultiplier;
            _isSprinting = true;
            _movementSpeedInternal = Mathf.Clamp(_movementSpeedInternal, _movementSpeed, _movementSpeed * _sprintMultiplier);
        }
    }
    #endregion


    #region WASD Movement Events
    private void Movement_canceled(InputAction.CallbackContext obj) {
        _isMoving = false;
    }

    private void Movement_performed(InputAction.CallbackContext obj) {
        _isMoving = true;
    }
    #endregion

    private GameObject FindChildWithTag(GameObject parent, string tag) {
        GameObject child = null;

        foreach (Transform transform in parent.transform) {
            if (transform.CompareTag(tag)) {
                child = transform.gameObject;
                break;
            }
        }

        return child;
    }
}
