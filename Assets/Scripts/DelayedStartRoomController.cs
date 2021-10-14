using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DelayedStartRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int waitingRoonSceneIndex;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene(waitingRoonSceneIndex);
    }
}
