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

    protected override void Start()                                   // ���������� ������ Start �� LivingEntity.cs
    {
        base.Start();                                                 // ���������� ����������� Start �� LivingEntity.cs
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;                                     // ������������� ������ ��� ��������
    }

    void Update()
    {
        // ���� ��������
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); // GetAxisRaw �� ������ ����������� �����������
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // ���� ������� ��
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);     // ���, ������ �� ������ � ������� ����
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;                                              // ����� ����

        if (groundPlane.Raycast(ray, out rayDistance))                   // ���������� ����� ���� � ����������               
        {
            Vector3 point = ray.GetPoint(rayDistance);                  // ������������ ����� ����������� ���� � �����������
            controller.LookAt(point);
            crosshairs.position = point;
            if((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gunController.Aim(point);
            }
        }

        // ���������� �������
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0)) // ������ �������, ������ ������ ����������
        {
            gunController.OnTriggerRelease();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
    }
}
