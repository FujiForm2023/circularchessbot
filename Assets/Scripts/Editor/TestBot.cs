using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(mainBot))]
public class TestBot : Editor
{
    public struct A{
        public int[] abc;
    }
    // public int searchDepthTest = 4;
    public override void OnInspectorGUI()
    {
        mainBot meinBot = (mainBot)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Perft"))
        {
            A a = new A();
            a.abc = new int[4];
            a.abc[0] = 0;
            a.abc[1] = 1;
            a.abc[2] = 2;
            a.abc[3] = 3;

            Debug.Log("" + a.abc[0] + " " + a.abc[1] + " " + a.abc[2] + " " + a.abc[3]);

            A b = changeSomething(a);

            Debug.Log("" + b.abc[0] + " " + b.abc[1] + " " + b.abc[2] + " " + b.abc[3]);
            Debug.Log("" + a.abc[0] + " " + a.abc[1] + " " + a.abc[2] + " " + a.abc[3]);
            // Debug.Log("Perft");
            // float startTime = Time.realtimeSinceStartup;
            // meinBot.SearchMoves(meinBot.boardBot.currentPosition, searchDepthTest, 0);
            // float endTime = Time.realtimeSinceStartup;
            // Debug.Log("Perft time: " + (endTime - startTime));
        }
    }

    public A changeSomething(A a){
        a.abc[0] = 4;
        return a;
    }
}
