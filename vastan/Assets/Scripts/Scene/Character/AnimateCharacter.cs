using System;
using UnityEngine;

namespace ServerSideCalculations.Scene.Character
{
    public class AnimateCharacter : MonoBehaviour
    {
        public SceneCharacter SceneCharacter;
        
        public void Update()
        {
            if( SceneCharacter == null )
            {
                //Debug.Log("Can't animate: Character or controller is null");
                return;
            }
            
            if( Math.Abs( SceneCharacter.MoveDirection.magnitude ) > 1 )
            {
                //Debug.Log("Animate run" );
                //GetComponent<Animation>().CrossFade("run");
            }
            else
            {
                //Debug.Log("Animate Idle" );
                //GetComponent<Animation>().CrossFade("idle");
            }
        }
    }
}