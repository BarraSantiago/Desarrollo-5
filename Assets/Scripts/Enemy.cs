using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] public int health = 20;

    private void Awake()
    {
        healthSlider.maxValue = health;

        healthSlider.minValue = 0;

        healthSlider.value = health;
    }

    private void Update()
    {
        healthSlider.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
    }

    public void ModifyHealth(int healthModifier)
    {
        health += healthModifier;

        healthSlider.value = health;

        if (health > 0) return;
        
        Death();
       
    }

    private void Death()
    {
        gameObject.SetActive(false);
        healthSlider.gameObject.SetActive(false);
        
    }
}
