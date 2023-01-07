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
        Debug.Log("Pointer Click");
        if (!attachedCard.isTraversible()) {
            return;
        }

        //TODO - handle this card
    }


     /************************* 3. HOVERING *************************/
    public void OnPointerEnter(PointerEventData eventData) {
       Debug.Log("Pointer Enter");
       attachedCard.onPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
       attachedCard.onPointerExit();
    }
}
