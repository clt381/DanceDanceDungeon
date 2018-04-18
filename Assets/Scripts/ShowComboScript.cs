using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowComboScript : MonoBehaviour {

    BeatWindowTrigger beatTrigger;
    PlayerController pController;
    Animator myAnim;
    public Sprite[] buttons;        //storing possible sprites
    public Image[] myButtons;      //storing sprites to show on showcombination

    void Start()
    {
        GameObject playerController = GameObject.Find("PlayerController");
        pController = playerController.GetComponent<PlayerController>();
        beatTrigger = GameObject.Find("BeatWindow").GetComponent<BeatWindowTrigger>();
        myAnim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
    }

    // NEEDS ONE UPDATE BUTONS FUNCTION THAT CAN BE CALLED PER ONTRIGGEREXIT

    public void UpdateButtons(int activeNotes, string letter)    //just takes the letter of the note                       //THIS IS CALLED ON ACTIVATED NOTE INCREMENT
    {
        if (activeNotes < 4)               //if there aren't already 4 active notes
        {
            ShowButton(activeNotes, letter);
        }
        else if (activeNotes == 4)
        {
            if (CompareCombination(GetCurrentCombiantion()) == true)              //if the 4 loaded notes are a valid combination
            {
                myButtons[3].sprite = buttons[DetermineNumber(letter)];
                myAnim.SetTrigger("Show_ShowCombination");                      //can be reached from showbutton3               //MAYBE: add an additional combination related to mana here
            }
            else if (CompareCombination(GetCurrentCombiantion()) == false)
            {
                ShowButton(activeNotes, letter);              //update the last button
            }
        }
        else if (activeNotes == 6)          //this is a hardcoded return that means the combo is already full
        {
            if (CompareCombination(GetCurrentCombiantion()) == false)              //if the 4 loaded notes aren't a valid combination, trying to add a new note will redisplay the combination displayed
            {
                ShiftButtons(letter);
            }
            else if (CompareCombination(GetCurrentCombiantion()) == true)
            {
                ShiftButtons(letter);
                myAnim.SetTrigger("Show_ShowCombination");                      //we want to play the showcombination and then return to showidlebefore                    
            }
        }
    }

    string GetCurrentCombiantion()
    {
        string curCombo = string.Empty;
        for (int i = 0; i < beatTrigger.activatedNotes.Count; i++)
        {
            curCombo += beatTrigger.activatedNotes[i];                      //combine activated notes into a string
        }
        return curCombo;
    }

    bool CompareCombination(string _curCombo)           //returns true or false for combination match
    {
        bool comboMatch = false;
        for (int i = 0; i < pController.fencerAbilities.Count; i++)
        {
            if (_curCombo == pController.fencerAbilities[i])                      //if any of the ability strings are a match
            {
                comboMatch = true;
            }
        }
        for (int i = 0; i < pController.maidenAbilities.Count; i++)
        {
            if (_curCombo == pController.maidenAbilities[i])                      //if any of the ability strings are a match
            {
                comboMatch = true;
            }
        }
        return comboMatch;
    }

    public void ShowButton(int buttonImage, string buttonName)                            //updating buttons live; should be called whenever addnote is called THIS WORKS
    {
        if (buttonName == "A")              //actually setting the sprite value before playing animation
            myButtons[buttonImage - 1].sprite = buttons[0];         
        else if (buttonName == "B")
            myButtons[buttonImage - 1].sprite = buttons[1];
        else if (buttonName == "X")
            myButtons[buttonImage - 1].sprite = buttons[2];
        else if (buttonName == "Y")
            myButtons[buttonImage - 1].sprite = buttons[3];

        string buttonTrigger = "Show_Button";
        buttonTrigger += (buttonImage).ToString();
        myAnim.SetTrigger(buttonTrigger);
    }

    public void ShiftButtons(string _letter)
    {
        int newValue = DetermineNumber(_letter);
        Sprite newButton1 = myButtons[1].sprite;
        Sprite newButton2 = myButtons[2].sprite;
        Sprite newButton3 = myButtons[3].sprite;
        Sprite newButton4 = buttons[newValue];
        //assign new values
        myButtons[0].sprite = newButton1;
        myButtons[1].sprite = newButton2;
        myButtons[2].sprite = newButton3;
        myButtons[3].sprite = newButton4;
    }

    public void ClearButtons()                                          //THIS IS CALLED FROM EXIT TRIGGER
    {
        int _activeNotes = beatTrigger.activatedNotes.Count;
        if (_activeNotes < 1)
        {   
            return;                         //if there's zero buttons activated
        }
        else if (_activeNotes >= 1)
        {
            string clearTrigger = "Clear_Button";               //4 clear button animations, one for each possible number of activated notes
            clearTrigger += _activeNotes.ToString();
            myAnim.SetTrigger(clearTrigger);
        }
    }

    public void ResetTriggers()                     //used to return animator to a default idle state; so the animator never gets stuck
    {
        myAnim.ResetTrigger("Show_Button1");
        myAnim.ResetTrigger("Show_Button2");
        myAnim.ResetTrigger("Show_Button3");
        myAnim.ResetTrigger("Show_Button4");

        myAnim.ResetTrigger("Clear_Button1");
        myAnim.ResetTrigger("Clear_Button2");
        myAnim.ResetTrigger("Clear_Button3");
        myAnim.ResetTrigger("Clear_Button4");
    }

    string DetermineLetter(int number)
    {
        if (number == 1)
            return "A";
        else if (number == 2)
            return "B";
        else if (number == 3)
            return "X";
        else if (number == 4)
            return "Y";
        else
            return "NumberNotInRange";
    }

    int DetermineNumber(string letter)
    {
        if (letter == "A")
            return 0;
        else if (letter == "B")
            return 1;
        else if (letter == "X")
            return 2;
        else if (letter == "Y")
            return 3;
        else
            return 4;
    }
}
