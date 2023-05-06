using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
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
        if (!IsOwner) return; 
        if (Input.GetKeyDown(KeyCode.T)){
            // TestServerRpc(new ServerRpcParams());

            // TargetClientIds: Client(s) to send the data to
            TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams {TargetClientIds = new List<ulong> {0,1}}});
            // RandomizeInt();
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

    /* ServerRPC:
        1. Runs only on Server, not Client
        2. Must end with suffix "ServerRpc"
        3. ServerRpc must be defined inside NetworkBehaviour
        4. Must be attached to a GameObject within a Network Object
        5. Param must be Value Type, not Ref Type
    */
    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams){ 
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    /* ClientRPC:
        1. Must be called from Server. A Client cannot call a ClientRPC
        2. Run on Client
        3. Use ClientRPC when Server -> One/Multiple Clients
    */
    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams){ 
        Debug.Log("TestClientRpc");
    }


}
