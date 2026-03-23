using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleOfThirds : MonoBehaviour
{

    float screenOffset = 0.001f;
    public Camera targetCam;

void OnDrawGizmosSelected()
    {
       // cam = GetComponent<Camera>();
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        Camera cam = targetCam;
        //Figure out how big to make things
        var zero = cam.ScreenPointToRay(new Vector3(0, 0, 0)).GetPoint(screenOffset);
        var right = cam.ScreenPointToRay(new Vector3(Screen.width, 0, 0)).GetPoint(screenOffset);
        var up = cam.ScreenPointToRay(new Vector3(0, Screen.height, 0)).GetPoint(screenOffset);
        var upDirection = transform.up * (up - zero).magnitude;
        var rightDirection = transform.right * (right - zero).magnitude;
        //Get our points away from the screen
        var bottomLeft =
              cam.ScreenPointToRay(new Vector3(Screen.width / 3, 0, 0)).GetPoint(screenOffset);
        var bottomRight =
              cam.ScreenPointToRay(new Vector3(2 * Screen.width / 3, 0, 0)).GetPoint(screenOffset);
        var leftBottom =
              cam.ScreenPointToRay(new Vector3(0, Screen.height / 3, 0)).GetPoint(screenOffset);
        var leftTop =
              cam.ScreenPointToRay(new Vector3(0, 2 * Screen.height / 3, 0)).GetPoint(screenOffset);
        //Draw
        Gizmos.DrawRay(bottomLeft, upDirection);
        Gizmos.DrawRay(bottomRight, upDirection);
        Gizmos.DrawRay(leftBottom, rightDirection);
        Gizmos.DrawRay(leftTop, rightDirection);

    }
}
