using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(mainBot))]
public class TestBot : Editor
{
    struct PointStruct {
        public int X;
        public int Y;

    }

    class PointClass {
        public int X;
        public int Y;

    }
    // public int searchDepthTest = 4;
    public override void OnInspectorGUI()
    {
        mainBot meinBot = (mainBot)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Perft"))
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            PointStruct pointStruct = default;
            for (int i = 0; i < 100_000_000; i++) {
                pointStruct = new PointStruct();
                pointStruct.X = i;
                pointStruct.Y = i + 1;
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log("Struct time: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100_000_000; i++) {
                PointClass pointClass = new PointClass();
                pointClass.X = i;
                pointClass.Y = i + 1;
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log("Class time: " + stopwatch.ElapsedMilliseconds + "ms");
            // Debug.Log("Perft");
            // float startTime = Time.realtimeSinceStartup;
            // meinBot.SearchMoves(meinBot.boardBot.currentPosition, searchDepthTest, 0);
            // float endTime = Time.realtimeSinceStartup;
            // Debug.Log("Perft time: " + (endTime - startTime));
        }
    }
}
