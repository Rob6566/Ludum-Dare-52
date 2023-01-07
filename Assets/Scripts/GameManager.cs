using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public GameObject cardContainer;
    public GameObject playerObject;
    public int score=0;
    public int sanity=10;
    public int SANITY_MAX=10;
    public int blood=10;
    public int BLOOD_MAX=100;
    public float visibility_range=5f; //How far we can see
    public int bloodDepletionRate=0;
    public int BLOOD_DEPLETION_RATE_MAX=5;

    public List<CardSO> availableCards = new List<CardSO>();

    public List<Card> cardsOnMap = new List<Card>();

    public int CARD_WIDTH_OFFSET=20;
    public int CARD_HEIGHT_OFFSET=0;
    public int MAP_SIZE_FROM_START=10;

    public int playerXPosition=11;
    public int playerYPosition=11;

    public Vector3 BOARD_SCALE = new Vector3(1f, 1f, 1f);
    public Vector3 SELECTED_SCALE = new Vector3(1.2f, 1.2f, 1.2f);

    public Sprite cardBackSprite;
    public Sprite cardDepletedSprite;
    



    public float dragSpeed = 500000;
    private Vector3 dragOrigin;

    //Camera handling
    private Vector3 cameraVelocity = Vector3.zero;
    public float cameraSmoothTime = 0.1f;
    Camera camera;
    [SerializeField] Vector3 cameraOffset;

    //Camera smooth following
    private void LateUpdate() {
        Vector3 targetPosition = playerObject.transform.position+cameraOffset;
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, targetPosition, ref cameraVelocity, cameraSmoothTime);
    }


 
 
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;


        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);
 
        transform.Translate(move, Space.World);  
    }


    void Start() {
        camera=GameObject.FindWithTag("MainCamera").GetComponent<Camera>();   
        startGame();
    }

    void startGame() {
        score=0;
        sanity=10;
        SANITY_MAX=10;
        blood=10;
        BLOOD_MAX=100;
        visibility_range=1f;

        generateMap();
    }

    void generateMap() {
        int xMin=0;
        int xMax=(MAP_SIZE_FROM_START*2);
        int yMin=xMin;
        int yMax=xMax;
        for(int x=xMin; x<=xMax; x++) {
            for(int y=yMin; y<=yMax; y++) {
                CardSO cardSO=getRandomCard(Mathf.Abs((MAP_SIZE_FROM_START+1)-x)+Mathf.Abs((MAP_SIZE_FROM_START+1)-y), false);
                Card thisCard = getCardFromSO(cardSO);
                cardsOnMap.Add(thisCard);
                GameObject cardObject = Instantiate(cardPrefab);
                thisCard.instantiateOnMap(cardObject, cardContainer, x, y);
                
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

    }

    public void updateUI() {

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
    }

    public void modifyBlood(int modifier) {
        blood+=modifier;
        blood=Mathf.Min(blood, BLOOD_MAX);
        sanity=Mathf.Max(blood, 0);
        checkGameState();
        updateUI();
    }

    public void modifyScore(int modifier) {
        score+=modifier;
        checkGameState();
        updateUI();
    }
}
