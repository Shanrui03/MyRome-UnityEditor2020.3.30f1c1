using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Moving Speed")]
    public float speed = 12f;

    [Header("Gravity")]
    public float gravity = -9.81f;

    [Header("Jump Distance")]
    public float jumpHeight = 3f;

    [Header("Player Avator")]
    public GameObject playerAvator;

    private CharacterController controller;
    private Animator playerAnimator;

    private Vector3 velocity;
    private bool isGround;

    public static bool isTalking = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAnimator = playerAvator.gameObject.GetComponent<Animator>();
        isTalking = false;
    }


    void Update()
    {
        movement();
    }
    void movement()
    {
        //x跟z轴移动：
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z; //根据角色的朝向进行基于x轴与z轴的移动   

        if (!isTalking)
        {
            controller.Move(move * speed * Time.deltaTime);

            //考虑重力的y轴移动：
            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);

            if(move != Vector3.zero)
            {
                playerAnimator.SetBool("Walk", true);
            }
            else
            {
                playerAnimator.SetBool("Walk", false);
            }
            //isGround = Physics.CheckSphere(groundCheck.position, groundDistance, GroundMask);
            isGround = controller.isGrounded;

            if (isGround && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            //跳跃
            if (Input.GetButtonDown("Jump") && isGround)
            {
                playerAnimator.SetBool("Jump", true);
                velocity.y = Mathf.Sqrt(-2 * jumpHeight * gravity); //v=sqrt（2gh）；
            }
            else if(isGround)
            {
                playerAnimator.SetBool("Jump", false);
            }
        }
        else
        {
            playerAnimator.SetBool("Walk", false);
            playerAnimator.SetBool("Jump", false);
        }

    }

    public static void EnterTalking()
    {
        isTalking = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void LeaveTalking()
    {
        isTalking = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


}
