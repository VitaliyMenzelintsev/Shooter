using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;

    protected override void Start()                                   // перезапись метода Start из LivingEntity.cs
    {
        base.Start();                                                 // выполнение функционала Start от LivingEntity.cs
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;                                     // инициализация камеры как основной
    }

    void Update()
    {
        // ввод движения
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); // GetAxisRaw не делает инерционное сглаживание
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // ввод взгляда ГГ
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);     // луч, идущий от камеры к позиции мыши
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;                                              // длина луча

        if(groundPlane.Raycast(ray, out rayDistance))                   // сохранение длины луча в переменную               
        {
            Vector3 point = ray.GetPoint(rayDistance);                  // отслеживание точки пересечения луча и поверхности
            //Debug.DrawLine(ray.origin, point, Color.red);             // отображение луча
            controller.LookAt(point);
        }

        // управдение оружием
        if (Input.GetMouseButton(0))
        {
            gunController.Shoot();
        }
    }
}
