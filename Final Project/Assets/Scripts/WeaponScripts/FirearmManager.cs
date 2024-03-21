using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* This class controls the firearm it is a component of. It reads when the 
 * shoot button is pressed, shoots a ray to determine where the landing point 
 * is and if the point is within range, it then instantiates a 
 * projectile object at the bullet spawn point of the weapon, pointing 
 * towards the hit point of the ray.
 */

public class FirearmManager : MonoBehaviour {
    #region Weapon Fields
    [Header("Weapon Settings")]

    [Tooltip("Weapon fire rate, lower this for a slower fire rate")]
    [SerializeField] private float _roundsPerSecond;

    [Tooltip("Set to true to allow weapon to fire full auto")]
    [SerializeField] private bool _isFullAuto;

    [Space]

    [Tooltip("How many hit points of damage the weapon does")]
    [SerializeField] private float _weaponDamage;

    [Tooltip("How many how many meters of range the weapon can fire")]
    [SerializeField] private float _weaponRange;

    [Space]

    [Tooltip("Magazine size of the weapon")]
    [SerializeField] private int _magazineCapacity;

    [Tooltip("Max Ammo able to be held by the player for this weapon")]
    [SerializeField] private int _maxAmmoCount;

    [Tooltip("How long the weapon takes to reload in seconds")]
    [SerializeField] private float _reloadTime;

    // Private fields
    private float _timeUntilNextShot;
    private bool _isShooting, _isReloading;
    private int _bulletsLeftInMag; // Current number of bullets in the magazine
    private int _bulletsLeft; // Current number of bullets NOT in the mag
    #endregion

    [Space(10f)]

    #region Projectile Fields
    [Header("Projectile Settings")]

    [Tooltip("Layer(s) which the shot projectile should collide with")]
    [SerializeField] private LayerMask _collideWith;

    [Tooltip("Transform of projectile spawn point")]
    [SerializeField] private Transform _projectileSpawnTransform;

    [Space]

    [Tooltip("List of all shootable projectiles")]
    [SerializeField] private GameObject[] _projectiles = new GameObject[7];

    [Tooltip("Projectile to be shot")]
    [SerializeField] private GameObject _projectile;


    //Private Fields
    private Vector3 _projectileHitPoint = Vector3.zero;
    #endregion

    #region UI Fields
    [SerializeField] private TextMeshProUGUI _ammoCount;
    [SerializeField] private TextMeshProUGUI _weaponDisplayName;
    [SerializeField] private Image _primaryIcon;
    [SerializeField] private Image _secondaryIcon;
    #endregion

    #region Properties 
    public bool IsShooting { get { return _isShooting; } }
    public float WeaponDamage { get { return _weaponDamage; }  set { _weaponDamage = value; } }
    public float WeaponRange { get { return _weaponRange; } set { _weaponRange = value; } }
    public int MagazineSize { get { return _magazineCapacity; } set { _magazineCapacity = value; } }

    #endregion


    Input _input; // Instance of input script

    /* The awake method is responsible for setting all fields which 
     * do not auto fill, or are dependant on the weapon, like fire rate
     */
    private void Awake() {
        #region Find and Initialize Empty Fields
        if (_collideWith == 0) {
            SetLayerMask();
        }

        if (_projectileSpawnTransform == null) {
            FindBulletSpawn();
        }

        if (_projectiles[0] == null) {
            SetProjectiles();
        }

        if ( _projectile == null) {
            SetProjectile();
        }

        if (_roundsPerSecond == 0) {
            SetRateOfFire();
        }

        if (_weaponDamage == 0) {
            SetWeaponDamage();
        }

        if (_weaponRange == 0) {
            SetWeaponRange();
        }

        if (_magazineCapacity == 0) {
            SetMagazineSize();
        }

        if (_reloadTime == 0) {
            SetReloadTime();
        }

        if (_maxAmmoCount == 0) {
            SetMaxAmmoCount();
        }

        SetFullAuto();
        #endregion
    }

    /* The start method ensures the input is set up correctly
     * and that all required events are subscribed to
     */
    public void Start() {
        _input = new Input();
        _input.player.Enable();

        _input.player.fire.performed += Fire_performed;
        _input.player.fire.canceled += Fire_canceled;

        _input.player.reload.performed += Reload_performed;

        _bulletsLeftInMag = _magazineCapacity;
        _bulletsLeft = _maxAmmoCount;

        UpdateAmmoText();
        UpdateWeaponDisplayName();
    }

    private void Update() {
        

        // Check if player is pushing the shoot key, if they are, and enough time has passed
        // since the last shot, fire another projectile
        if (_isShooting && Time.time >= _timeUntilNextShot) {
            // Find screen center
            Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);

            // Shoot a ray from the camera position to the screen center
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);

            // If ray collides with one of the predetermined layer masks
            if (Physics.Raycast(ray, out RaycastHit raycastHit, _weaponRange, _collideWith) && !_isReloading) {
                _projectileHitPoint = raycastHit.point;
                Shoot();
            }
            _timeUntilNextShot = Time.time + 1f / _roundsPerSecond;
        }
    }

    private void OnEnable() {
        _isReloading = false;

        if (this.gameObject.name == "M4" || this.gameObject.name == "AK47" || this.gameObject.name == "M249" || this.gameObject.name == "M107") {
            _primaryIcon.enabled = true; _secondaryIcon.enabled = false;
        } else {
            _primaryIcon.enabled = false; _secondaryIcon.enabled = true;
        }

        UpdateAmmoText();
        UpdateWeaponDisplayName();
    }

    private void Shoot() {
        if (_bulletsLeftInMag > 0) {
            Vector3 aimDirection = (_projectileHitPoint - _projectileSpawnTransform.position).normalized;
            Instantiate(_projectile, _projectileSpawnTransform.position, Quaternion.LookRotation(aimDirection, Vector3.forward));
            if (!_isFullAuto) {
                _isShooting = false;
            }
            _bulletsLeftInMag--;
            UpdateAmmoText();
        } else if (_bulletsLeftInMag <= 0 && this.gameObject.activeInHierarchy) {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload() {
        _ammoCount.text = "| Reloading...";
        _isReloading = true;

        yield return new WaitForSeconds(_reloadTime);
        int bulletsToAdd = _magazineCapacity - _bulletsLeftInMag;

        if (_bulletsLeft - bulletsToAdd >= 0) {
            _bulletsLeftInMag = _magazineCapacity;
            _bulletsLeft -= bulletsToAdd;
        }

        _isReloading = false;
        UpdateAmmoText();
    }

    // Finds and returns a child object with a certain tag


    /// <summary>
    /// Finds and returns a child object with the provided tag
    /// </summary>
    /// <param name="parent">Parent object of children to be searched</param>
    /// <param name="tag">Tag to be searched for</param>
    /// <returns>Child object with with passed tags</returns>
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

    private void UpdateAmmoText() {
        _ammoCount.text = ("| " + _bulletsLeftInMag + " / " + _bulletsLeft);
    }

    private void UpdateWeaponDisplayName() {
        _weaponDisplayName.text = gameObject.name;
    }

    #region Setting Local Fields
    // Find the layer mask(s) the raycast shot to calculate bullet landing point should collide with
    private void SetLayerMask() {
        _collideWith = LayerMask.GetMask("Terrain", "Ground", "Enemy");
    }

    // Fill the projectiles[] array
    private void SetProjectiles() {
        for (int i = 0; i < 5; i++) {
            _projectiles[i] = (GameObject)Resources.Load(("Prefabs/Projectiles/Bullet0" + (i + 1)), typeof(GameObject));
        }
    }

    // Find the spawn point from which bullets will be instantiated
    private void FindBulletSpawn() {
        _projectileSpawnTransform = FindChildWithTag(this.gameObject, "BulletSpawn").transform;
    }

    // Select the correct projectile from the projectile array, according to the weapon
    private void SetProjectile() {
        switch (this.gameObject.name) {
            case "M4":
                _projectile = _projectiles[0];
                break;
            case "AK47":
                _projectile = _projectiles[1];
                break;
            case "Uzi":
                _projectile = _projectiles[2];
                break;
            case "M249":
                _projectile = _projectiles[0];
                break;
            case "M1911":
                _projectile = _projectiles[3];
                break;
            case "M107":
                _projectile = _projectiles[4];
                break;
            case "M4Shot":
                _projectile = _projectiles[4];
                break;

        }
    }

    // Set the correct fire rate, according to the weapon
    private void SetRateOfFire() {
        switch (this.gameObject.name) {
            case "M4":
                _roundsPerSecond = 5f;
                break;
            case "AK47":
                _roundsPerSecond = 5f;
                break;
            case "Uzi":
                _roundsPerSecond = 7f;
                break;
            case "M249":
                _roundsPerSecond = 10f;
                break;
            case "M1911":
                _roundsPerSecond = 5f;
                break;
            case "M107":
                _roundsPerSecond = 1f;
                break;
            case "M4Shot":
                _roundsPerSecond = 2f;
                break;
        }
    }


    // Select the correct fire mode, according to the weapon
    private void SetFullAuto() {
        switch (this.gameObject.name) {
            case "M4":
                _isFullAuto = true;
                break;
            case "AK47":
                _isFullAuto = true;
                break;
            case "Uzi":
                _isFullAuto = true;
                break;
            case "M249":
                _isFullAuto = true;
                break;
            case "M1911":
                _isFullAuto = false;
                break;
            case "M107":
                _isFullAuto = false;
                break;
            case "M4Shot":
                _isFullAuto = false;
                break;
        }
    }

    // Set the correct damage, according to the weapon
    private void SetWeaponDamage() {
        switch (this.gameObject.name) {
            case "M4":
                _weaponDamage = 20f;
                break;
            case "AK47":
                _weaponDamage = 25f;
                break;
            case "Uzi":
                _weaponDamage = 10f;
                break;
            case "M249":
                _weaponDamage = 20f;
                break;
            case "M1911":
                _weaponDamage = 15f;
                break;
            case "M107":
                _weaponDamage = 100f;
                break;
            case "M4Shot":
                _weaponDamage = 80f;
                break;
        }
    }

    private void SetWeaponRange() {
        switch (this.gameObject.name) {
            case "M4":
                _weaponRange = 100f;
                break;
            case "AK47":
                _weaponRange = 90f;
                break;
            case "Uzi":
                _weaponRange = 50f;
                break;
            case "M249":
                _weaponRange = 100f;
                break;
            case "M1911":
                _weaponRange = 50f;
                break;
            case "M107":
                _weaponRange = 500f;
                break;
            case "M4Shot":
                _weaponRange = 50f;
                break;
        }
    }

    private void SetMagazineSize() {
        switch (this.gameObject.name) {
            case "M4":
                _magazineCapacity = 30;
                break;
            case "AK47":
                _magazineCapacity = 30;
                break;
            case "Uzi":
                _magazineCapacity = 25;
                break;
            case "M249":
                _magazineCapacity = 100;
                break;
            case "M1911":
                _magazineCapacity = 8;
                break;
            case "M107":
                _magazineCapacity = 10;
                break;
            case "M4Shot":
                _magazineCapacity = 6;
                break;
        }
    }

    private void SetMaxAmmoCount() {
        switch (this.gameObject.name) {
            case "M4":
                _maxAmmoCount = 240;
                break;
            case "AK47":
                _maxAmmoCount = 240;
                break;
            case "Uzi":
                _maxAmmoCount = 200;
                break;
            case "M249":
                _maxAmmoCount = 800;
                break;
            case "M1911":
                _maxAmmoCount = 64;
                break;
            case "M107":
                _maxAmmoCount = 80;
                break;
            case "M4Shot":
                _maxAmmoCount = 48;
                break;
        }
    }

    private void SetReloadTime() {
        switch (this.gameObject.name) {
            case "M4":
                _reloadTime = 3f;
                break;
            case "AK47":
                _reloadTime = 3f;
                break;
            case "Uzi":
                _reloadTime = 2.5f;
                break;
            case "M249":
                _reloadTime = 5f;
                break;
            case "M1911":
                _reloadTime = 1.5f;
                break;
            case "M107":
                _reloadTime = 5f;
                break;
            case "M4Shot":
                _reloadTime = 4f;
                break;
        }
    }
    #endregion


    #region Events
    private void Fire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {
        _isShooting = false;
    }

    private void Fire_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {
        _isShooting = true;
    }

    private void Reload_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (_bulletsLeftInMag < _magazineCapacity && this.gameObject.activeInHierarchy) {
            StartCoroutine(Reload());
        }
    }
    #endregion

    public void RestockAmmo() {
        _bulletsLeft = _maxAmmoCount;
        UpdateAmmoText();
    }
}
