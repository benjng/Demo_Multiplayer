using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Multiplayer;
using UnityEngine;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform bulletTransform;
    private Transform spawnedObjectTransform;

    // Init a randomData
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData{
            _int = 56,
            _bool = true,
            message = "",
        }, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Struct Definition
    public struct MyCustomData : INetworkSerializable{
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }


    // When network starts
    public override void OnNetworkSpawn(){
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; randomNumber: " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }

    void Update()
    {
        // Avoid controlling other "Player" prefabs that doesn't belong to this script 
        // (e.g. Client1 is not the owner of Client2's character)
        if (!IsOwner) return; 

        if (Input.GetKeyDown(KeyCode.Alpha1)){ // SERVER ACTS
            TestServerRpc(new ServerRpcParams());
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){ // CLIENT ACTS
            TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams {TargetClientIds = new List<ulong> {0,1}}});
        }
        // if (Input.GetKeyDown(KeyCode.Alpha3)){
        //     RandomizeInt();
        // }
        if (Input.GetKeyDown(KeyCode.T)){
            SpawnBulletOnServerServerRpc(new ServerRpcParams());
        }
        if (Input.GetKeyDown(KeyCode.R)){
            Destroy(spawnedObjectTransform.gameObject); // Destroy on both local/network
        }

        Movement();
    }

    void RandomizeInt(){
        randomNumber.Value = new MyCustomData {
            _int = Random.Range(1,100),
            _bool = false,
            message = "Uvuvwevwevwe onyetenvewve ugwemubwem ossas",
        };
    }

    void Movement(){
        Vector3 moveDir = new Vector3(0,0,0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }


    // ===================================================================

    [ServerRpc]
    void SpawnBulletOnServerServerRpc(ServerRpcParams serverRpcParams){
        spawnedObjectTransform = Instantiate(bulletTransform); // Shows on local only
        spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true); // Spawn over network
        spawnedObjectTransform.position = gameObject.transform.position;
        if (serverRpcParams.Receive.SenderClientId == 0){
            spawnedObjectTransform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    // ===================================================================
    /* ServerRPC:
        1. Call from Client/Host, Run on Server
        2. Must end with suffix "ServerRpc"
        3. ServerRpc must be defined inside NetworkBehaviour
        4. Must be attached to a GameObject within a Network Object
        5. Param must be Value Type, not Ref Type
    */
    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams){ 
        Debug.Log("Calling TestServerRpc by: " + OwnerClientId + "; Sent by: " + serverRpcParams.Receive.SenderClientId);
    }

    /* ClientRPC:
        1. Call from Server, Run on Client(s)
        2. Use ClientRPC when Server -> One/Multiple Clients
    */
    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams){ 
        Debug.Log("TestClientRpc");
    }
}
