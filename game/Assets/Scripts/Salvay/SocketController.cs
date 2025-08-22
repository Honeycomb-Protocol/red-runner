using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum SocketEventsType
{
    paymentInitiated,
    paymentFailed,
    paymentComplete,
    payoutInitiated,
    payoutFailed,
    payoutComplete,
    assetMintInitiated,
    assetMintFailed,
    assetMintComplete,
    marketAssetListed,
    marketAssetUnListed,
    marketAssetBought,
    loginInitiated,
    qrGenerated,
    qrScanned,
    qrLoginCompleted
}

public interface SocketEventListener
{
    public void OnSocketMessageReceived(SocketEventsType messageHeader, string payLoad = null);
    public void RemoveListener();
}

public class SocketController : SingletonBase<SocketController>
{
    private SocketManager _manager;
    private List<SocketEventListener> _listeners = new List<SocketEventListener>();

    public void AddListener(SocketEventListener listener)
    {
        _listeners.Add(listener);
    }

    public void RemoveListener(SocketEventListener listener)
    {
        _listeners.Remove(listener);
    }
    private void BroadcastToAllListeners(SocketEventsType eventsType, string payload = null)
    {
        foreach (var listener in _listeners)
        {
            listener?.OnSocketMessageReceived(eventsType, payload);
        }
    }

    public void ConnectSocketWithUserId(string userId)
    {
        if (_manager != null)
        {
            _manager.Close();
            _manager = null;
        }

        SocketOptions options = new SocketOptions
        {
            AutoConnect = false,
            Reconnection = true,
            QueryParamsOnlyForHandshake = true,
            // Create and setup SocketManager
            AdditionalQueryParams = new PlatformSupport.Collections.ObjectModel.ObservableDictionary<string, string> { { "userId", userId } }
        };

        _manager = new SocketManager(new Uri(StaticDataBank.SOCKET_URL), options);

        // Set subscriptions
        _manager.Socket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        List<string> allNames = Enum.GetNames(typeof(SocketEventsType)).ToList();
        _manager.Socket.On(allNames, OnSocketEvent);

        // Start connecting to the server
        _manager.Open();
    }

    private void OnSocketEvent()
    {
        SocketEventsType currentType = (SocketEventsType)Enum.Parse(typeof(SocketEventsType), _manager.Socket.CurrentPacket.EventName);
        Debug.Log("Socket event received: " + currentType);
        switch (currentType)
        {
            case SocketEventsType.paymentInitiated:
                BroadcastToAllListeners(SocketEventsType.paymentInitiated);
                break;
            case SocketEventsType.paymentFailed:
                BroadcastToAllListeners(SocketEventsType.paymentFailed);
                break;
            case SocketEventsType.paymentComplete:
                BroadcastToAllListeners(SocketEventsType.paymentComplete);
                break;
            case SocketEventsType.payoutInitiated:
                BroadcastToAllListeners(SocketEventsType.payoutInitiated);
                break;
            case SocketEventsType.payoutFailed:
                BroadcastToAllListeners(SocketEventsType.payoutFailed);
                break;
            case SocketEventsType.payoutComplete:
                BroadcastToAllListeners(SocketEventsType.payoutComplete, GetAmountStringFromPayload(_manager.Socket.CurrentPacket.Payload).ToString(CultureInfo.InvariantCulture));
                break;
            case SocketEventsType.assetMintInitiated:
                BroadcastToAllListeners(SocketEventsType.assetMintInitiated);
                break;
            case SocketEventsType.assetMintFailed:
                BroadcastToAllListeners(SocketEventsType.assetMintFailed);
                break;
            case SocketEventsType.assetMintComplete:
                BroadcastToAllListeners(SocketEventsType.assetMintComplete, GetItemIdStringFromPayload(_manager.Socket.CurrentPacket.Payload));
                break;
            case SocketEventsType.marketAssetListed:
            case SocketEventsType.marketAssetUnListed:
            case SocketEventsType.marketAssetBought:
                BroadcastToAllListeners(currentType);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float GetAmountStringFromPayload(string jsonString)
    {
        JArray jsonArray = JArray.Parse(jsonString);

        // Access the second element, which is the JSON object containing the amount
        JObject jsonObject = (JObject)jsonArray[1];

        // Get the value of the "amount" key
        return (jsonObject["payload"] ?? 0f).Value<float>();
    }

    private string GetItemIdStringFromPayload(string jsonString)
    {
        JArray jsonArray = JArray.Parse(jsonString);

        // Access the second element, which is the JSON object containing the amount
        JObject jsonObject = (JObject)jsonArray[1];

        // Get the value of the "amount" key
        return (jsonObject["payload"] ?? 0f).Value<string>();
    }

    // Connected event handler implementation
    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Connected!");
    }

    // Unity OnDestroy event
    void OnDestroy()
    {
        this._manager?.Close();
        this._manager = null;
    }

    private void OnApplicationQuit()
    {
        _manager?.Close();
        _manager = null;
    }
}