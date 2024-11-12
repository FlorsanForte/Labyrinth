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

    private float nextFireTime = 0f;

    private void Update()
    {
        if (!IsOwner) return;
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
        bullet.GetComponent<Bullet>().attackerID = OwnerClientId;
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