using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    //each slot should have static elements (such as anticipation for attacks) and dynamic elements that move with the agent in the slot (such as animated effects, stat change numbers, etc.)
    Leviathan leviathanScript;

    //list future enemytypes and thir scripts here

    public Transform slot1;
    public Transform slot2;
    public Animator slot1UIAnim;
    public Animator slot2UIAnim;
    public Image slot1CountDown;
    public Image slot2CountDown;

    public Sprite[] numberSprites;      //store list of number sprites to assign to countdown image

    [HideInInspector]
    public int targetSlot1 = 1;
    [HideInInspector]
    public int targetSlot2 = 2;
    [HideInInspector]
    public int targetSlot1And2 = 3;

    [HideInInspector]
    public int countDown;               //changes per attack
    [HideInInspector]
    public int countDownThreshold;             //manipulated by attacktype of enemy script

    void Start()
    {
        slot1 = transform.Find("Slot1");
        slot2 = transform.Find("Slot2");
        slot1UIAnim = slot1.transform.Find("UIGraphics").GetComponentInChildren<Animator>();     //getting animator components from 'anticipation' childobjects
        slot2UIAnim = slot2.transform.Find("UIGraphics").GetComponentInChildren<Animator>();
        leviathanScript = GameObject.Find("Leviathan").GetComponent<Leviathan>();
    }

    void Update()
    {
    }

    //to be used in tandem with 'move' ability
    public IEnumerator SwitchDynamicElements()           //should lerp the dynamic elements of each agent ("agent") to each other's transform's, then set their parents to each other
    {
        Transform slot1Agent = slot1.transform.Find("Agent");       //getting the dynamic elements
        Transform slot2Agent = slot2.transform.Find("Agent");

        if (slot1Agent != null && slot2Agent != null)
        {
            Vector2 startPos1 = slot1Agent.position;              //where agent1 starts
            Vector2 endPos1 = slot2Agent.position;                    //where agent1 ends
            Vector2 startPos2 = slot2Agent.position;                      //where agent2 starts
            Vector2 endPos2 = slot1Agent.position;                        //where agent2 ends

            float elapsedTime = 0f;
            float moveTime = 0.3f;
            while (elapsedTime < moveTime)
            {
                slot1Agent.position = Vector2.Lerp(startPos1, endPos1, elapsedTime / moveTime);
                slot2Agent.position = Vector2.Lerp(startPos2, endPos2, elapsedTime / moveTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            //then set their parents after moving
            slot1Agent.SetParent(slot2, true);
            slot1Agent.localPosition = Vector2.zero;      //resets position         (must be localposition to be 0 relative to parent slot)
            slot2Agent.SetParent(slot1, true);
            slot2Agent.localPosition = Vector2.zero;      //resets position
        }
        
    }

    void CountDown(Animator targetAnim)
    {
        //should trigger a pulse animation per call and decrease the int countdown and set corresponding number sprite in countdown childobject
        if (countDown == countDownThreshold)        //IF THE countdownthreshold is several beats away from countdown pulse
        {
            targetAnim.SetTrigger("CountDown_Appear");
        }
        else if (countDown < (countDownThreshold - 1) && countDown > 1)                    //only starts countdown at 5 notes left
        {
            targetAnim.SetTrigger("CountDown_Pulse");
            //slot1CountDown.sprite = numberSprites[countDown - 1];       //-1 accounting for array element order start at 0
        }
        else if (countDown == 1)
        {
            targetAnim.SetTrigger("CountDown_Zero");                //some kind of execution/disappear animation
        }
    }

    public void UpdateCountDown(string _attackType)          //called ontrigger enter
    {
        if (countDown > 0)                                       //countdown is incremented in beatwindowTrigger triggerenter
        {
            if (_attackType == leviathanScript.fangStrike)
            {
                CountDown(slot2UIAnim);
            }
            else if (_attackType == leviathanScript.hellFire)
            {
                CountDown(slot1UIAnim);
                CountDown(slot2UIAnim);
            }
            else if (_attackType == leviathanScript.serpentSong)
            {
                CountDown(slot1UIAnim);
                CountDown(slot2UIAnim);
            }
        }
    }
}
