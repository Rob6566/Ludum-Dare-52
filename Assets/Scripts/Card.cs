using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//A science class. Abstract superclass - we'll be implementing abilities in sub-classes
public class Card: MonoBehaviour
{
    protected GameManager gameManager;
    protected CardSO cardSO;
    public Sprite sprite;
    public string name;
    public string textOnGrab;
    public int cardProbability;
    public int sanityModifier;
    public int bloodModifier;
    public int scoreModifier;
    public bool resetBloodDrain;
    public bool impassable=false;
    public bool visited=false;

    public int xCoord;
    public int yCoord;

    public GameObject cardObject;

    public void init(GameManager newGameManager, CardSO newCardSO) {
        gameManager=newGameManager;
        cardSO=newCardSO;
        name=cardSO.name;
        textOnGrab=cardSO.textOnGrab;
        cardProbability=cardSO.cardProbability;
        sanityModifier=cardSO.sanityModifier;
        bloodModifier=cardSO.bloodModifier;
        scoreModifier=cardSO.scoreModifier;
        resetBloodDrain=cardSO.resetBloodDrain;
        impassable=cardSO.impassable;
    }

    public void instantiateOnMap(GameObject cardPrefab, GameObject parentContainer, int newXCoord, int newYCoord) {
        xCoord=newXCoord;
        yCoord=newYCoord;
        cardObject = Instantiate(cardPrefab);

        cardObject.transform.SetParent(parentContainer.transform);
        cardObject.transform.localPosition=new Vector3(xCoord, yCoord, 0)+new Vector3(-550, -475, 0);
        cardObject.transform.localScale=gameManager.BOARD_SCALE;

    }

    public float getDistanceFromCoords(int x, int y) {
        float xDiff=Mathf.Abs(xCoord-x);
        float yDiff=Mathf.Abs(yCoord-y);
        return Mathf.Sqrt(xDiff*xDiff + yDiff*yDiff);
    }


    //Whether the player can immediately visit this card
    public bool isTraversible() {
        List<Card> adjacentCards=this.getAdjacentCards();
        foreach(Card thisCard in adjacentCards) {
            if (thisCard.visited) {
                return true;
            }
        }
        return false;
    }

    //How far this tile is from player
    public int distanceFromPlayer(int playerXCoord, int playerYCoord) {
        return Mathf.Abs(xCoord-playerXCoord)+Mathf.Abs(yCoord-playerYCoord);
    }

    public List<Card> getAdjacentCards() {
        int xMin=0;
        int xMax=(gameManager.MAP_SIZE_FROM_START*2)+1;
        int yMin=xMin;
        int yMax=xMax;

        List<Card> returnCards=new List<Card>();

        if (xCoord>xMin) {
            returnCards.Add(gameManager.getCardFromCoords(xCoord-1, yCoord));
        }
        if (xCoord<xMax) {
            returnCards.Add(gameManager.getCardFromCoords(xCoord+1, yCoord));
        }
        if (yCoord>yMin) {
            returnCards.Add(gameManager.getCardFromCoords(xCoord, yCoord-1));
        }
        if (yCoord<yMax) {
            returnCards.Add(gameManager.getCardFromCoords(xCoord, yCoord+1));
        }
        return returnCards;
    }


    public void onGrab() {
        gameManager.modifySanity(sanityModifier);
        gameManager.modifyBlood(bloodModifier);
        gameManager.modifyScore(scoreModifier);
        this.customOnGrab();
    }

    //Overridden in subclasses
    public virtual void customOnGrab() {
    
    } 
}
