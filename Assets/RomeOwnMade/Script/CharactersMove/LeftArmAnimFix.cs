using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftArmAnimFix : MonoBehaviour
{
    public Animator anim;
    public Collider weaponCol;
    public Vector3 a;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        if(anim.GetBool("defense"))
        {
            Transform leftArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            leftArm.localEulerAngles += a;
            anim.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm, Quaternion.Euler(leftArm.localEulerAngles));
        }
    }

    public void WeaponEnable()
    {
        weaponCol.enabled = true;
    }
    public void WeaponDisable()
    {
        weaponCol.enabled = false;
    }
}
