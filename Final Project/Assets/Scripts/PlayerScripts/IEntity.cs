using UnityEngine;
/* This interface describes all entities present in the game
 */
public interface IEntity {

    public void TakeDamage(float damage) { }

    private void OnTriggerEnter(Collider other) { }

    private void Death() { }
}