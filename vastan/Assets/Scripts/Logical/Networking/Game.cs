using System.Collections.Generic;
using System.Linq;
using System.IO;
using ServerSideCalculations;
using ServerSideCalculations.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game functionality used by both Client and Server
/// </summary>
public abstract class Game : MonoBehaviour
{
	#region Constants
	
	public const float DEGREES_PER_CALC_ANGLE = 120f;
	
	public const float ATTEMPT_JUMP_DURATION = .2f; // How long to wait for the player to touch the ground after hitting the jump key.  Avoids missed jumps caused by minor dips in the ground.
	public const float JUMP_SPEED = 10.0F;
	public const float GRAVITY = 12.0F;
		
	public const float MIN_TURN = -360F;
	public const float MAX_TURN = 360F;
	public const float MIN_TILT = -60F; // How far the user can look up/down
	public const float MAX_TILT = 60F;
	
	public const float ROUND_LENGTH = .05f; // 20 Rounds per second.  Smaller = more accuracy, but worse performance.

	#endregion
	
	public bool IsActive;

    public string GameLevelName;
    public string GameLevelFile;
    public Level GameLevel;

    // Use this for initialization
    public void Start ()
	{
		#region Network
		RemoteIP = "192.168.1.3";
		RemotePort = 25000;
		ListenPort = 25000;
		RemoteGUId = "";
		UseNat = false;
		#endregion Network

		#region Scenes
		LastLevelPrefix = 0;
		CurrentGameState = GameStates.MainMenu;
        levels = getLevelInfoList();
		#endregion Scenes
		
		//UnityEngine.Debug.Log()
		SceneCharacters = new Dictionary<int, SceneCharacter3D> ();
	}
	
	
	#region Networking
	
	public string RemoteIP { get; set; }
	public int RemotePort { get; set; }
	public int ListenPort { get; set; }
	public string RemoteGUId { get; set; }
	public bool UseNat { get; set; }
	
	
	public void DrawNetworkingGUI ()
	{
		if (!IsActive) {
			return;
		}
		
		GUILayout.Space (10);
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10);
		
		// Display network info
		if (UseNat) {
			GUILayout.Label ("GUId: " + Network.player.guid + " - ");
		}
		
		GUILayout.Label ("Local IP/port: " + Network.player.ipAddress + "/" + Network.player.port);
		GUILayout.Label (" - External IP/port: " + Network.player.externalIP + "/" + Network.player.externalPort);
		GUILayout.EndHorizontal ();
	}
	
	
	public void OnDisconnectedFromServer ()
	{
		// Return to the main menu after disconnecting from the server

		if (!IsActive) {
			return;
		}
		
		CurrentGameState = GameStates.MainMenu;
        SceneManager.LoadScene("level_scene");
        //Application.LoadLevel("level_scene");
	}


	public void RemoveSceneCharacter (int id)
	{
		Debug.Log ("Player disconnected with char ID " + id);
		var sceneChar = SceneCharacters [id];
		if (sceneChar == null) {
			return;
		}

		SceneCharacters.Remove (id);
		sceneChar.GoAway ();
	}
	
	#endregion Networking
	
	
	#region Characters

	public SceneInfo SceneInformation;

	public GameObject PlayerPrefab3D;

	public GameObject AIPrefab3D;

    public GameObject PlasmaPrefab;

	public GameObject PlayerPrefab{ get { return PlayerPrefab3D; } }
	public GameObject AIPrefab{ get { return AIPrefab3D; } }


	public Dictionary<int, SceneCharacter3D> SceneCharacters { get; set; }

    public List<Projectile> Projectiles = new List<Projectile>();
	


    public void LoadSceneInfo ()
	{
		SceneInformation = GameObject.FindObjectOfType<SceneInfo> ();
	}
	/// <summary>
	/// Creates a new in-game SceneCharacter and model with the given Character data
	/// </summary>
	/// <param name="newCharacter">New character.</param>
    ///
	public void InstantiateSceneCharacter (Character newCharacter)
	{
		var charId = newCharacter.Id;

		// Don't try to add an existing character
		if (SceneCharacters.ContainsKey (charId)) {
			return;
		}

		while (Application.isLoadingLevel) {
			Debug.Log ("Waiting for level to load before instantiating character...");
			System.Threading.Thread.Sleep (100);
		}

		// Create a new Player prefab at the spawn point location
		Debug.Log ("Spawn point is " + SceneInformation.PlayerSpawn);

		var prefab = newCharacter.Team == "AI" ? AIPrefab : PlayerPrefab;

        var incarn = GameLevel.get_incarn();

        var playerInstantiation = (GameObject)GameObject.Instantiate(
            prefab,
            incarn.position,
            incarn.rotation);

		var inGameCharacter = playerInstantiation.GetComponent<SceneCharacter>();
		inGameCharacter.BaseCharacter = newCharacter;
        inGameCharacter.PlayerName = newCharacter.CharName;

        var sc3d = playerInstantiation.GetComponent<SceneCharacter3D>();
        sc3d.recolor_walker(new Color(newCharacter.R, newCharacter.G, newCharacter.B));
		
		// Add the new player's character to the list of characters on the server
		Debug.Log ("Adding character " + inGameCharacter.BaseCharacter.CharName + " with ID " + charId);
		SceneCharacters.Add (charId, sc3d);
	}

    public void InstantiatePlasma(SceneCharacter3D character) {
        var pos = character.head.transform.position;
        pos += character.head.transform.forward * 1.1f;
        var plasma = (GameObject)GameObject.Instantiate(
            PlasmaPrefab,
            pos,
            character.head.transform.rotation);
        Projectiles.Add(plasma.GetComponent<Plasma>());
    }

    public void RespawnCharacter (int charId)
	{
		Debug.Log ("Respawning char " + charId);
		var p = SceneInformation.PlayerSpawn.transform.position;
		Debug.Log ("Respawn point is " + p.x + ", " + p.y + ", " + p.z);
		var sceneChar = SceneCharacters [charId];
		sceneChar.transform.position = SceneInformation.PlayerSpawn.transform.position;

		sceneChar.BaseCharacter.IsAlive = true;
		sceneChar.BaseCharacter.CurrentHealth = sceneChar.BaseCharacter.MaxHealth;
	}

	#endregion Characters

	
	
	#region Levels

    public Dictionary<string, Dictionary<string,string>> getLevelInfoList()
    {
        //DirectoryInfo resources = new DirectoryInfo(Application.dataPath + "/Resources/");
        //FileInfo[] fileInfo = resources.GetFiles("*.xml");

        List<string> levelfiles =
        new List<string>{
            "abtf",
            "indra",
            "bwadi"
        };
        
        foreach(var f in levelfiles)
        { 
            levels.Add(f, Level.get_levelinfo(f)); 
        }
        return levels;
    }


    public Dictionary<string, Dictionary<string, string>> levels = new Dictionary<string, Dictionary<string, string>>();

    public int LastLevelPrefix { get; set; }
	public GameStates CurrentGameState;
    public string LevelScene = "level_scene";
    public string MainMenuScene = "Main Menu Scene";
	public enum GameStates :int
	{
		MainMenu,
		LoadingLevel,
		LevelLoaded,
        PlayingMatch,
        MatchOver
    }
	;
	
	#endregion


	#region Combat
	
	public bool FriendlyFire = false;

	#endregion Combat

}

