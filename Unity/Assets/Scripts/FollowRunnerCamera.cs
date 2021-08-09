using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRunnerCamera : MonoBehaviour
{
  public Transform mPlayer;

  public Vector3 mPositionOffset = new Vector3(0.0f, 2.0f, -2.5f);
  public Vector3 mAngleOffset = new Vector3(0.0f, 0.0f, 0.0f);
  [Tooltip("The damping factor to smooth the changes in position and rotation of the camera.")]
  public float mDamping = 10.0f;

  void Start()
  {
  }

  void Update()
  {
  }

  void LateUpdate()
  {
    CameraMove_Follow();
  }

  void CameraMove_Follow()
  {
    Vector3 targetPos = mPlayer.position;
    targetPos.y += mPositionOffset.y;
    Vector3 relativePos = targetPos - transform.position;
    //relativePos.y += mPositionOffset.y;

    Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
    transform.rotation = rotation;


    //Vector3 desiredPosition = mPlayer.position + transform.forward * mPositionOffset.z;
    //transform.position = desiredPosition;

    Vector3 forward = mPlayer.forward;
    Vector3 desiredPosition = targetPos + forward * mPositionOffset.z;
    //desiredPosition.y = mPositionOffset.y;

    // Finally, we change the position of the camera, not directly, but by applying Lerp.
    // Don't use Time.deltaTime. It jitters.
    Vector3 position = Vector3.Lerp(transform.position,
        desiredPosition,
        0.01f * mDamping);

    transform.position = position;
  }
}