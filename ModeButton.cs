using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeButton : MonoBehaviour
{
    public void SwitchMode()
    {
        // Board
        BoardVisual boardVisual = GameObject.FindObjectOfType<BoardVisual>();
        boardVisual.SwitchMode();

        // Button
        Text buttonText = this.GetComponentInChildren<Text>();
        if (boardVisual.circularMode)
        {
            buttonText.text = "Mode : Circular";
        }
        else
        {
            buttonText.text = "Mode : Flatten";
        }
    }
}
