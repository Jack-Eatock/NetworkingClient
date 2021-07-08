using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 5f;
    public float jumpspeed = 5f;
    private bool[] inputs;
    private float yVelocity = 0;
    public float gravity = -9.91f;
    public CharacterController Controller;

    Vector2 _inputDirection;
    Vector3 _moveDirection;

    public Vector3 RealPos = Vector3.zero;

     private void Start() {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpspeed *= Time.fixedDeltaTime;
    }


    private void FixedUpdate() {

        GrabInputs();

        // Send input to server
        SendInputToServer();

        // Try to predict movement as best as possible.
        ClientSidePrediction();


    }

    private void GrabInputs() {
         inputs = new bool[] {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space),

        };
    }

    private void SendInputToServer() {
        ClientSend.PlayerMovement(inputs);
    }

    public void UpdatePosFromServer(Vector3 _newPos) {

        RealPos = _newPos;
      
        /*
        // caluculate distance ( length of a - b ) 
        _distanceFromNewPos = (transform.position - _newPos).magnitude;

        // calculate time ( time = distance / speed )
        _timeTaken = _distanceFromNewPos / (5f * Time.fixedDeltaTime);

        MoveToDestinationInTime(_newPos);
        */
        if ( (transform.position - _newPos).magnitude >= .2f) {

            
            // _inputDirection = Vector2.zero;
            // _moveDirection = Vector3.zero;
             //yVelocity = 0;

             //transform.position = Vector3.Lerp(transform.position, _newPos, 0.1f); ;
              //transform.position = _newPos;
        }
      

       // transform.position = Vector3.Lerp(transform.position, _newPos, 0.1f); ;
    }

    private void ClientSidePrediction() {
         _inputDirection = Vector2.zero;

        if (inputs[0]) {_inputDirection.y  += 1;}
        if (inputs[1]) { _inputDirection.y -= 1;}
        if (inputs[2]) {_inputDirection.x  += 1;}
        if (inputs[3]) { _inputDirection.x -= 1;}
       
         _moveDirection = -transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (Controller.isGrounded) {
            yVelocity = 0;
            if (inputs[4]) {
                yVelocity = jumpspeed;
            }
        }

        yVelocity += gravity;

        if (transform.position == RealPos) {
             _moveDirection.y = yVelocity;
             Controller.Move(_moveDirection);
        }
        else {

            /*
            // If in air and not far dont reset pos give benefit of doubt
            if (!Controller.isGrounded && ((transform.position - RealPos).magnitude <= 0.2f)) {
                return;
            }
            */

            transform.position = RealPos;
        }

       
    }
}
