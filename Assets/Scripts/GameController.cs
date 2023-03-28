using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform player1Spawn;

    bool isGameReady = false;

    void Start()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            PhotonNetwork.Instantiate(playerPrefab.name, player1Spawn.position, Quaternion.identity);
        else
        {
            //spawn player 2 on the opposite side
            Vector3 player2Spawn = player1Spawn.position;
            player2Spawn.x *= -1;

            PhotonNetwork.Instantiate(playerPrefab.name, player2Spawn, Quaternion.identity);
        }
    }

    void Update()
    {
        //leave game if the second player left
        if(!isGameReady && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            isGameReady = true;
        }
        else if(isGameReady && PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            StartCoroutine(Disconnect());
        }
    }

    IEnumerator Disconnect()
    {
        PhotonNetwork.LeaveRoom();

        while(PhotonNetwork.InRoom)
            yield return null;

        PhotonNetwork.LoadLevel("Lobby");
    }

    public void DisconnectFromRoom()
    {
        //start time again so the coroutine can run
        Time.timeScale = 1f;
        StartCoroutine(Disconnect());
    }

    public void Rematch()
    {
        //if player wants a rematch, load the game again
        //second player also needs to select rematch to spawn in
        Time.timeScale = 1f;
        PhotonNetwork.LoadLevel("Game");
    }

    public static void QuitGame()
    {
        Application.Quit();
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; 
		#endif
    }
}
