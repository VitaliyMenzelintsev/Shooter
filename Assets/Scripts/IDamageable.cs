using UnityEngine;

public interface IDamageable 
{
    void TakeHit(float damage, RaycastHit hit);           // количество урона и направление луча

    void TakeDamage(float damage);
}
