using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
  public float speed;
  public float jumpforce;
  private float moveinput;
  private Rigidbody2D rb;
  public bool isGrounded;

  public LayerMask groundLayer;

  // Start is called before the first frame update
  void Start() {
    rb = GetComponent<Rigidbody2D>();
  }

  void FixedUpdate() {
    moveinput = Input.GetAxis("Horizontal");
    rb.velocity = new Vector2(moveinput * speed, rb.velocity.y);
  }




  void Update() {
    if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
      rb.velocity += Vector2.up * jumpforce;
    }

  }

  private void OnCollisionEnter2D(Collision2D collision) {
    if ((groundLayer & (1 << collision.collider.gameObject.layer)) != 0) {
      isGrounded = true;
    }
  }

}

