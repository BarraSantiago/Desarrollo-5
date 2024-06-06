using EnemyUnits;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;

    BoxCollider triggerBox;

    private void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
        triggerBox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponent<EnemyBehaviour>();
        if (enemy != null)
        {
            Debug.Log("Dmg");
            enemy.TakeDamage(damage);
        }
    }

    public void EnableTriggerBox()
    {
        triggerBox.enabled = true;
    }

    public void DisableTriggerBox()
    {
        triggerBox.enabled = false;
    }
}

