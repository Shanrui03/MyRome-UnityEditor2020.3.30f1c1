using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Moving Speed")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Gravity")]
    public float gravity = -9.81f;

    [Header("Jump Distance")]
    public float jumpHeight = 3f;

    [Header("Player Avator")]
    public GameObject playerAvator;
    public GameObject playerHead;
    public CinemachineBrain m_brain;
    public GameObject followCamera;
    public GameObject jumpCamera;
    public PlayerInput pi;

    [Header("Audio")]
    public AudioSource walkAudio;

    private CharacterController controller;
    private Animator playerAnimator;
    private float jumpingTime;
    private Vector3 startCameraPos;


    private Vector3 velocity;
    private Vector3 move;
    private bool isJumping;
    private bool isRolling;

    private float lerpTarget;

    private float speed;
    public static bool isGround = false;
    public static bool isTalking = false;
    public static bool isAttacking = false;
    public static bool isInArena = false;
    public static bool isDefensing = false;
    public static bool playerCanMove = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerAnimator = playerAvator.GetComponent<Animator>();
        pi = GetComponent<PlayerInput>();
        jumpingTime = 0f;
        isGround = true;
        isTalking = false;
        isAttacking = false;
        isJumping = false;
        isRolling = false;
        isInArena = false;
        isDefensing = false;
        playerCanMove = false;
    }


    void Update()
    {
        if(playerCanMove)
        {
            AniStateControl();
            movement();
            isDefensing = pi.defense;
        }
    }

    void movement()
    {
        isGround = controller.isGrounded;
        playerAnimator.SetBool("isGround", isGround);
        if(!isGround)
        {
            jumpingTime += Time.deltaTime;
            if(jumpingTime >= 1.8f)
            {
                playerAnimator.SetBool("isFalling", true);
                jumpingTime = 0f;
            }
        }
        else
        {
            playerAnimator.SetBool("isFalling", false);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (!isJumping)
            move = transform.right * x + transform.forward * z;

        if (pi.run)
            speed = runSpeed;
        else
            speed = walkSpeed;

        if (!isTalking && !PauseMenu.GameIsPaused)
        {
            //GroundMove
            controller.Move(move * speed * Time.deltaTime);
            if (move != Vector3.zero)
            {
                if (!walkAudio.isPlaying && !isJumping)
                {
                    walkAudio.Play();
                }
            }
            else
            {
                walkAudio.Stop();
            }


            //JumpMove
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            if (isGround && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            if (Input.GetButtonDown("Jump") && isGround)
            {
                velocity.y = Mathf.Sqrt(-2 * jumpHeight * gravity); //v=sqrt£¨2gh£©£»
                walkAudio.Stop();
            }
        }
        else
        {
            walkAudio.Stop();
        }

    }

    public void AniStateControl()
    {
        if (!isTalking)
        {
            //Walk or Run
            playerAnimator.SetFloat("forward", pi.Dmag * Mathf.Lerp(playerAnimator.GetFloat("forward"), ((pi.run) ? 2.0f : 1.0f), 0.2f));
            //if (pi.Dmag > 0.1f)
            //{
            //    playerAvator.transform.forward = Vector3.Slerp(playerAvator.transform.forward, pi.Dvec, 0.3f);
            //}



            //Jump
            if(pi.jump)
            {
                playerAnimator.SetTrigger("jump");
            }

            //Roll
            if(pi.roll && move != Vector3.zero)
            {
                if(pi.Dup >= 0)
                    playerAnimator.SetTrigger("roll");
                else
                    playerAnimator.SetTrigger("rollback");
            }

            if (isInArena && !isRolling)
            {
                //Attack
                if (pi.attack)
                {
                    playerAnimator.SetTrigger("attack");
                }

                //Defense
                if (isAttacking)
                {
                    playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Defence"), 0.0f);
                    playerAnimator.SetBool("defense", false);
                }
                else
                {
                    playerAnimator.SetBool("defense", pi.defense);
                }
            }
            else if (isRolling)
            {
                playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Defence"), 0.0f);
                playerAnimator.SetBool("defense", false);
            }
        }
        else
        {
            playerAnimator.SetFloat("forward", 0f);
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

    public void EnterJumping()
    {
        m_brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        pi.inputEnabled = false;
        isJumping = true;
        walkAudio.Stop();
        if(!isInArena)
        {
            followCamera.SetActive(false);
            jumpCamera.SetActive(true);
        }
        
    }
    public void ExitJumping()
    {
        m_brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        if (!isInArena && isJumping)
        {
            followCamera.SetActive(true);
            jumpCamera.SetActive(false);
        }
        pi.inputEnabled = true;
        isJumping = false;

    }
    public void OnFallingEnter()
    {
        m_brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        pi.inputEnabled = false;
        isJumping = true;
        walkAudio.Stop();
        if (!isInArena)
        {
            followCamera.SetActive(false);
            jumpCamera.SetActive(true);
        }
    }

    public void EnterRolling()
    {
        EnterJumping();
        isRolling = true;
    }
    public void ExitRolling()
    {
        ExitJumping();
        isRolling = false;
    }



    public void OnAttack1hAEnter()
    {
        lerpTarget = 1.0f;
        isAttacking = true;
    }
    public void OnAttackidleEnter()
    {
        lerpTarget = 0f;
        isAttacking = false;
    }
    public void OnAttack1hAUpdate()
    {
        playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Attack"), Mathf.Lerp(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Attack")), lerpTarget, 0.1f));
    }
    public void OnAttackidleUpdate()
    {
        playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Attack"), Mathf.Lerp(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Attack")), lerpTarget, 0.1f));
    }

    public void OnDefenseidleEnter()
    {
        playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Defence"), 0.0f);
    }
    public void OnDefenseEnter()
    {
        playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Defence"), 1.0f);
    }

    public void SetPlayerDead(bool isDead)
    {
        playerAnimator.SetBool("death", isDead);
        isTalking = isDead;
    }

    private bool CheckState(string stateName,string layerName = "Base Layer")
    {
        return playerAnimator.GetCurrentAnimatorStateInfo(playerAnimator.GetLayerIndex(layerName)).IsName(stateName);
    }
}
