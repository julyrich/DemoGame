using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField createInput;
    [SerializeField] TMP_InputField joinInput;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] TextMeshProUGUI errorTxt;


    public void CreateRoom()
    {
        //limit to 2 players per room
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        SetPlayerName();

        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorTxt.text = message;
        errorTxt.gameObject.SetActive(true);
    }

    public void JoinRoom()
    {
        SetPlayerName();

        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorTxt.text = message;
        errorTxt.gameObject.SetActive(true);
    }

    void SetPlayerName()
    {
        if(nameInput.text != "")
            PhotonNetwork.NickName = nameInput.text;
        else
            PhotonNetwork.NickName = "Nobody";
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }
}
