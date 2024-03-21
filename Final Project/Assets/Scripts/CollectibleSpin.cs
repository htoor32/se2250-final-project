using UnityEngine;

/*
 * This class is applied to call collectable objects. It applies
 * a spin and an up and down movement to them to make them appear
 * more "collectable".
 */

public class CollectibleSpin : MonoBehaviour {
    [SerializeField] private float _rotateSpeed = 200, _frequency = 5f, _amplitude = 0.1f, _yOffset = 0.2f;
    private float _yStartCoord;

    private void Start () {
        /*  To ensure the object moves up and down relative to the position it was placed,
         *  the starting y-coordinate is taken
         */
        _yStartCoord = transform.position.y;
    }
    
    /* A note on this sin function:
     *      Sin functions follow the form: a * Sin( f * t ) + y
     * The y here needs to be the y-coordinate where the object was placed, plus twice
     * the amplitude, otherwise the object will clip into the ground if placed close 
     * to it. However, this variable is left visible in the inspector for greater
     * customizability.
     */

    private void FixedUpdate() {
        // Apply rotation around the z-axis as the objects are rotated -90 degrees around the x-axis
        transform.Rotate(Vector3.forward * _rotateSpeed *  Time.deltaTime);

        // Apply up-and-down movement
        transform.position = new Vector3(transform.position.x, _amplitude * Mathf.Sin(_frequency * (Time.time % 10)) + (_yStartCoord + _yOffset), transform.position.z);
    }
}
