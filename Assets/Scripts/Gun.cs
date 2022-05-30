using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single }   // режимы огня
    public FireMode fireMode;

    public Transform muzzle;              // дуло оружия (firepoint TD)
    public Projectile projectile;         // паблик поле для снарядов
    public float msBetweenShots = 100;    // скорострельность
    public float muzzleVelocity = 35;     // начальная скорость пули
    public int burstCount;                // количество выстрелов в очереди

    float nextShotTime;

    bool triggerReleasedSinceLastShot;   // триггер запущенный с моменат последнего выстрела
    int shotsRemainingInBurst;           // выстрелов осталось в очереди

    void Start()
    {
        shotsRemainingInBurst = burstCount;
    }

    void Shoot()
    {
        if(Time.time > nextShotTime)
        {
            if(fireMode == FireMode.Burst)
            {
                if(shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if(fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            nextShotTime = Time.time + msBetweenShots / 1000; // стрелять можно, если позволяет скорострельность
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;   // создание снаряда при стрельбе
            newProjectile.SetSpeed(muzzleVelocity);         // применение начальной скорости
        }
    }

    public void OnTriggerHold()       // удержание клавиши огня
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()     // отпускание клавиши огня
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
