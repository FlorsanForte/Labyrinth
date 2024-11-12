using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    private static GameUIManager instance;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Awake(){
        if(instance != null){
            Destroy(this);
            return;
        }
        instance = this;
    }
    public static void SetHealthText(string health){
        instance.healthText.text = health;
    }
}
