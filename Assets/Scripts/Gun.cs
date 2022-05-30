using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single }   // ������ ����
    public FireMode fireMode;

    public Transform muzzle;              // ���� ������ (firepoint TD)
    public Projectile projectile;         // ������ ���� ��� ��������
    public float msBetweenShots = 100;    // ����������������
    public float muzzleVelocity = 35;     // ��������� �������� ����
    public int burstCount;                // ���������� ��������� � �������

    float nextShotTime;

    bool triggerReleasedSinceLastShot;   // ������� ���������� � ������� ���������� ��������
    int shotsRemainingInBurst;           // ��������� �������� � �������

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

            nextShotTime = Time.time + msBetweenShots / 1000; // �������� �����, ���� ��������� ����������������
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;   // �������� ������� ��� ��������
            newProjectile.SetSpeed(muzzleVelocity);         // ���������� ��������� ��������
        }
    }

    public void OnTriggerHold()       // ��������� ������� ����
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()     // ���������� ������� ����
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
