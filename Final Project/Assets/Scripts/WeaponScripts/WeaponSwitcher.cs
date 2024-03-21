using UnityEngine;

/* This class is responsible for reading when the switch weapon button is pressed
 * and switching to the next weapon in the weapon holder. This script must be
 * applied to the weapon holder game object, present in each character
 */
public class WeaponSwitcher : MonoBehaviour
{
    /* This integer represents the index of the weapon selected, as all
     * weapons are children to the weapon holder, they are represented 
     * as an array and indexed via _selectedWeapon
     */
    private int _selectedWeapon = 0;
    private Input _input;

    void Start() {
        _input = new Input();
        _input.player.Enable();

        // Subscribe to the switchWeapon.performed event
        _input.player.switchWeapon.performed += SwitchWeapon_performed;

        // Ensure the correct weapon is equipped on start up
        SwitchWeapon();
    }

    // Increment selected weapon or wrap around to zero, then switch weapon
    private void SwitchWeapon_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {

        if (_selectedWeapon >= transform.childCount - 1) {
            _selectedWeapon = 0;
        } else {
            _selectedWeapon++;
        }

        SwitchWeapon();
    }

    // Switch to selected weapon
    private void SwitchWeapon() {
        int i = 0;
        foreach (Transform weapon in transform) {
            if (i == _selectedWeapon) {
                weapon.gameObject.SetActive(true);
            } else {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }
}
