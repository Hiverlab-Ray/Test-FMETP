using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test_WebcamTexture : MonoBehaviour
{
    public Renderer renderer;
    public WebCamTexture webcamTexture;

    private void Awake()
    {
        webcamTexture = new WebCamTexture();
    }

    // Start is called before the first frame update
    void Start()
    {
        webcamTexture.Stop();
        renderer.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }

    // Update is called once per frame
    void Update()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        Debug.Log(webcamTexture.isPlaying);
        if (!webcamTexture.isPlaying)
        {
            webcamTexture.Play();
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuite");
        webcamTexture.Stop();
    }
}
