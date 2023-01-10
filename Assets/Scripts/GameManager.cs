using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

public enum CardSuit {hearts, diamonds, spades, clubs};


[System.Serializable]
public class HighScore {
    public string name;
    public string score;
    public HighScore(string _name, string _score) {
        name = _name;
        score = _score;
    }
}

[System.Serializable]
public class SuitedCard {
    public int cardNumber;
    public CardSuit cardSuit;
}

[System.Serializable]
public class HighScoreData
{
    public List<HighScore> scores;
}

public class GameManager : MonoBehaviour
{
    //AUDIO
    public AudioManager audioManager;
    public int MUSIC_INTRO=0;
    public int MUSIC_DEFEAT=1;
    public int MUSIC_GAMEPLAY_MUSIC=2;
    public int MUSIC_WIN=3;
    public int MUSIC_GAMEPLAY_AMBIENCE=4;
    public int MUSIC_DEATH=5;
    public int MUSIC_POSTDEATH=6;
    
    public int SOUND_SANITY=0;
    public int SOUND_FIGHT_1=1;
    public int SOUND_FIGHT_2=2;
    public int SOUND_FIGHT_3=3;
    public int SOUND_FIGHT_4=4;
    public int SOUND_SCORE=5;
    public int SOUND_POKER=6;

    
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
    public Sprite cardFrontNoEffectsBasic;
    public Sprite cardFrontWithEffectsBasic;

    public Sprite bloodIcon;
    public Sprite sanityIcon;
    public Sprite scoreIcon;

    public TextMeshProUGUI bloodTXT;
    public TextMeshProUGUI bloodlustTXT;
    public TextMeshProUGUI sanityTXT;
    public TextMeshProUGUI scoreTXT;

    public TextMeshProUGUI scoreObjectiveTXT;
    

    public float dragSpeed = 50;
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
    private int statMoves=0;
    private int scoreFromCards=0;
    private int scoreFromItems=0;
    private int scoreLostFromMovement=0;


    //Intro screen controls
    bool runningIntro=false;
    public Image introImage;
    public GameObject introOverlay;
    public GameObject introButton;
    public TextMeshProUGUI introText;
    int introStep=0;
    bool introFading=false;
    bool introFadeDidSwitch=false;
    float timeSinceIntroFadeStart=0f;
    float INTRO_FADE_DURATION=1f;
    float INTRO_FADE_WAIT_DURATION=0.5f;
    public List<Sprite> introImages = new List<Sprite>();


    //Camera handling
    private Vector3 cameraVelocity = Vector3.zero;
    public float cameraSmoothTime = 0.1f;
    Camera camera;
    public Camera UIcamera;
    [SerializeField] Vector3 cameraOffset;
    bool manuallyOperatingCamera=false;
    float CAMERA_MANUAL_MOVE_SPEED = 5f;

    //Canvasses
    public List<GameObject> canvasses = new List<GameObject>();

    //High Scores
    public const string SERVER_URL ="https://ldjam51.rob6566.click/LudumDare-server/";
    public const string SCORES_URL="scores.php?game_id=ludum52-postjam";
    public const string ADD_SCORE_URL="add_score.php";
    public GameObject scorePrefab;
    public GameObject scoreHolder;
    string playerName;
    public TMP_InputField playerNameInput;
    public GameObject invalidName;

    //Blood cost overlay
    public GameObject bloodCostOverlay;
    public TextMeshProUGUI bloodCostTXT;


    //Since cards aren't fully featured, we allow users to turn them off 
    public bool complexMode = true;

    //Card Suits
    public List<Sprite> cardSuitSprites = new List<Sprite>();
    public List<SuitedCard> recentCards = new List<SuitedCard>();
    public GameObject cardSuitPrefab;
    public GameObject cardSuitContainer;
    public GameObject starFlush;
    public GameObject starStraight;
    public GameObject starSet;
    public TextMeshProUGUI txtFlush;
    public TextMeshProUGUI txtStraight;
    public TextMeshProUGUI txtSet;
    private int pendingFlushPoints;
    private int pendingStraightPoints;
    private int pendingSetPoints;
    private List<int> flushScores=new List<int>{0,0,0,5,10,20,40,80,160};
    private List<int> straightScores=new List<int>{0,0,0,10,20,40,80,160,320};
    private List<int> setScores=new List<int>{0,0,5,10,20,40,80,160,320};

    //public Toggle complexModeToggle;
    public GameObject complexModeUIContainer;

    //Fading animations
    public GameObject textAnimationPrefab;
    public int recentSanityGain=0;



    //Camera smooth following
    private void LateUpdate() {
        if (!manuallyOperatingCamera) {
            Vector3 targetPosition = playerObject.transform.position+cameraOffset;
            camera.transform.position = Vector3.SmoothDamp(camera.transform.position, targetPosition, ref cameraVelocity, cameraSmoothTime);
        }
    }


 
 
    void Update() {
        runIntroFade();

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

                if (recentSanityGain!=0) {
                    GameObject textPopup = UnityEngine.Object.Instantiate(textAnimationPrefab);   
                    textPopup.transform.SetParent(cardContainer.transform);
                    
                    FadingAnimationHandler textHandler = textPopup.GetComponent<FadingAnimationHandler>();
                    textHandler.init(4f, playerObject.transform.position,  recentSanityGain.ToString()+" sanity"); 
                    recentSanityGain=0;
                }
                
                
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


            //Mouse scroll to zoom camera
            if (Input.GetAxis("Mouse ScrollWheel")!=0) {
                float minFov = 30f;
                float maxFov = 90f;
                float sensitivity = -10f;
                
                float fov = camera.fieldOfView;
                fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity;
                fov = Mathf.Clamp(fov, minFov, maxFov);
                camera.fieldOfView = fov;
                Debug.Log("Mouse ScrollWheel = "+Input.GetAxis("Mouse ScrollWheel"));
                manuallyOperatingCamera=true;
            }

            //Mouse drag to move camera
            if (Input.GetMouseButtonDown(1)) {
                dragOrigin = Input.mousePosition;
                return;
            }
    
            if (!Input.GetMouseButton(1)) return;
            Vector3 pos = camera.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(pos.x * -dragSpeed, pos.y * -dragSpeed, 0);
    
            camera.transform.Translate(move, Space.World);  
            manuallyOperatingCamera=true;
            
        }
    }

    //Auto-runs when game starts
    void Start() {
        camera=GameObject.FindWithTag("MainCamera").GetComponent<Camera>();   
        camera.backgroundColor=Color.white;
        UIcamera.enabled=false;
        UIcamera.enabled=true;
        winLossOverlay.SetActive(false);
        invalidName.SetActive(false);

        setCanvasStatus("SplashCanvas", true);

        GameObject audioContainer = GameObject.FindWithTag("AudioManager");
        audioManager = audioContainer.GetComponentInChildren<AudioManager>();

        audioManager.fadeInMusic(MUSIC_INTRO, 0, 1f);

        StartCoroutine(LoadScores());
    }

    public void Intro() {
        if (!validateName()) {
            return;
        }

        introStep=0;
        runningIntro=true;
        setCanvasStatus("IntroCanvas", true);
        introOverlay.SetActive(false);
        introButton.SetActive(true);
        introFading=false;
    }

    public void ProgressIntro() {
        introStep++;

        if (introStep==5) {
            startGame();
            return;
        }

        introFading=true;
        introFadeDidSwitch=false;
        timeSinceIntroFadeStart=0f;
        introButton.SetActive(false);

        if (introStep<2) {
            introOverlay.GetComponent<Image>().color = Color.white;
        }
        else if(introStep==3) {
            introOverlay.GetComponent<Image>().color = Color.red;
        }
        else if(introStep==4) {
            introOverlay.GetComponent<Image>().color = Color.black;
        }
    }

    //Runs on every frame to fade the intro scenes in and out
    void runIntroFade() {
        if (!runningIntro || !introFading) {
            return;
        }

        timeSinceIntroFadeStart+=Time.deltaTime;

        /*float INTRO_FADE_DURATION=1f;
          float INTRO_FADE_WAIT_DURATION=1f;*/


        Image introOverlayImage = introOverlay.GetComponent<Image>();
        if (timeSinceIntroFadeStart<INTRO_FADE_DURATION) {
            introOverlay.SetActive(true);
            Color32 introColour=introOverlayImage.color;
            introColour.a=(byte)(255*(timeSinceIntroFadeStart/INTRO_FADE_DURATION));
            introOverlayImage.color=introColour;
        }
        else if (!introFadeDidSwitch) {
            //Swap over the page we're viewing

            introImage.sprite=introImages[introStep];
            if (introStep==1) {
                introText.text="One day, after a big harvest, you partake in some of your mushrooms.<br><br>Arriving home befuddled, you forget to hang up your garlic ward.";
            }
            else if (introStep==2) {
                audioManager.fadeInMusic(MUSIC_DEATH, 1, 1f);
                introText.text="In your slumber, you're bitten by a vampire!<br><br>Your eyes glaze with bloodlust, your fangs lengthen, your skin starts to sparkle (??).";
                camera.backgroundColor=Color.red;
            }
            else if (introStep==3) {
                audioManager.fadeInMusic(MUSIC_POSTDEATH, 2, 1f);
                introText.text="<color=#ffffff>You know you cannot rest until you've harvested enough blood to quench your burning thirst.</color>";
                camera.backgroundColor=Color.black;
            }
            else if (introStep==4) {
                introText.text="<color=#ffffff>Click tiles to move. WASD overrides the camera.<br><br>Get to 100 Blood.<br><br>Keep Sanity and Blood above 0.</color>";
                camera.backgroundColor=Color.black;
                introButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text="Start";
            }


            introFadeDidSwitch=true;
        }

        float timeSinceFadeOutStart=timeSinceIntroFadeStart-INTRO_FADE_DURATION-INTRO_FADE_WAIT_DURATION;
        if (timeSinceFadeOutStart>0) {
            if (timeSinceFadeOutStart>INTRO_FADE_DURATION) {
                introFading=false;
                introButton.SetActive(true);
                introOverlay.SetActive(false);
            }
            else {
                Color32 introColour=introOverlayImage.color;
                introColour.a=(byte)(255*(1f-(timeSinceFadeOutStart/INTRO_FADE_DURATION)));
                introOverlayImage.color=introColour;
            }
        }

    }


    public void startGame() {
        if (!validateName()) {
            return;
        }

        audioManager.fadeInMusic(MUSIC_GAMEPLAY_MUSIC, 4, 1f);

        complexMode=true;//complexModeToggle.isOn;
        if (!complexMode) {
            cardFrontWithEffects=cardFrontWithEffectsBasic;
            cardFrontNoEffects=cardFrontNoEffectsBasic;
            scoreObjectiveTXT.text="Optional:  Increase score by winning in less moves and picking up scoring items";
        }
        complexModeUIContainer.SetActive(complexMode);


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
        statMoves=0;
        scoreFromItems=0;
        scoreLostFromMovement=0;
        gameOver=false;
        runningIntro=false;
        introFading=false;
        manuallyOperatingCamera=false;
        
        camera.backgroundColor=Color.black;

        bloodCostOverlay.SetActive(false);

        playerSpeechBubble.SetActive(false);
        playerSpeechTxt.text="";

        List<SuitedCard> recentCards = new List<SuitedCard>();

        setCanvasStatus("GameCanvas", true);
        setCanvasStatus("ControlPanelCanvas", true, false);

        starFlush.SetActive(false);
        starStraight.SetActive(false);
        starSet.SetActive(false);
        txtFlush.text="";
        txtStraight.text="";
        txtSet.text="";

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
        updateUI();
    }

    //Checks whether the player has won or lost
    void checkGameState() {
        if (gameOver) {
            return;
        }
        
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
            calculateCardCombos(true); //Cash out any card combos
            updateUI();
            winLossOverlay.SetActive(true);
            audioManager.fadeInMusic(won ? MUSIC_WIN  : MUSIC_DEFEAT, 1, 1f);
            winLoseTitleTXT.text = "<b><color=#"+(won ?  "0C8C2F" : "9A0707")+">" + (won ? "You win!" : "You lose!")+"</color></b>";
            if (won) {
                winLoseConditionTXT.text="<b><color=#0C8C2F>You got to "+BLOOD_MAX.ToString()+" blood!</color></b>";   
            }
            else {
                winLoseConditionTXT.text="<b><color=#9A0707>Your "+(lostDueToSanity ? "sanity" : "blood")+" dropped to 0.</color></b>";   
            }

            winLoseStatsTXT.text=
            "Blood Harvested: "+statBloodHarvested.ToString()+
            "<br>Actions Taken: "+statMoves.ToString()+
            "<br>Distance Moved: "+statDistanceMoved.ToString()+
            "<br>Blood spent on movement: "+statBloodSpentOnMovement.ToString()+
            "<br>Sanity gained: "+statSanityGained.ToString()+
            "<br>Score from events: "+scoreFromItems.ToString()+
            "<br>Score lost from movement: "+scoreLostFromMovement.ToString();

            winLoseScoreTXT.text="Score: "+score.ToString();

            StartCoroutine(SaveScore());
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
            thisCard.setLoseOverlay();
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
        newCard.init(this, cardSO, getRandomSuitedCard());
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

        recentSanityGain=modifier;
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


    public int getMoveCost(Card cardToMoveTo, bool updateStat=false, bool showMoveCostOverlay=false) {
        int distanceMoved=cardToMoveTo.distanceFromPlayer(playerXPosition, playerYPosition);
        if (updateStat) {
            statDistanceMoved+=distanceMoved;
        }
        
        int bloodCost = Mathf.Max((distanceMoved-1)*(distanceMoved-1), 0);
        int distanceCost=bloodCost;
        bloodCost+=bloodThirst;

        if (showMoveCostOverlay) {
            bloodCostTXT.text="Blood cost - "+bloodCost.ToString()+" ("+distanceCost.ToString()+" Distance, "+bloodThirst.ToString()+" Bloodthirst)";
        }

        return bloodCost;
    }

    public void movePlayer(Card cardToMoveTo) {

        //Blood movement cost
        int bloodCost=getMoveCost(cardToMoveTo, true, false);
        blood-=bloodCost;

        statBloodSpentOnMovement+=bloodCost;
        statMoves++;

        int scoreToLose=((int)statMoves / 10) + 1;
        scoreLostFromMovement+=scoreToLose;
        score-=scoreToLose;

        //Modify bloodthirst
        if(cardToMoveTo.resetBloodthirst) {
            bloodThirst=1;
        }
        else {
            bloodThirst=Mathf.Min(bloodThirst+1, BLOODTHIRST_RATE_MAX);
        }
        
        newPosition=cardToMoveTo.cardObject.transform.position;
        oldPosition=playerObject.transform.position;
        timeSinceLastAction=0f;
        cardAfterMovement=cardToMoveTo;
        
        lockActions=true;
        playerMoving=true;
        manuallyOperatingCamera=false;
        playerSpeechBubble.SetActive(false);
        playerSpeechTxt.text="";


        //Display Suited Card if we're in complex mode
        if (complexMode) {
            
            GameObject cardSuitObject = Instantiate(cardSuitPrefab);
            cardSuitObject.transform.SetParent(cardSuitContainer.transform);
            cardSuitObject.transform.localScale=new Vector3(1f, 1f, 1f);
            cardSuitObject.transform.SetAsFirstSibling();
            
            GameObject cardSuitIMG=cardSuitObject.transform.GetChild(0).gameObject;
            TextMeshProUGUI cardSuitTXT=cardSuitObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

            cardSuitIMG.GetComponent<Image>().sprite=cardSuitSprites[(int)cardToMoveTo.suitedCard.cardSuit];
            cardSuitTXT.text=cardToMoveTo.suitedCard.cardNumber.ToString();
            cardSuitTXT.color=((int)cardToMoveTo.suitedCard.cardSuit<2) ? Color.black : Color.red;

            //Destroy our 9th suited card if we have too many
            if (cardSuitContainer.transform.childCount>8) {
                Destroy(cardSuitContainer.transform.GetChild(8).gameObject);
            }

            //Add the new suited card to our list
            recentCards.Insert(0, cardToMoveTo.suitedCard);
            if (recentCards.Count>8) {
                recentCards.RemoveAt(8);
            }

            
            
            calculateCardCombos(false);

        }

        updateUI();

        checkGameState();
    }

    void calculateCardCombos(bool cashOut=false) {
        //Calculate flushes
            CardSuit currentSuit=recentCards[0].cardSuit;
            int flushCount=0;
            foreach(SuitedCard thisCard in recentCards) {
                if (thisCard.cardSuit==currentSuit) {
                    flushCount++;
                }
                else {
                    break;
                }
            }

            int flushPoints=flushScores[flushCount];
            if ((flushPoints==0 || cashOut) && pendingFlushPoints>0) {
                score+=pendingFlushPoints;
                scoreFromCards+=pendingFlushPoints;
                pendingFlushPoints=0;
            }
            else if (flushPoints>0) {
                pendingFlushPoints=flushPoints;
            }

            starFlush.SetActive(pendingFlushPoints>0);
            txtFlush.text=(pendingFlushPoints>0 ? "Flush ("+flushCount.ToString()+" cards) - "+flushPoints.ToString()+" points" : "");


        
        
            //Calculate sets
            int currentNumber=recentCards[0].cardNumber;
            int setCount=0;
            foreach(SuitedCard thisCard in recentCards) {
                if (thisCard.cardNumber==currentNumber) {
                    setCount++;
                }
                else {
                    break;
                }
            }

            int setPoints=setScores[setCount];
            if ((setPoints==0 || cashOut) && pendingSetPoints>0) {
                score+=pendingSetPoints;
                scoreFromCards+=pendingSetPoints;
                pendingSetPoints=0;
            }
            else if (setPoints>0) {
                pendingSetPoints=setPoints;
            }

            starSet.SetActive(pendingSetPoints>0);
            txtSet.text=(pendingSetPoints>0 ? "Set ("+setCount.ToString()+" "+currentNumber.ToString()+"s) - "+setPoints.ToString()+" points" : "");


            
            
            //Calculate straights
            if (recentCards.Count>1) {
                int currentDirection=recentCards[1].cardNumber-recentCards[0].cardNumber; //5 6 7  - currentDirection=1
                currentNumber=recentCards[0].cardNumber-currentDirection;
                int straightCount=0;
                if (currentDirection==1 || currentDirection==-1) {
                    foreach(SuitedCard thisCard in recentCards) {
                        if (thisCard.cardNumber==currentNumber+currentDirection) {
                            straightCount++;
                            currentNumber+=currentDirection;
                        }
                        else {
                            break;
                        }
                    }
                }
                

                int straightPoints=straightScores[straightCount];
                if ((straightPoints==0 || cashOut) && pendingStraightPoints>0) {
                    score+=pendingStraightPoints;
                    scoreFromCards+=pendingStraightPoints;
                    pendingStraightPoints=0;
                }
                else if (straightPoints>0) {
                    pendingStraightPoints=straightPoints;
                }

                starStraight.SetActive(pendingStraightPoints>0);
                txtStraight.text=(pendingStraightPoints>0 ? "Straight ("+straightCount.ToString()+") - "+straightPoints.ToString()+" points" : "");
            }

    }

    void setCanvasStatus(string canvasTag, bool newState, bool hideOthers=true) {
        foreach(GameObject thisCanvas in canvasses) {
            if (thisCanvas.tag==canvasTag) {
                thisCanvas.SetActive(newState);
            }
            else if (hideOthers) {
                thisCanvas.SetActive(false);
            }
        }
    }



    IEnumerator SaveScore() {
        
        WWWForm form = new WWWForm();
        form.AddField("user_name", playerName);
        form.AddField("score", score);
        form.AddField("game_id", "ludum52-postjam");

        using (UnityWebRequest webRequest = UnityWebRequest.Post(SERVER_URL+ADD_SCORE_URL, form)) {
            // Request and wait for the desired page.
            //webRequest.SetRequestHeader("secretkey", "12345");
            yield return webRequest.SendWebRequest();

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    break;
                case UnityWebRequest.Result.Success:
                    break;
            }
        }
    }

    IEnumerator LoadScores() {
        
         //webRequest= new UnityWebRequest();
        string uri=SERVER_URL+SCORES_URL;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            //webRequest.SetRequestHeader("secretkey", "12345");
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    
                    string jsonString=webRequest.downloadHandler.text;
                    var data = JsonUtility.FromJson<HighScoreData>(jsonString);
                    int scoreUpto=0;
                    foreach (HighScore thisScore in data.scores) {
                        if (scoreUpto>10) {
                            break;
                        }

                        GameObject gameObject = Instantiate(scorePrefab);

                        gameObject.transform.SetParent(scoreHolder.transform);      
                        gameObject.transform.localPosition=new Vector3(-25, 35-(50*scoreUpto), 0);
                        gameObject.transform.localScale=new Vector3(1f, 1f, 1f);
                        TextMeshProUGUI txtName = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                            txtName.text=thisScore.name;
                        TextMeshProUGUI txtScore = gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
                            txtScore.text=thisScore.score;
                        scoreUpto++;
                    }
                    break;
            }
        }
    }


    public bool validateName() {

        string tempPlayerName=playerNameInput.text;
        tempPlayerName=tempPlayerName.Trim();
        if (tempPlayerName.Length>30 || tempPlayerName.Length<2) {
            invalidName.SetActive(true);
            return false;
        }
        else if (!Regex.IsMatch(tempPlayerName, "^[a-zA-Z0-9 ]*$")) {
            invalidName.SetActive(true);
            return false;
        }
        else {
            playerName=tempPlayerName;
            return true;
        }
    }

    public SuitedCard getRandomSuitedCard() {
        CardSuit cardSuit = (CardSuit)Random.Range(0, 3);
        int cardNumber = Random.Range(2,9);

        SuitedCard returnCard = new SuitedCard();
        returnCard.cardSuit=cardSuit;
        returnCard.cardNumber=cardNumber;
        return returnCard;
    }


}
