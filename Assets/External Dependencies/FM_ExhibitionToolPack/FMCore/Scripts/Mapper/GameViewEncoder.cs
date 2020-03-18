using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

public enum GameViewCaptureMode { RenderCam, MainCam, FullScreen}
public enum GameViewResize { Full, Half, Quarter, OneEighth }
public class GameViewEncoder : MonoBehaviour
{
    public GameViewCaptureMode CaptureMode = GameViewCaptureMode.RenderCam;
    public GameViewResize Resize = GameViewResize.Quarter;

    public Camera MainCam;
    public Camera RenderCam;

    public Vector2 Resolution = new Vector2(512, 512);
    public bool MatchScreenAspect = true;

    [Range(10, 100)]
    public int Quality = 40;

    [Range(1f, 60f)]
    public float StreamFPS = 20f;
    float interval = 0.05f;

    bool NeedUpdateTexture = false;
    bool EncodingTexture = false;

    public Texture2D CapturedTexture;
    RenderTexture rt;
    Texture2D Screenshot;

    public UnityEventByteArray OnDataByteReadyEvent;

    //[Header("Pair Encoder & Decoder")]
    public int label = 1001;
    int dataID = 0;
    int maxID = 1024;
    int chunkSize = 8096; //32768
    float next = 0f;
    bool stop = false;
    byte[] dataByte;

    public int dataLength;

    private void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(SenderCOR());
    }

    private void Update()
    {
        if (CaptureMode == GameViewCaptureMode.MainCam)
        {
            if (MainCam == null) MainCam = this.GetComponent<Camera>();
        }

        if (CaptureMode != GameViewCaptureMode.RenderCam)
        {
            Resolution = new Vector2(Screen.width, Screen.height);
            Resolution /= Mathf.Pow(2, (int)Resize);

            if (RenderCam != null)
            {
                if (RenderCam.targetTexture != null) RenderCam.targetTexture = null;
            }
        }
        else
        {
            if (MatchScreenAspect)
            {
                if (Screen.width > Screen.height) Resolution.y = Resolution.x / (float)(Screen.width) * (float)(Screen.height);
                if (Screen.width < Screen.height) Resolution.x = Resolution.y / (float)(Screen.height) * (float)(Screen.width);
            }
        }

        Resolution.x = Mathf.RoundToInt(Resolution.x);
        Resolution.y = Mathf.RoundToInt(Resolution.y);
    }

    void CheckResolution()
    {
        if (rt == null)
        {
            rt = new RenderTexture(Mathf.RoundToInt(Resolution.x), Mathf.RoundToInt(Resolution.y), 16, RenderTextureFormat.ARGB32);
        }
        else
        {
            if (rt.width != Mathf.RoundToInt(Resolution.x) || rt.height != Mathf.RoundToInt(Resolution.y))
            {
                Destroy(rt);
                rt = new RenderTexture(Mathf.RoundToInt(Resolution.x), Mathf.RoundToInt(Resolution.y), 16, RenderTextureFormat.ARGB32);
            }
        }

        if (CapturedTexture == null)
        {
            CapturedTexture = new Texture2D(Mathf.RoundToInt(Resolution.x), Mathf.RoundToInt(Resolution.y), TextureFormat.RGB24, false);
        }
        else
        {
            if (CapturedTexture.width != Mathf.RoundToInt(Resolution.x) || CapturedTexture.height != Mathf.RoundToInt(Resolution.y))
            {
                Destroy(CapturedTexture);
                CapturedTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            }
        }
    }


    IEnumerator ProcessCapturedTexture()
    {
        //render texture to texture2d
        CapturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        CapturedTexture.Apply();
        //encode to byte
        StartCoroutine(EncodeBytes());
        yield break;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CaptureMode == GameViewCaptureMode.MainCam)
        {
            if (NeedUpdateTexture)
            {
                NeedUpdateTexture = false;
                CheckResolution();
                Graphics.Blit(source, rt);

                StartCoroutine(ProcessCapturedTexture());
            }
        }

        Graphics.Blit(source, destination);
    }

    IEnumerator RenderTextureRefresh()
    {
        if (NeedUpdateTexture)
        {
            NeedUpdateTexture = false;
            EncodingTexture = true;

            yield return new WaitForEndOfFrame();

            CheckResolution();

            if (CaptureMode == GameViewCaptureMode.RenderCam)
            {
                if (RenderCam != null)
                {
                    RenderCam.targetTexture = rt;
                    RenderCam.Render();

                    // Backup the currently set RenderTexture
                    RenderTexture previous = RenderTexture.active;

                    // Set the current RenderTexture to the temporary one we created
                    RenderTexture.active = rt;

                    //RenderTexture to Texture2D
                    StartCoroutine(ProcessCapturedTexture());

                    // Reset the active RenderTexture
                    RenderTexture.active = previous;
                }
            }

            if (CaptureMode == GameViewCaptureMode.FullScreen)
            {
                if (Resize == GameViewResize.Full)
                {
                    // cleanup
                    if (CapturedTexture != null) Destroy(CapturedTexture);
                    CapturedTexture = ScreenCapture.CaptureScreenshotAsTexture();
                    StartCoroutine(EncodeBytes());
                }
                else
                {
                    // cleanup
                    if (Screenshot != null) Destroy(Screenshot);
                    Screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                    Graphics.Blit(Screenshot, rt);

                    //RenderTexture to Texture2D
                    StartCoroutine(ProcessCapturedTexture());
                }
            }
        }
    }

	IEnumerator SenderCOR()
    {
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                interval = 1f / StreamFPS;
                next = Time.realtimeSinceStartup + interval;

                if (!EncodingTexture)
                {
                    NeedUpdateTexture = true;
                    if(CaptureMode != GameViewCaptureMode.MainCam) StartCoroutine(RenderTextureRefresh());
                }
                yield return null;
            }
            yield return null;
        }
    }

    IEnumerator EncodeBytes()
    {
        if (CapturedTexture != null)
        {
            yield return null;
            //==================getting byte data==================
            dataByte = CapturedTexture.EncodeToJPG(Quality);
            dataLength = dataByte.Length;
            //==================getting byte data==================
            int _length = dataByte.Length;
            int _offset = 0;

            byte[] _meta_label = BitConverter.GetBytes(label);
            byte[] _meta_id = BitConverter.GetBytes(dataID);
            byte[] _meta_length = BitConverter.GetBytes(_length);

            int chunks = Mathf.FloorToInt(dataByte.Length / chunkSize);
            for (int i = 0; i <= chunks; i++)
            {
                int SendByteLength = (i == chunks) ? (_length % chunkSize + 16) : (chunkSize + 16);
                byte[] _meta_offset = BitConverter.GetBytes(_offset);
                byte[] SendByte = new byte[SendByteLength];

                Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
                Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
                Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);

                Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);
                Buffer.BlockCopy(dataByte, _offset, SendByte, 16, SendByte.Length - 16);
                OnDataByteReadyEvent.Invoke(SendByte);
                _offset += chunkSize;
            }

            dataID++;
            if (dataID > maxID) dataID = 0;
        }

        EncodingTexture = false;
        //TimeEnd = Time.realtimeSinceStartup;
        //Debug.Log((int)(1f / (TimeEnd - TimeStart)));
        //TimeStart = Time.realtimeSinceStartup;
        yield break;
    }

    //float TimeStart = 0f;
    //float TimeEnd = 0f;

    void OnEnable()
    {
        StartAll();
    }
    void OnDisable()
    {
        StopAll();
    }
    void OnApplicationQuit()
    {
        StopAll();
    }
    void OnDestroy()
    {
        StopAll();
    }

    void StopAll()
    {
        stop = true;
        StopAllCoroutines();
    }
    void StartAll()
    {
        if (Time.realtimeSinceStartup < 3f) return;
        stop = false;
        StartCoroutine(SenderCOR());

        NeedUpdateTexture = false;
        EncodingTexture = false;
    }
}
