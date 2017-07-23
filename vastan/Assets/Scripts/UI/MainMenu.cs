using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using ServerSideCalculations.Networking;
using ServerSideCalculations;

public class MainMenu : MonoBehaviour
{

    public GameObject title_quad;
    public GameObject ui_panel;

    public Button connect;
    public Button start_server;
    public Button exit;

    public InputField nickname;
    public InputField server;
    public InputField port;
    public InputField remote_guid;

    public Toggle client_nat;
    public Toggle server_nat;

    RawImage image;
    MovieTexture movie;
    bool fade_started = false;
    bool fade_over = false;

    string remoteIP = "localhost";

    int remotePort = 25000;
    int listenPort = 25000;
    string player_name = "dummy";
    string remoteGUID = "";
    bool useNat = false;
    public GameServer gameServer;
    public GameClient gameClient;
    public ColorPicker colorPicker;
		
	// Do this when the object is first created
	public void Awake ()
	{
		// Network level loading is done in a separate channel.
		DontDestroyOnLoad (this);

		GetComponent<NetworkView>().group = 1;

        image = title_quad.GetComponent<RawImage>();
        movie = (MovieTexture)image.texture;
        movie.Play();

        ui_panel.SetActive(false);

        exit.onClick.AddListener(ExitClicked);
        start_server.onClick.AddListener(StartServerClicked);
        connect.onClick.AddListener(ConnectClicked);
	}

	// Draw GUI objects every frame
	public void OnGUI ()
	{
		//DrawDBGui();
        if (!movie.isPlaying && !fade_started) {
            StartCoroutine("Fade");
            fade_started = true;
        }
        if (fade_over && ui_panel) {
            ui_panel.SetActive(true);
        }

        if (Network.peerType != NetworkPeerType.Disconnected && ui_panel) {
            ui_panel.SetActive(false);
        }

        remoteIP = server.textComponent.text;
        remotePort = int.Parse(port.textComponent.text);
        gameClient.MyName = nickname.textComponent.text;
        gameClient.UseNat = client_nat.isOn;
        gameServer.UseNat = server_nat.isOn;
        remoteGUID = remote_guid.textComponent.text;

        //gameClient.MyColor = colorPicker.CurrentColor;
    }

    IEnumerator Fade() {
        for (float f = 1f; f >= 0; f -= 0.1f) {
            Color c = image.color;
            c.r = c.g = c.b = f;
            image.color = c;
            yield return null;
        }
        fade_over = true;
    }


    public void ExitClicked() {
        Debug.Log("Exit clicked");
        Application.Quit();
    }

    public void ConnectClicked() {
        if (gameClient.UseNat) {
            if (remoteGUID == null) {
                Debug.LogWarning("InvalId GUId given, must be a valId one as reported by Network.player.guid or returned in a HostData struture from the master server");
                return;
            }
            else {
                Network.Connect(remoteGUID);
            }
        }
        else {
            Network.Connect(remoteIP, remotePort);
        }
        
        gameClient.IsActive = true;

        ui_panel.SetActive(false);
        title_quad.SetActive(false);
    }

    public void StartServerClicked() {
        //gameServer.useNat = useNat;
        Network.InitializeServer(32, listenPort, gameServer.UseNat);
        gameServer.IsActive = true;

        // Notify our objects that the level and the network is ready
        foreach (GameObject go in FindObjectsOfType(typeof(GameObject))) {
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
        }
        ui_panel.SetActive(false);
        title_quad.SetActive(false);
    }
}
