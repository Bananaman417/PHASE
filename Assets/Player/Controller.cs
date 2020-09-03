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
  Camera cam;

  public LayerMask groundLayer;
  public LayerMask waterLayer;
  public LayerMask pillRed;
  public LayerMask wallRed;
  public LayerMask pillGreen;
  public LayerMask wallGreen;
  public LayerMask pillBlue;
  public LayerMask wallBlue;
  public LayerMask itemWheels;
  public LayerMask itemLegs;
  public LayerMask itemHook;

  public Phase phase = Phase.None;
  public Mode mode = Mode.Wheels;
  public GameObject Legs;
  public GameObject Wheels;
  public GameObject Hook;
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

  public Level[] Levels;
  public int currentLevel = 0;
  bool startingLevel = false;

  public AudioSource audio1;
  public AudioSource audio2;
  public AudioClip[] clips;

  // Start is called before the first frame update
  void Start() {
    cam = Camera.main;
    rb = GetComponent<Rigidbody2D>();
    ChangeMode(Mode.Wheels);
    hooking = false;
    goingleft = false;
    currentLevel = 0;
    MoveToLevelStart();
  }

  float prevMove = 0;
  void FixedUpdate() {
    if (timeInWater > 1 || hooking || startingLevel) return; // Stop the movements
    moveinput = Input.GetAxis("Horizontal");
    rb.velocity = new Vector2(moveinput * speed, rb.velocity.y);

    if (moveinput < 0) {
      if (bodySR != null) bodySR.flipX = true;
      if (moveSR1 != null) moveSR1.flipX = true;
      if (moveSR2 != null) moveSR2.flipX = true;
      if (moveAnimator != null) moveAnimator.Play("MoveL");
      if (mode == Mode.Hook) Hook.transform.rotation = Quaternion.Euler(0, 0, 45 + 90);
      goingleft = true;
      if (!audio1.isPlaying) {
        if (mode == Mode.Legs)
          audio1.clip = clips[(int)Sounds.Legs];
        else
          audio1.clip = clips[(int)Sounds.Wheels];
        if (stopSoundCoroutine != null)
          StopCoroutine(stopSoundCoroutine);
        stopSoundCoroutine = null;
        audio1.volume = 1;
        audio1.Play();
        prevMove = moveinput;
      }
    }
    else if (moveinput > 0) {
      if (bodySR != null) bodySR.flipX = false;
      if (moveSR1 != null) moveSR1.flipX = false;
      if (moveSR2 != null) moveSR2.flipX = false;
      if (moveAnimator != null) moveAnimator.Play("MoveR");
      if (mode == Mode.Hook) Hook.transform.rotation = Quaternion.Euler(0, 0, 45);
      goingleft = false;
      if (!audio1.isPlaying) {
        if (mode == Mode.Legs)
          audio1.clip = clips[(int)Sounds.Legs];
        else
          audio1.clip = clips[(int)Sounds.Wheels];
        if (stopSoundCoroutine != null)
          StopCoroutine(stopSoundCoroutine);
        stopSoundCoroutine = null;
        audio1.volume = 1;
        audio1.Play();
        prevMove = moveinput;
      }
    }
    else {
      if (moveAnimator != null) {
        moveAnimator.Play("Idle");
      }
      if (prevMove != 0 && stopSoundCoroutine == null) {
        stopSoundCoroutine = StartCoroutine(StopSound());
        prevMove = 0;
      }
    }
  }

  Coroutine stopSoundCoroutine = null;
  IEnumerator StopSound() {
    float volume = 1;
    while (volume > 0) {
      volume -= Time.deltaTime * 3.5f;
      if (volume < 0) volume = 0;
      audio1.volume = volume;
      audio2.volume = volume;
      yield return null;
    }
    audio1.Stop();
    audio2.Stop();
    yield return null;
    audio1.volume = 1;
    audio2.volume = 1;
    stopSoundCoroutine = null;
  }


  void Update() {
    cam.transform.rotation = Quaternion.identity;
    if (inWater) {
      timeInWater += Time.deltaTime;
    }

    ElectroShock.SetActive(timeInWater > 1);
    if (timeInWater > 5) { // do some restart
      RestartLevel();
      return;
    }

    if (timeInWater > 1 || startingLevel || hooking) return; // Stop the movements

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

  readonly float hookscale = 0.3909113f;

  IEnumerator HookLaunch(Vector2 hit, float dist) {
    rb.velocity = Vector3.zero;
    bool notgood = false;
    if (dist == 0) {
      dist = 8f;
      notgood = true;
    }

    audio1.clip = clips[(int)Sounds.Shoot];
    audio1.Play();

    // Start elongating anim until we reach the spot
    float hooklen = .2f;
    while (hooklen < dist * hookscale) {
      hooklen += 2 * Time.fixedDeltaTime;
      Hook.transform.localScale = new Vector3(hooklen, .5f, 1);
      yield return null;
    }

    if (notgood) {
      audio2.clip = clips[(int)Sounds.Rewwing];
      audio2.Play();
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

      audio2.clip = clips[(int)Sounds.RopeSwirl];
      audio2.Play();
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
      audio1.clip = clips[(int)Sounds.Rewwing];
      audio1.Play();
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

  bool inWater = false;
  float timeInWater = 0;

  private void OnCollisionEnter2D(Collision2D collision) {
    if ((groundLayer & (1 << collision.collider.gameObject.layer)) != 0) {
      isGrounded = true;
    }

    if ((pillRed & (1 << collision.collider.gameObject.layer)) != 0) {
      phase = Phase.Red;
      bodySR.color = new Color32(255, 100, 100, 255);
      SetLayer(gameObject, 14); // Phase Red
      collision.collider.gameObject.SetActive(false);
    }
    else if ((pillBlue & (1 << collision.collider.gameObject.layer)) != 0) {
      phase = Phase.Blue;
      bodySR.color = new Color32(100, 100, 255, 255);
      SetLayer(gameObject, 15); // Phase Blue
      collision.collider.gameObject.SetActive(false);
    }
    else if ((pillGreen & (1 << collision.collider.gameObject.layer)) != 0) {
      phase = Phase.Green;
      bodySR.color = new Color32(100, 255, 100, 255);
      SetLayer(gameObject, 16); // Phase Green
      collision.collider.gameObject.SetActive(false);
    }
    else if ((itemWheels & (1 << collision.collider.gameObject.layer)) != 0) {
      ChangeMode(Mode.Wheels);
      collision.collider.gameObject.SetActive(false);
    }
    else if ((itemLegs & (1 << collision.collider.gameObject.layer)) != 0) {
      ChangeMode(Mode.Legs);
      collision.collider.gameObject.SetActive(false);
    }
    else if ((itemHook & (1 << collision.collider.gameObject.layer)) != 0) {
      ChangeMode(Mode.Hook);
      collision.collider.gameObject.SetActive(false);
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

  void SetLayer(GameObject obj, int newLayer) {
    obj.layer = newLayer;

    foreach (Transform child in obj.transform) {
      SetLayer(child.gameObject, newLayer);
    }
  }

  void MoveToLevelStart() {
    transform.rotation = Quaternion.identity;
    rb.velocity = Vector3.zero;
    rb.gravityScale = 1;
    bodySR.color = new Color32(255, 255, 255, 255);
    gameObject.layer = 0;
    ChangeMode(Mode.Wheels);
    StartCoroutine(MoveToLevelPosition(transform.position, Levels[currentLevel].Start.transform.position));
  }

  IEnumerator MoveToLevelPosition(Vector3 start, Vector3 end) {
    startingLevel = true;
    float pos = 0;
    float speed = 0;
    while(pos < 3) {
      if (pos < 1.5f) 
        speed += .1f;
      else speed -= .1f;
      if (speed < .25f) speed = .25f;
      pos += speed * Time.deltaTime;

      transform.position = (1 - pos / 3) * start + (pos / 3) * end;
      yield return null;
    }
    // Fade???

    transform.position = Levels[currentLevel].Start.transform.position;
    yield return null;
    startingLevel = false;
  }

  void RestartLevel() {
    startingLevel = true;
    // Reset all the items
    for (int i = 0; i < Levels[currentLevel].Items.Length; i++) {
      Levels[currentLevel].Items[i].SetActive(true);
    }
    MoveToLevelStart();
  }
}

public enum Mode { Wheels, Legs, Hook };
public enum Phase { None, Red, Green, Blue };

[System.Serializable]
public class Level {
  public int Number;
  public GameObject Start;
  public GameObject End;
  public GameObject LevelFrame;
  public GameObject[] Items;
}

public enum Sounds {  Legs=0, Phase=1, Rewwing=2, RopeSwirl=3, Shoot=4, ThumpFsFs=5, Wheels=6, WhipSwing=7 }
