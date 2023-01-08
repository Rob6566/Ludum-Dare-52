using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public GameObject cardContainer;
    public GameObject playerObject;
        public GameObject playerSpeechBubble;
        public TextMeshProUGUI playerSpeechTxt;
    public int score=100;
    public int sanity=5;
    public int SANITY_MAX=10;
    public int blood=10;
    public int BLOOD_MAX=100;
    public float visibility_range=5f; //How far we can see
    public int bloodThirst=0;
    public int BLOODTHIRST_RATE_MAX=20;

    public List<CardSO> availableCards = new List<CardSO>();

    public List<Card> cardsOnMap = new List<Card>();

    public int CARD_WIDTH_OFFSET=20;
    public int CARD_HEIGHT_OFFSET=0;
    public int MAP_SIZE_FROM_START=10;

    public float SECONDS_TO_TRAVERSE_TILE=0.5f;
    float timeSinceLastAction=0f;

    public int playerXPosition=11;
    public int playerYPosition=11;

    public Vector3 BOARD_SCALE = new Vector3(1f, 1f, 1f);
    public Vector3 SELECTED_SCALE = new Vector3(1.2f, 1.2f, 1.2f);

    public Sprite cardBackSprite;
    public Sprite cardDepletedSprite;
    public Sprite cardFrontNoEffects;
    public Sprite cardFrontWithEffects;
    public Sprite bloodIcon;
    public Sprite sanityIcon;
    public Sprite scoreIcon;

    public TextMeshProUGUI bloodTXT;
    public TextMeshProUGUI bloodlustTXT;
    public TextMeshProUGUI sanityTXT;
    public TextMeshProUGUI scoreTXT;
    

    public float dragSpeed = 500000;
    private Vector3 dragOrigin;

    //Handling movement of player
    public bool gameOver=false;
    public bool lockActions=false;
    public bool playerMoving=false;
    private Vector3 oldPosition;
    private Vector3 newPosition;
    private Card cardAfterMovement;


    //End game controls
    public GameObject winLossOverlay;
    public TextMeshProUGUI winLoseTitleTXT;
    public TextMeshProUGUI winLoseConditionTXT;
    public TextMeshProUGUI winLoseStatsTXT;
    public TextMeshProUGUI winLoseScoreTXT;


    private int statBloodHarvested=0;
    private int statDistanceMoved=0;
    private int statBloodSpentOnMovement=0;
    private int statSanityGained=0;
    private int scoreFromItems=0;
    private int scoreLostFromMovement=0;


    //Camera handling
    private Vector3 cameraVelocity = Vector3.zero;
    public float cameraSmoothTime = 0.1f;
    Camera camera;
    public Camera UIcamera;
    [SerializeField] Vector3 cameraOffset;
    bool manuallyOperatingCamera=false;
    float CAMERA_MANUAL_MOVE_SPEED = 5f;

    //Camera smooth following
    private void LateUpdate() {
        if (!manuallyOperatingCamera) {
            Vector3 targetPosition = playerObject.transform.position+cameraOffset;
            camera.transform.position = Vector3.SmoothDamp(camera.transform.position, targetPosition, ref cameraVelocity, cameraSmoothTime);
        }
    }


 
 
    void Update() {
        if (gameOver) {
            return;
        }

        //Animate the player
        if (playerMoving) {
            timeSinceLastAction+=Time.deltaTime;
            if (timeSinceLastAction<SECONDS_TO_TRAVERSE_TILE) {
                    playerObject.transform.position=oldPosition+((newPosition-oldPosition)*(timeSinceLastAction/SECONDS_TO_TRAVERSE_TILE));
            }
            else {
                playerObject.transform.position=newPosition;
                playerMoving=false;
                lockActions=false;
                playerXPosition = cardAfterMovement.xCoord;
                playerYPosition = cardAfterMovement.yCoord;
                cardAfterMovement.setVisited();
                CalculateVisibleTiles();

                updateUI();
                
                //Speech bubble
                if (cardAfterMovement.textOnGrab!="") {
                    playerSpeechBubble.SetActive(true);
                    playerSpeechTxt.text=cardAfterMovement.textOnGrab;
                }
            }
        }
        else {
            bool moveCamera=false;
            Vector3 cameraDirection = new Vector3(0, 1, 0);
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                moveCamera=true;
                cameraDirection=new Vector3(0, 1, 0);
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                moveCamera=true;
                cameraDirection=new Vector3(0, -1, 0);
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                moveCamera=true;
                cameraDirection=new Vector3(-1, 0, 0);
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                moveCamera=true;
                cameraDirection=new Vector3(1, 0, 0);
            }

            if (moveCamera) {
                manuallyOperatingCamera=true;
                camera.transform.position=camera.transform.position+(cameraDirection*Time.deltaTime*CAMERA_MANUAL_MOVE_SPEED);
            }
        }
    }


    void Start() {
        camera=GameObject.FindWithTag("MainCamera").GetComponent<Camera>();   
        UIcamera.enabled=false;
        UIcamera.enabled=true;
        winLossOverlay.SetActive(false);
        startGame();
    }

    void startGame() {
        score=100;
        sanity=5;
        SANITY_MAX=10;
        blood=10;
        BLOOD_MAX=100;
        visibility_range=1f;
        statBloodHarvested=0;
        statDistanceMoved=0;
        statBloodSpentOnMovement=0;
        statSanityGained=0;
        scoreFromItems=0;
        scoreLostFromMovement=0;
        gameOver=false;

        generateMap();
    }

     //Restart the game
    public void returnToSplash() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void generateMap() {
        int xMin=0;
        int xMax=(MAP_SIZE_FROM_START*2);
        int yMin=xMin;
        int yMax=xMax;
        int cardUpto=0;
        for(int y=yMin; y<=yMax; y++) {
            for(int x=xMin; x<=xMax; x++) {
                CardSO cardSO=getRandomCard(Mathf.Abs((MAP_SIZE_FROM_START+1)-x)+Mathf.Abs((MAP_SIZE_FROM_START+1)-y), false);
                Card thisCard = getCardFromSO(cardSO);
                cardsOnMap.Add(thisCard);
                GameObject cardObject = Instantiate(cardPrefab);
                thisCard.instantiateOnMap(cardObject, cardContainer, x, y, cardUpto);
                cardUpto++;
                
                if (x==MAP_SIZE_FROM_START && y==MAP_SIZE_FROM_START) {
                    playerObject.transform.localPosition=new Vector3(x*CARD_WIDTH_OFFSET, y*CARD_HEIGHT_OFFSET, 0);
                    thisCard.setVisited();
                }
            }
        }
        playerObject.transform.SetAsLastSibling();
        CalculateVisibleTiles();
    }

    //Checks whether the player has won or lost
    void checkGameState() {
        bool lostDueToSanity=false;
        bool won=false;
        if (blood<0) {
            gameOver=true;
        }
        else if (blood>=BLOOD_MAX) {
            gameOver=true;
            won=true;
        }
        else if (sanity<=0) {
            gameOver=true;
            lostDueToSanity=true;
        }

        if (gameOver) {
            winLossOverlay.SetActive(true);
            winLoseTitleTXT.text = "<b><color=#"+(won ?  "0C8C2F" : "9A0707")+">" + (won ? "You win!" : "You lose!")+"</color></b>";
            if (won) {
                winLoseConditionTXT.text="<b><color=#0C8C2F>You got to "+BLOOD_MAX.ToString()+" blood!</color></b>";   
            }
            else {
                winLoseConditionTXT.text="<b><color=#9A0707>Your "+(lostDueToSanity ? "sanity" : "blood")+" dropped to 0.</color></b>";   
            }

            winLoseStatsTXT.text=
            "Blood Harvested: "+statBloodHarvested.ToString()+
            "<br>Distance Moved: "+statDistanceMoved.ToString()+
            "<br>Blood spent on movement: "+statBloodSpentOnMovement.ToString()+
            "<br>Sanity gained: "+statSanityGained.ToString()+
            "<br>Score from events: "+scoreFromItems.ToString()+
            "<br>Score lost from movement: "+scoreLostFromMovement.ToString();

            winLoseScoreTXT.text="Score: "+score.ToString();
        }
    }

    public void updateUI() {
        bloodTXT.text="Blood: "+blood.ToString()+"/"+BLOOD_MAX.ToString();
        bloodlustTXT.text="Bloodthirst: "+bloodThirst;
        sanityTXT.text="Sanity: "+sanity.ToString()+"/"+SANITY_MAX.ToString();
        scoreTXT.text="Score: "+score.ToString();
    }

    public void CalculateVisibleTiles() {
        foreach(Card thisCard in cardsOnMap) {
            if (thisCard.getDistanceFromCoords(playerXPosition, playerYPosition)<=visibility_range) {
                thisCard.setVisible();
            }
        }
    }

    //Gets random cardSOs that's allowed to be generated at a set range from the start
    CardSO getRandomCard(int generationRange, bool mustBeImpassable) {

        //Filter out cards that don't match filters
        List<CardSO> cardsMatchingFilter = new List<CardSO>();
        int probabilitySum=0;
        foreach(CardSO thisCard in availableCards) {
            if (generationRange<thisCard.generationMinRange || generationRange>thisCard.generationMaxRange) {
                continue;
            }
            if (mustBeImpassable ^ thisCard.impassable) { //exclude impassible XOR passible
                continue;
            }
            cardsMatchingFilter.Add(thisCard);
            probabilitySum+=thisCard.cardProbability;
        };

        //Get random card, weighted by card.probability
        int randomNumber=UnityEngine.Random.Range(0, probabilitySum);
        int probabilityMin=0;
        foreach(CardSO thisCard in cardsMatchingFilter) {
            int probabilityMax=thisCard.cardProbability+probabilityMin;
            if (randomNumber>=probabilityMin && randomNumber<probabilityMax) {
    return thisCard;
            }
            probabilityMin=probabilityMax;
        }

        return null;
    }

    //Create a card based on the class attached to the cardSO
    public Card getCardFromSO(CardSO cardSO) {
        Card newCard = (Card)System.Activator.CreateInstance(System.Type.GetType(cardSO.cardClass));
        newCard.init(this, cardSO);
        return newCard;
    }

    public Card getCardFromCoords(int xCoord, int yCoord) {
        int mapSize=((MAP_SIZE_FROM_START*2)+1);
        return cardsOnMap[yCoord*mapSize+xCoord];
    }

    public void modifySanity(int modifier) {
        sanity+=modifier;
        sanity=Mathf.Min(sanity, SANITY_MAX);
        sanity=Mathf.Max(sanity, 0);
        checkGameState();
        updateUI();

        if (modifier>0) {
            statSanityGained+=modifier;
        }
    }

    public void modifyBlood(int modifier) {
        blood+=modifier;
        blood=Mathf.Min(blood, BLOOD_MAX);
        blood=Mathf.Max(blood, 0);
        checkGameState();
        updateUI();

        statBloodHarvested+=modifier;
    }

    public void modifyScore(int modifier, bool fromItem=true) {
        score+=modifier;
        checkGameState();
        updateUI();

        if (fromItem && modifier>0) {
            scoreFromItems+=modifier;
        }
    }

    public void movePlayer(Card cardToMoveTo) {

        //Blood movement cost
        int distanceMoved=cardToMoveTo.distanceFromPlayer(playerXPosition, playerYPosition);
        statDistanceMoved+=distanceMoved;
        int bloodCost = Mathf.Max(distanceMoved-1, 0);
        bloodCost+=bloodThirst;
        blood-=bloodCost;

        statBloodSpentOnMovement+=bloodCost;

        //Modify bloodthirst
        if(cardToMoveTo.resetBloodthirst) {
            bloodThirst=1;
        }
        else {
            bloodThirst=Mathf.Min(bloodThirst+1, BLOODTHIRST_RATE_MAX);
        }

        checkGameState();

        updateUI();

        
        newPosition=cardToMoveTo.cardObject.transform.position;
        oldPosition=playerObject.transform.position;
        timeSinceLastAction=0f;
        cardAfterMovement=cardToMoveTo;
        
        lockActions=true;
        playerMoving=true;
        manuallyOperatingCamera=false;
        playerSpeechBubble.SetActive(false);
        playerSpeechTxt.text="";
    }
}
