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
  public LayerMask waterLayer;

  public SpriteRenderer sr;
  public GameObject Legs;
  public GameObject Wheels;

  public Mode mode = Mode.Wheels;
  private Animator moveAnimator;
  private SpriteRenderer moveSR;
  public  Animator wheelsAnimator;
  public  Animator legsAnimator;
  public SpriteRenderer wheelsSR;
  public SpriteRenderer legsSR;

  // Start is called before the first frame update
  void Start() {
    rb = GetComponent<Rigidbody2D>();
    ChangeMode(Mode.Wheels);
  }

  void FixedUpdate() {
    moveinput = Input.GetAxis("Horizontal");
    rb.velocity = new Vector2(moveinput * speed, rb.velocity.y);

    if (moveinput < 0) {
      sr.flipX = true;
      if (moveSR != null) moveSR.flipX = true;
      if (moveAnimator != null) {
        moveAnimator.Play("Move");
        moveAnimator.speed = 1;
      }
    }
    else if (moveinput > 0) {
      sr.flipX = false;
      if (moveSR != null) moveSR.flipX = false;
      if (moveAnimator != null) {
        moveAnimator.Play("Move");
        moveAnimator.speed = -1;
      }
    }
    else {
      if (moveAnimator != null) {
        moveAnimator.Play("Idle");
      }
    }
  }



  void Update() {
    if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
      rb.velocity += Vector2.up * jumpforce;
      isGrounded = false;
    }

  }

  private void OnCollisionEnter2D(Collision2D collision) {
    if ((groundLayer & (1 << collision.collider.gameObject.layer)) != 0) {
      isGrounded = true;
    }
    if ((waterLayer & (1 << collision.collider.gameObject.layer)) != 0) {
      Debug.Log("Water");
    }
  }

  private void OnTriggerEnter2D(Collider2D collision) {
    if ((groundLayer & (1 << collision.gameObject.layer)) != 0) {
      isGrounded = true;
    }
    if ((waterLayer & (1 << collision.gameObject.layer)) != 0) {
      Debug.Log("Water");
    }
  }

  public void ChangeMode(Mode newMode) {
    mode = newMode;
    if (mode == Mode.Wheels) {
      Wheels.SetActive(true);
      Legs.SetActive(false);
      moveAnimator = wheelsAnimator;
      moveSR = wheelsSR;
    }
    else if (mode == Mode.Legs) {
      Wheels.SetActive(false);
      Legs.SetActive(true);
      moveAnimator = legsAnimator;
      moveSR = legsSR;
    }
  }
}

public enum Mode { Wheels, Legs, Fins };