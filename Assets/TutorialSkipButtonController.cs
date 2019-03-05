using Loom.ZombieBattleground;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSkipButtonController : MonoBehaviour {

    private int tutorialID=0;
    public HiddenUI hiddenUI;
    public void jumpTutorialButtonPressed()
    {
        hiddenUI.JumpToTutorial(tutorialID);
    }
    public void setTutorialID(string id)
    {
        tutorialID = int.Parse(id);
    }
}
