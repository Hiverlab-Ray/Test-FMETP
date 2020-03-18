using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MicEncoder))]
[CanEditMultipleObjects]
public class MicEncoder_Editor : Editor
{
    private MicEncoder MEncoder;

    SerializedProperty StreamGameSoundProp;
    SerializedProperty OutputSampleRateProp;
    SerializedProperty OutputChannelsProp;


    SerializedProperty StreamFPSProp;


    SerializedProperty OnDataByteReadyEventProp;


    SerializedProperty labelProp;
    SerializedProperty dataLengthProp;

    void OnEnable()
    {

        StreamGameSoundProp = serializedObject.FindProperty("StreamGameSound");
        OutputSampleRateProp = serializedObject.FindProperty("OutputSampleRate");
        OutputChannelsProp = serializedObject.FindProperty("OutputChannels");


        StreamFPSProp = serializedObject.FindProperty("StreamFPS");


        OnDataByteReadyEventProp = serializedObject.FindProperty("OnDataByteReadyEvent");

        labelProp = serializedObject.FindProperty("label");
        dataLengthProp = serializedObject.FindProperty("dataLength");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (MEncoder == null) MEncoder = (MicEncoder)target;

        serializedObject.Update();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Capture");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamGameSoundProp, new GUIContent("Stream Game Sound"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Audio Info");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OutputChannelsProp, new GUIContent("Output Channels"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OutputSampleRateProp, new GUIContent("Output Sample Rate"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Encoded");

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(StreamFPSProp, new GUIContent("StreamFPS"));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnDataByteReadyEventProp, new GUIContent("OnDataByteReadyEvent"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Pair Encoder & Decoder ");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(labelProp, new GUIContent("label"));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(dataLengthProp, new GUIContent("Encoded Size(byte)"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
