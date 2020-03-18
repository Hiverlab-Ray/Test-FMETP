using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FMSocketIO;

public struct FMSocketIOData
{
    public FMSocketIOEmitType EmitType;
    public string DataString;
    public byte[] DataByte;
}
public enum FMSocketIONetworkType { Server, Client }
public enum FMSocketIOEmitType { All, Server, Others }
public class FMSocketIOManager : MonoBehaviour {

    public static FMSocketIOManager instance;
    public bool AutoInit = true;
    public FMSocketIONetworkType NetworkType = FMSocketIONetworkType.Client;

    [System.Serializable]
    public class SocketIOSettings
    {
        public string IP="127.0.0.1";
        public int port=3000;
        public bool sslEnabled=false;
        public int reconnectDelay=5;
        public int ackExpirationTime=1800;
        public int pingInterval=25;
        public int pingTimeout=60;
        public string socketID;

    }
    public SocketIOSettings Settings;
    public void Action_SetIP(string _ip)
    {
        Settings.IP = _ip;
    }
    public void Action_SetPort(string _port)
    {
        Settings.port = int.Parse(_port);
    }

    [HideInInspector]
    public SocketIOComponent socketIO;
    [HideInInspector]
    public SocketIOComponentWebGL socketIOWebGL;

    bool isInitialised = false;
    float delayTimer = 0f;
    float delayThreshold = 1f;
    bool HasConnected = false;
    public bool Ready = false;

    //networking: receive data
    public UnityEventByteArray OnReceivedByteDataEvent;
    public UnityEventString OnReceivedStringDataEvent;

    IEnumerator WaitForSocketIOConnected()
    {
        while (!Ready) yield return null;
        On("OnReceiveData", OnReceivedData);
    }
    void OnReceivedData(SocketIOEvent e)
    {
        FMSocketIOData _data = JsonUtility.FromJson<FMSocketIOData>(e.data);
        if (_data.DataString.Length > 1) OnReceivedStringDataEvent.Invoke(_data.DataString);
        if (_data.DataByte.Length > 1) OnReceivedByteDataEvent.Invoke(_data.DataByte);
    }
    void OnReceivedData(FMSocketIOData _data)
    {
        if (_data.DataString.Length > 1) OnReceivedStringDataEvent.Invoke(_data.DataString);
        if (_data.DataByte.Length > 1) OnReceivedByteDataEvent.Invoke(_data.DataByte);
    }

    public void Action_OnReceivedData(string _string)
    {
        Debug.Log(_string);
    }
    public void Action_OnReceivedData(byte[] _byte)
    {
        Debug.Log("byte: "+_byte.Length);
    }

    //Sender functions
    #region Sender
    public void Send(string _stringData, FMSocketIOEmitType _type)
    {
        if (!Ready) return;
        FMSocketIOData _data = new FMSocketIOData();
        _data.DataString = _stringData;
        _data.DataByte = new byte[1];
        _data.EmitType = _type;

        Emit("OnReceiveData", JsonUtility.ToJson(_data));
    }
    public void Send(byte[] _byteData, FMSocketIOEmitType _type)
    {
        if (!Ready) return;
        FMSocketIOData _data = new FMSocketIOData();
        _data.DataString = "";
        _data.DataByte = _byteData;
        _data.EmitType = _type;

        Emit("OnReceiveData", JsonUtility.ToJson(_data));
    }
    public void SendToAll(byte[] _byteData)
    {
        Send(_byteData, FMSocketIOEmitType.All);
    }
    public void SendToServer(byte[] _byteData)
    {
        Send(_byteData, FMSocketIOEmitType.Server);
    }
    public void SendToOthers(byte[] _byteData)
    {
        Send(_byteData, FMSocketIOEmitType.Others);
    }
    public void SendToAll(string _stringData)
    {
        Send(_stringData, FMSocketIOEmitType.All);
    }
    public void SendToServer(string _stringData)
    {
        Send(_stringData, FMSocketIOEmitType.Server);
    }
    public void SendToOthers(string _stringData)
    {
        Send(_stringData, FMSocketIOEmitType.Others);
    }
    #endregion


    void Awake()
    {
        Application.runInBackground = true;
        if (instance == null) instance = this;
    }

    private void Start()
    {
        //auto init?
        if (AutoInit) Init();
        StartCoroutine(WaitForSocketIOConnected());
    }

    void Update()
    {
        if (isInitialised) delayTimer += Time.deltaTime;

        if (delayTimer > delayThreshold)
        {
            if (!HasConnected)
            {
                HasConnected = true;
                Connect();

                On("connect", (SocketIOEvent e) =>
                {
                    Debug.Log("SocketIO connected");
                    Ready = true;
                    if (NetworkType == FMSocketIONetworkType.Server) Emit("RegServerId");
                });

                //On("open", (SocketIOEvent e) =>
                //{
                //    Debug.Log("SocketIO opened");
                //    Ready = true;
                //});
            }
        }
    }
    public void InitAsServer()
    {
        NetworkType = FMSocketIONetworkType.Server;
        Init();
    }
    public void InitAsClient()
    {
        NetworkType = FMSocketIONetworkType.Client;
        Init();
    }
    public void Init()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL = gameObject.AddComponent<SocketIOComponentWebGL>();
            socketIOWebGL.IP = Settings.IP;
            socketIOWebGL.port = Settings.port;

            socketIOWebGL.sslEnabled = Settings.sslEnabled;

            socketIOWebGL.Init();
        }
        else
        {
            socketIO = gameObject.AddComponent<SocketIOComponent>();
            socketIO.IP = Settings.IP;
            socketIO.port = Settings.port;
            socketIO.sslEnabled = Settings.sslEnabled;

            socketIO.reconnectDelay = Settings.reconnectDelay;
            socketIO.ackExpirationTime = Settings.ackExpirationTime;
            socketIO.pingInterval = Settings.pingInterval;
            socketIO.pingTimeout = Settings.pingTimeout;

            socketIO.Init();
        }
        isInitialised = true;
    }

    public void Connect()
    {
        Debug.Log("SocketIOManager: " + "Start Connect");
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.Connect();
        }
        else
        {
            socketIO.Connect();
        }
    }


    public void Close()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.Close();
        }
        else
        {
            socketIO.Close();
        }
    }

    public void Emit(string e)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.Emit(e);
        }
        else
        {
            socketIO.Emit(e);
        }
    }
    public void Emit(string e, Action<string> action)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.Emit(e, action);
        }
        else
        {
            socketIO.Emit(e, action);
        }
    }
    public void Emit(string e, string data)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.Emit(e, data);
        }
        else
        {
            socketIO.Emit(e, data);
        }
    }
    public void Emit(string e, string data, Action<string> action)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.Emit(e, data, action);
        }
        else
        {
            socketIO.Emit(e, data, action);
        }
    }

    public void On(string e, Action<SocketIOEvent> callback)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.On(e, callback);
        }
        else
        {
            socketIO.On(e, callback);
        }
    }
    public void Off(string e, Action<SocketIOEvent> callback)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            socketIOWebGL.Off(e, callback);
        }
        else
        {
            socketIO.Off(e, callback);
        }
    }
}
