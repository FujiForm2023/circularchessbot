using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text; 

public class UCI : MonoBehaviour
{
    private BoardBot boardBot;
    private BoardVisual boardVisual;
    private byte uciStatus = 0;
    public GameObject? UCIStatus;
    enum uciStatusEnum : byte
    {
        notready = 0,
        uciok = 1,
        isready = 2,
        readyok = 3
    }

    void Awake(){
        boardBot = GetComponent<BoardBot>();
        boardVisual = GetComponent<BoardVisual>();
        if (UCIStatus != null){
            Text text = UCIStatus?.GetComponent<Text>();
            text.text = "UCI OK : Standby";
            uciStatus = 1;
        }
    }

    public void SendUCICommand(string uciArg)
    {
        Debug.Log("pass");
        if (UCIStatus == null || uciStatus == 0){
            Debug.LogWarning("UCI is not initialized");
            return;
        }
        StringBuilder stringBuilder = new StringBuilder();
        string[] args = uciArg.Split(' ');
        switch (args[0]){
            case "position":
                foreach (string arg in args){
                    if (arg != "position"){
                        stringBuilder.Append(arg);
                        stringBuilder.Append(' ');
                    }
                }
                boardBot.currentPosition.CircularChessLoader(stringBuilder.ToString());
                boardVisual.ResetChildren();
                boardVisual.RedrawBoard();
                break;
            case "quit":
                uciStatus = 0;
                break;
            case "stop":
                break;
            default:
                Debug.LogWarning("No command named " + args[0]);
                break;
        }
    }
}
