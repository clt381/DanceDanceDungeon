using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EffectPrefabScript : MonoBehaviour {

    //this script will have all the necessary effects prefab functionality and anim events

    Fencer fencer;
    ShieldMaiden maiden;
    TextMeshProUGUI numbers;

	void Start () {
        fencer = GameObject.Find("Fencer").GetComponent<Fencer>();
        maiden = GameObject.Find("ShieldMaiden").GetComponent<ShieldMaiden>();
        numbers = transform.Find("Number").GetComponent<TextMeshProUGUI>();
	}

    public void _SetNumberVariable()
    {
        //print(numbers.text);                    //this is returning a null reference...
        //numbers.text = numbers.text;            //the variable itself already has the correct value; maybe setting to self will work?
    }
	
}
