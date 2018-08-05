using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Vastan.Game;
using Vastan.Util;

namespace Vastan.Networking
{
    public enum PlayerStatus : int 
    {
        Connected = 0,
        Playing = 1,
        Spectating = 2,
        Dead = 4,
        HasControl = 8
    }

    public enum ServerStatus : int
    {
        Idle = 0,
        LoadStage = 1,
        VoteStage = 2,
        GatherPlayers = 4,
        GameInProgress = 8,
    }

    class ServerPlayer
    {
        public int connection;
        public int lag = 0;
        public PlayerStatus status = PlayerStatus.Connected;
        public VastanCharacter character;
    }

    class VastanServer : NetworkedSystem
    {
        private static int InputFramesToKeep = 50;
        private int framesToKeep = Mathf.RoundToInt(1.5f / Constants.FrameTime);

        Dictionary<int,ServerPlayer> players = new Dictionary<int,ServerPlayer>();
        
        public VastanServer()
        {
            if(Debug.isDebugBuild)
            {
                InitWithLag("127.0.0.1");
            }
            else
            {
                Init();
            }
            Connect("127.0.0.1");
            Log.Debug("Opened host socket " + socketId);
        }

        public void Update()
        {
            while(ReceiveData()) { }
            int new_max_lag = 0;
            foreach(KeyValuePair<int, ServerPlayer> kvp in players)
            {   
                var this_lag = GetLag(players[kvp.Key].connection);   
            }
        }

        public override void ConnectionReceived(int host, int connection)
        {
            players.Add(connection, new ServerPlayer());
        }

        public override void Disconnection(int host, int connection)
        {
            if (players.ContainsKey(connection))
            {
                players.Remove(connection);
            }

        }

        public override void DataReceived(int host, int connection, int channel, byte[] buffer, int size)
        {

        }

        public override void BroadcastReceived(int host, byte[] buffer, int size)
        {

        }
    }
}