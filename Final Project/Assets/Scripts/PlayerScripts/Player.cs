using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class describes the player and any actions the player can do 
 * outside of weaponry, aim, or movement.
 */

public class Player : MonoBehaviour, IEntity {

    [Tooltip("How many hit points the player has")]
    [SerializeField] private float _maxHealth = 100f;

    [Tooltip("A reference to the primary weapon of the character")]
    [SerializeField] private GameObject _primaryWeapon;
    [Tooltip("A reference to the secondary weapon of the character")]
    [SerializeField] private GameObject _secondaryWeapon;

    [Tooltip("A reference to the weapon holder within the player")]
    [SerializeField] private GameObject _weaponHolder;

    [SerializeField] private Image _healthBar;

    /* Each weapon game object contains a child object, an empty object, which contains all
     * equippable sights available to that weapon. This will always be an array containing
     * the set of sights available for that weapon. NOTE: these sights MUST be placed under
     * the Sights game object under the primary weapon, simply placing it in this array from
     * the inspector will cause a null reference exception.
     */
    [Tooltip("Array of all possible equippable sights")]
    [SerializeField] private List<GameObject> _primarySights;

    private MovementManager _movementManager;
    private int _healthPacks; // Number of health packs the player currently holds
    private int _upgradesReceived = 0; // Number of weapon upgrades the player has picked up
    private Input _input;
    private float _currentHealth;

    private void Awake() {
        // Find the movement manager script on this player
        _movementManager = GetComponent<MovementManager>();

        // Find the weapon holder
        if (_weaponHolder == null) {
            _weaponHolder = FindChildWithTag("WeaponHolder");
        }

        // Find the primary weapon under the weapon holder
        if (_primaryWeapon == null) {
            _primaryWeapon = FindChildWithTag(_weaponHolder, "PrimaryWeapon");
        }

        // Find the secondary weapon under the weapon holder
        if (_secondaryWeapon == null) {
            _secondaryWeapon = FindChildWithTag(_weaponHolder, "SecondaryWeapon");
        }

        // Fill the _primarySights array with any equippable sights
        if (true) {
            _primarySights = FindSights();
        }
    }

    private void Start() {
        _healthPacks = 0;
        _upgradesReceived = 0;
        _currentHealth = _maxHealth;

        // Instantiate input & enable
        _input = new Input();
        _input.player.Enable();

        // Subscribe to event
        _input.player.useItem.performed += UseItem_performed;
    }

    private void UseItem_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        _healthPacks--;
        _currentHealth += 25;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        _healthBar.fillAmount = _currentHealth / _maxHealth;
    }


    /* Getting called every time the player enters a collider tagged as a trigger, this method
     * checks the tag of the collided trigger, and does the following depending on the condition:
     * If the tag represents a health pickup: adds to the health pack inventory
     * If the tag represents a stamina pickup: increases stamina max variable from the movement script
     * If the tag represents an enemy projectile: decrements health depending on projectile and 
     *      and checks health to see if player is still alive, if not, call game over from the game 
     *      manager class
     * If the tag represents a weapon upgrade crate: Check the name of this player, and calls the 
     *     appropriate upgrade method
     */
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("HealthPickup")) {
            _healthPacks++;
            Destroy(other.gameObject);

        } else if (other.gameObject.CompareTag("StaminaPickup")) {
            _movementManager.StaminaMax += 25f;
            Destroy(other.gameObject);

        } else if (other.gameObject.CompareTag("EnemyProjectile")) {
            _currentHealth -= other.gameObject.GetComponent<ProjectileController>().ProjectileDamage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);


            _healthBar.fillAmount = _currentHealth / _maxHealth;
            if (_currentHealth <= 0) {
                Death();
            }
            Destroy(other.gameObject);

        } else if (other.gameObject.CompareTag("UpgradePickup")) {
            switch (this.gameObject.name) {
                case "Infantry":
                    UpgradeInfantry();
                    break;
                case "Recon":
                    UpgradeRecon();
                    break;
                case "Heavy":
                    UpgradeHeavy();
                    break;
            }
            Destroy(other.gameObject);
        } else if (other.gameObject.CompareTag("AmmoPickup")) {
            _primaryWeapon.GetComponent<FirearmManager>().RestockAmmo();
            _secondaryWeapon.GetComponent<FirearmManager>().RestockAmmo();
            Destroy(other.gameObject);
        }
    }

    private void Death() {
        // --- --- TO BE IMPLEMENTED: MUST CREATE GAMEMANAGER FIRST WITH GAMEOVER METHOD --- ---
    }

    private void UpgradeInfantry() {
        switch (_upgradesReceived) {
            case 0: // M4 sight upgrade
                _upgradesReceived++;

                // Change sight
                _primarySights[0].SetActive(false);
                _primarySights[1].SetActive(true);

                // Increase range stat
                _primaryWeapon.GetComponent<FirearmManager>().WeaponRange += 100f;
                break;
            case 1: // Uzi damage upgrade
                _upgradesReceived++;

                // Upgrade damage stat
                _secondaryWeapon.GetComponent<FirearmManager>().WeaponDamage += 4f;
                break;
            case 2: // M4 magazine upgrade
                _upgradesReceived++;

                _primaryWeapon.GetComponent<FirearmManager>().MagazineSize += 30;
                break;
            case 3: // M4 damage upgrade
                _upgradesReceived++;

                // Upgrade damage stat
                _secondaryWeapon.GetComponent<FirearmManager>().WeaponDamage += 5f;
                break;
            default:
                Debug.Log("All Upgrades Collected");
                _upgradesReceived++;
                // --- --- TO BE IMPLEMENTED: Collected all upgrades    --- ---
                break;
        }
    }

    private void UpgradeRecon() {
        switch (_upgradesReceived) {
            case 0:
                // --- --- TO BE IMPLEMENTED: M107 sight upgrade 1      --- ---
                break;
            case 1:
                // --- --- TO BE IMPLEMENTED: M1911 damage upgrade      --- ---
                break;
            case 2:
                // --- --- TO BE IMPLEMENTED: M107 sight upgrade 2      --- ---
                break;
            case 3:
                // --- --- TO BE IMPLEMENTED: M107 damage upgrade       --- ---
                break;
            default:
                // --- --- TO BE IMPLEMENTED: Collected all upgrades    --- ---
                break;
        }
    }

    private void UpgradeHeavy() {
        switch (_upgradesReceived) {
            case 0:
                // --- --- TO BE IMPLEMENTED: M249 sight upgrade        --- ---
                break;
            case 1:
                // --- --- TO BE IMPLEMENTED: M4Shot damage upgrade     --- ---
                break;
            case 2:
                // --- --- TO BE IMPLEMENTED: M249 magazine upgrade     --- ---
                break;
            case 3:
                // --- --- TO BE IMPLEMENTED: M107 damage upgrade       --- ---
                break;
            default:
                // --- --- TO BE IMPLEMENTED: Collected all upgrades    --- ---
                break;
        }
    }

    // Finds a child of the player with a tag matching the one passed
    private GameObject FindChildWithTag (string tag) {
        foreach (Transform transform in this.gameObject.transform) {
            if (transform.CompareTag(tag)) {
                return transform.gameObject;
            }
        }
        return null;
    }

    // Finds a child of the passed game object with a tag matching the one passed
    private GameObject FindChildWithTag (GameObject go, string tag) {
        foreach (Transform transform in go.transform) {
            if (transform.CompareTag(tag)) {
                return transform.gameObject;
            }
        }
        return null;
    }

    // Finds and fills a list of all possible sights
    private List<GameObject> FindSights () {
        var list = new List<GameObject>();
        foreach (Transform weaponComponent in _primaryWeapon.transform) {
            if (weaponComponent.CompareTag("WeaponSights")) {
                foreach (Transform sight in weaponComponent.transform) {
                    list.Add(sight.gameObject);
                }
            }
        }
        return list;
    }
}