using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
    public CharacterController cc;
    private MyPlayerInput playerInput;

    private void Start(){
        playerInput = new();
        playerInput.Enable();
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            listener.enabled = true;
            vc.Priority = 1;
        }else
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
        }else if (IsClient && IsLocalPlayer)
        {
            MoveServerRpc(moveInput);
            LookAroundServerRpc(mouseDelta);
        }
    }
    [ServerRpc]
    private void CreateSphereServerRpc(){
        CreateSphere();
    }
    private void CreateSphere(){
        var instance = Instantiate(spawnedObjectPrefab);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn(true);
    }

    private void LookAround(Vector2 _input){
        float rotationAmount = _input.x * rotationSpeed;
        accumulatedRotation += rotationAmount; 
        transform.rotation = Quaternion.Euler(0,accumulatedRotation,0);
    }

    private void Move(Vector2 _input){
        Vector3 calcMove = _input.x * playerTransform.right + _input.y * playerTransform.forward;
        cc.Move(playerSpeed * Time.deltaTime * calcMove);
    }

    [ServerRpc]
    private void LookAroundServerRpc(Vector2 _input){
        LookAround(_input);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 _input){
        Move(_input);
    }
}
