using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ServerSideCalculations.Networking;
using ServerSideCalculations;



public class MainMenu : MonoBehaviour {

    // Do this when the object is first created
    public void Awake() {
        // Network level loading is done in a separate channel.
        DontDestroyOnLoad(this);

        GetComponent<NetworkView>().group = 1;

        colorPicker.onValueChanged.AddListener(color => {
            gameClient.MyColor = colorPicker.CurrentColor;
        });
        gameClient.MyColor = colorPicker.CurrentColor;
    }


    // Draw GUI objects every frame
    public void OnGUI() {
        DrawNetworkingGUI();
        //DrawDBGui();
    }

    string remoteIP = "localhost";

    int remotePort = 25000;
    int listenPort = 25000;
    string player_name = "dummy";
    string remoteGUId = "";
    bool useNat = false;
    public GameServer gameServer;
    public GameClient gameClient;
    public ColorPicker colorPicker;

    private void DrawNetworkingGUI() {
        GUILayout.BeginHorizontal();

        if (Network.peerType == NetworkPeerType.Disconnected) {
            // Not connected to a server
            useNat = GUILayout.Toggle(useNat, "Use NAT punchthrough");
            gameClient.UseNat = useNat;
            gameServer.UseNat = useNat;

            // Connect to an existing server
            if (GUILayout.Button("Connect")) {
                if (useNat) {
                    if (remoteGUId == null) {
                        Debug.LogWarning("InvalId GUId given, must be a valId one as reported by Network.player.guid or returned in a HostData struture from the master server");
                        return;
                    }
                    else {
                        Network.Connect(remoteGUId);
                    }
                }
                else {
                    Network.Connect(remoteIP, remotePort);
                }

                //gameClient.useNat = useNat;
                gameClient.IsActive = true;
                //gameClient.MyColor = colorPicker.CurrentColor;
            }

            // Start a new server
            if (GUILayout.Button("Start Server")) {
                //gameServer.useNat = useNat;
                Network.InitializeServer(32, listenPort, useNat);
                gameServer.IsActive = true;

                // Notify our objects that the level and the network is ready
                foreach (GameObject go in FindObjectsOfType(typeof(GameObject))) {
                    go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
                }
            }

            if (useNat) {
                remoteGUId = GUILayout.TextField(remoteGUId, GUILayout.MinWidth(145));
            }
            else {
                remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(100));
                remotePort = int.Parse(GUILayout.TextField(remotePort.ToString()));
                gameClient.MyName = GUILayout.TextField(player_name);
            }

            if (GUILayout.Button("Exit from Main Menu")) {
                Application.Quit();
            }
        }
        
        if (GUILayout.Button("Weapons test  scene - April 2018")) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("weapons_test");
        }
    }

    public void SetColor(Color c) {
        gameClient.MyColor = c;
    }
}



