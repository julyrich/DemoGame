using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform player1Spawn;


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

    public static void QuitGame()
    {
        Application.Quit();
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; 
		#endif
    }
}
