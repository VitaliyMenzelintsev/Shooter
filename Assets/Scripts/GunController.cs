using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;             // точка креплени€ оружи€
    public Gun startingGun;                  // стартовое оружие
    Gun equippedGun;                         // снар€жЄнное оружие


    public void Start()
    {
        if(startingGun != null)              // если в начале нет оружи€
        {
            EquipGun(startingGun);           // запустить метод экипировани€ с аргументом стартового оружи€
        }
    }

    public void EquipGun(Gun gunToEquip)     // экипировать оружие
    {
        if (equippedGun != null) Destroy(equippedGun.gameObject);         // уничтожение экипированного оружи€ при экипировке нового

        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;          // создание нового экземпл€ра оружи€
        equippedGun.transform.parent = weaponHold;                       // снар€жЄнное оружие прив€зать к точке креплени€
    }

    public void OnTriggerHold()
    {
        if(equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }
}
