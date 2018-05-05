using UnityEngine;
using System.Collections;
using ServerSideCalculations.Characters;
using ServerSideCalculations;

public class ThrowawayCharacterController2D : MonoBehaviour
{
    
    public Vector2 MoveDirection;

    public bool Grounded;
    
    public bool WasGroundedLastFrame = false;

    public float JumpSpeed;

    public float MoveSpeed;


    public void Move (float horizontalMovement, float duration, bool timeToJump)
    {
        WasGroundedLastFrame = Grounded;
        
        var groundHit = Physics2D.Raycast (GetFeetPoint (), -Vector2.up);
        this.Grounded = groundHit.collider != null && groundHit.distance < .1f;
        ////Debug.Log ("Grounded = " + Grounded + " : " + (groundHit.collider != null ? groundHit.collider.gameObject.transform.position.x : "n/a") + " : " + groundHit.distance);
        
        if (Grounded) {
            WarpToTopOf (groundHit.collider.gameObject);
        }    
        
        // Move horizontally
        if (!this.Grounded) {
            MoveDirection.y -= Game.GRAVITY / 3f * duration;
        } else {
            if (!WasGroundedLastFrame) {
                WarpToTopOf (groundHit.collider.gameObject);
            }
            
            if (timeToJump) {
                ////Debug.Log ("Attempting to jump!");
                MoveDirection.y = JumpSpeed / 5f;
            } else {
                WasGroundedLastFrame = true;
                MoveDirection.y = 0f;
            }
        }
        
        this.transform.Move (
            horizontalMovement * MoveSpeed * duration,
            MoveDirection.y);
    }
    

    public Vector2 GetFeetPoint ()
    {
        return new Vector2 (
            this.transform.position.x,
            this.transform.position.y - this.GetComponent<Collider2D>().bounds.size.y * .6f
        );
    }

    
    public void WarpToTopOf (GameObject ground)
    {
        var groundY = ground.transform.position.y;
        var topOfGround = groundY + ground.GetComponent<Collider2D>().bounds.size.y / 2f;
        
        var myY = transform.position.y;
        var myBottom = myY - this.gameObject.GetComponent<Collider2D>().bounds.size.y / 2f;
        
        var moveUp = topOfGround - myBottom;
        
        this.transform.MoveUp (moveUp);
    }
}
