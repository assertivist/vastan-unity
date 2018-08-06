using UnityEngine;
using UnityEngine.Networking;
using Vastan.Game;
using Vastan.Util;
using Vastan.Config;
using Vastan.Sound;

namespace Vastan.Networking
{
    class VastanClient : NetworkedSystem
    {
		public Camera myCamera;
		public PlayerConfig myPlayerConfig;
        Vastan.Sound.Manager soundManager;

		public VastanCharacter myCharacter;

        public VastanClient()
        {
            Init();
            Connect("127.0.0.1");
            Log.Debug("Opened client socket " + socketId);
        }

        public void Update() 
        {
            while(ReceiveData()) {}

        }

        public override void ConnectionReceived(int host, int connection)
        {
            // ???
            Log.Debug("Receiving connection on client?");
        }

        public override void Disconnection(int host, int connection)
        {
            Log.Debug("Receiving disconnection on client?");
        }

        public override void DataReceived(int host, int connection, int channel, byte[] buffer, int size)
        {
            // Message from server
            Log.Debug("Message from server");

        }

        public override void BroadcastReceived(int host, byte[] buffer, int size)
        {
            // Message from server

        }
    }
}