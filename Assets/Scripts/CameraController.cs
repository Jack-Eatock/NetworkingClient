using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Vector3 cameraVelocity;
    public Vector3 cameraOffset;
    public float distInfront =2f;
    public float CameraFollowSmoothTime = 2f;


     private void FixedUpdate() {
         
   
        Vector3.SmoothDamp(transform.position, player.transform.position + cameraOffset, ref cameraVelocity, CameraFollowSmoothTime);
        transform.position = transform.position + cameraVelocity * Time.deltaTime;
        transform.position += player.transform.forward * distInfront;
    }

}
