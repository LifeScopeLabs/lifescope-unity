 using UnityEngine;
 using System.Collections;
 
 public class CameraControls : MonoBehaviour
 {
     public float speed = 5f;
     public void Update()
     {
         if(Input.GetKey(KeyCode.RightArrow))
         {
             transform.Rotate(0, speed, 0, Space.Self);
         }
         if(Input.GetKey(KeyCode.LeftArrow))
         {
             transform.Rotate(0, -speed, 0, Space.Self);
         }
         if(Input.GetKey(KeyCode.DownArrow))
         {
             transform.position -= transform.forward * (3 * speed);
         }
         if(Input.GetKey(KeyCode.UpArrow))
         {
             transform.position += transform.forward * (3 * speed);
         }
     }
 }