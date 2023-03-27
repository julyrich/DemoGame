using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] Transform attackPoint;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] float attackRange = 1.08f;
    [SerializeField] int playerHealth = 100;
    [SerializeField] int playerMaxHealth = 100;
    [SerializeField] int killBonus = 40;
    [SerializeField] Transform HUD;
    [SerializeField] TextMeshProUGUI scoreTxt;
    [SerializeField] TextMeshProUGUI playerNameTxt;
    //fireball settings
    // [SerializeField] float fireballExplosionRadius {get; set;} = 1f;

    PhotonView photonView;
    CharacterController2D characterController;
    HealthBar healthBar;
    ScoreboardController scoreboard;
    float attackTimeCnt = 0;
    int score = 0;
    int playerNumber;
    bool isGameFinished = false;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        photonView = gameObject.GetComponent<PhotonView>();
        characterController = gameObject.GetComponent<CharacterController2D>();
        healthBar = HUD.GetComponentInChildren<HealthBar>(true);
        scoreboard = GameObject.FindObjectOfType<ScoreboardController>(true);
        playerNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        //adapt HUD to the player, first player HUD on the left side, second player HUD on the right side
        if(playerNumber != 0 && photonView.IsMine)
        {
            AdaptHUD();
        }
        //initialise HUD
        healthBar.SetMaxHealth(playerMaxHealth);
        healthBar.SetHealth(playerHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            //higher attack speed = more attacks per second
            if(attackTimeCnt >= (1/attackSpeed))
            {
                //only allow attack if the player is grounded
                if(Input.GetMouseButtonDown(0) && characterController.m_Grounded)
                {
                    Attack();
                    attackTimeCnt = 0;
                }
            }
            else
            {
                attackTimeCnt += Time.deltaTime;
            }

            //update scoreboard
            photonView.RPC("UpdateScoreboard", RpcTarget.All, playerNumber);

            //update player names
            photonView.RPC("SetPlayerName", RpcTarget.All, PhotonNetwork.NickName);

            if(isGameFinished)
                Finish(playerNumber);
        }

        //update score text
        scoreTxt.text = score.ToString();
    }

    [PunRPC]
    void UpdateScoreboard(int playerNumber)
    {
        if(scoreboard != null)
            scoreboard.SetScore(playerNumber, score);
    }

    [PunRPC]
    void SetPlayerName(string name)
    {
        playerNameTxt.text = name;
    }

    void AdaptHUD()
    {
        for(var i = 0; i < HUD.childCount; i++)
        {
            //move HUD to the right side
            Transform child = HUD.GetChild(i);
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            Vector3 newPosition = rectTransform.anchoredPosition; 
            newPosition.x *= -1;
            rectTransform.anchoredPosition = newPosition;

            //flip hp bar
            if(child.tag == "HPBar")
            {
                Vector3 newScale = child.localScale;
                newScale.x *= -1;
                child.localScale = newScale;

                //unflip hp text
                TextMeshProUGUI hpText = child.GetComponentInChildren<TextMeshProUGUI>();
                Vector3 newTxtScale = hpText.transform.localScale;
                newTxtScale.x *= -1;
                hpText.transform.localScale = newTxtScale;
            }
            else
            {
                //reverse text alignment
                TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
                if(text.alignment == TextAlignmentOptions.Left)
                {
                    text.alignment = TextAlignmentOptions.MidlineRight;
                }
                else
                {
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                }
            }
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        bool otherPlayerHit = false;
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach(Collider2D enemyHit in hitEnemies)
        {
            //only hit other player once (since there are 2 colliders)
            if(enemyHit.gameObject != gameObject && !otherPlayerHit)
            {
                enemyHit.GetComponent<PlayerCombat>().Hit(attackDamage);
                photonView.RPC("IncreaseScore", RpcTarget.All, attackDamage);
                otherPlayerHit = true;

                //check if that killed the enemy
                int otherPlayerHealth = enemyHit.GetComponent<PlayerCombat>().GetPlayerHealth();
                if(otherPlayerHealth <= 0 && photonView.IsMine)
                {
                    //get kill bonus in this case and display final results
                    photonView.RPC("GetKillBonus", RpcTarget.All, playerNumber);
                    isGameFinished = true;
                }
            }
        }
    }

    public void Hit(int damage)
    {
        photonView.RPC("TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void TakeDamage(int damage)
    {
        animator.SetTrigger("PlayerHit");
        playerHealth -= damage;
        if(playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
        healthBar.SetHealth(playerHealth);
    }

    void Die()
    {
        animator.SetBool("PlayerDead", true);
        isGameFinished = true;
    }

    [PunRPC]
    void IncreaseScore(int value)
    {
        score += value;
    }

    [PunRPC]
    void GetKillBonus(int playerNumber)
    {
        scoreboard.SetKillBonus(playerNumber, killBonus);
        UpdateScoreboard(playerNumber);
        scoreboard.FinishGame();
    }

    public int GetPlayerHealth()
    {
        return playerHealth;
    }

    void Finish(int playerNumber)
    {
        //don't allow players to move or attack anymore
        gameObject.GetComponent<PlayerMovement>().enabled = false;
        this.enabled = false;
        Time.timeScale = 0f;

        //display scoreboard
        scoreboard.SetVictoryOrDefeat(playerNumber);
        scoreboard.gameObject.SetActive(true);
    }

    // void CreateFireBall(GameObject fireBallObject, int damage)
    // {
    //     GameObject newFireball = Instantiate(fireBallObject, transform.position, new Quaternion(0,0,0,0));
    //     FireBallController fireball = newFireball.GetComponent<FireBallController>();

    //     //throw fire ball in the direction the player is facing
    //     if(transform.localScale.x < 0)
    //         fireball.projectileSpeed *= -1;

    //     fireball.SetDamage(damage);
    //     fireball.SetStunDuration(fireballStun);
    //     fireball.SetExplosionRadius(fireballExplosionRadius);
    // }

    // public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    // {
    //     if(stream.IsWriting)
    //     {
    //         stream.SendNext(playerHealth);
    //     }
    //     else if(stream.IsReading)
    //     {
    //         playerHealth = (int)stream.ReceiveNext();
    //     }
    // }
}
