using System;
using UnityEngine;
using UnityEngine.Networking;
using Vastan.Game;
using Vastan.Util;

namespace Vastan.Networking 
{
    abstract class NetworkedSystem : System.Object 
    {
        public int socketId;
        public int reliableChannelId;
        private static int MaxConnections = 16;
        private int connectionId;
        private bool isConnected = false;
        private HostTopology topology;

        public void InitWithLag(string address)
        {
            Init();
            socketId = NetworkTransport.AddHostWithSimulator(
                topology, 10, 200, Constants.SocketPort, address);
        }

        public void Init() 
        {
            NetworkTransport.Init();
            var config = new ConnectionConfig();
            reliableChannelId = config.AddChannel(QosType.Reliable);
            topology = new HostTopology(config, MaxConnections);
            socketId = NetworkTransport.AddHost(topology, Constants.SocketPort);
        }

        public void Connect(string address)
        {
            byte error;
            connectionId = NetworkTransport.Connect(
                socketId, address, Constants.SocketPort, 0, out error);

            if (HasError(error))
            {
                Log.Debug("Error connecting to server " + address);
            }
            else 
            {
                isConnected = true;
            }
        }

        public bool HasError(byte error) 
        {
            if ((NetworkError)error != NetworkError.Ok)
            {
                Log.Debug(((NetworkError)error).ToString());
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetLag(int connectionId) 
        {
            byte error;
            int lag = NetworkTransport.GetCurrentRTT(
                socketId, connectionId, out error);
            if (HasError(error)) 
            {
                lag = 999;
                return lag;
            }
            else 
            {
                return lag; 
            }
        }

        public bool SendReliableMessage(int host, int connection, byte[] thing) {
            return SendMessage(host, connection, reliableChannelId, thing);
        }

        public bool SendMessage(int host, int connection, int channel, byte[] thing)
        {
            byte error;

            NetworkTransport.Send(
                host, 
                connection, 
                channel, 
                thing, 
                thing.Length, 
                out error);

            if (HasError(error))
            {
                Log.Debug("Error sending message to " + host);
                return false;
            }
            else 
            {
                return true;
            }
        }

        public bool ReceiveData()
        {
            byte error;
            int recvHostId;
            int recvConnectionId;
            int recvChannelId;
            byte[] recvBuffa = new byte[1024];
            int buffaSize = 1024;
            int dataSize;

            NetworkEventType recvNetworkEvent = NetworkTransport.Receive(
                out recvHostId,
                out recvConnectionId,
                out recvChannelId,
                recvBuffa,
                buffaSize,
                out dataSize,
                out error);

            if (HasError(error)) {
                Log.Debug("Couldn't recieve data");
                return false;
            }
            if (dataSize > buffaSize) {
                Log.Debug("received message that overflowed buffa");
                return false;
            }
            switch(recvNetworkEvent)
            {
                case NetworkEventType.Nothing:
                    Log.Debug("No data");
                    return false;
                case NetworkEventType.ConnectEvent:
                    Log.Debug(String.Format("ConnectEvent({0},{1})", recvHostId, recvConnectionId));
                    ConnectionReceived(recvHostId, recvConnectionId);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Log.Debug(String.Format("DisconnectEvent({0},{1})", recvHostId, recvConnectionId));
                    Disconnection(recvHostId, recvConnectionId);
                    break;
                case NetworkEventType.DataEvent:
                    Log.Debug(String.Format("DataEvent({0},{1},{2})", recvHostId, recvConnectionId, recvChannelId));
                    DataReceived(recvHostId, recvConnectionId, recvChannelId, recvBuffa, dataSize);
                    break;
                case NetworkEventType.BroadcastEvent:
                    Log.Debug(String.Format("BroadcastEvent({0})", recvHostId));
                    BroadcastReceived(recvHostId, recvBuffa, dataSize);
                    break;
                default:
                    Log.Error("Unknown network message received");
                    Log.Error(recvNetworkEvent.ToString());
                    break;
            }   
            return true;
        }

        abstract public void ConnectionReceived(int host, int connection);
        abstract public void Disconnection(int host, int connection);
        abstract public void DataReceived(int host, int connection, int channel, byte[] buffer, int size);
        abstract public void BroadcastReceived(int host, byte[] buffer, int size);

    }

    
}