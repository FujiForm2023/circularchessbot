using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailButton : MonoBehaviour
{
    public bool adding = true;
    public GameObject text;
    public void ChangeDetail()
    {
        // Board
        BoardVisual boardVisual = GameObject.FindObjectOfType<BoardVisual>();
        if (adding)
        {
            boardVisual.HigherDetail();
        }
        else
        {
            boardVisual.LowerDetail();
        }

        // Button
        Text buttonText = text.GetComponentInChildren<Text>();
        buttonText.text = "Detail Level : " + boardVisual.detailLevel;
    }
}
