#pragma warning disable 0618
using System;
using System.Collections.Generic;
using ServerSideCalculations;
using ServerSideCalculations.Characters;
using ServerSideCalculations.Networking;
using UnityEngine;


public class GameServer : Game
{

    #region Initialization

    public string GameLevelName;
    public Level GameLevel;

    // Use this for initialization
    new void Start()
    {
        base.Start();

        RoundsToKeep = Mathf.RoundToInt(1.5f / ROUND_LENGTH);

        Players = new Dictionary<string, Player>();
        Rounds = new Dictionary<int, Round>();

        CommandsToKeep = 50; // Keeps a buffer of 2-3s worth

        PosTolerance = .5f;
        DirTolerance = 5f;
    }

    #endregion


    #region GUI

    public void OnGUI()
    {
        if (!IsActive)
        {
            return;
        }

        DrawNetworkingGUI();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Diff x:" + XDiff + " y:" + YDiff + " z:" + ZDiff);

        foreach (var item in levels)
        {
            if (GUILayout.Button(item.Key))
            {
                LoadLevel(item.Key, LastLevelPrefix + 1);
            }
        }

        if (GUILayout.Button("Exit"))
        {
            this.IsActive = false;
            Network.Disconnect();
            Application.Quit();
        }

        GUILayout.EndHorizontal();
    }

    #endregion


    #region Network Connectivity

    public Dictionary<string, Player> Players { get; set; } // List of players by their network GUId

    // 1: What to do when we initialize a server
    public void OnServerInitialized()
    {
        Debug.Log("useNat = " + UseNat);
        if (UseNat)
        {
            Debug.Log("==> GUId is " + Network.player.guid + ". Use this on clients to connect with NAT punchthrough.");
        }
        Debug.Log("==> Local IP/port is " + Network.player.ipAddress + "/" + Network.player.port + ". Use this on clients to connect directly.");
    }

    int newPlayerId = 1;

    void OnPlayerConnected(NetworkPlayer client)
    {
        if (!IsActive)
        {
            return;
        }
        // 2A: Load the character and assign it to the player
        Player player = new Player(client,
            new Character {
                Id = newPlayerId,
                CharName = "Player" + newPlayerId,
                R = UnityEngine.Random.Range(0, 1),
                G = UnityEngine.Random.Range(0, 1),
                B = UnityEngine.Random.Range(0, 1)
            }); 
        Players.Add(player.Id, player);
        newPlayerId++;


        // 2B: Put the player into the current game, if there is one
        if (CurrentGameState == Game.GameStates.LevelLoaded)
        {
            Debug.Log("Sending load level ");
            Debug.Log(GameLevelName);
            GetComponent<NetworkView>().RPC("ClientLoadLevel", client, GameLevelName, LastLevelPrefix);
            InstantiatePlayer(player);

            foreach (var character in SceneCharacters.Values)
            {
                GetComponent<NetworkView>().RPC("InitNewSceneCharacter", client, character.BaseCharacter.Serialize());
            }
        }

        Debug.Log("Player connected from " + client.ipAddress + ":" + client.port);
    }


    void OnPlayerDisconnected(NetworkPlayer client)
    {
        var charId = Players[client.guid].BaseCharacter.Id;
        Network.RemoveRPCs(client);
        GetComponent<NetworkView>().RPC("PlayerDisconnected", RPCMode.Others, charId);
        Players.Remove(client.guid);
        RemoveSceneCharacter(charId);
    }


    /// <summary>
    /// Gets the player by char identifier.
    /// </summary>
    /// <returns>The player by char identifier.</returns>
    /// <param name="charId">Char identifier.</param>
    public Player GetPlayerByCharId(int charId)
    {
        foreach (Player player in Players.Values)
        {
            if (player.BaseCharacter.Id == charId)
            {
                return player;
            }
        }

        return null;
    }

    #endregion


    #region Level Control

    public bool CreatedObjectsYet { get; set; }

    /// 3: Load a new scene, destroying everything from the previous scene except for the camera and anything attached
    public void LoadLevel(string level, int levelPrefix)
    {
        if (!IsActive)
        {
            return;
        }

        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        Debug.Log("Load level!");

        #region 3A: Tell clients to load this level, and assign them each a player
        Debug.Log("Sending load level");

        GetComponent<NetworkView>().RPC("ClientLoadLevel", RPCMode.Others, level, levelPrefix);

        LastLevelPrefix = levelPrefix;

        // There is no reason to send any more data over the network on the default channel,
        // because we are about to load the level, thus all those objects will get deleted anyway
        Network.SetSendingEnabled(0, false);

        // We need to stop receiving because first the level must be loaded first.
        // Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
        Network.isMessageQueueRunning = false;

        // All network views loaded from a level will get a prefix into their NetworkViewID.
        // This will prevent old updates from clients leaking into a newly created scene.
        Network.SetLevelPrefix(levelPrefix);
        //Application.LoadLevel (level);
        UnityEngine.SceneManagement.SceneManager.LoadScene("level_scene");

        GameLevelName = level;

        Network.isMessageQueueRunning = true;// Allow receiving data again
        Network.SetSendingEnabled(0, true);// Now the level has been loaded and we can start sending out data to clients

        foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
        }
        #endregion 3A

        // 3B: Set the state to loading scene so we know to create the objects instead of updating them
        CurrentGameState = Game.GameStates.LoadingLevel;

        Debug.Log("Done with initial level loac");
    }

    /// <summary>
    /// Creates the objects.
    /// </summary>
    private void CreateObjects()
    {
        LoadSceneInfo();

        if (SceneInformation == null)
        {
            // Scene is not loaded yet, wait for next round
            return;
        }

        Debug.Log("Time to create world objects");

        //TODO: REmove? Position the camera
        GameObject mainCamera = GameObject.FindGameObjectWithTag( "MainCamera" );
		Vector3 pos = new Vector3(20, 20, 20);
		pos.y += 25;
		mainCamera.transform.position = pos;
        mainCamera.transform.LookAt(Vector3.zero);

        GameLevel = new Level();
        GameLevel.load(GameLevelName);
        GameObject level_object = GameLevel.game_object();
        level_object.transform.parent = SceneInformation.transform;

        Debug.Log("Me: " + GetComponent<NetworkView>().owner.guid);

        // 4: Create a character for each player
        foreach (Player player in Players.Values)
        {
            Debug.Log("Creating character " + player.BaseCharacter.CharName + " for player: " + player.Id);
            InstantiatePlayer(player);
        }

        Debug.Log("Level: " + Application.loadedLevelName);

        // 5: Create an AI to attack the players
        InstantiateAI();

        // 6A: Set the state to loaded so we know we can start updating the game normally
        CurrentGameState = Game.GameStates.LevelLoaded;
    }

    /// <summary>
    /// Instantiates a player
    /// </summary>
    /// <param name="player">Player</param>
    private void InstantiatePlayer(Player player)
    {
        var newCharacter = player.BaseCharacter;
        var charId = newCharacter.Id;

        // 4A: Tell all clients to create an instance of each player's character
        GetComponent<NetworkView>().RPC("InitNewSceneCharacter", RPCMode.Others, newCharacter.Serialize());

        // 4B: Create each player's character on the server
        InstantiateSceneCharacter(newCharacter);

        // 4C: Assign each player to their newly created character
        player.InGameCharacter = SceneCharacters[charId];

        Debug.Log("Assigning " + player.InGameCharacter.name + " to " + player.Id + " at " + player.NetworkPlayerLocation + ", " + player.NetworkPlayerLocation.guid + " and move speed " + ((Character)player.InGameCharacter).MoveSpeed);
        GetComponent<NetworkView>().RPC("AssignPlayerCharacter", player.NetworkPlayerLocation, charId, player.InGameCharacter.BaseCharacter.Serialize());
    }


    private int AICount = 0;
    private const int AI_ID_INDEX = 1000;

    /// <summary>
    /// Instantiates an AI to attack players
    /// </summary>
    private void InstantiateAI()
    {
        Debug.Log("instantiating AI");
        Debug.Log("spawn point is " + SceneInformation.AiSpawn);

        var aiInstantiation = (GameObject)GameObject.Instantiate(
            AIPrefab,
            SceneInformation.AiSpawn.position,
            SceneInformation.AiSpawn.rotation);
        Debug.Log("AI Instantiation: " + aiInstantiation);

        IArtificialIntelligence aiSceneCharacter = null;

        aiSceneCharacter = aiInstantiation.GetComponent<AI3D>();
       

        if (aiSceneCharacter == null)
        {
            throw new InvalidOperationException("AI prefab needs an AI component that matches the 3D/2D aspect of the game");
        }

        ////Debug.Log ("AI scene char: " + aiSceneCharacter);	

        int charId = AI_ID_INDEX + AICount;
        AICount++;

        aiSceneCharacter.Server = this;
        ((Character)aiSceneCharacter.GetSceneChar()).Team = "AI";
        ((Character)aiSceneCharacter.GetSceneChar()).Id = charId;

        Debug.Log("Adding AI character " + aiSceneCharacter.BaseCharacter.CharName + " with ID " + charId);

        SceneCharacters.Add(charId, aiSceneCharacter.GetSceneChar());
        foreach (var charId2 in SceneCharacters.Keys)
        {
            Debug.Log("Scene characters contains key: " + charId2);
        }

        GetComponent<NetworkView>().RPC("InitNewSceneCharacter", RPCMode.Others, aiSceneCharacter.BaseCharacter.Serialize());
    }

    #endregion


    #region Round Processing

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        ////Debug.Log ("Update");

        if (!IsActive)
        {
            ////Debug.Log ("Not active!");
            return;
        }

        ////Debug.Log ("Checking state: " + CurrentGameState);

        if (CurrentGameState == GameStates.LevelLoaded)
        {
            ////Debug.Log ("Objects already created, so create a new round");
            CreateRound();
        }
        else if (CurrentGameState == Game.GameStates.LoadingLevel)
        {

            ////Debug.Log ("Loading, so create the objects");

            // 3C: Give the server time to load the scene before creating objects
            CreateObjects();
        }
    }

    public bool BandwidthReduction = true;
    public Dictionary<int, Round> Rounds { get; set; }
    public float TimeLastRoundSent { get; set; }
    public int CurrentRound { get; set; }
    public int RoundsToKeep { get; set; }

    /// <summary>
    /// Creates the round.
    /// </summary>
    private void CreateRound()
    {
        // 6B: Wait until it's time to create a new round.  20x per second or so.
        if (BandwidthReduction && (Time.time < TimeLastRoundSent + ROUND_LENGTH || CurrentGameState != Game.GameStates.LevelLoaded))
        {
            return;
        }

        //===== Start a new round =====
        //6C: Mark the number and time for the new round
        CurrentRound++;
        TimeLastRoundSent = Time.time;
        Round newRound = new Round(CurrentRound, TimeLastRoundSent);

        // Update all charater locations
        foreach (SceneCharacter3D character in SceneCharacters.Values)
        {
            // Make sure nobody falls through the ground
            if (character.transform.position.y < 0)
            {
                character.transform.position = new Vector3(
                    character.transform.position.x,
                    10f,
                    character.transform.position.z
                );
            }

            // 6D: Save the character state in the round
            ObjectState characterState = character.GetCurrentState();
            newRound.CurrentObjectStates.Add(((Character)character).Id, characterState);

            // 7: Send the character's state to all players
            ////Debug.Log ("Updating char " + character.BaseCharacter.Id + character.transform.ToCoordinates ());
            GetComponent<NetworkView>().RPC("UpdateCharacter", RPCMode.Others, character.BaseCharacter.Id, character.transform.position, character.transform.localEulerAngles.y, character.walking, character.head.transform.localRotation, character.state.velocity, character.state.momentum, character.crouch_factor);
        }


        // 8: Send the new round to players
        GetComponent<NetworkView>().RPC("NewRound", RPCMode.Others, newRound.RoundNumber);

        // Remove old round(s)
        int[] roundNums = new int[Rounds.Keys.Count];
        Rounds.Keys.CopyTo(roundNums, 0);
        foreach (int oldRound in roundNums)
        {
            if (oldRound < CurrentRound - RoundsToKeep)
            {
                Rounds.Remove(oldRound);
            }
        }

        // Add this round to the list of recent rounds
        Rounds.Add(CurrentRound, newRound);
    }


    /// <summary>
    /// This is called by a Client player to acknowledge that it has received the given round number
    /// </summary>
    [RPC]
    public void RespondToRound(int roundNum, NetworkMessageInfo info)
    {
        //Debug.Log( "Player " + info.sender.guid + " responding to round " + roundNum );

        if (!Players.ContainsKey(info.sender.guid))
        {
            Debug.Log("Players of size " + Players.Count + " does not contain key " + info.sender.guid);
            return;
        }

        // 8C: Server takes not of the last round that was responded to by the player
        Players[info.sender.guid].RoundLastRespondedTo = roundNum;
    }

    #endregion


    #region Client Movement

    public float XDiff { get; set; }
    public float YDiff { get; set; }
    public float ZDiff { get; set; }
    public float AngDiff { get; set; }
    /*public float XAngDiff { get; set; }
    public float YAngDiff { get; set; }
    public float ZAngDiff { get; set; }*/

    public int CommandsToKeep { get; set; }

    /// <summary>
    /// This is called by a Client player to request move/look control
    /// </summary>
    [RPC]
    public void RequestControl(byte[] controlCommand, NetworkMessageInfo info)
    {
        Player player = Players[info.sender.guid];

        var control = (ControlCommand)controlCommand.Deserialize();

        // 10C: Run the control command
        player.InGameCharacter.ExecuteControlCommand(control);

        // 10D: Record the player's state after running the control command
        ObjectState characterState = player.InGameCharacter.GetCurrentState();
        player.StatesAfterControls.Add(control.Id, characterState);
        player.LastControlNumApplied = control.Id;
    }


    public float PosTolerance { get; set; }
    public float DirTolerance { get; set; }

    /// <summary>
    /// This is called periodically by a Client player 
    /// to validate their position and receive corrections
    /// </summary>
    [RPC]
    public void ValidatePosition(int lastCorrectionRepondedTo, int lastControlApplied, Vector3 clientPosition, float clientAngle, NetworkMessageInfo info)
    {
        Player player = Players[info.sender.guid];

        // 11B: Make sure the client has used the last correction we sent
        if (lastCorrectionRepondedTo < player.LastCorrectionSent)
        {
            return;
        }

        #region 11C: Get the difference between the player's state and the server's
        ObjectState serverStateAfterControl = player.StatesAfterControls[lastControlApplied];
        if (serverStateAfterControl == null)
        {
            return;
        }

        XDiff = clientPosition.x - serverStateAfterControl.Position.x;
        YDiff = clientPosition.y - serverStateAfterControl.Position.y;
        ZDiff = clientPosition.z - serverStateAfterControl.Position.z;

        //XAngDiff = clientDirection.x - serverStateAfterControl.Forward.x;
        //YAngDiff = clientDirection.y - serverStateAfterControl.Forward.y;
        //ZAngDiff = clientDirection.z - serverStateAfterControl.Forward.z;

        AngDiff = clientAngle - serverStateAfterControl.Angle;

        Debug.Log(
            "Diff: " + XDiff.ToString() +
            ", " + YDiff.ToString() +
            ", " + ZDiff.ToString() +
            ", " + AngDiff.ToString()
        );
        #endregion 11C

        #region 11D: If the player is out of tolerance, then send a correction
        if (Mathf.Abs(XDiff) > PosTolerance ||
            Mathf.Abs(YDiff) > PosTolerance ||
            Mathf.Abs(ZDiff) > PosTolerance ||
            Mathf.Abs(AngDiff) > DirTolerance )
            //Mathf.Abs(XAngDiff) > DirTolerance ||
            //Mathf.Abs(YAngDiff) > DirTolerance ||
            //Mathf.Abs(ZAngDiff) > DirTolerance)
        {
            Debug.Log("Sending correction...");
            player.LastCorrectionSent = lastControlApplied;

            // Send the current position
            // If we send the correction to the old position, it forces the client to apply more contorl commands on its own, creating a larger margin of error
            SceneCharacter3D scenechar = (SceneCharacter3D)player.InGameCharacter;
            GetComponent<NetworkView>().RPC("CorrectPosition", info.sender, player.LastControlNumApplied, player.InGameCharacter.state.pos, scenechar.state.velocity, scenechar.state.momentum, player.InGameCharacter.state.angle, scenechar.state.velocity, scenechar.crouch_factor);
        }
        #endregion 11D
    }

    #endregion
    /*
    [RPC]
    public void ClientColor(Color color, NetworkMessageInfo info)
    {
        Player player = Players[info.sender.guid];
        player.BaseCharacter.Color = color;
        player.InGameCharacter.recolor();
        GetComponent<NetworkView>().RPC("UpdateCharacterColor", RPCMode.Others, player.BaseCharacter.Id, color);
    }
    */
    public void ClientName(byte[] client_name_s, NetworkMessageInfo info)
    {
        string client_name = (string)client_name_s.Deserialize();
        Player player = Players[info.sender.guid];
        player.InGameCharacter.PlayerName = client_name;
        GetComponent<NetworkView>().RPC("UpdateCharacterName", RPCMode.Others, player.BaseCharacter.Id, client_name_s);
    }


    #region Combat

    public bool LatencyCompensation = true;
    public bool AttackerMoved = false;
    public bool TargetMoved = false;

    [RPC]
    public void RequestProjectile(NetworkMessageInfo info) {
        if (!IsActive) return;
        var attacker_player = Players[info.sender.guid];
        Character attacker = attacker_player.BaseCharacter;
        SceneCharacter scene_attacker = SceneCharacters[attacker.Id];



        var can_fire = false;

        if (can_fire) {
            GetComponent<NetworkView>().RPC("InstantiateProjectile", RPCMode.Others, attacker.Id);
        }
        
    }

    [RPC]
    // 14: Server processes attack request
    public void RequestAttack(NetworkMessageInfo info)
    {
        if (!IsActive)
        {
            return;
        }

        AttackerMoved = false;

        //Debug.Log ("Client is requesting an attack");
        var attackingPlayer = Players[info.sender.guid];
        Character attacker = attackingPlayer.BaseCharacter;
        SceneCharacter3D sceneAttacker = SceneCharacters[attacker.Id];

        // 14A: Server checks attack validation
        if (!attacker.IsAlive)
        {
            return;
        }

        // 14B: Server sends attack message to all clients
        GetComponent<NetworkView>().RPC("ReceiveCharacterAttacks", RPCMode.Others, attacker.Id);

        // 16: Server calculates attack results
        #region Determine Hit

        var roundWhenPlayerAttacked = Rounds[attackingPlayer.RoundLastRespondedTo];

        // 16A: Server rewinds the attacker to its position when the client attacked
        #region Rewind Attacker State
        var presentAttackerState = new ObjectState(0,
            sceneAttacker.transform.position,
            sceneAttacker.state.angle,
            Quaternion.identity,
            sceneAttacker.state.velocity,
            sceneAttacker.state.momentum,
            sceneAttacker.crouch_factor,
            sceneAttacker.walking
        );

        if (LatencyCompensation)
        {
            Debug.Log("Rewinding attacker");
            var oldAttackerState = roundWhenPlayerAttacked.CurrentObjectStates[attacker.Id];
            sceneAttacker.transform.position = oldAttackerState.Position;
            sceneAttacker.state.angle = oldAttackerState.Angle;
            sceneAttacker.state.velocity = oldAttackerState.Velocity;
            sceneAttacker.state.momentum = oldAttackerState.Momentum;
            sceneAttacker.crouch_factor = oldAttackerState.CrouchFactor;
        }
        #endregion Rewind Attacker State

        foreach (var potentialTarget in SceneCharacters.Values)
        {

            TargetMoved = false;

            // Check friendly fire
            if (!FriendlyFire && ((Character)potentialTarget).Team == attacker.Team)
            {
                continue;
            }

            // 16B: Server rewinds each potential target to its position when the client attacked
            #region Rewind Target State
            var presentTargetState = new ObjectState(0, 
                potentialTarget.transform.position, 
                potentialTarget.state.angle, 
                Quaternion.identity, 
                potentialTarget.state.velocity, 
                potentialTarget.state.momentum,
                potentialTarget.crouch_factor,
                potentialTarget.walking
            );

            if (LatencyCompensation)
            {
                Debug.Log("Rewinding target");
                var oldTargetState = roundWhenPlayerAttacked.CurrentObjectStates[((Character)potentialTarget).Id];
                potentialTarget.transform.position = oldTargetState.Position;
                potentialTarget.state.angle = oldTargetState.Angle;
                potentialTarget.state.velocity = oldTargetState.Velocity;
                potentialTarget.state.momentum = oldTargetState.Momentum;
                potentialTarget.crouch_factor = oldTargetState.CrouchFactor;
            }
            #endregion Rewind Target State

            // 16C: Server checks attack logic
            if (potentialTarget.transform.IsWithinArc(sceneAttacker.transform, attacker.ArmLength + 2f, 120f))
            {
                CharacterHits(attacker, (Character)potentialTarget); // 16D-E
            }

            // 16F: Server returns each potential target to its current position
            #region Reset Target State
            if (LatencyCompensation && !TargetMoved)
            {
                Debug.Log("Unwinding target");
                potentialTarget.transform.position = presentTargetState.Position;
                potentialTarget.state.angle = presentTargetState.Angle;
                potentialTarget.state.velocity = presentTargetState.Velocity;
                potentialTarget.state.momentum = presentTargetState.Momentum;
                potentialTarget.crouch_factor = presentTargetState.CrouchFactor;
            }
            #endregion Reset Target State
        }

        #region Reset Attacker State

        // 16G: Server returns the attacker to the current position
        if (LatencyCompensation && !AttackerMoved)
        {
            Debug.Log("Unwinding attacker");
            sceneAttacker.transform.position = presentAttackerState.Position;
            sceneAttacker.state.angle = presentAttackerState.Angle;
            sceneAttacker.state.velocity = presentAttackerState.Velocity;
            sceneAttacker.state.velocity = presentAttackerState.Momentum;
            sceneAttacker.crouch_factor = presentAttackerState.CrouchFactor;
        }
        #endregion Reset Attacker State

        #endregion Determine Hit
    }

    public void CharacterHits(Character attacker, Character victim)
    {
        // 16D: Server sends attack results to all clients
        Debug.Log("Character " + attacker.CharName + " hit " + victim.CharName + "!!!");
        GetComponent<NetworkView>().RPC("ReceiveCharacterHits", RPCMode.Others, attacker.Id, victim.Id);

        // 16E: Client and server perform hit effects
        victim.CurrentHealth -= 10;

        if (victim.CurrentHealth <= 0f)
        {
            CharacterDies(victim);
        }
    }


    public void CharacterDies(Character character)
    {
        Debug.Log("Character " + character.CharName + " died!");

        character.IsAlive = false;

        RespawnCharacter(character.Id);
        TargetMoved = true;

        var player = GetPlayerByCharId(character.Id);
        if (player != null)
        {
            ObjectState characterState = player.InGameCharacter.GetCurrentState();
            player.StatesAfterControls[player.LastControlNumApplied] = characterState;
        }

        GetComponent<NetworkView>().RPC("ReceiveCharacterDies", RPCMode.Others, character.Id);
    }

    #endregion Combat

}

