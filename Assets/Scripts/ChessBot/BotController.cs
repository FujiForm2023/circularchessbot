using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    public mainBot mainBot1;
    public mainBot mainBot2;
    public mainBot? whoIsMoving;
    public BoardBot boardBot;
    public bool allowedBotMove = false;
    public float interval = 1f; // second
    public float startTime;
    public bool inSearch = false;
    public float startDelay = 5f;
    public bool startMoving = false;

    void Start(){
        startTime = Time.realtimeSinceStartup;
    }

    void Update(){
        if (!startMoving && (Time.realtimeSinceStartup - startTime > startDelay)){
            whoIsMoving = mainBot1;
            startMoving = true;
        }
        if (!inSearch && Time.realtimeSinceStartup - startTime > interval && whoIsMoving != null){
            whoIsMoving.botControllerAllowed = allowedBotMove;
            inSearch = true;
            startTime = Time.realtimeSinceStartup;
        }
        if (whoIsMoving != null)
        {
            if (inSearch && startMoving && boardBot.currentPosition.gameStatus == 0 && !whoIsMoving.botControllerAllowed)
            {
                inSearch = false;
                if (whoIsMoving == mainBot1){
                    whoIsMoving = mainBot2;
                }
                else {
                    whoIsMoving = mainBot1;
                }
                startTime = Time.realtimeSinceStartup;
            }
        }
    }
}
