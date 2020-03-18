using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using FMSocketIO;
using WebSocketSharp;
using WebSocketSharp.Net;

using System.Runtime.InteropServices;

[System.Serializable]
public class EventJson
{
    public string socketEvent;
    public string eventData;
}

public class SocketIOComponentWebGL : MonoBehaviour {

    public static SocketIOComponentWebGL instance;
    public string sid;

    // Use this for initialization
    void Awake () {
        if(instance == null) instance = this;
    }

    public string IP = "127.0.0.1";
    public int port = 3000;
    public bool sslEnabled = false;

    int packetId;
    Dictionary<string, List<Action<SocketIOEvent>>> eventHandlers;
    List<Ack> ackList;

    public void Init()
    {
        eventHandlers = new Dictionary<string, List<Action<SocketIOEvent>>>();

        ackList = new List<Ack>();

		AddSocketIO();

		AddEventListeners();
    }

    private void OnConnected(SocketIOEvent e)
    {
        Debug.Log("[Event] SocketIO connected");
    }

    #if UNITY_WEBGL
	[DllImport("__Internal")]
	private static extern void WebSocketAddSocketIO(string _src);
	[DllImport("__Internal")]
	private static extern void WebSocketAddEventListeners(string _gameobject);
	[DllImport("__Internal")]
	private static extern void WebSocketConnect(string _src, string _gameobject);
    [DllImport("__Internal")]
    private static extern void WebSocketClose();
    [DllImport("__Internal")]
    private static extern void WebSocketEmitEvent(string _e);
    [DllImport("__Internal")]
    private static extern void WebSocketEmitData(string _e, string _data);
    [DllImport("__Internal")]
    private static extern void WebSocketEmitEventAction(string _e, string _packetId, string _gameobject);
    [DllImport("__Internal")]
    private static extern void WebSocketEmitDataAction(string _e, string _data, string _packetId, string _gameobject);
    [DllImport("__Internal")]
    private static extern void WebSocketOn(string _e);
    #endif

    void AddSocketIO()
    {
		Debug.Log("adding script!!!!!!!");

		string src = "http" + (sslEnabled ? "s" : "") + "://" + IP + (!sslEnabled && port != 0 ? ":" + port.ToString() : "") + "/socket.io/socket.io.js/";
        #if UNITY_WEBGL
        WebSocketAddSocketIO(src);
        #endif

		//Application.ExternalEval(@"
		//              var socketIOScript = document.createElement('script');
		//              socketIOScript.setAttribute('src', 'http" + (sslEnabled ? "s" : "") + @"://" + IP + (!sslEnabled && port != 0 ? ":" + port.ToString() : "") + @"/socket.io/socket.io.js');
		//              document.head.appendChild(socketIOScript);
		//          ");
	}

    void AddEventListeners()
    {
        #if UNITY_WEBGL
        WebSocketAddEventListeners(gameObject.name);
        #endif

		//Application.ExternalEval(@"
  //              window.socketEvents = {};

  //              window.socketEventListener = function(event, data){
  //                  var socketData = {
  //                      socketEvent: event,
  //                      eventData: typeof data === 'undefined' ? '' : JSON.stringify(data)
  //                  };

  //                  SendMessage('" + gameObject.name + @"', 'InvokeEventCallback', JSON.stringify(socketData));
  //              };
  //          ");
	}

    public void Connect()
    {
        Debug.Log("=========start connecting=========");
        string src = "http" + (sslEnabled ? "s" : "") + "://" + IP + (!sslEnabled && port != 0 ? ":" + port.ToString() : "") + "/";

        #if UNITY_WEBGL
        WebSocketConnect(src, gameObject.name);
        #endif

        //Application.ExternalEval(@"
        //        window.socketIO = io.connect('http" + (sslEnabled ? "s" : "") + @"://" + IP + (!sslEnabled && port != 0 ? ":" + port.ToString() : "") + @"/');

        //        window.socketIO.on('connect', function(){
        //            SendMessage('" + gameObject.name + @"', 'SetSocketID', window.socketIO.io.engine.id);
        //        });

        //        for(var socketEvent in window.socketEvents){
        //            window.socketIO.on(socketEvent, window.socketEvents[socketEvent]);
        //        }
        //    ");
        Debug.Log("=========end connecting=========");
    }
    public void Close()
    {
        #if UNITY_WEBGL
        WebSocketClose();
        #endif
        //Application.ExternalEval(@"
        //        if(typeof window.socketIO !== 'undefined')
        //            window.socketIO.disconnect();
        //    ");
    }


    public void Emit(string e)
    {
        #if UNITY_WEBGL
        WebSocketEmitEvent(e);
        #endif
        //Application.ExternalEval(@"
        //        if(typeof window.socketIO !== 'undefined')
        //            window.socketIO.emit('" + e + @"');
        //    ");
    }

    public void Emit(string e, string data)
    {
        #if UNITY_WEBGL
        WebSocketEmitData(e, string.Format("{0}", data));
        #endif
        //Application.ExternalEval(@"
        //        if(typeof window.socketIO !== 'undefined')
        //            window.socketIO.emit('" + e + @"', " + data + @");
        //    ");
    }

    public void Emit(string e, Action<string> action)
    {
        packetId++;

        #if UNITY_WEBGL
        WebSocketEmitEventAction(e, packetId.ToString(), gameObject.name);
        #endif
        //Application.ExternalEval(@"
        //        if(typeof window.socketIO !== 'undefined'){
        //            window.socketIO.emit('" + e + @"', function(data){
        //                var ackData = {
        //                    packetID: " + packetId.ToString() + @",
        //                    data: typeof data === 'undefined' ? '' : JSON.stringify(data)
        //                };

        //                SendMessage('" + gameObject.name + @"', 'InvokeAck', JSON.stringify(ackData));
        //            });
        //        }
        //    ");

        ackList.Add(new Ack(packetId, action));
    }

    public void Emit(string e, string data, Action<string> action)
    {
        packetId++;
        #if UNITY_WEBGL
        WebSocketEmitDataAction(e, data, packetId.ToString(), gameObject.name);
        #endif
        //Application.ExternalEval(@"
        //        if(typeof window.socketIO !== 'undefined'){
        //            window.socketIO.emit('" + e + @"', " + data + @", function(data){
        //                var ackData = {
        //                    packetID: " + packetId.ToString() + @",
        //                    data: typeof data === 'undefined' ? '' : JSON.stringify(data)
        //                };

        //                SendMessage('" + gameObject.name + @"', 'InvokeAck', JSON.stringify(ackData));
        //            });
        //        }
        //    ");

        ackList.Add(new Ack(packetId, action));
    }

    public void On(string e, Action<SocketIOEvent> callback)
    {
        if (!eventHandlers.ContainsKey(e)) eventHandlers[e] = new List<Action<SocketIOEvent>>();

        eventHandlers[e].Add(callback);

        #if UNITY_WEBGL
        WebSocketOn(e);
        #endif
        //Application.ExternalEval(@"
        //        if(typeof window.socketEvents['" + e + @"'] === 'undefined'){
        //            window.socketEvents['" + e + @"'] = function(data){
        //                window.socketEventListener('" + e + @"', data);
        //            };

        //            if(typeof window.socketIO !== 'undefined'){
        //                window.socketIO.on('" + e + @"', function(data){
        //                    window.socketEventListener('" + e + @"', data);
        //                });
        //            }
        //        }
        //    ");
    }

    public void Off(string e, Action<SocketIOEvent> callback)
    {
        if (!eventHandlers.ContainsKey(e)) return;

        List<Action<SocketIOEvent>> _eventHandlers = eventHandlers[e];

        if (!_eventHandlers.Contains(callback)) return;

        _eventHandlers.Remove(callback);

        if (_eventHandlers.Count == 0) eventHandlers.Remove(e);
    }

    public void InvokeAck(string ackJson)
    {
        Ack ack;
        Ack ackData = JsonUtility.FromJson<Ack>(ackJson);

        for (int i = 0; i < ackList.Count; i++)
        {
            if (ackList[i].packetId == ackData.packetId)
            {
                ack = ackList[i];
                ackList.RemoveAt(i);
                ack.Invoke(ackJson);
                return;
            }
        }
    }

    public void SetSocketID(string socketID)
    {
        sid = socketID;
        //Debug.Log("socket id !: "+socketID);
        FMSocketIOManager.instance.Settings.socketID = sid;
    }

    public void InvokeEventCallback(string eventJson)
    {
        //Debug.Log("getting event!");
        EventJson eventData = JsonUtility.FromJson<EventJson>(eventJson);

        if (!eventHandlers.ContainsKey(eventData.socketEvent)) return;

        for (int i = 0; i < eventHandlers[eventData.socketEvent].Count; i++)
        {
            SocketIOEvent socketEvent = new SocketIOEvent(eventData.socketEvent, eventData.eventData);
            eventHandlers[eventData.socketEvent][i](socketEvent);
        }
    }
}
