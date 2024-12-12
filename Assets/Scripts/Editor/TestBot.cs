using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(mainBot))]
public class TestBot : Editor
{
    public int searchDepthTest = 2;
    public override void OnInspectorGUI()
    {
        mainBot meinBot = (mainBot)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Perft"))
        {
            Debug.Log("Perft");
            float startTime = Time.realtimeSinceStartup;
            meinBot.SearchMoves(meinBot.boardBot.currentPosition, searchDepthTest, 0);
            float endTime = Time.realtimeSinceStartup;
            Debug.Log("Nodes : " + meinBot.nodedOccurs);
            Debug.Log("Perft time: " + (endTime - startTime));
        }
    }
}
