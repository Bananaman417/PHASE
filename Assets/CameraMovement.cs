using UnityEngine;

public class CameraMovement : MonoBehaviour {
  public Transform Player;
  public Vector3 pos;

  void Update() {
    if (Mathf.Abs(transform.position.x - Player.position.x) > 3 || Mathf.Abs(transform.position.y - Player.position.y) > 1.5f) {
      Vector3 dist = Player.position - transform.position;
      dist.z = 0;

      if (dist.sqrMagnitude > 4)
        pos = transform.position + dist * Time.deltaTime * 3.5f;
      else
        pos = transform.position + dist * Time.deltaTime * 1.5f;
      pos.z = -10;
      transform.position = pos;
    }
  }
}
