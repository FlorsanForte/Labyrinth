using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private float swayIntensity = 0.05f;
    [SerializeField] private float swaySmoothness = 5f;
    [SerializeField] private Transform firePoint; 
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] VisualEffect muzzleFlash;

    private Vector3 initialPosition;
    private float nextFireTime = 0f;

    private void Start()
    {
        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleSway();

        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    private void HandleSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");

        //Vector3 targetPosition = new Vector3(-mouseX * swayIntensity, -mouseY * swayIntensity, 0);
        Vector3 targetPosition = new Vector3(-mouseX * swayIntensity, 0, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + targetPosition, Time.deltaTime * swaySmoothness);
    }

    public void Fire()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (IsServer && IsLocalPlayer)
            {
                Shoot();
            }
            else if (IsClient && IsLocalPlayer)
            {
                ShootServerRpc();
            }
        }
    }

    private void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        var bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn(true);
        bullet.GetComponent<Rigidbody>().velocity = firePoint.forward * bulletSpeed;
        muzzleFlash.Play();
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }
}
