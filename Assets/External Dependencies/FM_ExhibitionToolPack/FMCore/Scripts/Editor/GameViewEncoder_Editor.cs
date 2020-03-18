using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameViewEncoder))]
[CanEditMultipleObjects]
public class GameViewEncoder_Editor : Editor
{
    private GameViewEncoder GVEncoder;

    SerializedProperty CaptureModeProp;

    SerializedProperty ResizeProp;

    SerializedProperty MainCamProp;
    SerializedProperty RenderCamProp;
    SerializedProperty ResolutionProp;
    SerializedProperty MatchScreenAspectProp;

    SerializedProperty QualityProp;
    SerializedProperty StreamFPSProp;
    SerializedProperty CapturedTextureProp;
    SerializedProperty OnDataByteReadyEventProp;

    SerializedProperty labelProp;
    SerializedProperty dataLengthProp;

    void OnEnable()
    {
        CaptureModeProp = serializedObject.FindProperty("CaptureMode");

        ResizeProp = serializedObject.FindProperty("Resize");

        MainCamProp = serializedObject.FindProperty("MainCam");
        RenderCamProp = serializedObject.FindProperty("RenderCam");
        ResolutionProp = serializedObject.FindProperty("Resolution");
        MatchScreenAspectProp = serializedObject.FindProperty("MatchScreenAspect");

        QualityProp = serializedObject.FindProperty("Quality");
        StreamFPSProp = serializedObject.FindProperty("StreamFPS");
        CapturedTextureProp = serializedObject.FindProperty("CapturedTexture");
        OnDataByteReadyEventProp = serializedObject.FindProperty("OnDataByteReadyEvent");

        labelProp = serializedObject.FindProperty("label");
        dataLengthProp = serializedObject.FindProperty("dataLength");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (GVEncoder == null) GVEncoder = (GameViewEncoder)target;

        serializedObject.Update();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Mode");

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(CaptureModeProp, new GUIContent("Capture Mode"));
            GUILayout.EndHorizontal();

            if (GVEncoder.CaptureMode == GameViewCaptureMode.MainCam)
            {
                if(GVEncoder.MainCam == null)
                {
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.GetComponent<Camera>();
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.AddComponent<Camera>();
                }
                else
                {
                    if (GVEncoder.MainCam != GVEncoder.gameObject.GetComponent<Camera>()) GVEncoder.MainCam = null;

                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.GetComponent<Camera>();
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.AddComponent<Camera>();
                }
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("capture camera with screen aspect", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (GVEncoder.CaptureMode == GameViewCaptureMode.RenderCam)
            {
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("render texture with free aspect", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (GVEncoder.CaptureMode == GameViewCaptureMode.FullScreen)
            {
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("capture full screen with UI Canvas", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }


        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Settings");
            GUILayout.BeginVertical("box");
            {
                if (GVEncoder.CaptureMode == GameViewCaptureMode.MainCam)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MainCamProp, new GUIContent("MainCam"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResizeProp, new GUIContent("Resize"));
                    GUILayout.EndHorizontal();
                }
                if (GVEncoder.CaptureMode == GameViewCaptureMode.RenderCam)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(RenderCamProp, new GUIContent("RenderCam"));
                    GUILayout.EndHorizontal();

                    if (GVEncoder.RenderCam == null)
                    {
                        //GUILayout.BeginVertical("box");
                        {
                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = Color.red;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(" Render Camera cannot be null", style);
                            GUILayout.EndHorizontal();

                        }
                        //GUILayout.EndVertical();
                    }

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResolutionProp, new GUIContent("Resolution"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MatchScreenAspectProp, new GUIContent("MatchScreenAspect"));
                    GUILayout.EndHorizontal();
                }
                if (GVEncoder.CaptureMode == GameViewCaptureMode.FullScreen)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResizeProp, new GUIContent("Resize"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(QualityProp, new GUIContent("Quality"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamFPSProp, new GUIContent("StreamFPS"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Encoded");
            if (GVEncoder.CapturedTexture != null)
            {
                GUILayout.Label("Preview " + " ( " + GVEncoder.CapturedTexture.width + " x " + GVEncoder.CapturedTexture.height + " ) ");
            }
            else
            {
                GUILayout.Label("Preview (Empty)");
            }
            GUILayout.BeginVertical("box");
            {
                const float maxLogoWidth = 430.0f;
                EditorGUILayout.Separator();
                float w = EditorGUIUtility.currentViewWidth;
                Rect r = new Rect();
                r.width = Math.Min(w - 40.0f, maxLogoWidth);
                r.height = r.width / 4.886f;
                Rect r2 = GUILayoutUtility.GetRect(r.width, r.height);
                r.x = r2.x;
                r.y = r2.y;
                if (GVEncoder.CapturedTexture != null)
                {
                    GUI.DrawTexture(r, GVEncoder.CapturedTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(r, new Texture2D((int)r.width, (int)r.height, TextureFormat.RGB24, false), ScaleMode.ScaleToFit);
                }
            }
            GUILayout.EndVertical();

            //GUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(CapturedTextureProp, new GUIContent("Captured Texture"));
            //GUILayout.EndHorizontal();

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


                //GUILayout.BeginHorizontal();
                //GUILayout.Label("Encoded Size(byte): " + GVEncoder.dataLength);
                //GUILayout.EndHorizontal();

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
