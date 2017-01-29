#pragma warning disable 0618

using System.Collections.Generic;
using ServerSideCalculations;
using ServerSideCalculations.Characters;
using ServerSideCalculations.Networking;
using UnityEngine;
using System.Linq;
//using UnityEngine.SceneManagement;

public class GameClient : Game
{

    #region Attributes

    public Camera MyCamera;

    //public Color MyColor;
    public string MyName;

    public Character MyCharacter;
    public SceneCharacter MyPlayer;

    public Level GameLevel;
    public string GameLevelFile;

    public List<Character> CharactersToInstantiate = new List<Character>();

    public Dictionary<int, ControlCommand> ControlCommands { get; set; }
    public int CurrentControlCommandId { get; set; }
    public ControlCommand CurrentControlCommand { get; set; }

    #endregion


    #region Initialization

    new void Start()
    {
        base.Start();

        ControlCommands = new Dictionary<int, ControlCommand>();

        LoadSounds();

        CharacterIntendedStates = new Dictionary<int, ObjectState>();
        CharacterPositionDiffs = new Dictionary<int, Vector3>();
        CharacterDirectionDiffs = new Dictionary<int, Vector3>();

        //CameraDistance = 3.0f;
        //CameraHeight = 2.0f;
        //CameraHeightDamping = 2.0f;
        //CameraRotationDamping = 10f;
        //CameraTurnCorrectionSpeed = .5f;
    }

    #endregion


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (!IsActive)
        {
            return;
        }

        Debug.Log("Level: " + Application.loadedLevelName + "\n MyCharacterx: " + MyCharacter);
        if (MyCharacter == null || Application.loadedLevelName == MainMenuScene)
        {
            return;
        }

        if (SceneInformation == null)
        {
            LoadSceneInfo();
            GameLevel = new Level();
            GameLevel.load(GameLevelFile);
            GameObject level_object = GameLevel.game_object();
            level_object.transform.parent = SceneInformation.transform;
        }



        if (CharactersToInstantiate.Count > 0)
        {
            foreach (var character in CharactersToInstantiate)
            {
                InstantiateSceneCharacter(character);
            }

            CharactersToInstantiate.Clear();
        }

        if (MyPlayer == null && SceneCharacters != null && SceneCharacters.ContainsKey(MyCharacter.Id))
        {
            MyPlayer = SceneCharacters[MyCharacter.Id];
            MyPlayer.Client = this;
            MyPlayer.BaseCharacter = MyCharacter;
            MyPlayer.tag = "Player";
            //MyPlayer.BaseCharacter.Color = MyColor;
            //MyPlayer.recolor();
        }

        if (CurrentGameState != GameStates.LevelLoaded ||
            MyPlayer == null)
        {
            return;
        }

        InterpolateCharacters();
        UpdateControls();
        MovePlayer();
        UpdateCamera();
        UpdateCombatControls();
        ValidateMyPosition();
    }


    #region Synchronization with Server

    public int RoundLastReceived { get; set; }
    public float TimeRoundLastReceived { get; set; }

    [RPC]
    public void NewRound(int newRound)
    {
        //8A: Client takes note of the new round and time that it was received
        RoundLastReceived = newRound;
        TimeRoundLastReceived = Time.time;

        // 8B: Client lets the server know that it received the new round notification
        GetComponent<NetworkView>().RPC("RespondToRound", RPCMode.Server, newRound);
    }



    public bool CharacterInterpolation = true;
    public Dictionary<int, ObjectState> CharacterIntendedStates { get; set; }

    //TODO: This may just need to have jump in it...
    [RPC]
    public void UpdateCharacter(int charId, Vector3 position, Vector3 direction, Vector3 moveDirection, Quaternion headRot)
    {
        ////Debug.Log ("Update called for " + charId + " at " + position.x + ", " + position.y + ", " + position.z);

        // Don't update self if using client-side prediction
        if (CurrentGameState != Game.GameStates.LevelLoaded
            || MyPlayer == null
            || (ClientSidePredition && charId.Equals(((Character)MyPlayer).Id))
            || !SceneCharacters.ContainsKey(charId)
            )
        {
            return;
        }

        SceneCharacter character = SceneCharacters[charId];
        ObjectState charState = new ObjectState(charId, position, direction, headRot);
        character.MoveDirection = moveDirection;

        // TODO: fix this to call a special method just updating the legs
        character.GetComponent<SceneCharacter3D>().Move(1f, 0f, Time.deltaTime, false);

        if (!CharacterInterpolation)
        {
            character.transform.position = position;
            character.transform.forward = direction;
            return;
        }

        // 7A) Store the character state from the server
        if (CharacterIntendedStates.ContainsKey(charId))
        {
            CharacterIntendedStates.Remove(charId);
            CharacterPositionDiffs.Remove(charId);
            CharacterDirectionDiffs.Remove(charId);
        }

        CharacterIntendedStates.Add(charId, charState);

        // 7B) Calculate the position difference
        Vector3 currentCharPosition = character.transform.position;
        Vector3 intendedCharacterPosition = charState.Position;
        CharacterPositionDiffs.Add(charId, new Vector3(
                    intendedCharacterPosition.x - currentCharPosition.x,
                    intendedCharacterPosition.y - currentCharPosition.y,
                    intendedCharacterPosition.z - currentCharPosition.z
        ));

        // Calculate the direction difference
        Vector3 currentCharDirection = character.transform.forward;
        Vector3 intendedCharacterDirection = charState.Forward;
        CharacterDirectionDiffs.Add(charId, new Vector3(
                    intendedCharacterDirection.x - currentCharDirection.x,
                    intendedCharacterDirection.y - currentCharDirection.y,
                    intendedCharacterDirection.z - currentCharDirection.z
        ));
    }

    [RPC]
    public void UpdateCharacterName(int charId, byte[] name)
    {
        SceneCharacter character = SceneCharacters[charId];
        string player_name = (string)name.Deserialize();
        character.name = player_name;
    }
    /*
    [RPC]
    public void UpdateCharacterColor(int charId, Color color)
    {
        SceneCharacter character = SceneCharacters[charId];
        character.BaseCharacter.Color = color;
        character.recolor();
    }*/

    public Dictionary<int, Vector3> CharacterPositionDiffs { get; set; }
    public Dictionary<int, Vector3> CharacterDirectionDiffs { get; set; }

    // 7D: Move each character closer toward its intended location
    public void InterpolateCharacters()
    {
        foreach (int charId in CharacterIntendedStates.Keys)
        {
            SceneCharacter character = SceneCharacters[charId];
            float portionOfDiffToMove = Mathf.Min(Time.deltaTime / ROUND_LENGTH, 1f);

            // Interpolate toward the intended position
            Vector3 curentPosition = character.transform.position;
            Vector3 positionDiff = CharacterPositionDiffs[charId];

            float newXPos = curentPosition.x + (positionDiff.x * portionOfDiffToMove);
            float newYPos = curentPosition.y + (positionDiff.y * portionOfDiffToMove);
            float newZPos = curentPosition.z + (positionDiff.z * portionOfDiffToMove);

            character.transform.position = new Vector3(
                newXPos,
                newYPos,
                newZPos
            );

            // Interpolate toward the intended direction
            Vector3 curentDirection = character.transform.forward;
            Vector3 directionDiff = CharacterDirectionDiffs[charId];

            float newXDir = curentDirection.x + (directionDiff.x * portionOfDiffToMove);
            float newYDir = curentDirection.y + (directionDiff.y * portionOfDiffToMove);
            float newZDir = curentDirection.z + (directionDiff.z * portionOfDiffToMove);

            character.transform.forward = new Vector3(
                newXDir,
                newYDir,
                newZDir
            );
        }
    }


    /**
	* If the player is moving, valIdate the player's position
	*/
    //Transform previousTransform = player.transform;
    public bool PositionCorrection = true;
    public float ValidatePositionInterval;
    private float LastTimeValidatedPosition;
    private int LastCorrectionRespondedTo;

    // 11: Valdidate the player character's position with the server
    private void ValidateMyPosition()
    {
        if (MyPlayer.MissingController() || !PositionCorrection)
        {
            return;
        }

        ////Debug.Log ("Current speed: " + MyPlayer.GetCurrentSpeed ());
        // 11A: Make a validation requests multiple times per second as long as the player is moving
        if (MyPlayer.GetCurrentSpeed() >= .1 && Time.time >= (LastTimeValidatedPosition + ValidatePositionInterval))
        {
            //Debug.Log( "Validating my position @ " + Time.time );
            Debug.Log("Validating my position " + MyPlayer.GetCurrentSpeed());
            GetComponent<NetworkView>().RPC("ValidatePosition", RPCMode.Server, LastCorrectionRespondedTo, CurrentControlCommandId, MyPlayer.transform.position, MyPlayer.transform.forward);
            LastTimeValidatedPosition = Time.time;
        }
    }


    /**
	 * 12: Reposition the player to match the server
	*/
    [RPC]
    public void CorrectPosition(int lastControlCommandApplied, Vector3 correctPosition, Vector3 correctMomentum, Vector3 correctDirection)
    {
        ////Debug.Log (MyPlayer.networkView.viewID + ") " + "Correcting my position");
        LastCorrectionRespondedTo = lastControlCommandApplied;

        // 12A: Reposition the player to match when the server sent the correction 
        MyPlayer.transform.position = correctPosition;
        MyPlayer.transform.forward = correctDirection;

        // 12B: Make up for control commands which were sent between when the server sent the correction and now
        ////Debug.Log (MyPlayer.networkView.viewID + ") " + "Applying missing commands, starting with " + (lastControlCommandApplied + 1) + ", up to " + CurrentControlCommandId);
        while (ControlCommands.ContainsKey(lastControlCommandApplied + 1))
        {
            Debug.Log(MyPlayer.GetComponent<NetworkView>().viewID + ") " + "Applying CC " + lastControlCommandApplied + 1);
            MyPlayer.ExecuteControlCommand(ControlCommands[lastControlCommandApplied + 1]);
            lastControlCommandApplied++;
        }

        UpdateCamera();
    }

    #endregion Synchronization with server


    #region Connection

    // What to do when we connect to a server
    public void OnConnectedToServer()
    {

        Debug.Log("Sucessfully connected to Server");

        //var nv = GetComponent<NetworkView>();
        //nv.RPC("ClientColor", RPCMode.Server, MyColor);
        //nv.RPC("ClientName", RPCMode.Server, MyName.Serialize());

        if (!IsActive)
        {
            return;
        }

        // Notify our objects that the level and the network is ready
        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
        {
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
        }
    }

    [RPC]
    public void PlayerDisconnected(int id)
    {
        if (CharacterIntendedStates.ContainsKey(id))
        {
            CharacterIntendedStates.Remove(id);
        }

        RemoveSceneCharacter(id);
    }



    #endregion Connection

    #region Receive Game Data

    #region Character Data

    [RPC]
    public void InitNewSceneCharacter(byte[] characterData)
    {
        CharactersToInstantiate.Add((Character)characterData.Deserialize());
    }


    // 4C: Assign the player's character the tag of "Player" so the client know which one to follow
    [RPC]
    public void AssignPlayerCharacter(int charId, byte[] serializedCharacter)
    {
        if (!IsActive)
        {
            Debug.Log("can't assign char when client isn't active yet");
            return;
        }

        Debug.Log("Assigned to character " + charId);
        MyCharacter = (Character)serializedCharacter.Deserialize();



    }

    #endregion Character Data

    // Add RPCs to receive other data from the server here

    #endregion Receive Game Data


    #region Levels

    /// <summary>
    /// Load a new scene, destroying nearly everything of the previous scene.
    /// </summary>
    /// <param name="level">Level.</param>
    /// <param name="levelPrefix">Level prefix.</param>
    [RPC]
    public void ClientLoadLevel(string level, int levelPrefix)
    {
        Debug.Log("(Client) Load Level received " + level + " - " + levelPrefix);

        if (!IsActive)
        {
            return;
        }

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
        Application.LoadLevel (level);
        //SceneManager.LoadScene("level_scene");
        GameLevelFile = level;

        // Allow receiving data again
        Network.isMessageQueueRunning = true;

        // Now the level has been loaded and we can start sending out data to clients
        Network.SetSendingEnabled(0, true);

        foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;


        // Flag that the scene is finished loading
        CurrentGameState = Game.GameStates.LevelLoaded;
    }

    #endregion Levels


    #region Controls

    /// <summary>
    /// 9: Send a new control request every frame
    /// </summary>
    private void UpdateControls()
    {
        // 9A: Create a new control command to record player's keyboard/mouse input for the frame
        CurrentControlCommandId++;
        CurrentControlCommand = new ControlCommand(((Character)MyPlayer).Id, CurrentControlCommandId);

        // 9B: Set the duration of the control command to be the duration since the last frame
        CurrentControlCommand.Duration = Time.deltaTime;

        UpdateMovementControls();

        UpdateMouseLookControls();

        // 9F: Add the control command for this frame to the list of control commands
        ControlCommands.Add(CurrentControlCommandId, CurrentControlCommand);
    }

    #region Movement controls

    private void UpdateMovementControls()
    {
        // 9C: Update forward/backward movement
        CurrentControlCommand.Forward = Input.GetAxis("Vertical");

        // 9C: Update strafe left/right movement
        CurrentControlCommand.Turn = Input.GetAxis("Horizontal");

        //Jumping
        if (Input.GetButton("Jump"))
        {
            // 9D: Send a jump request to the server and make the local player character jump
            CurrentControlCommand.Jump = true;
        }
    }

    #endregion Movement controls


    #region Look controls

    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);

    Vector2 _smoothMouse;

    public float TurnAngle { get; set; }
    public float PitchAngle { get; set; }

    public float TimeLastSendLookRequest { get; set; }
    public float SendLookRequestInterval { get; set; }


    private void UpdateMouseLookControls()
    {
        // 9E: Add the vertical and horizontal rotation from the mouse
        // Very simple smooth mouselook modifier for the MainCamera in Unity
        // by Francis R. Griffiths-Keam - www.runningdimensions.com

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        CurrentControlCommand.LookHorz = _smoothMouse.x;
        CurrentControlCommand.LookVert = _smoothMouse.y;
    }

    #endregion Look controls


    #region Combat controls

    private void UpdateCombatControls()
    {
        // 13A: Client requests an attack
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("CLICKED!!");

            // 13B: Client starts attack animation/sound/logic
            CharacterAttacks(((Character)MyPlayer).Id);

            // 13C: Client sends attack request to server
            GetComponent<NetworkView>().RPC("RequestAttack", RPCMode.Server);
        }
    }

    #endregion Combat Controls

    #endregion Controls


    #region Movement

    public bool ClientSidePredition = true;

    // 10: Move the player's character based on the control command for this frame
    private void MovePlayer()
    {
        // 10A: Move the player locally
        //Debug.Log( "Client is attempting to move player " + Player + " with character " + Player.Character + ";forward: " + CurrentControlCommand.Forward );
        if (ClientSidePredition)
        {
            MyPlayer.ExecuteControlCommand(CurrentControlCommand);
        }

        // 10B: Send the control request to the server
        GetComponent<NetworkView>().RPC("RequestControl", RPCMode.Server, CurrentControlCommand.Serialize());
    }

    #endregion


    #region Camera

    public float CameraDistance;
    public float CameraHeight;
    public float CameraHeightDamping;
    public float CameraRotationDamping;
    public float TurnCorrection;    // The extra angle that will be added to smooth for server-sent corrections
    public float CameraTurnCorrectionSpeed;
    public bool ThirdPerson;
    public Vector3 StaticCameraPosition = new Vector3(-13.59f, 11.85f, -38.12f);

    private void UpdateCamera()
    {
        ////Debug.Log ("I am at " + MyPlayer.transform.ToCoordinates ());

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ThirdPerson = !ThirdPerson;
        }

        if (MyCamera == null)
        {
            Debug.Log("Camera = " + Camera.main.name);
            MyCamera = Camera.main;
        }
        
        ((SkinnedMeshRenderer)MyPlayer.GetComponentInChildren<SkinnedMeshRenderer>()).enabled = ThirdPerson;
        if (ThirdPerson)
        {
            // Calculate the current rotation angles
            float wantedRotationAngle = MyPlayer.transform.eulerAngles.y;
            float currentRotationAngle = MyCamera.transform.eulerAngles.y;

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, CameraRotationDamping * Time.deltaTime);

            // Convert the angle into a rotation
            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to X distance meters behind the player
            Vector3 pos = MyPlayer.transform.position - currentRotation * Vector3.forward * CameraDistance;
            pos.y += CameraHeight;
            MyCamera.transform.position = pos;

            // Look at the player
            MyCamera.transform.LookAt(MyPlayer.transform.position + MyPlayer.transform.up * MyPlayer.BaseCharacter.Height * ((SceneCharacter3D)MyPlayer).PitchAngle); // Without this, the camera doesn't turn L/R at all
        }
        else
        {
            MyCamera.transform.position = MyPlayer.transform.position + new Vector3(0, .4f, 0);
            MyCamera.transform.rotation = MyPlayer.transform.rotation;
            //Debug.Log ("Pitch: " + MyPlayer.PitchAngle);
            MyCamera.transform.Rotate(new Vector3(-10 * ((SceneCharacter3D)MyPlayer).PitchAngle, 0, 0));
        }
        
    }
    #endregion Camera


    #region GUI

    /// <summary>
    /// Put all drawing effets here
    /// </summary>
    public void OnGUI()
    {

        if (!IsActive)
        {
            return;
        }

        if (CurrentGameState == Game.GameStates.LevelLoaded)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Exit"))
            {
                this.IsActive = false;
                Network.Disconnect();
                Application.Quit();
            }

            GUILayout.EndHorizontal();
        }
    }




    /// <summary>
    /// I don't know why this doesn't come with Unity be default...
    /// </summary>
    private void Draw2DRotated(Texture texture, float x, float y, float angle, float length, float width, string label)
    {
        //Debug.Log( "Drawing texture " + label + " at " + x + ", " + y + " @ " + angle + "*" );

        // Draw the texture
        Matrix4x4 matrixBackup = GUI.matrix;// Save the original matrix
        GUIUtility.RotateAroundPivot(angle, new Vector2(x, y));
        GUI.DrawTexture(new Rect(x, y - width / 2, length, width), texture);

        // Draw text
        GUI.Label(new Rect(x, y - width / 2, length, width), label);
        GUI.matrix = matrixBackup;// Restore it at the end
    }


    #endregion GUI

    #region Sound

    public static Dictionary<Sound.SoundId, AudioSource> Sounds { get; set; }

    public AudioSource None;

    #region Prep
    public AudioSource Grunt1;
    #endregion

    #region Execution
    public AudioSource Woosh1;
    #endregion

    #region Block
    public AudioSource Clink1;
    #endregion

    #region Hit
    public AudioSource MetalHitFlesh1;
    #endregion

    #region Pain
    public AudioSource Hurt1;
    #endregion

    #region Death
    public AudioSource Distortion;
    #endregion


    private void LoadSounds()
    {
        Sounds = new Dictionary<Sound.SoundId, AudioSource>();
        Sounds.Add(Sound.SoundId.None, None);
        Sounds.Add(Sound.SoundId.Grunt1, Grunt1);
        Sounds.Add(Sound.SoundId.Woosh1, Woosh1);
        Sounds.Add(Sound.SoundId.Clink1, Clink1);
        Sounds.Add(Sound.SoundId.MetalHitFlesh1, MetalHitFlesh1);
        Sounds.Add(Sound.SoundId.Hurt1, Hurt1);
        Sounds.Add(Sound.SoundId.Distortion, Distortion);
    }

    #endregion Sound

    #region Combat

    /// <summary>
    /// 15: Clients start attack animation/sound/logic
    /// </summary>
    /// <param name="characterId">Character identifier.</param>
    [RPC]
    public void ReceiveCharacterAttacks(int characterId)
    {
        Debug.Log("Received from the server that character " + characterId + " attacked!");

        // Don't receive for self
        if (characterId == ((Character)MyPlayer).Id)
        {
            return;
        }

        CharacterAttacks(characterId);
    }


    /// <summary>
    /// Play sound/animation/logic for when a character attacks.  This is
    ///    separate from the ReceiveCharacterAttacks() method for client
    ///    prediction
    /// </summary>
    /// <param name="characterId">Character identifier.</param>
    private void CharacterAttacks(int characterId)
    {
        SceneCharacter character = SceneCharacters[characterId];

        // Make a sound from the character
        Instantiate(Sounds[character.AttackSound], this.transform.position, this.transform.rotation);
    }


    /// <summary>
    /// 16E) Client and server perform hit effects
    /// </summary>
    /// <param name="actingCharId">Acting char identifier.</param>
    /// <param name="victimId">Victim identifier.</param>
    [RPC]
    public void ReceiveCharacterHits(int actingCharId, int victimId)
    {
        SceneCharacter attacker = SceneCharacters[actingCharId];
        SceneCharacter victim = SceneCharacters[victimId];

        Debug.Log("Recevied that character " + ((Character)attacker).CharName + " hit " + ((Character)victim).CharName + "!!");

        Instantiate(Sounds[victim.InjuredSound], victim.transform.position, victim.transform.rotation);

        ((Character)victim).CurrentHealth -= 10;
    }


    /// <summary>
    /// Receives from the server that a character has died
    /// </summary>
    /// <param name="charId">Char identifier.</param>
    [RPC]
    public void ReceiveCharacterDies(int charId)
    {
        Debug.Log("Received that character " + charId + " died!");
        SceneCharacter character = SceneCharacters[charId];
        character.MoveDirection = new Vector3(0f, 0f, 0f);
        Instantiate(Sounds[character.DeathSound], character.transform.position, character.transform.rotation);
        ((Character)character).IsAlive = false;
        RespawnCharacter(charId);
    }

    #endregion Combat
}
