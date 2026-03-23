using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool Interactable = true;
    public Animator anim;
    public bool ArrangeSequence = false;
    public int TopIndex;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(Interactable)
        {
            if (ArrangeSequence)
            {
                transform.SetSiblingIndex(TopIndex);
            }
            anim.SetBool("IsHovered", true);
        }
       
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if(Interactable)
        {
            anim.SetBool("IsHovered", false);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(Interactable)
        {
            anim.SetBool("IsPressed", true);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if(Interactable)
        {
            anim.SetBool("IsPressed", false);
        }
    }
}
