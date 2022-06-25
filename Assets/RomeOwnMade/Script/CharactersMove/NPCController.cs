using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;
public class NPCController : MonoBehaviour
{
    public ActorData NPCActor;
    public Transform player;
    private Animator NPCAnimator;
    private Transform startTransform;
    private void Awake()
    {
        NPCAnimator = this.gameObject.GetComponent<Animator>();
        NPCActor.isTalking = false;
    }
    // Update is called once per frame
    void Update()
    {
        if(NPCActor.isTalking)
        {
            NPCAnimator.SetBool("Talk", true);
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
        else
        {
            NPCAnimator.SetBool("Talk", false);
        }
    }
}
