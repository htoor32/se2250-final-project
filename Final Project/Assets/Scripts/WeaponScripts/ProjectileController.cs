using UnityEngine;


/* This class is responsible for controlling the behavior of a projectile
 */
public class ProjectileController : MonoBehaviour {
    [SerializeField] private float _projectileVelocity = 100f;
    [SerializeField] private float _projectileDamage = 10f;
    [SerializeField] private GameObject _parentWeapon; // Weapon this projectile was shot from

    public float ProjectileDamage {
        get { return _projectileDamage; }
        set { _projectileDamage = value; }
    }

    private Rigidbody projectileRigidBody;

    private void Awake() {
        projectileRigidBody = GetComponent<Rigidbody>();
        _parentWeapon = GetCurrentlyEquippedWeapon();
        _projectileDamage = _parentWeapon.GetComponent<FirearmManager>().WeaponDamage; // set projectile damage according to weapon damage
    }

    private void Start() {
        // Set projectile velocity and call destroy to make sure the object destroys itself after a certain time if it hits nothing
        projectileRigidBody.velocity = transform.forward * _projectileVelocity;
        Destroy(gameObject, 10);
    }
    private void OnTriggerEnter(Collider other) {
        Destroy(gameObject);
    }

    // Finds and return currently equipped weapon
    private GameObject GetCurrentlyEquippedWeapon() {
        GameObject[] weapon = GameObject.FindGameObjectsWithTag("PrimaryWeapon");
        if (weapon.Length <= 0 ) {
            weapon = GameObject.FindGameObjectsWithTag("SecondaryWeapon");
        }

        return weapon[0];
    }
}