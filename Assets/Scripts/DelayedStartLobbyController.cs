using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DelayedStartLobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject DelayedStartButton;

    [SerializeField]
    private GameObject DelayedCancelButton;

    [SerializeField]
    private int RoomSize;

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        DelayedStartButton.SetActive(true);
    }
    public void QuickStart()
    {
        DelayedStartButton.SetActive(false);
        DelayedCancelButton.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Quick Start");
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a room");

        CreateRoom();
    }
    void CreateRoom()
    {
        Debug.Log("Creating room now");
        int RandomRoomNumber = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)RoomSize };
        PhotonNetwork.CreateRoom("Room" + RandomRoomNumber, roomOps);
        Debug.Log(RandomRoomNumber);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create romm. . . trying again");
        CreateRoom();
    }
    public void QuickCancel()
    {
        DelayedCancelButton.SetActive(false);
        DelayedStartButton.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }
}
