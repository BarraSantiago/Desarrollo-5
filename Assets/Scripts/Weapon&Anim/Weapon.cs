using EnemyUnits;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float damage = 0;
    private float initialDamage;

    private HashSet<EnemyBehaviour> hitEnemies;

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

        hitEnemies = new HashSet<EnemyBehaviour>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponent<EnemyBehaviour>();
        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            Debug.Log("Dmg");
            Debug.Log(damage);
            enemy.RecibeDamageFromPlayer(damage);
            AudioManager.instance.Play("PlayerAttack");
            hitEnemies.Add(enemy);
        }
    }

    public void EnableTriggerBox()
    {
        triggerBox.enabled = true;
        hitEnemies.Clear();
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

