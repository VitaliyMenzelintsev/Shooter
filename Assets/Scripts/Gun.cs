using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single }   // режимы огня
    public FireMode fireMode;

    public Transform[] projectileSpawnPoint;       // дуло оружия (firepoint TD)
    public Projectile projectile;         // паблик поле для снарядов
    public float msBetweenShots = 100;    // скорострельность
    public float muzzleVelocity = 35;     // начальная скорость пули
    public int burstCount;                // количество выстрелов в очереди
    public int projectilesPerMagazine;    // патронов в обойме всего
    bool isReloading;                     // перезарядка
    public float reloadTime = 0.3f;

    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(0.05f, 2f);       // отдача оружия визуальная минимальный-максимальный
    public Vector2 recoilAngleMinMax = new Vector2(3, 5);     // угол подброса
    public float timeOfReturnToPosition = 0.1f;
    Vector3 recoilSmoothDampVelocity;    // сила отдачи оружия назад
    private float recoilRotSmoothDampVelocity;   // сила подброса оружия вверх
    private float recoilAngle;                   // угол отдачи

    private float nextShotTime;

    private bool triggerReleasedSinceLastShot;   // триггер запущенный с моменат последнего выстрела
    private int shotsRemainingInBurst;           // выстрелов осталось в очереди
    private int projectilesRemainingInMagazine;  // патронов в обойме осталось
   
    void Start()
    {
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMagazine = projectilesPerMagazine;
    }

    void LateUpdate()          // запускается после всех других методов в рамках кадра
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, timeOfReturnToPosition); // отдача назад
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, timeOfReturnToPosition);           // подброс ствола
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;   
        
        if(!isReloading && projectilesRemainingInMagazine == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if(!isReloading && Time.time > nextShotTime && projectilesRemainingInMagazine > 0)
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
            for(int i = 0; i < projectileSpawnPoint.Length; i++)
            {
                if(projectilesRemainingInMagazine == 0)
                {
                    break;
                }
                projectilesRemainingInMagazine--;
                nextShotTime = Time.time + msBetweenShots; 
                Projectile newProjectile = Instantiate(projectile, projectileSpawnPoint[i].position, projectileSpawnPoint[i].rotation) as Projectile;  
                newProjectile.SetSpeed(muzzleVelocity);         
            }
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);   
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 25);
        }
    }

    public void Reload()
    {
        if(!isReloading && projectilesRemainingInMagazine != projectilesPerMagazine)
        {
            StartCoroutine(AnimateReload());
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(0.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectilesRemainingInMagazine = projectilesPerMagazine;
    }

    public void Aim(Vector3 aimPoint)   // отслеживание курсора
    {
        if (!isReloading)
        {
            transform.LookAt(aimPoint);
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
