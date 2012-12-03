// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonHandler.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using ExitGames.Client.Photon;
using UnityEngine;

/// <summary>
/// Internal Monobehaviour that allows Photon to run an Update loop.
/// </summary>
internal class PhotonHandler : Photon.MonoBehaviour, IPhotonPeerListener
{
    public static PhotonHandler SP;

    public int updateInterval;

    public int updateIntervalOnSerialize;

    private int nextSendTickCount = Environment.TickCount;

    private int nextSendTickCountOnSerialize = Environment.TickCount;

    private void Awake()
    {
        if (SP != null && SP != this)
        {
            Debug.LogError("Error: we already have an PhotonMono around!");
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this);
        SP = this;

        this.updateInterval = 1000 / PhotonNetwork.sendRate;
        this.updateIntervalOnSerialize = 1000 / PhotonNetwork.sendRateOnSerialize;
    }

    private void Update()
    {
        if (PhotonNetwork.networkingPeer == null)
        {
            Debug.LogError("NetworkPeer broke!");
            return;
        }

        if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated || PhotonNetwork.connectionStateDetailed == PeerState.Disconnected)
        {
            return;
        }

        // the messageQueue might be paused. in that case a thread will send acknowledgements only. nothing else to do here.
        if (!PhotonNetwork.isMessageQueueRunning)
        {
            return;
        }

        bool doDispatch = true;
        while (PhotonNetwork.isMessageQueueRunning && doDispatch)
        {
            // DispatchIncomingCommands() returns true of it found any command to dispatch (event, result or state change)
            Profiler.BeginSample("DispatchIncomingCommands");
            doDispatch = PhotonNetwork.networkingPeer.DispatchIncomingCommands();
            Profiler.EndSample();
        }

        if (PhotonNetwork.isMessageQueueRunning && Environment.TickCount > this.nextSendTickCountOnSerialize)
        {
            PhotonNetwork.networkingPeer.RunViewUpdate();
            this.nextSendTickCountOnSerialize = Environment.TickCount + this.updateIntervalOnSerialize;
        }

        if (Environment.TickCount > this.nextSendTickCount)
        {
            bool doSend = true;
            while (PhotonNetwork.isMessageQueueRunning && doSend)
            {
                // Send all outgoing commands
                Profiler.BeginSample("SendOutgoingCommands");
                doSend = PhotonNetwork.networkingPeer.SendOutgoingCommands();
                Profiler.EndSample();
            }

            this.nextSendTickCount = Environment.TickCount + this.updateInterval;
        }
    }

    /// <summary>Called by Unity when the application is closed. Tries to disconnect.</summary>
    public void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }

    /// <summary>Called by Unity after a new level was loaded.</summary>
    public void OnLevelWasLoaded(int level)
    {
        PhotonNetwork.networkingPeer.NewSceneLoaded();
    }

    public static void StartThread()
    {
        System.Threading.Thread sendThread = new System.Threading.Thread(new System.Threading.ThreadStart(MyThread));
        sendThread.Start();
    }

    // keeps connection alive while loading
    public static void MyThread()
    {
        while (PhotonNetwork.networkingPeer != null && PhotonNetwork.networkingPeer.IsSendingOnlyAcks)
        {
            while (PhotonNetwork.networkingPeer.SendOutgoingCommands())
            {
            }

            System.Threading.Thread.Sleep(200);
        }
    }

    #region Implementation of IPhotonPeerListener

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == DebugLevel.ERROR)
        {
            Debug.LogError(message);
        }
        else if (level == DebugLevel.WARNING)
        {
            Debug.LogWarning(message);
        }
        else if (level == DebugLevel.INFO && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
        {
            Debug.Log(message);
        }
        else if (level == DebugLevel.ALL && PhotonNetwork.logLevel == PhotonLogLevel.Full)
        {
            Debug.Log(message);
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
    }

    public void OnEvent(EventData photonEvent)
    {
    }

    #endregion
}