using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FireBallController : MonoBehaviour
{
    public int parentPlayerViewId;
    public float projectileSpeed = 2f;
    [SerializeField] float explosionRadius = 1f;
    [SerializeField] int damage = 40;
    [SerializeField] ParticleSystem explosionEffect;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] float maxTravelRange = 50f;

    float startPosX;
    PhotonView photonView;
    bool objectHit = false;


    void Start()
    {
        photonView = gameObject.GetComponent<PhotonView>();

        //flip ball if necessary
        if(projectileSpeed < 0 && photonView.IsMine)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        startPosX = transform.position.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(photonView.IsMine)
        {
            float newPos = transform.position.x + projectileSpeed * Time.fixedDeltaTime;
            transform.position = new Vector3(newPos, transform.position.y, transform.position.z);

            //only travel for a limited range
            if(Mathf.Abs(startPosX - transform.position.x) >= maxTravelRange)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //only hit once
        if(photonView.IsMine && !objectHit)
        {
            objectHit = true;
            //check enemy layer mask
            if((enemyLayers.value & 1 << col.gameObject.layer) != 0)
            {
                List<ContactPoint2D> contacts = new List<ContactPoint2D>();
                col.GetContacts(contacts);
                
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(contacts[0].point, explosionRadius, enemyLayers);
                if(hitEnemies.Length > 0)
                {
                    List<GameObject> playersHit = new List<GameObject>();

                    foreach(Collider2D hit in hitEnemies)
                    {
                        //only hit player once
                        if(playersHit.Contains(hit.gameObject) == false)
                        {
                            hit.GetComponent<PlayerCombat>().Hit(damage);
                            playersHit.Add(hit.gameObject);

                            PhotonView playerView = PhotonView.Find(parentPlayerViewId);
                            playerView.RPC("IncreaseScore", RpcTarget.All, damage);

                            //check if that killed the enemy
                            if(hit.GetComponent<PlayerCombat>().GetPlayerHealth() <= 0)
                            {
                                playerView.GetComponent<PlayerCombat>().KilledPlayer();
                            }
                        }
                    }

                    //explode on the first enemy impact
                    photonView.RPC("Explode", RpcTarget.All, new Vector3(contacts[0].point.x, contacts[0].point.y));
                }
            }
        }
    }

    [PunRPC]
    void Explode(Vector3 initPos)
    {
        ParticleSystem explosion = Instantiate(explosionEffect, initPos, Quaternion.identity);

        explosion.Play();

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + (transform.lossyScale.x/2), transform.position.y, transform.position.z), explosionRadius);
    }
}
