using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EffectsManager : MonoBehaviour {

    PlayerController pController;

    //should take values on hit animations or cast animations and then activate animator effects relative to slot
    public Transform slot1;
    public Transform slot2;
    public Transform slot3;

    public GameObject effectsPrefab;

	// Use this for initialization
	void Start () {
        pController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ShowDamage(float amount, int slotNum)        //called from animevents helper scripts
    {
        Transform location = transform;

        if (slotNum == 1)
        {
            Transform[] childTransforms = GameObject.FindWithTag("Slot1UI").GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                if (child.name == "DamageEffect")
                {
                    location = child;
                }
            }
        }
        else if (slotNum == 2)
        {
            Transform[] childTransforms = GameObject.FindWithTag("Slot2UI").GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                if (child.name == "DamageEffect")
                {
                    location = child;
                }
            }
        }
        else if (slotNum == 3)
        {
            Transform[] childTransforms = GameObject.FindWithTag("Slot3UI").GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                if (child.name == "DamageEffect")
                {
                    location = child;
                }
            }
        }       

        GameObject numEffectClone = Instantiate(effectsPrefab, location.position, location.rotation, location.transform);

        Animator effectAnim = numEffectClone.GetComponent<Animator>();
        TextMeshProUGUI textNumber = numEffectClone.transform.Find("Number").GetComponent<TextMeshProUGUI>();

        textNumber.text = amount.ToString();


        if (amount < 0)
        {
            if (slotNum == 3)
            {
                effectAnim.SetTrigger("Number_Damage_Play");
            }
            else if (slotNum == 1 || slotNum == 2)
            {
                effectAnim.SetTrigger("Number_Damage_Play");
            }
        }
        else if (amount > 0)
        {
            effectAnim.SetTrigger("Number_Heal_Play");
        }
        Destroy(numEffectClone, 1.5f);                  //destroy the clone after 1.5 seconds
    }
}
