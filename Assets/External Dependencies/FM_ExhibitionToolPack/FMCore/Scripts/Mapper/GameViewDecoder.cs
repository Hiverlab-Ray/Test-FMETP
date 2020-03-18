using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameViewDecoder : MonoBehaviour
{
    public Texture2D ReceivedTexture;
    public GameObject TestQuad;
    public RawImage TestImg;

    public UnityEventTexture2D OnReceivedTexture2D;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
    }

	bool ReadyToGetFrame = true;

    //[Header("Pair Encoder & Decoder")]
    public int label = 1001;
	int dataID = 0;
    int maxID = 1024;
    int dataLength = 0;
	int receivedLength = 0;

    byte[] dataByte;
	public void Action_ProcessImageData(byte[] _byteData)
	{
        if (!enabled) return;
        if (_byteData.Length <= 8) return;

        int _label = BitConverter.ToInt32(_byteData, 0);
        if (_label != label) return;
        int _dataID = BitConverter.ToInt32(_byteData, 4);
        //if (_dataID < dataID) return;

        if (_dataID != dataID) receivedLength = 0;
        dataID = _dataID;
        dataLength = BitConverter.ToInt32(_byteData, 8);
        int _offset = BitConverter.ToInt32(_byteData, 12);
        if (receivedLength == 0) dataByte = new byte[dataLength];
        receivedLength += _byteData.Length - 16;
        Buffer.BlockCopy(_byteData, 16, dataByte, _offset, _byteData.Length - 16);

        if (ReadyToGetFrame)
        {
            if (receivedLength == dataLength) StartCoroutine(ProcessImageData(dataByte));
        }

    }

	IEnumerator ProcessImageData(byte[] _byteData)
	{
        ReadyToGetFrame = false;

        if (ReceivedTexture == null) ReceivedTexture = new Texture2D(0, 0);
        ReceivedTexture.LoadImage(_byteData);

        if (TestQuad != null) TestQuad.GetComponent<Renderer>().material.mainTexture = ReceivedTexture;
		if (TestImg != null) TestImg.texture = ReceivedTexture;
		OnReceivedTexture2D.Invoke(ReceivedTexture);
		ReadyToGetFrame = true;

		yield return null;
	}

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
