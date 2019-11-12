//Player_Controller.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Player_Controller : MonoBehaviour
{
    private static Player_Controller instance;
    public static Player_Controller Instance
    {
        get
        { if (instance == null) Debug.LogError("Player_Controller cannot find static reference to initialized Player_Controller."); return instance; }
        set
        { instance = value; }
    }
    private enum PlayerColors { Red, Green, Blue, Default, TotalColors };            // Used for indexing into PlayerColors[]

    //GetComponent<Animator>().Play("Player_Jump");
    //GetComponent<Animator>().Play("Player_Running");
    //GetComponent<Animator>().Play("Player_Fall");
    //GetComponent<Animator>().Play("Player_WallJumpRight");
    //GetComponent<Animator>().Play("Player_WallJumpLeft");


    // These need to be initialized inside the Unity Editor
    // public GameObject Player;
    public LayerMask m_groundLayer;
    public Rewind m_rewindButton;               // Refernce to the rewind button in the UI
    GameObject m_playerShield;
    GameObject m_gameOverScreen;
    AssistantCrayonManager m_assistantManager;  // manages the crayons around player

    // Player Components
    private Collider2D m_collider2D;            // Collider for the player object
    private Rigidbody2D m_rigidBody2D;          // Rigid Body for the player object
    private Animator m_animator;				// Handles Player's Animations
    private SpriteRenderer[] m_bodySprites;     // Array that holds all the sprites to make up the player icon

    // Player's Action States
    public bool m_grounded;                     // Tracks if the player is grounded
    private bool m_falling;                     // Tracks if the player is falling
    private bool m_jumping;                     // Tracks if the player is jumping
    private bool m_damaged;                     // Tracks if the player is being damaged
    // private bool m_moving;                      // Tracks if the player is moving
    private bool m_rewinding;                   // Tracks if the player is rewinding
    private bool m_protected;                   // Tracks if the player has a protection shield
    private bool m_invincible;                  // Tracks if the player is currently invincible

    // Player Position Tracking
    private Vector3 m_origin;                   // Where the player starts each level
    //private Vector3 m_prevPosition;             // Vector of the player's previous position
    private bool m_onZonePlat3;                 // Tracks what platform zone the player is currently on
    private bool m_onZonePlat2;                 // Tracks what platform zone the player is currently on

    // Player's Ability Trackers
    private float m_jumpDelta;                  // The amount of time elapsed between jump and double jump
    private float m_jumpCount;                  // Tracks how many times the player has jumped since they were last on a ground platform
    public float m_distanceTraveled;           // Tracks how far the player has traveled
    public int m_totalTimesJumped;             // Keeps track of how many times the player has jumped over the course of the game.
    private int m_maxJumps;                     // The max number of jumps the player has after they leave the ground

    // Player's Attributes
    private int m_currentHealth;                // Player's current health
    private bool m_canJump;                     // Bool to stop the player from being able to jump in certain situations                   
    private int m_maxHealth;                    // Player's max health
    private int m_overLoadedHealth;             // 20% of the max health that can temporarily be added above 100%
    private float m_jumpHeight;                 // How high the player can jump
    public Vector2 m_velocity;                  // How fast the player moves
    private Vector2 m_savedVelocity;            // Keeps track of how fast the player was moving before the player was stopped
    private float m_baseSpeed;                  // Keeps track of the player's current speed in runner mode
    private float m_plat1Speed;                 // How fast the player should move on platform1
    private float m_plat2Speed;                 // How fast the player should move on platform2
    private bool m_wallHit;
    private Color32[] m_playerColors;           // Used to store the 32 bit color channels that give the player color

    // Score Tracking Things
    //private float m_score;                    // Player's score
    private float m_scoreMultiplier;            // Player's score multiplier
    private float m_zone2Multiplier;            // Score multiplier for zone 2
    private float m_zone3Multiplier;            // Score multiplier for zone 3

    // Event Buffers and Timers
    private float m_damageBuffer;               // Prevents the player from being damaged again for a short time after being damaged
    private float m_jumpBuffer;                 // Buffers the tap input for jumping
    private float m_increaseVelocityTimer;      // Timer allowing player's velocity to be increased after set amount of time
    private bool m_runnerMode;                  // Determines which mode is currently being played. Runner mode active if true, Story mode active if false.

    // Collectable related data members
    private bool m_coinCollected;               // Tracks if the player has collected the coin

    // mobile touch data members
    private Vector3 m_startPos;                 // Starting position of the touch
    private Vector3 m_endPos;                   // Ending position of the touch
    private float m_startTime;                  // Timestamp for when a touch begins
    private float m_endTime;                    // Timestamp for when a touch ends
    private float m_swipeDistance;              // The distance of the user's swipe
    private float m_minSwipeDist;               // minimum distance to be registered a swipe

    // Time variables
    private float m_elapsedTime;                // Time passed since the last frame
    private float m_finishTime;                 // Simulates one minute for the rewind time

    private bool m_jumpButtonDown;              // Tracks if the jump button is currently being held down
    private bool m_jumpButtonPressed;           // Tracks if the jump button has been pressed

    private float m_jumpTime;                   // Max jump time
    private Vector3 m_jumpVector;               // Max jump height vector

    // Variables for Challenges
    private float zone1SecondTimer;
    private float zone2SecondTimer;
    private float zone3SecondTimer;
    private float zone1MinuteTimer;
    private float zone2MinuteTimer;
    private float zone3MinuteTimer;
    private int prevZoneMinute;
    private GameObject m_zone1, m_zone2;

    #region ACCESSORS AND MUTATORS
    public float AdjustScoreMultiplier
    {
        get { return m_scoreMultiplier; }
        set { m_scoreMultiplier = value; }
    }

    public float AdjustSpeed
    {
        //get { return m_speed; }
        set { m_baseSpeed += value; }
    }

    public bool CanJump
    {
        get { return m_canJump; }
        set { m_canJump = value; }
    }

    public bool Coin
    {
        get { return m_coinCollected; }
        set { m_coinCollected = value; }
    }

    public int CurrentHealth
    {
        get { return m_currentHealth; }
        set { m_currentHealth = value; }
    }

    public bool Damaged
    {
        get { return m_damaged; }
        //set { m_damaged = value; }
    }

    public float DistanceTraveled
    {
        get { return m_distanceTraveled; }
        //set { m_distanceTraveled = value; }
    }

    public bool Grounded
    {
        get { return m_grounded; }
        set { m_grounded = value; }
    }

    public bool Invincible
    {
        get { return m_invincible; }
        set { m_invincible = value; if (m_invincible) this.gameObject.GetComponentInChildren<ParticleSystem>().Play(); else this.gameObject.GetComponentInChildren<ParticleSystem>().Stop(); }
    }

    public float JumpHeight
    {
        get { return m_jumpHeight; }
        //set { m_jumpHeight = value; }
    }

    public float JumpCount
    {
        set { m_jumpCount = value; }
    }

    public bool Jumping
    {
        get { return m_jumping; }
        set { m_jumping = value; }
    }

    public int MaxHealth
    {
        get { return m_maxHealth; }
    }

    public int MaxJumps
    {
        get { return m_maxJumps; }
        set { m_maxJumps = value; }
    }

    public bool RunnerMode // Returns true if runner mode was selected; false otherwise
    {
        get { return m_runnerMode; }
        set { m_runnerMode = value; }
    }

    public bool Protected
    {
        //TODO: Perhaps a particle effect?
        get { return m_protected; }
        set
        {
            m_protected = value;
            if (m_protected == true)
                AkSoundEngine.PostEvent("Shield_Pickup", gameObject);
        }
    }

    public bool Rewinding
    {
        get { return m_rewinding; }
        set { m_rewinding = value; }
    }

    void SetJumpAnimation()
    {
        //get { return m_animator.GetBool("Jump"); }
        m_animator.SetBool("Jump", false);
    }

    public float SetScoreMultiplier
    {
        set { m_scoreMultiplier = value; }
    }

    public int TotalJumps
    {
        get { return m_totalTimesJumped; }
        //set { m_totalTimesJumped = value; }
    }

    public Vector2 Velocity
    {
        get { return m_velocity; }
        set { m_velocity = value; }
    }

    public bool Zone2
    {
        get { return m_onZonePlat2; }
    }

    public bool Zone3
    {
        get { return m_onZonePlat3; }
    }

    void delayCanJump()//need for Invoke call to delay the m_canJump bool being set to false
    {
        m_canJump = false;
    }
    #endregion //ACCESSORS AND MUTATORS

    // Use this for initialization
    void Awake()
    {
        Instance = this;

        if (gameObject.tag != "Player")
            gameObject.tag = "Player";
        // Show initial coin amount
        GameObject scoreObject = GameObject.Find("CoinsDisplay");
        if (scoreObject != null && PlayerPrefs.HasKey("Coins"))
            scoreObject.GetComponent<Text>().text = "" + PlayerPrefs.GetInt("Coins");

        // Checks to see which mode is loaded
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name.StartsWith("Runner"))
        {
            m_runnerMode = true;
            m_velocity.Set(15.0f, 0.0f);
            m_zone1 = GameObject.Find("Zone1");
            m_zone2 = GameObject.Find("Zone2");
        }
        else if (currentScene.name.StartsWith("Story"))
        {
            m_runnerMode = false;
            m_velocity.Set(7.0f, 0.0f);
        }

        // Animations
        m_animator = gameObject.GetComponent<Animator>();

        // Player Components
        m_rigidBody2D = GetComponent<Rigidbody2D>();
        m_collider2D = GetComponent<Collider2D>();
        m_bodySprites = GetComponentsInChildren<SpriteRenderer>();

        // Player Attributes
        m_maxHealth = 50;
        m_overLoadedHealth = 10;
        m_jumpHeight = 15.0f;
        m_jumpVector.Set(m_velocity.x, m_jumpHeight, 0.0f);
        m_currentHealth = m_maxHealth;
        m_baseSpeed = m_velocity.x;
        m_plat1Speed = m_velocity.x + 5;
        m_plat2Speed = m_velocity.x + 7;
        m_jumpTime = 0.35f;
        m_playerColors = new Color32[(int)PlayerColors.TotalColors];
        m_playerColors[(int)PlayerColors.Red] = new Color32(230, 9, 9, 255);
        m_playerColors[(int)PlayerColors.Green] = new Color32(38, 230, 30, 255);
        m_playerColors[(int)PlayerColors.Blue] = new Color32(62, 71, 230, 255);
        m_playerColors[(int)PlayerColors.Default] = new Color32(255, 255, 255, 255);

        // Player's Ability Trackers
        m_maxJumps = 1;
        m_jumpCount = 0;

        // Score things
        m_scoreMultiplier = 1.0f;
        m_zone2Multiplier = 1.0f;
        m_zone3Multiplier = 2.0f;

        // Collectable related variables
        m_coinCollected = false;

        // Event Buffers and Timers
        m_jumpBuffer = 0.0f;
        m_damageBuffer = 1.0f;
        m_increaseVelocityTimer = 0.0f;

        // Player's Action States
        m_falling = false;
        m_jumping = false;
        m_grounded = false;
        m_canJump = true;

        m_finishTime = 60f;
        m_origin = this.gameObject.transform.position;
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("EquippedGreen") == 2)
        {
            for (int index = 0; index < m_bodySprites.Length; ++index)
            {
                if (m_bodySprites[index].name.Contains("Hat") || m_bodySprites[index].name.Contains("Arm") || m_bodySprites[index].name.Contains("Leg"))
                {
                    m_bodySprites[index].color = m_playerColors[(int)PlayerColors.Green];
                }
            }
            //this.gameObject.GetComponentInChildren<SpriteRenderer>().color = m_playerColors[(int)PlayerColors.Green];
        }
        else if (PlayerPrefs.GetInt("EquippedRed") == 2)
        {
            for (int index = 0; index < m_bodySprites.Length; ++index)
            {
                if (m_bodySprites[index].name.Contains("Hat") || m_bodySprites[index].name.Contains("Arm") || m_bodySprites[index].name.Contains("Leg"))
                {
                    m_bodySprites[index].color = m_playerColors[(int)PlayerColors.Red];
                }
            }
        }
        else if (PlayerPrefs.GetInt("EquippedBlue") == 2)
        {
            for (int index = 0; index < m_bodySprites.Length; ++index)
            {
                if (m_bodySprites[index].name.Contains("Hat") || m_bodySprites[index].name.Contains("Arm") || m_bodySprites[index].name.Contains("Leg"))
                {
                    m_bodySprites[index].color = m_playerColors[(int)PlayerColors.Blue];
                }
            }
        }
        else if (PlayerPrefs.GetInt("EquippedDefault") == 2)
        {
            for (int index = 0; index < m_bodySprites.Length; ++index)
            {
                if (m_bodySprites[index].name.Contains("Hat") || m_bodySprites[index].name.Contains("Arm") || m_bodySprites[index].name.Contains("Leg"))
                {
                    m_bodySprites[index].color = m_playerColors[(int)PlayerColors.Default];
                }
            }
        }

        prevZoneMinute = 0;

        //initialize assistant manager script
       m_assistantManager = AssistantCrayonManager.Instance;
    }
    // Update is called once per frame
    void Update()
    {
        //catch fall through floor in runner mode
        if(m_runnerMode && transform.position.y < 1)
        {
            Vector3 resetPosition = transform.position;
            resetPosition.y = 1;
            transform.position = resetPosition;

        }
        //Debug.Log(transform.position.y);

        // In runner mode increase the player's velocity every 15 seconds until the player reaches a speed of 25
        if (m_runnerMode && m_baseSpeed <= 25)
        {
         
            if (m_increaseVelocityTimer <= 15)
            {
                m_increaseVelocityTimer += Time.deltaTime;
            }
            else
            {
                m_velocity.x += 2; // Increase player's speed after 15 seconds
                m_baseSpeed += 2;
                m_plat1Speed += 2;
                m_plat2Speed += 2;
                m_increaseVelocityTimer = 0.0f; // Reset the timer
            }
            if (!m_wallHit)
                m_jumpVector.Set(m_velocity.x, m_jumpHeight, 0.0f);
            else
                m_jumpVector.Set(-m_velocity.x, m_jumpHeight, 0.0f);
        }


        if (m_runnerMode && m_velocity.x < 15)
        {
            m_velocity.x = m_baseSpeed;
        }

        // Delays the amount of taps being registered at one time
        if (m_jumpBuffer > 0)
            m_jumpBuffer -= Time.deltaTime;

        // Time elapsed since game started
        m_elapsedTime += Time.deltaTime;


        #region Health Loss Over Time
        //if (m_rigidBody2D.velocity == Vector3.zero)
        //    m_moving = false;
        //else
        //    m_moving = true;
        // If the player is moving reduce their health over time
        //if (m_playerMoving)
        //updateHealth();
        //void updateHealth()
        //{
        //    m_trackerHP += Time.deltaTime;
        //    if (m_onZonePlat2)
        //    {
        //        if (m_trackerHP >= 3.0f)
        //        {
        //            m_currentHealth -= 1;
        //            m_trackerHP = 0;
        //        }
        //    }
        //    else if (m_onZonePlat3)
        //    {
        //        if (m_trackerHP >= 1.0f)
        //        {
        //            m_currentHealth -= 1;
        //            m_trackerHP = 0;
        //        }
        //    }
        //    else
        //    {
        //        if (m_trackerHP >= 5.0f)
        //        {
        //            m_currentHealth -= 1;
        //            m_trackerHP = 0;
        //        }
        //    }
        //}
        #endregion // Health Loss Over Time

        // Tracks how far the player has traveled from the player's starting point. This is also how the base score is calculated.
        m_distanceTraveled = Vector3.Distance(m_origin, this.gameObject.transform.position);

        // Handles the player taking damage
        if (m_damaged)
        {
            m_damageBuffer -= Time.deltaTime;
            if (m_damageBuffer <= 0)
            {
                m_damaged = false;
                m_damageBuffer = 1.0f;
            }
        }

        //Jumping code
        if (!this.Rewinding) // Did the user choose to rewind?
        {
            if (m_grounded && Input.GetMouseButtonDown(0) && m_canJump) // Check to see if the player is on a platform
            {
                AkSoundEngine.PostEvent("Jump1", gameObject);
                m_jumping = true;
                m_jumpCount++;
                m_totalTimesJumped++;
                GetComponent<Animator>().Play("Player_Jump");
                StartCoroutine(JumpRoutine());
                GetComponent<Animator>().Play("Player_Falling");
            }
            if (!m_grounded && m_jumpCount < m_maxJumps && m_canJump) // Check to see if the player is doulbe jumping
            {
                if (Input.GetMouseButtonDown(0) && m_jumpBuffer <= 0)
                {
                    AkSoundEngine.PostEvent("Jump1", gameObject);
                    m_jumpCount++;
                    m_jumpBuffer = 0.1f;
                    m_totalTimesJumped++;
                    GetComponent<Animator>().Play("Player_Jump");
                    StartCoroutine(JumpRoutine());
                    GetComponent<Animator>().Play("Player_Falling");
                    if (m_jumpCount == m_maxJumps)
                        Invoke("delayCanJump", 0.1f);

                    if (!m_grounded && m_jumpCount < m_maxJumps && m_canJump) // Check again because the player may have triple jump ability
                    {
                        if (Input.GetMouseButtonDown(0) && m_jumpBuffer <= 0)
                        {
                            AkSoundEngine.PostEvent("Jump1", gameObject);
                            m_jumpCount++;
                            m_totalTimesJumped++;
                            GetComponent<Animator>().Play("Player_Jump");
                            StartCoroutine(JumpRoutine());
                            GetComponent<Animator>().Play("Player_Falling");
                            if (m_jumpCount == m_maxJumps)
                                Invoke("delayCanJump", 0.1f);
                        }
                    }
                }
            }
            //Jump(); // This is the old jump
            //Swipe(); // This is the old fast fall functionality
        }

        if (m_grounded) // if the player is back on the ground reset everything
        {
            resetPlayerToGrounded();
            GetComponent<Animator>().Play("Player_Running");
        }

        m_rigidBody2D.velocity = new Vector3(m_velocity.x, m_rigidBody2D.velocity.y);

        #region zoneChallenge 
        if (m_runnerMode)
        {
            if (PlayerPrefs.GetInt("StayonZone") == 1)
            {
                if (!Player_Controller.Instance.Zone2 && !Player_Controller.Instance.Zone3)
                {
                    zone1SecondTimer += Time.deltaTime;

                    if (zone1SecondTimer > 60)
                    {
                        zone1SecondTimer = 0;
                        zone1MinuteTimer++;
                    }

                    if (zone1MinuteTimer > PlayerPrefs.GetInt("MinutesinOneGame"))
                    {
                        zone1MinuteTimer = PlayerPrefs.GetInt("MinutesinOneGame");
                    }
                }

                if (zone1MinuteTimer > PlayerPrefs.GetInt("MinutesinOneGameCounter"))
                {
                    PlayerPrefs.SetInt("MinutesinOneGameCounter", (int)zone1MinuteTimer);
                }
            }
            else if (PlayerPrefs.GetInt("StayonZone") == 2)
            {
                if (Player_Controller.Instance.Zone2 == true)
                {
                    zone2SecondTimer += Time.deltaTime;

                    if (zone2SecondTimer > 60)
                    {
                        zone2SecondTimer = 0;
                        zone2MinuteTimer++;
                    }

                    if (zone2MinuteTimer > PlayerPrefs.GetInt("MinutesinOneGame"))
                    {
                        zone2MinuteTimer = PlayerPrefs.GetInt("MinutesinOneGame");
                    }
                }

                if (zone2MinuteTimer > PlayerPrefs.GetInt("MinutesinOneGameCounter"))
                {
                    PlayerPrefs.SetInt("MinutesinOneGameCounter", (int)zone2MinuteTimer);
                }
            }
            else if (PlayerPrefs.GetInt("StayonZone") == 3)
            {
                if (Player_Controller.Instance.Zone3 == true)
                {
                    zone3SecondTimer += Time.deltaTime;

                    if (zone3SecondTimer > 60)
                    {
                        zone3SecondTimer = 0;
                        zone3MinuteTimer++;
                    }

                    if (zone3MinuteTimer > PlayerPrefs.GetInt("MinutesinOneGame"))
                    {
                        zone3MinuteTimer = PlayerPrefs.GetInt("MinutesinOneGame");
                    }
                }

                if (zone3MinuteTimer > PlayerPrefs.GetInt("MinutesinOneGameCounter"))
                {
                    PlayerPrefs.SetInt("MinutesinOneGameCounter", (int)zone3MinuteTimer);
                }
            }

            if (PlayerPrefs.GetInt("StayonZone") == 1)
            {
                if (!Player_Controller.Instance.Zone2 && !Player_Controller.Instance.Zone3)
                {
                    zone1SecondTimer += Time.deltaTime;
                    prevZoneMinute = PlayerPrefs.GetInt("MinutesTotalCounter");

                    if (zone1SecondTimer > 60)
                    {
                        zone1SecondTimer = 0;
                        zone1MinuteTimer++;
                        PlayerPrefs.SetInt("MinutesTotalCounter", 1 + prevZoneMinute);
                    }
                }

                if (PlayerPrefs.GetInt("MinutesTotalCounter") > PlayerPrefs.GetInt("MinutesTotal"))
                {
                    PlayerPrefs.SetInt("MinutesTotalCounter", PlayerPrefs.GetInt("MinutesTotal"));
                }
            }
            else if (PlayerPrefs.GetInt("StayonZone") == 2)
            {
                if (Player_Controller.Instance.Zone2 == true)
                {
                    zone2SecondTimer += Time.deltaTime;
                    prevZoneMinute = PlayerPrefs.GetInt("MinutesTotalCounter");

                    if (zone2SecondTimer > 60)
                    {
                        zone2SecondTimer = 0;
                        zone2MinuteTimer++;
                        PlayerPrefs.SetInt("MinutesTotalCounter", 1 + prevZoneMinute);
                    }
                }

                if (PlayerPrefs.GetInt("MinutesTotalCounter") > PlayerPrefs.GetInt("MinutesTotal"))
                {
                    PlayerPrefs.SetInt("MinutesTotalCounter", PlayerPrefs.GetInt("MinutesTotal"));
                }
            }
            else if (PlayerPrefs.GetInt("StayonZone") == 3)
            {
                if (Player_Controller.Instance.Zone3 == true)
                {
                    zone3SecondTimer += Time.deltaTime;
                    prevZoneMinute = PlayerPrefs.GetInt("MinutesTotalCounter");

                    if (zone3SecondTimer > 60)
                    {
                        zone3SecondTimer = 0;
                        zone3MinuteTimer++;
                        PlayerPrefs.SetInt("MinutesTotalCounter", 1 + prevZoneMinute);
                    }
                }

                if (PlayerPrefs.GetInt("MinutesTotalCounter") > PlayerPrefs.GetInt("MinutesTotal"))
                {
                    PlayerPrefs.SetInt("MinutesTotalCounter", PlayerPrefs.GetInt("MinutesTotal"));
                }
            }

            // Increase score multiplier based on player's current zone
            if (gameObject != null && m_zone1 != null && m_zone2 != null)
            {
                if (transform.position.y > m_zone1.transform.position.y && transform.position.y < m_zone2.transform.position.y)
                {
                    if (m_onZonePlat3)
                    {
                        m_onZonePlat3 = false;
                        AdjustScoreMultiplier -= m_zone3Multiplier;
                    }
                    if (!m_onZonePlat2)
                    {
                        m_onZonePlat2 = true;
                        AdjustScoreMultiplier += m_zone2Multiplier;
                    }
                    m_velocity.Set(m_plat1Speed, 0.0f);
                }
                else if (transform.position.y > m_zone2.transform.position.y)
                {
                    if (m_onZonePlat2)
                    {
                        m_onZonePlat2 = false;
                        AdjustScoreMultiplier -= m_zone2Multiplier;
                    }
                    if (!m_onZonePlat3)
                    {
                        m_onZonePlat3 = true;
                        AdjustScoreMultiplier += m_zone3Multiplier;
                    }
                    m_velocity.Set(m_plat2Speed, 0.0f);
                }
                if (transform.position.y < m_zone2.transform.position.y)
                {
                    if (m_onZonePlat3)
                    {
                        m_onZonePlat3 = false;

                        AdjustScoreMultiplier -= m_zone3Multiplier;
                    }
                    if (m_onZonePlat2)
                    {
                        m_onZonePlat2 = false;
                        AdjustScoreMultiplier -= m_zone2Multiplier;
                    }
                }
            }
        }
        #endregion //StayonZones challenges
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Ground" && !m_runnerMode)
        {
            if (coll.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
                m_canJump = true;
                GetComponent<Animator>().Play("Player_Running");
                m_velocity.Set(7.0f, 0.0f);
                if (m_wallHit)
                {
                    //this is so when you fall along the wall and hit the ground, the player gets forced away from the wall so the bool can reset
                    m_wallHit = false;
                    m_rigidBody2D.AddForce(new Vector2(-1000, 100));
                }
            }
        }
        else if (coll.gameObject.tag == "Ground")
        {
            if (coll.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
                m_canJump = true;
            }
            m_velocity.Set(m_baseSpeed, 0.0f);
        }
        else if (coll.gameObject.tag == "Plat1")
        {
            if (coll.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
                m_canJump = true;
            }
            m_velocity.Set(m_plat1Speed, 0.0f);
        }
        else if (coll.gameObject.tag == "Plat2")
        {
            if (coll.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
                m_canJump = true;
            }
            m_velocity.Set(m_plat2Speed, 0.0f);
        }

        //if (coll.gameObject.tag == "Wall")
        //{
        //    if (Input.GetMouseButtonDown(0) && m_jumpBuffer <= 0 /*timePassed >= keyDelay*/)
        //    {
        //        m_velocity = -m_velocity;
        //        m_jumpCount = 0;
        //    }
        //}

        if (coll.gameObject.tag == "WallRight")
        {

            m_jumpCount = 0;
            m_wallHit = true;
            m_canJump = true;
        }
        if (coll.gameObject.tag == "WallLeft")
        {
            GetComponent<Animator>().Play("Player_WallJump_Left");
            m_jumpCount = 0;
            m_wallHit = false;
            m_canJump = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == Enemy_Controller_Storymode.TAG)
        {
            int damage = collision.gameObject.GetComponent<Enemy_Controller_Storymode>().Damage;
            TakeDamage(damage);
        }


    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Plat1" || collision.gameObject.tag == "Plat2")
        {
            m_grounded = false;
        }

        if (collision.gameObject.tag == "WallRight")
        {
            m_grounded = false;
        }

        if (collision.gameObject.tag == "WallLeft")
        {
            m_grounded = false;
        }
    }

    #region oldJump
    //void WallJump()//old wall jump code
    //{
    //    m_jumpCount = 0;
    //    m_rigidBody2D.drag = 6;
    //    Physics2D.queriesStartInColliders = false;
    //    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, Velocity.x);
    //    Velocity = new Vector2(-Velocity.x, Velocity.y/*, 0.0f*/);
    //}
#endregion

    void OnCollisionStay2D(Collision2D collision)
    {
        //if (Input.GetMouseButtonDown(0) && !m_grounded)
        //{
        //    WallJump();
        //    //        //m_velocity = -m_velocity;
        //    //        //just testing out some things
        //    //        m_rigidBody2D.AddForce(new Vector2(m_velocity.x, 6));
        //    //        m_jumpCount = 0;
        //}

        if (collision.gameObject.tag == "Ground" && !m_runnerMode)
        {
            if (collision.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
            }
        }
        else if (collision.gameObject.tag == "Ground")
        {
            if (collision.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
            }
            m_velocity.Set(m_baseSpeed, 0.0f);
        }
        else if (collision.gameObject.tag == "Plat1")
        {
            if (collision.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
            }
            m_velocity.Set(m_plat1Speed, 0.0f);
        }
        else if (collision.gameObject.tag == "Plat2")
        {
            if (collision.gameObject.transform.position.y <= transform.position.y)
            {
                m_grounded = true;
            }
            m_velocity.Set(m_plat2Speed, 0.0f);
        }
    }

    void ResetVelocity()
    {
        if (m_runnerMode)
            m_velocity.x = 15.0f;
        else if (!m_runnerMode)
            m_velocity.x = 7.0f;
    }

    #region oldJump
    //void Jump()
    //{
    //    if (m_grounded && Input.GetMouseButtonDown(0) && m_canJump) // Check to see if the player is on a platform
    //    {
    //        m_jumping = true;
    //        m_rigidBody2D.velocity = new Vector3(m_rigidBody2D.velocity.x, m_jumpHeight + 10);
    //        m_jumpDelta = 0.0f;
    //        m_jumpCount++;
    //        m_totalTimesJumped++;
    //    }

    //    if (!m_grounded && m_jumpCount < m_maxJumps && m_canJump) // Player is not on a platform and not double jumped
    //    {
    //        m_jumping = true;
    //        if (Input.GetMouseButtonDown(0) && m_canJump)
    //        {
    //            //if they push the jump really fast give them a boost
    //            if (m_jumpDelta < 0.15)
    //            {
    //                m_rigidBody2D.velocity = new Vector3(m_rigidBody2D.velocity.x, m_jumpHeight + 10);
    //                m_animator.SetBool("Jump", true);
    //                m_jumpDelta = 0.0f;
    //                m_jumpCount++;

    //                if (m_jumpCount == m_maxJumps)
    //                    m_canJump = false;

    //                m_totalTimesJumped++;
    //                m_jumpBuffer = 0.1f;
    //            }
    //        }

    //        if (!m_grounded && m_jumpCount < m_maxJumps && m_canJump) // Check again because the player may have triple jump ability
    //        {
    //            if (Input.GetMouseButtonDown(0) && m_jumpBuffer <= 0)
    //            {
    //                //if they push the jump really fast give them a boost
    //                if (m_jumpDelta < 0.15)
    //                {
    //                    m_rigidBody2D.velocity = new Vector3(m_rigidBody2D.velocity.x, m_jumpHeight + 13);
    //                    //m_animator.SetBool("Jump", true);
    //                    m_jumpDelta = 0.0f;
    //                    m_jumpCount++;
    //                    if (m_jumpCount == m_maxJumps)
    //                        m_canJump = false;
    //                }
    //                else //else normal jump
    //                {
    //                    m_rigidBody2D.velocity = new Vector3(m_rigidBody2D.velocity.x, m_jumpHeight + 10);
    //                    m_jumpDelta = 0.0f;
    //                    //m_animator.SetBool("Jump", true);
    //                    m_jumpCount++;
    //                    if (m_jumpCount == m_maxJumps)
    //                        m_canJump = false;
    //                }
    //                m_totalTimesJumped++;
    //            }
    //        }
    //    }
    //}
#endregion

    void MoveFaster()
    {
        m_velocity.x++;
        m_baseSpeed++;
        m_plat1Speed++;
        m_plat2Speed++;
    }

    public void TakeDamage(int _damage)
    {
        // Turn off certain powerups if the player is hit
        if (m_protected && !m_runnerMode)
        {
            m_playerShield.SetActive(false);
            m_protected = false;
            return;
        }


        if (!m_invincible)
        {
            if (!m_damaged)
            {
                if (!m_runnerMode)
                {
                    GetComponent<Animator>().Play("Player_Hurt");
                    AkSoundEngine.PostEvent("Damaged_Feedback", gameObject);
                    m_currentHealth -= _damage;
                    //damageFeedback();
                    //Invoke("resetDamageFeedback", 0.1f);
                    //Invoke("damageFeedback", 0.2f);
                    //Invoke("resetDamageFeedback", 0.3f);
                    //Invoke("damageFeedback", 0.4f);
                    //Invoke("resetDamageFeedback", 0.5f);
                    //Invoke("damageFeedback", 0.6f);
                    //Invoke("resetDamageFeedback", 0.7f);
                    m_damaged = true;
                }

                if (!m_protected && m_runnerMode)
                {
                    RemoveAssistantCrayon();
                }
            }
        }
    }

    void damageFeedback()
    {
        //this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        this.gameObject.GetComponentInChildren<TrailRenderer>().enabled = false;
    }

    void resetDamageFeedback()
    {
        //this.gameObject.GetComponent<SpriteRenderer>().color = m_playerColors[(int)PlayerColors.Default];
        this.gameObject.GetComponentInChildren<TrailRenderer>().enabled = true;
        if (PlayerPrefs.GetInt("EquippedGreen") == 2)
        {
            this.gameObject.GetComponent<SpriteRenderer>().color = m_playerColors[(int)PlayerColors.Green];
        }
        else if (PlayerPrefs.GetInt("EquippedRed") == 2)
        {
            this.gameObject.GetComponent<SpriteRenderer>().color = m_playerColors[(int)PlayerColors.Red];
        }
        else if (PlayerPrefs.GetInt("EquippedBlue") == 2)
        {
            this.gameObject.GetComponent<SpriteRenderer>().color = m_playerColors[(int)PlayerColors.Blue];
        }
        else if (PlayerPrefs.GetInt("EquippedDefault") == 2)
        {
            this.gameObject.GetComponent<SpriteRenderer>().color = m_playerColors[(int)PlayerColors.Default];
        }
    }

    public void Fall()
    {
        m_falling = true;
        m_rigidBody2D.velocity = new Vector3(m_rigidBody2D.velocity.x, -m_jumpHeight * 2.0f);


    }

    IEnumerator JumpRoutine()
    {
        float timer = 0;

        while (Input.GetMouseButton(0) && timer < m_jumpTime)
        {
            ///Calculate how far through the jump we are as a percentage
            ///apply the full jump force on the first frame, then apply less force
            ///each consecutive frame

            if (!m_wallHit)
            {
                float proportionCompleted = timer / m_jumpTime;
                //Vector3 thisFrameJumpVector = Vector3.Lerp(m_jumpVector, Vector3.zero, proportionCompleted); // Use lerp to get a more jetpack floaty feeling jump. Use this in tandem with AddForce().
                Vector3 thisFrameJumpVector = Vector3.MoveTowards(m_jumpVector, Vector3.zero, proportionCompleted); // Use MoveTowards to get a more instantaneous jump. Use this in tandem with manipulating the velocity directly.
                m_rigidBody2D.velocity = new Vector3(thisFrameJumpVector.x, thisFrameJumpVector.y, thisFrameJumpVector.z);
                //m_rigidBody2D.AddForce(thisFrameJumpVector); // Need to adjust the force to be much higher for this to work.
                timer += Time.deltaTime;
                if (!m_runnerMode)
                    m_velocity = new Vector2(7, 0);
                yield return null;
            }
            else
            {
                GetComponent<Animator>().Play("Player_WallJump_Right");
                float proportionCompleted = timer / m_jumpTime;
                //Vector3 thisFrameJumpVector = Vector3.Lerp(m_jumpVector, Vector3.zero, proportionCompleted); // Use lerp to get a more jetpack floaty feeling jump. Use this in tandem with AddForce().
                Vector3 thisFrameJumpVector = Vector3.MoveTowards(m_jumpVector, Vector3.zero, proportionCompleted); // Use MoveTowards to get a more instantaneous jump. Use this in tandem with manipulating the velocity directly.
                m_rigidBody2D.velocity = new Vector3(-thisFrameJumpVector.x, thisFrameJumpVector.y, thisFrameJumpVector.z);
                //m_rigidBody2D.AddForce(thisFrameJumpVector); // Need to adjust the force to be much higher for this to work.
                timer += Time.deltaTime;

                if (!m_runnerMode)
                    m_velocity = new Vector2(-7, 0);
                yield return null;
            }
        }
        m_jumping = false;
        GetComponent<Animator>().Play("Player_Falling");
    }

    private void resetPlayerToGrounded()
    {
        ///<summary>
        /// reset the player's states
        /// </summary>
        m_jumping = false;
        m_falling = false;

        // reset the counts, timers, and buffers
        m_jumpDelta = 0.0f;
        m_jumpCount = 0;
        m_startTime = 0.0f;
        m_endTime = 0.0f;

        // reset touch positions
        m_startPos = Vector3.zero;
        m_endPos = Vector3.zero;

        // reset gravity
        m_rigidBody2D.gravityScale = 5.0f;
    }

    public void AddHealth()
    {
        m_currentHealth += 10;
        AkSoundEngine.PostEvent("Health_Pickup", gameObject);
        if (m_currentHealth > m_maxHealth)
            m_currentHealth = m_maxHealth;
    }

    public void AddAssistantCrayon()
    {
        if(m_assistantManager.AddAssistant())
        {
            //increase score multiplier
            AdjustScoreMultiplier += 1;
        }
    }

    public void RemoveAssistantCrayon()
    {
        if(m_assistantManager.RemoveAssistant())
        {
            AdjustScoreMultiplier -= 1;
        }
        else
        {
            //SceneManager.UnloadSceneAsync("Runner Level");
            //SceneManager.LoadScene("RunnerGameOver");
            m_currentHealth = -1;
        }
    }

    public void CrackedScreen()
    {
        crackedScreen.Instance.GetComponent<Image>().enabled = true;
    }
    public void GameOverScreen()
    {
        m_gameOverScreen.SetActive(true);
    }

    public void getGameOverScreen(GameObject _screen)
    {
        m_gameOverScreen = _screen;
    }

}