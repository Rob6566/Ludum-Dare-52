using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//A card class. Abstract superclass - we can implement custom behaviour in sub-classes
public class Card
{
    protected GameManager gameManager;
    protected CardSO cardSO;
    public Sprite sprite;
    public string cardName;
    public string textOnGrab;
    public int cardProbability;
    public int sanityModifier;
    public int bloodModifier;
    public int scoreModifier;
    public bool resetBloodthirst;
    public bool impassable=false;
    public bool visited=false;
    public bool visible=false;

    public int xCoord;
    public int yCoord;
    public int cardNumber;

    public GameObject cardObject;

    public Image cardImage;
    public Image cardFrontImage;
    public TextMeshProUGUI cardNameUI;
    public GameObject cardOverlayObject;
    public Image cardOverlayImage;

    public TextMeshProUGUI leftBonusUI;
    public TextMeshProUGUI rightBonusUI;
    public GameObject leftBonusIMG;
    public GameObject rightBonusIMG;

    public GameObject cardSuitIMG;
    public TextMeshProUGUI cardSuitTXT;

    public SuitedCard suitedCard;
    

    public void init(GameManager newGameManager, CardSO newCardSO, SuitedCard newSuitedCard) {
        gameManager=newGameManager;
        cardSO=newCardSO;
        sprite=cardSO.sprite;
        cardName=cardSO.cardName;
        textOnGrab=cardSO.textOnGrab;
        cardProbability=cardSO.cardProbability;
        sanityModifier=cardSO.sanityModifier;
        bloodModifier=cardSO.bloodModifier;
        scoreModifier=cardSO.scoreModifier;
        resetBloodthirst=cardSO.resetBloodthirst;
        impassable=cardSO.impassable;
        suitedCard=newSuitedCard;
    }

    public void instantiateOnMap(GameObject newCardObject, GameObject parentContainer, int newXCoord, int newYCoord, int newCardNumber) {
        xCoord=newXCoord;
        yCoord=newYCoord;
        cardNumber=newCardNumber;
        cardObject=newCardObject;

        cardObject.transform.SetParent(parentContainer.transform);
        cardObject.transform.localPosition=new Vector3(xCoord*gameManager.CARD_WIDTH_OFFSET, yCoord*gameManager.CARD_HEIGHT_OFFSET, 0)/*+new Vector3(-550, -475, 0)*/;
        cardObject.transform.localScale=gameManager.BOARD_SCALE;

        //CardHandler cardHandler=cardObject./*transform.GetChild(0).gameObject.*/GetComponent<CardHandler>();
        CardHandler cardHandler=cardObject.GetComponent<CardHandler>();
        cardHandler.init(this);

        cardFrontImage = cardObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();
        cardImage = cardObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>();
            cardImage.sprite=sprite;
        cardNameUI = cardObject.transform.GetChild(0).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
            cardNameUI.text=cardName;
        leftBonusIMG=cardObject.transform.GetChild(0).GetChild(3).gameObject;
        leftBonusUI=cardObject.transform.GetChild(0).GetChild(4).gameObject.GetComponent<TextMeshProUGUI>();
        rightBonusIMG=cardObject.transform.GetChild(0).GetChild(5).gameObject;
        rightBonusUI=cardObject.transform.GetChild(0).GetChild(6).gameObject.GetComponent<TextMeshProUGUI>();

        cardSuitIMG=cardObject.transform.GetChild(0).GetChild(7).gameObject;
        cardSuitTXT=cardObject.transform.GetChild(0).GetChild(8).gameObject.GetComponent<TextMeshProUGUI>();

        if (gameManager.complexMode) {
            cardSuitIMG.GetComponent<Image>().sprite=gameManager.cardSuitSprites[(int)suitedCard.cardSuit];
            cardSuitTXT.text=suitedCard.cardNumber.ToString();
            cardSuitTXT.color=((int)suitedCard.cardSuit<2) ? Color.black : Color.red;
        }
        else {
            cardSuitTXT.text="";
            cardSuitIMG.SetActive(false);
        }
    

        
        cardOverlayObject = cardObject.transform.GetChild(0).GetChild(9).gameObject;
        cardOverlayImage=cardOverlayObject.GetComponent<Image>();

        leftBonusUI.text="";
        rightBonusUI.text="";
        if (sanityModifier!=0 || bloodModifier!=0 || scoreModifier!=0) {
            cardImage.gameObject.transform.localScale=new Vector3(70f, 70f, 70f);
            cardFrontImage.sprite=gameManager.cardFrontWithEffects;
            showModifiers();
        }
        else {
            rightBonusIMG.SetActive(false);
            leftBonusIMG.SetActive(false);
            cardImage.gameObject.transform.localScale=new Vector3(100f, 100f, 100f);
            cardFrontImage.sprite=gameManager.cardFrontNoEffects;
        }


        updateUI();
    }

    public float getDistanceFromCoords(int x, int y) {
        float xDiff=Mathf.Abs(xCoord-x);
        float yDiff=Mathf.Abs(yCoord-y);
        return Mathf.Sqrt(xDiff*xDiff + yDiff*yDiff);
    }

    public void setVisited() {
        this.visited=true;
        updateUI();
    }

    public void setVisible() {
        if (!this.visible) {
            this.visible=true;
            updateUI();
        }
    }

    public void updateUI() {
        cardOverlayObject.SetActive(this.visited || !this.visible);
        if (this.visited) {
            cardOverlayImage.sprite=gameManager.cardDepletedSprite;
        }
        else if (!this.visible) {
            cardOverlayImage.sprite=gameManager.cardBackSprite;
        }
    }


    //Whether the player can currently visit this card
    public bool isTraversible() {
        if (visited || !visible) {
            return false;
        }

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
        int xMax=(gameManager.MAP_SIZE_FROM_START*2);
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
        if (gameManager.lockActions || gameManager.gameOver) {
            return;
        }
    
        this.customOnGrab();
        gameManager.modifySanity(sanityModifier);
        gameManager.modifyBlood(bloodModifier);
        gameManager.modifyScore(scoreModifier);
        gameManager.movePlayer(this);
    }

    public void onPointerEnter() {
        if (isTraversible() && !gameManager.gameOver) {
            cardObject.transform.localScale=gameManager.SELECTED_SCALE;
            gameManager.bloodCostOverlay.SetActive(true);
            gameManager.bloodCostOverlay.transform.SetParent(cardObject.transform);
            gameManager.bloodCostOverlay.transform.localPosition= new Vector3(0,0,0);
            gameManager.bloodCostOverlay.transform.localScale= gameManager.BOARD_SCALE;
            gameManager.bloodCostOverlay.transform.SetParent(gameManager.cardContainer.transform);
            gameManager.bloodCostOverlay.transform.SetAsLastSibling();
            int bloodCost=gameManager.bloodThirst;
            int distanceCost=distanceFromPlayer(gameManager.playerXPosition, gameManager.playerYPosition)-1;
            bloodCost+=distanceCost;
            gameManager.bloodCostTXT.text="Blood cost - "+bloodCost.ToString()+" ("+distanceCost.ToString()+" Distance, "+gameManager.bloodThirst.ToString()+" Bloodthirst)";
        }
    }

    public void onPointerExit() {
        cardObject.transform.localScale=gameManager.BOARD_SCALE;
        gameManager.bloodCostOverlay.SetActive(false);
    }

    //Overridden in subclasses
    public virtual void customOnGrab() {
    
    } 

    void showModifiers() {

        bool shownBlood = false;
        bool shownSanity = false;
        bool shownScore = false;
        rightBonusIMG.SetActive(false);

        if(sanityModifier!=0 || bloodModifier!=0 || scoreModifier!=0) {
            List<string> modsToDisplay=new List<string>();
            if (sanityModifier!=0) {
                modsToDisplay.Add("sanity");
            }
            if (bloodModifier!=0) {
                modsToDisplay.Add("blood");
            }
            if (scoreModifier!=0) {
                modsToDisplay.Add("score");
            }

            int modUpto=0;
            foreach(string mod in modsToDisplay) {
                GameObject bonusImg = (modUpto==0 ?  leftBonusIMG : rightBonusIMG);
                TextMeshProUGUI bonusTxt = (modUpto==0 ?  leftBonusUI : rightBonusUI);

                Sprite modSprite = gameManager.bloodIcon;
                int modifier = bloodModifier;
                if (mod=="sanity") {
                    modSprite = gameManager.sanityIcon;
                    modifier = sanityModifier;
                }
                else if (mod=="score") {
                    modSprite = gameManager.scoreIcon;
                    modifier = scoreModifier;
                }
                bonusImg.GetComponent<Image>().sprite=modSprite;
                bonusImg.SetActive(true);

                string colorPrefix = "<b><color=#"+(modifier<0 ? "9A0707" : "0C8C2F")+">";
                bonusTxt.text = colorPrefix+modifier.ToString()+"</color></b>";
                modUpto++;
            }


        }
    }
}
