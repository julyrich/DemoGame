using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] CharacterController2D controller;
    [SerializeField] float runSpeed = 10f;
    [SerializeField] Animator animator;
    
    PhotonView view;
    float horizontalMove = 0f;
    bool doJump = false;
    Vector2 screenBounds;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        view = gameObject.GetComponent<PhotonView>();

        //get movement bounds
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }

    // Update is called once per frame
    void Update()
    {
        if(view.IsMine)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

            if(Input.GetButtonDown("Jump"))
            {
                doJump = true;
                animator.SetBool("isJumping", true);
            }
        }
    }

    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, doJump);

        doJump = false;
    }

    void LateUpdate()
    {
        //don't let player move outside the camera bounds
        Vector3 viewPos = transform.position;
        viewPos.x = Mathf.Clamp(viewPos.x, screenBounds.x * -1, screenBounds.x);
        transform.position = viewPos;
    }

    public void onLanding()
    {
        animator.SetBool("isJumping", false);
    }

}
