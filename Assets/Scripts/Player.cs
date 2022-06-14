using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5;

    public Transform crosshairs;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;


    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;                                     // инициализация камеры как основной
        FindObjectOfType<SpawnManager>().OnNewWave += OnNewWave;
    }

    protected override void Start()                                  
    {
        base.Start();                                                
    }

    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    void Update()
    {
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); // GetAxisRaw не делает инерционное сглаживание
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);     // луч, от камеры к позиции мыши
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;                                              // длина луча

        if (groundPlane.Raycast(ray, out rayDistance))                   // сохранение длины луча в переменную               
        {
            Vector3 point = ray.GetPoint(rayDistance);                  // отслеживание точки пересечения луча и поверхности
            controller.LookAt(point);
            crosshairs.position = point;
            if((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gunController.Aim(point);
            }
        }

        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            gunController.OnTriggerRelease();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
    }
}
