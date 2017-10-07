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

	public GameObject PlayerPrefab{ get { return PlayerPrefab3D; } }
	public GameObject AIPrefab{ get { return AIPrefab3D; } }


	public Dictionary<int, SceneCharacter3D> SceneCharacters { get; set; }

    public List<Plasma> Plasmas { get; set; }
	


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

		var playerInstantiation = (GameObject)GameObject.Instantiate (prefab, SceneInformation.PlayerSpawn.position, SceneInformation.PlayerSpawn.rotation);
		var inGameCharacter = playerInstantiation.GetComponent<SceneCharacter> ();
		inGameCharacter.BaseCharacter = newCharacter;
        inGameCharacter.PlayerName = newCharacter.CharName;

        var sc3d = playerInstantiation.GetComponent<SceneCharacter3D>();
        //recolor_walker(sc3d.walker, newCharacter.GetColor());
		
		// Add the new player's character to the list of characters on the server
		Debug.Log ("Adding character " + inGameCharacter.BaseCharacter.CharName + " with ID " + charId);
		SceneCharacters.Add (charId, sc3d);
	}

    public void InstantiatePlasma(SceneCharacter3D attacker) {
        var p = new Plasma();
    }

    public static void recolor_walker(GameObject w, Color c) {
        recolor_object(w, "Walker.Head.Glass", new Color(56f / 255f, 69f / 255f, 188f / 255f));
        recolor_object(w, "Walker.Head.Tubes", new Color(75f / 255f, 71f / 255f, 71f / 255f));
        recolor_object(w, "Walker.Head.Main", c);
        recolor_object(w, "Walker.Leg.High.Left", c);
        recolor_object(w, "Walker.Leg.High.Right", c);
        recolor_object(w, "Walker.Leg.Low.Left", c);
        recolor_object(w, "Walker.Leg.Low.Right", c);
    }

    static void recolor_object(GameObject parent, string name, Color c) {
        GameObject go = parent.transform.Find("walker_orig/" + name).gameObject;
        Mesh m = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = colors.ToArray();
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

