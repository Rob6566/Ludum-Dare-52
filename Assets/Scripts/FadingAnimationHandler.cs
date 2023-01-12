using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadingAnimationHandler : MonoBehaviour
{
    Vector3 origin;
    Vector3 destination;
    float travelTime;
    float timeSpent;


    public void init(float newTravelTime, Vector3 newOrigin, string text, Sprite sprite, int numberOfOtherTexts, float textScale) {
        origin=newOrigin+new Vector3(0f, .5f*(numberOfOtherTexts+1), 0);
        travelTime=newTravelTime;
        timeSpent=0f;
        gameObject.transform.localScale=new Vector3(1f, 1f, 1f);
        destination=origin+new Vector3(0,1,0);
        gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text=text;
        gameObject.transform.GetChild(0).localScale=new Vector3(textScale, textScale, textScale);
        gameObject.transform.GetChild(1).gameObject.GetComponent<Image>().sprite=sprite;
        gameObject.transform.position=origin;
    }

    void Update() {
        if (destination==null || origin==null) {
            return;
        }
        
        timeSpent+=Time.deltaTime;

        //Hover in starting position for half our time
        if (timeSpent<(travelTime/2)) {
            gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color=new Color32(0,0,0,255);
            return;
        }

        float reducedTravelTime=travelTime-(travelTime/2);
        float reducedTimeSpent=timeSpent-(travelTime/2);

        gameObject.transform.position=origin+((destination-origin)*(reducedTimeSpent/reducedTravelTime));
        gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color=new Color32(0,0,0,(byte)Mathf.Floor(255-(255*(reducedTimeSpent/reducedTravelTime))));
        gameObject.GetComponent<Image>().color=new Color32(255,255,255,(byte)Mathf.Floor(255-(255*(reducedTimeSpent/reducedTravelTime))));
        gameObject.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(255,255,255,(byte)Mathf.Floor(255-(255*(reducedTimeSpent/reducedTravelTime))));

        if (timeSpent>=travelTime) {
            Destroy(gameObject);
        }
    }
}
