﻿using System.Collections;
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

  public GameObject Legs;
  public GameObject Wheels;
  public GameObject Hook;

  public Mode mode = Mode.Wheels;
  private Animator moveAnimator;
  private SpriteRenderer bodySR;
  private SpriteRenderer moveSR1;
  private SpriteRenderer moveSR2;
  public  Animator wheelsAnimator;
  public SpriteRenderer bodyWheelSR;
  public SpriteRenderer wheelsSR1;
  public SpriteRenderer wheelsSR2;
  public  Animator legsAnimator;
  public SpriteRenderer bodyLegsSR;
  public SpriteRenderer legsSR1;
  public SpriteRenderer legsSR2;
  public GameObject ElectroShock;
  public SpriteRenderer hookSR;

  // Start is called before the first frame update
  void Start() {
    rb = GetComponent<Rigidbody2D>();
    ChangeMode(Mode.Wheels);
  }

  void FixedUpdate() {
    if (timeInWater > 1) return; // Stop the movements
    moveinput = Input.GetAxis("Horizontal");
    rb.velocity = new Vector2(moveinput * speed, rb.velocity.y);

    if (moveinput < 0) {
      if (bodySR != null) bodySR.flipX = true;
      if (moveSR1 != null) moveSR1.flipX = true;
      if (moveSR2 != null) moveSR2.flipX = true;
      if (moveAnimator != null) moveAnimator.Play("MoveL");
    }
    else if (moveinput > 0) {
      if (bodySR != null) bodySR.flipX = false;
      if (moveSR1 != null) moveSR1.flipX = false;
      if (moveSR2 != null) moveSR2.flipX = false;
      if (moveAnimator != null) moveAnimator.Play("MoveR");
    }
    else {
      if (moveAnimator != null) {
        moveAnimator.Play("Idle");
      }
    }
  }



  void Update() {
    if (inWater) {
      timeInWater += Time.deltaTime;
    }

    ElectroShock.SetActive(timeInWater > 1);
    if (timeInWater > 5) // do some restart
      ;

    if (timeInWater > 1) return; // Stop the movements

    if (mode == Mode.Legs) {
      if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
        rb.velocity += Vector2.up * jumpforce;
        isGrounded = false;
      }
    }
    else if (mode == Mode.Hook) {
      if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
        // Raycast to find the ceil
        // Start elongating anim until we reach the spot
        // If no spot, then go back after a while
        // If spot, move player on the line, and swing, then leave the hook, and fall down
        // Make movements impossible while hooking
      }
    }

    if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeMode(Mode.Wheels);
    if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeMode(Mode.Legs);
    if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeMode(Mode.Hook);

  }

  bool inWater = false;
  float timeInWater = 0;

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
      inWater = true;
    }
  }

  private void OnTriggerExit2D(Collider2D collision) {
    if ((waterLayer & (1 << collision.gameObject.layer)) != 0) {
      inWater = false;
      timeInWater = 0;
    }
  }

  public void ChangeMode(Mode newMode) {
    mode = newMode;
    if (mode == Mode.Wheels) {
      Wheels.SetActive(true);
      Legs.SetActive(false);
      Hook.SetActive(false);
      bodySR = bodyWheelSR;
      moveAnimator = wheelsAnimator;
      moveSR1 = wheelsSR1;
      moveSR2 = wheelsSR2;
    }
    else if (mode == Mode.Legs) {
      Wheels.SetActive(false);
      Legs.SetActive(true);
      Hook.SetActive(false);
      bodySR = bodyLegsSR;
      moveAnimator = legsAnimator;
      moveSR1 = legsSR1;
      moveSR2 = legsSR2;
    }
    else if (mode == Mode.Hook) {
      Wheels.SetActive(false);
      Legs.SetActive(false);
      Hook.SetActive(true);
      bodySR = bodyWheelSR;
      moveAnimator = wheelsAnimator;
      moveSR1 = wheelsSR1;
      moveSR2 = wheelsSR2;
    }
  }
}

public enum Mode { Wheels, Legs, Hook };