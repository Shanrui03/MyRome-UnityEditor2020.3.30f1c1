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

    [Header("Audio")]
    public AudioSource walkAudio;

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
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;    

        if (!isTalking && !PauseMenu.GameIsPaused)
        {
            controller.Move(move * speed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);

            if(move != Vector3.zero)
            {
                playerAnimator.SetBool("Walk", true);
                if (!walkAudio.isPlaying)
                {
                    walkAudio.Play();
                }
            }
            else
            {
                playerAnimator.SetBool("Walk", false);
                walkAudio.Stop();
            }

            isGround = controller.isGrounded;
            if (isGround && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            
            if (Input.GetButtonDown("Jump") && isGround)
            {
                playerAnimator.SetBool("Jump", true);
                velocity.y = Mathf.Sqrt(-2 * jumpHeight * gravity); //v=sqrt��2gh����
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
            walkAudio.Stop();
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
