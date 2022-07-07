using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Key Settings")]
    public string keyUp = "w";
    public string keyDown = "s";
    public string keyLeft = "a";
    public string keyRight = "d";
    
    public string keyRun = "LeftShift";
    public string keyJump = "space";
    public string keyAttack = "q";
    public string keyDefence = "r";
    public string keyRoll = "f";
    [Header("Ouptut Signals")]
    public float Dup;
    public float Dright;
    public float Dmag;
    public Vector3 Dvec;

    //1.pressing signal
    public bool run;
    public bool defense;
    //2.trigger once signal
    public bool jump;
    private bool lastJump;

    public bool attack;
    private bool lastAttack;

    public bool roll;
    private bool lastRoll;

    [Header("Others")]
    public bool inputEnabled = true;

    private float targetDup;
    private float targetDright;
    private float velocityDup;
    private float velocityDright;

    void Start()
    {
        
    }


    void Update()
    {
        targetDup = (Input.GetKey(keyUp) ? 1.0f : 0) - (Input.GetKey(keyDown) ? 1.0f : 0);
        targetDright = (Input.GetKey(keyRight) ? 1.0f : 0) - (Input.GetKey(keyLeft) ? 1.0f : 0);

        if(!inputEnabled)
        {
            targetDup = 0;
            targetDright = 0;
        }

        Dup = Mathf.SmoothDamp(Dup, targetDup, ref velocityDup, 0.1f);
        Dright = Mathf.SmoothDamp(Dright, targetDright, ref velocityDright, 0.1f);

        Vector2 tempDAxis = SquareToCircle(new Vector2(Dright, Dup));
        float Dright2 = tempDAxis.x;
        float Dup2 = tempDAxis.y;

        Dmag = Mathf.Sqrt(Dup2 * Dup2 + Dright2 * Dright2);
        //Run State
        run = Input.GetKey(keyRun);

        //Defense State
        defense = Input.GetKey(keyDefence);

        //Jump State
        bool newJump = Input.GetKey(keyJump);
        if (newJump != lastJump && newJump)
            jump = true;
        else
            jump = false;
        lastJump = newJump;

        //Attack State
        bool newAttack = Input.GetKey(keyAttack);
        if (newAttack != lastAttack && newAttack)
            attack = true;
        else
            attack = false;
        lastAttack = newAttack;

        //Roll State
        bool newRoll = Input.GetKey(keyRoll);
        if (newRoll != lastRoll && newRoll)
            roll = true;
        else
            roll = false;
        lastRoll = newRoll;

    }

    void CalculateForward()
    {
        Dvec = Dright * transform.right + Dup * transform.forward;
    }

    private Vector2 SquareToCircle(Vector2 input)
    {
        Vector2 output = Vector2.zero;
        output.x = input.x * Mathf.Sqrt(1 - (input.y * input.y) / 2.0f);
        output.y = input.y * Mathf.Sqrt(1 - (input.x * input.x) / 2.0f);
        return output;
    }


}
