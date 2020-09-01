using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
  public float speed;
  public float jumpforce;
  private float moveinput;
  private Rigidbody2D rb;
  public bool isGrounded;
  public bool hooking;
  public bool goingleft;

  public LayerMask groundLayer;
  public LayerMask waterLayer;
  public LayerMask pillRed;
  public LayerMask Red;
  public LayerMask pillGreen;
  public LayerMask Green;
  public LayerMask pillBlue;
  public LayerMask Blue;

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

  // Start is called before the first frame update
  void Start() {
    rb = GetComponent<Rigidbody2D>();
    ChangeMode(Mode.Wheels);
    hooking = false;
    goingleft = false;
  }

  void FixedUpdate() {
    if (timeInWater > 1 || hooking) return; // Stop the movements
    moveinput = Input.GetAxis("Horizontal");
    rb.velocity = new Vector2(moveinput * speed, rb.velocity.y);

    if (moveinput < 0) {
      if (bodySR != null) bodySR.flipX = true;
      if (moveSR1 != null) moveSR1.flipX = true;
      if (moveSR2 != null) moveSR2.flipX = true;
      if (moveAnimator != null) moveAnimator.Play("MoveL");
      if (mode == Mode.Hook) Hook.transform.rotation = Quaternion.Euler(0, 0, 45 + 90);
      goingleft = true;
    }
    else if (moveinput > 0) {
      if (bodySR != null) bodySR.flipX = false;
      if (moveSR1 != null) moveSR1.flipX = false;
      if (moveSR2 != null) moveSR2.flipX = false;
      if (moveAnimator != null) moveAnimator.Play("MoveR");
      if (mode == Mode.Hook) Hook.transform.rotation = Quaternion.Euler(0, 0, 45);
      goingleft = false;
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

    if (hooking) return;

    if (mode == Mode.Legs) {
      if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
        rb.velocity += Vector2.up * jumpforce;
        isGrounded = false;
      }
    }
    else if (mode == Mode.Hook) {
      if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
        hooking = true;
        // Raycast to find the ceil
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up + (goingleft ? Vector2.left : Vector2.right), 10, groundLayer);
        Debug.Log(hit.point.ToString() + " d=" + hit.distance);
        StartCoroutine(HookLaunch(hit.point, hit.distance));
      }
    }

    if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeMode(Mode.Wheels);
    if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeMode(Mode.Legs);
    if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeMode(Mode.Hook);

  }

  float hookscale = 0.3909113f;

  IEnumerator HookLaunch(Vector2 hit, float dist) {
    rb.velocity = Vector3.zero;
    bool notgood = false;
    if (dist == 0) {
      dist = 8f;
      notgood = true;
    }

    // Start elongating anim until we reach the spot
    float hooklen = .2f;
    while (hooklen < dist * hookscale) {
      hooklen += 2 * Time.fixedDeltaTime;
      Hook.transform.localScale = new Vector3(hooklen, .5f, 1);
      yield return null;
    }

    if (notgood) {
      // If no spot, then go back and stop
      while (hooklen > .2) {
        hooklen -= 3 * Time.fixedDeltaTime;
        Hook.transform.localScale = new Vector3(hooklen, .5f, 1);
        yield return null;
      }
      Hook.transform.localScale = new Vector3(.2f, .5f, 1);
    }
    else {
      // Move player on the line
      rb.gravityScale = 0;
      float amount = dist / 3;

      Vector2 orig = transform.position;
      Vector2 dest = orig;
      if (goingleft) {
        dest += (Vector2.up + Vector2.left).normalized * amount;
      }
      else {
        dest += (Vector2.up + Vector2.right).normalized * amount;
      }

      float reducedlength = hookscale *  dist * 2 / 3;

      float pos = 0;
      while (pos < 1) {
        pos += (1 + pos) * (1 + pos) * (1 + pos) * (1 + pos) * Time.deltaTime;
        transform.position = pos * dest + (1 - pos) * orig;
        Hook.transform.localScale = new Vector3(hooklen * (1-pos) + reducedlength * pos, .5f, 1);
        yield return null;
      }

      // Swing
      if (goingleft) {
        pos = 10;
        while (pos > 0) {
          if (pos > 5)
            pos -= 2 * (4 + pos / 2) * Time.deltaTime;
          else
            pos -= 2 * (7.5f - pos / 2) * Time.deltaTime;
          float x = hit.x + Mathf.Sin(9 * (pos - 5) * Mathf.Deg2Rad) * dist * 2 / 3;
          float y = hit.y - Mathf.Cos(9 * (pos - 5) * Mathf.Deg2Rad) * dist * 2 / 3;
          transform.position = new Vector3(x, y, 0);
          Hook.transform.rotation = Quaternion.Euler(0, 0, 45 + pos * 9);
          yield return null;
        }
      }
      else {
        pos = 0;
        while (pos < 10) {
          if (pos < 5)
            pos += 2 * (4 + pos / 2) * Time.deltaTime;
          else
            pos += 2 * (7.5f - pos / 2) * Time.deltaTime;
          float x = hit.x + Mathf.Sin(9 * (pos - 5) * Mathf.Deg2Rad) * dist * 2 / 3;
          float y = hit.y - Mathf.Cos(9 * (pos - 5) * Mathf.Deg2Rad) * dist * 2 / 3;
          transform.position = new Vector3(x, y, 0);
          Hook.transform.rotation = Quaternion.Euler(0, 0, 45 + pos * 9);
          yield return null;
        }
      }

      // Leave the hook, and fall down
      yield return new WaitForSeconds(.05f);
      rb.gravityScale = 1;
      pos = 0;
      while (pos < 1) {
        pos += (1 + pos) * (1 + pos) * (1 + pos) * (1 + pos) * Time.deltaTime;
        Hook.transform.localScale = new Vector3(reducedlength * (1 - pos) + .2f * pos, .5f, 1);
        yield return null;
      }
      Hook.transform.rotation = Quaternion.Euler(0, 0, 45 + (goingleft ? 90 : 0));
      Hook.transform.localScale = new Vector3(.2f, .5f, 1);
    }
    hooking = false;
  }

  public float zzz = 10;

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
      Wheels.SetActive(true);
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