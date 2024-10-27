using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] public GameObject spawnedObjectPrefab;
    [SerializeField] private float playerSpeed;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineVirtualCamera vc;
    [SerializeField] private AudioListener listener;
    [SerializeField] private float rotationSpeed = 0.1f;
    [SerializeField] private float accumulatedRotation;
    [SerializeField] public Transform bulletSpawnPoint;
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] public float bulletSpeed = 20;
    public CharacterController cc;
    private MyPlayerInput playerInput;
    public NetworkVariable<float> velocity = new NetworkVariable<float>();

    private float shootCooldown = 1f;
    private float lastShotTime;

    private void Start()
    {
        playerInput = new MyPlayerInput();
        playerInput.Enable();
        lastShotTime = -shootCooldown; // Initialize to allow immediate shooting
        playerInput.Player.LeftClick.performed += _ => OnClick();
    }

    private void OnClick()
    {
        if (Time.time >= lastShotTime + shootCooldown)
        {
            lastShotTime = Time.time;

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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            listener.enabled = true;
            vc.Priority = 1;
        }
        else
        {
            vc.Priority = 0;
        }
    }

    private void Update()
    {
        Vector2 moveInput = playerInput.Player.Movement.ReadValue<Vector2>();
        Vector2 mouseDelta = playerInput.Player.LookAround.ReadValue<Vector2>();

        if (IsServer && IsLocalPlayer)
        {
            Move(moveInput);
            LookAround(mouseDelta);
        }
        else if (IsClient && IsLocalPlayer)
        {
            MoveServerRpc(moveInput);
            LookAroundServerRpc(mouseDelta);
        }
    }

    private void LookAround(Vector2 _input)
    {
        float rotationAmount = _input.x * rotationSpeed;
        accumulatedRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, accumulatedRotation, 0);
    }

    private void Move(Vector2 _input)
    {
        Vector3 calcMove = _input.x * playerTransform.right + _input.y * playerTransform.forward;
        cc.Move(playerSpeed * Time.deltaTime * calcMove);
    }

    private void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        var bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn(true);
        bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
    }

    [ServerRpc]
    private void LookAroundServerRpc(Vector2 _input)
    {
        LookAround(_input);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 _input)
    {
        Move(_input);
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        Shoot();
    }
}
