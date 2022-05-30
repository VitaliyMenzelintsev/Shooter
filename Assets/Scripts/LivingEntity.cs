using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// классу наследуют Enemy и Player. Он описывает базовый функционал сущностей
public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;     // событие, на которое подпиcан SpawnManager

    protected virtual void Start()
    {
        health = startingHealth;
    }
    public void TakeHit(float damage, RaycastHit hit)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    protected void Die()
    {
        dead = true;
        if(OnDeath != null)    
        {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
