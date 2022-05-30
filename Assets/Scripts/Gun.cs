using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;              // дуло оружия (firepoint TD)
    public Projectile projectile;         // паблик поле для снарядов
    public float msBetweenShots = 100;    // скорострельность
    public float muzzleVelocity = 35;     // начальная скорость пули

    float nextShotTime;

    public void Shoot()
    {
        if(Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShots / 1000; // стрелять можно, если позволяет скорострельность
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;   // создание снаряда при стрельбе
            newProjectile.SetSpeed(muzzleVelocity);         // применение начальной скорости
        }
    }
}
