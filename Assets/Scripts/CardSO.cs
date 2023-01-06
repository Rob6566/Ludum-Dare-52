using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Card", fileName = "New Card")]
public class CardSO : ScriptableObject {
    public string name;
    public string textOnGrab;
    public Sprite sprite;
    public int cardProbability = 10;
    public int sanityModifier = 0;
    public int bloodModifier = 0;
    public int scoreModifier = 0;
    public bool resetBloodDrain = false;
    public string cardClass = "basicCard";
}
