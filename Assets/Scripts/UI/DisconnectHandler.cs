using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DisconnectHandler : MonoBehaviourPunCallbacks
{
    public static string latestRoomName;
    public static bool inGame = false;
    
    bool rejoinCalled = false;
    bool reconnectCalled = false;
    DisconnectCause previousDisconnectCause;

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogFormat("OnDisconnected(cause={0}) ClientState={1} PeerState={2}",
                            cause,
                            PhotonNetwork.NetworkingClient.State,
                            PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState);
        if (rejoinCalled)
        {
            Debug.LogErrorFormat("Rejoin failed, client disconnected, causes; prev.:{0} current:{1}", previousDisconnectCause, cause);
            rejoinCalled = false;
        }
        else if (reconnectCalled)
        {
            Debug.LogErrorFormat("Reconnect failed, client disconnected, causes; prev.:{0} current:{1}", previousDisconnectCause, cause);
            reconnectCalled = false;
        }
        HandleDisconnect(cause); // add attempts counter? to avoid infinite retries?
        previousDisconnectCause = cause;
    }

    void HandleDisconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            // cases that we can recover from
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.Exception:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.AuthenticationTicketExpired:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                TryReconnectAndRejoin();
                break;
            case DisconnectCause.None:
            case DisconnectCause.DisconnectByClientLogic:
                break;
            case DisconnectCause.OperationNotAllowedInCurrentState:
            case DisconnectCause.CustomAuthenticationFailed:
            case DisconnectCause.InvalidAuthentication:
            case DisconnectCause.ExceptionOnConnect:
                TryReconnectAndRejoin();
                break;
            case DisconnectCause.MaxCcuReached:
            case DisconnectCause.InvalidRegion:
                Debug.LogErrorFormat("Disconnection we cannot automatically recover from, cause: {0}, report it if you think auto recovery is still possible", cause);
                break;
        }
    }

    void TryReconnectAndRejoin()
    {
        if (inGame)
        {
            Destroy(RoomManager.Instance.gameObject);
            // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            PhotonNetwork.LoadLevel(0);
            inGame = false;
        }
        else
        {
            Debug.Log("calling PhotonNetwork.Reconnect()");
            reconnectCalled = PhotonNetwork.Reconnect();
        }
        if (!reconnectCalled)
        {
            Debug.LogError("PhotonNetwork.Reconnect() returned false, client stays disconnected.");
        }
    }

    public override void OnJoinedRoom()
    {
        if (rejoinCalled)
        {
            Debug.Log("Rejoin successful");
            rejoinCalled = false;
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (rejoinCalled)
        {
            if (inGame)
            {
                PhotonNetwork.Disconnect();
            }
            Debug.LogErrorFormat("Quick rejoin failed with error code: {0} & error message: {1}", returnCode, message);
            rejoinCalled = false;
        }
    }
    
    public override void OnConnectedToMaster()
    {
        if (reconnectCalled)
        {
            Debug.Log("Reconnect successful");
            reconnectCalled = false;
            // if (inGame)
            // {
            //     PhotonNetwork.JoinRoom(DisconnectHandler.latestRoomName);
            //     rejoinCalled = true;
            // }
        }
    }
}
