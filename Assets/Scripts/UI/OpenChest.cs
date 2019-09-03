using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider))]
public class OpenChest : MonoBehaviour
{

    private Animator anim;


    private bool isLocked = false;

    private bool isOpen = false;

    private void Start()
    {
        this.anim = this.GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        if (isLocked) {
            // TODO later. Magic spell to open it
        } else if (this.isOpen == false) {
            this.anim.SetTrigger("open");
            this.isOpen = true;
        } else if (this.isOpen) {
            this.anim.SetTrigger("close");
            this.isOpen = false;
        }
    }

}
