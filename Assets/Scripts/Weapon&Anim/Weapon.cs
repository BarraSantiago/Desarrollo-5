using EnemyUnits;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float damage = 0;
    private float initialDamage;

    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    private BoxCollider triggerBox;

    private void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
        triggerBox.enabled = false;
        initialDamage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponent<EnemyBehaviour>();
        if (enemy != null)
        {
            Debug.Log("Dmg");
            Debug.Log(damage);
            enemy.RecibeDamageFromPlayer(damage);
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

    public void ResetDamage()
    {
        damage = initialDamage;
    }
}

