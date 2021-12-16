using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // New
    public Transform Car;
    public Transform Raypoint;
    private Rigidbody _carRig;
    public float ForwardAccel = 8f, ReverseAccel = 4f, MaxSpeed = 50f, TurnStrength = 40f, gravityForce = 10f;
    public float Drag = 5f;
    public Transform CentreOfMass;

    public LayerMask whatIsGround;
    public float GroundRayLength = .5f;
    private float ChangeInForwardVelocity = 0;
    private bool isGrounded = true;

    private bool[] inputs;
    Vector2 _inputDirection;

    public Vector3 RealPos = Vector3.zero;
    public Quaternion RealRot = Quaternion.identity;

    private void Awake() {
        _carRig = Car.GetComponent<Rigidbody>();
        _carRig.centerOfMass = CentreOfMass.position;
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

    public void UpdatePosFromServer(Vector3 _newPos) { RealPos = _newPos; }

    public void UpdatePosFromServer(Quaternion _newRot) { RealRot = _newRot; }

    private void ClientSidePrediction() {

        _inputDirection = Vector2.zero;
        if (inputs[0]) { _inputDirection.y += 1; }
        if (inputs[1]) { _inputDirection.y -= 1; }
        if (inputs[2]) { _inputDirection.x -= 1; }
        if (inputs[3]) { _inputDirection.x += 1; }

        isGrounded = false; RaycastHit hit;
        if (Physics.Raycast(Raypoint.position, -transform.up, out hit, GroundRayLength, whatIsGround)) { isGrounded = true; }

        switch (_inputDirection.y) {
            case (-1): ChangeInForwardVelocity = _inputDirection.y * ReverseAccel; break;
            case (1): ChangeInForwardVelocity = _inputDirection.y * ForwardAccel; break;
            default: break;
        }

        // Before doing anything check sync
        if ((Car.position - RealPos).magnitude >= 0.1f) { Car.position = RealPos; Debug.Log("RESYNC - Position"); }
        if ((Car.rotation.eulerAngles - RealRot.eulerAngles).magnitude >= 0.1f) {  Debug.Log("RESYNC - Rotation"); Car.rotation = RealRot; }
      

        if (isGrounded) {

            var localVelocity = Car.InverseTransformDirection(_carRig.velocity);
            var forwardSpeed = localVelocity.z;
            // if moving
           // Debug.Log(forwardSpeed);
            // forward or back? 

            if (localVelocity.magnitude >= 0.1f) {
                if (forwardSpeed >= 0.01f) { Car.rotation = Quaternion.Euler(Car.rotation.eulerAngles + new Vector3(0f, _inputDirection.x * TurnStrength * 1 * Time.deltaTime, 0f)); }
                else { Car.rotation = Quaternion.Euler(Car.rotation.eulerAngles + new Vector3(0f, _inputDirection.x * TurnStrength * -1 * Time.deltaTime, 0f)); }
            }

            // nullify forces that are not in the forward direction.
            //if (localVelocity.x >= 0.1f) { localVelocity.x = localVelocity.x * 1 / Drag; }
            // Car.transform.TransformDirection(localVelocity);
            _carRig.AddForce(Car.forward * ChangeInForwardVelocity * Time.deltaTime * 100f); ChangeInForwardVelocity = 0;


        }
        


    }
}
