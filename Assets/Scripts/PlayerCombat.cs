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
    [SerializeField] int healAmount = 40;
    [SerializeField] Transform HUD;
    [SerializeField] TextMeshProUGUI scoreTxt;
    [SerializeField] TextMeshProUGUI playerNameTxt;
    [SerializeField] HealCooldownController healCooldown;
    //fireball settings
    [SerializeField] GameObject fireball;
    [SerializeField] float fireballAttackrate = 2f;

    PhotonView photonView;
    CharacterController2D characterController;
    HealthBar healthBar;
    ScoreboardController scoreboard;
    float attackTimeCnt = 0;
    float fireballAttackTimeCnt = 0;
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
        if(photonView.IsMine)
        {
            healCooldown.gameObject.SetActive(true);
            //set cooldown of the heal to 4s
            healCooldown.SetCooldownTime(4);
        }
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

            if(fireballAttackTimeCnt >= (1/fireballAttackrate))
            {
                //F key creates a fireball
                if(Input.GetKeyDown(KeyCode.F))
                {
                    CreateFireBall();
                    fireballAttackTimeCnt = 0;
                }
            }
            else
            {
                fireballAttackTimeCnt += Time.deltaTime;
            }

            if(healCooldown.healAvailable && Input.GetKeyDown(KeyCode.Q))
            {
                photonView.RPC("HealPlayer", RpcTarget.All, healAmount);
                healCooldown.Healed();
            }

            //update scoreboard
            photonView.RPC("UpdateScoreboard", RpcTarget.All, playerNumber);

            //update player names
            photonView.RPC("SetPlayerName", RpcTarget.All, PhotonNetwork.NickName);
            photonView.RPC("UpdateScoreboardName", RpcTarget.All, playerNumber, PhotonNetwork.NickName);

            if(isGameFinished)
                Finish(playerNumber);
        }

        //also update text alignment for first player
        if(playerNumber == 0 && !photonView.IsMine)
        {
            playerNameTxt.alignment = TextAlignmentOptions.MidlineRight;
            scoreTxt.alignment = TextAlignmentOptions.MidlineLeft;
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
    void UpdateScoreboardName(int playerNumber, string playerName)
    {
        if(scoreboard != null)
            scoreboard.SetPlayerName(playerNumber, playerName);
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

            //flip hp and cooldown bars
            if(child.tag == "HPBar" || child.tag == "CDBar")
            {
                Vector3 newScale = child.localScale;
                newScale.x *= -1;
                child.localScale = newScale;

                //unflip hp text
                TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
                Vector3 newTxtScale = text.transform.localScale;
                newTxtScale.x *= -1;
                text.transform.localScale = newTxtScale;
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
                    KilledPlayer();
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
        //change color to show damage taken
        StartCoroutine(DamageTick(.5f));

        playerHealth -= damage;
        if(playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
        healthBar.SetHealth(playerHealth);
    }

    IEnumerator DamageTick(float duration)
    {
        Material material = gameObject.GetComponent<SpriteRenderer>().material;
        Color originColor = material.color;

        material.color = new Color(255,255,255,1);

        float timeCnt = 0;
        while(timeCnt < duration)
        {
            timeCnt += Time.deltaTime;
            yield return null;
        }

        material.color = originColor;
    }

    void Die()
    {
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

    public void KilledPlayer()
    {
        photonView.RPC("GetKillBonus", RpcTarget.All, playerNumber);
        isGameFinished = true;
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

    [PunRPC]
    void HealPlayer(int value)
    {
        playerHealth += value;
        if(playerHealth >= playerMaxHealth)
            playerHealth = playerMaxHealth;

        healthBar.SetHealth(playerHealth);
    }

    void CreateFireBall()
    {
        Vector3 initPosition = transform.position;
        //throw the fireball in the players direction, a little away from the player
        if(transform.localScale.x < 0)
            initPosition -= new Vector3(1f, 0);
        else
            initPosition += new Vector3(1f, 0);

        GameObject newFireball = PhotonNetwork.Instantiate(fireball.name, initPosition, Quaternion.identity);

        FireBallController fireballControl = newFireball.GetComponent<FireBallController>();
        fireballControl.parentPlayerViewId = photonView.ViewID;

        //throw fire ball in the direction the player is facing
        if(transform.localScale.x < 0)
            fireballControl.projectileSpeed *= -1;
    }
}
