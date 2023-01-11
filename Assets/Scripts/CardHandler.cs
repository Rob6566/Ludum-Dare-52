using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    
    private Card attachedCard;

     public void init(Card newCard) {
         attachedCard=newCard;
     }

    public void OnPointerClick(PointerEventData eventData) {  
        if (!attachedCard.isTraversible() || eventData.button == PointerEventData.InputButton.Right) {
            return;
        }

        attachedCard.onGrab();
    }


     /************************* 3. HOVERING *************************/
    public void OnPointerEnter(PointerEventData eventData) {
       attachedCard.onPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
       attachedCard.onPointerExit();
    }
}
