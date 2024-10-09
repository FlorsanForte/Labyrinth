using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public GameObject spawnedObjectPrefab;
    // private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
    //     new MyCustomData{
    //         _int = 52,
    //         _bool = true,
    //     }, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    // public struct MyCustomData : INetworkSerializable
    // {
    //     public int _int;
    //     public bool _bool;
    //     public string message;

    //     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    //     {
    //         serializer.SerializeValue(ref _int);
    //         serializer.SerializeValue(ref _bool);
    //         serializer.SerializeValue(ref message);
    //     }
    // }
    // public override void OnNetworkSpawn()
    // {
    //     randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue)=>{
    //         Debug.Log(OwnerClientId+"; "+newValue._int+"; "+newValue._bool+"; "+newValue.message);
    //     };
    // }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            //TestServerRpc();
            if(IsServer){
                CreateSphere();
            }else{
                CreateSphereServerRpc();
            }
            
            // randomNumber.Value = new MyCustomData{
            //     _int=10,
            //     _bool=false,
            //     message="hello",
            // };
        }
        Vector3 moveDir = new Vector3(0,0,0);
        if (Input.GetKey(KeyCode.W))
        {
            moveDir.z = + 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir.z = - 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x = - 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x = + 1f;
        }
        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
    [ServerRpc]
    private void CreateSphereServerRpc(){
        CreateSphere();
    }
    private void CreateSphere(){
        var instance = Instantiate(spawnedObjectPrefab);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
    }
}
