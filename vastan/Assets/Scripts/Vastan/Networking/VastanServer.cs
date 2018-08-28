using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Vastan.Game;
using Vastan.Util;

namespace Vastan.Networking
{
    public enum ClientStatus : int 
    {
        Connected = 0,
        GotConfig = 1,
        Playing = 2,
        Spectating = 4,
        Dead = 8,
        HasControl = 16
    }

    public enum ServerStatus : int
    {
        Idle = 0,
        LoadStage = 1,
        VoteStage = 2,
        GatherPlayers = 4,
        GameInProgress = 8,
    }
    
    public class Client
    {
        public int Connection;
        public int Host;
        public ConnectionInfo Info;
        public int Lag = 0;
        public string Name;
        public ClientStatus Status = ClientStatus.Connected;
        public VastanCharacter Character;
        
        public Client(int host, int connection) {
            Host = host;
            Connection = connection;
            Info = NetworkedSystem.GetConnectionInfo(host, connection);
        }

        public void ReceivedConfig(ClientConfig config)
        {
            Name = config.Name;
        }
    }

    class VastanServer : NetworkedSystem
    {
        static int InputFramesToKeep = 50;
        static int FramesToKeep = Mathf.RoundToInt(1.5f / Constants.FrameTime);
        static float PosTolerance = .5f;
        static float DirTolerance = 5f;
        int maxLag = 0;
        //public VastanGame game;
        byte newPlayerId = 0;
        
        Dictionary<byte,Client> clients = new Dictionary<byte,Client>();      
        Dictionary<int, byte> connections = new Dictionary<int, byte>();
        
        byte GetPlayerId() {
            newPlayerId++;
            return newPlayerId;
        }

        public VastanServer()
        {
            if (Debug.isDebugBuild)
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
            while (ReceiveData()) { }
            maxLag = 0;
            foreach (KeyValuePair<byte, Client> kvp in clients)
            {
                ClientActions(kvp.Key, kvp.Value);
            }
        }

        void ClientActions(byte id, Client client) 
        {
            var thisLag = GetLag(client.Connection);
            if (maxLag < thisLag)
            {
                maxLag = thisLag;
            }         
        }

        void SendToAllClients(byte[] data)
        {
            foreach(Client client in clients.Values)
            {
                SendReliableMessage(client.Host, client.Connection, data);
            }
        }

        void SendToAllOtherClients(int ignoreHost, int ignoreConnection, byte[] data)
        {
            foreach (Client client in clients.Values) 
            {
                if (!(client.Host == ignoreHost && client.Connection == ignoreConnection))
                {
                    SendReliableMessage(client.Host, client.Connection, data);
                }
            }
        }

        public override void ConnectionReceived(int host, int connection)
        {
            
            byte theId = GetPlayerId();
            clients.Add(theId, new Client(host, connection));
            connections.Add(connection, theId);         
        }

        public override void Disconnection(int host, int connection)
        {
            if (connections.ContainsKey(connection)) 
            {
                var theClient = clients[connections[connection]];
                Log.Debug("{0} disconnected", theClient.Name);
            }
            else 
            {
                Log.Error("Disconnect recieved from unknown connection ID {0}", connection);
            }

        }

        public override void DataReceived(int host, int connection, int channel, BinaryReader reader, int size)
        {
            byte clientId = connections[connection];
            MessageType type = SerializedMessage.GetMessageType(reader);
            switch (type)
            {
                case MessageType.ClientConfig:
                    ClientConfig theConfig = new ClientConfig(reader);
                    Client theClient = clients[clientId];
                    theClient.ReceivedConfig(theConfig);
                    ClientJoin clientJoin = new ClientJoin(clientId, theClient);
                    SendToAllClients(clientJoin.Serialize());
                    break;
                case MessageType.Chat:
                    Chat theChat = new Chat(reader);
                    ClientChat theClientChat = new ClientChat(clientId, theChat.Chars);
                    SendToAllOtherClients(host, connection, theClientChat.Serialize());
                    break;
                default:
                    break;
            }
        }

        public override void BroadcastReceived(int host, byte[] buffer, int size)
        {
            Log.Debug("Broadcast Received");
        }
    }
}