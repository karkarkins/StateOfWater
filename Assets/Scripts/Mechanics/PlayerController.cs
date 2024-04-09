using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        public bool[] stateOfMatter;
        

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        public Animator animate;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        private void Start()
        {
            stateOfMatter[0] = true;
            stateOfMatter[1] = false;
            stateOfMatter[2] = false;
        }
        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update()
        {
            

            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                {
                    jumpState = JumpState.PrepareToJump;
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }

                if (Input.GetKey(KeyCode.Alpha1))
                {
                    stateOfMatter[0] = true;
                    stateOfMatter[1] = false;
                    stateOfMatter[2] = false;
                    animate.SetBool("Water", stateOfMatter[0]);
                    animate.SetBool("Ice", stateOfMatter[1]);
                    animate.SetBool("Steam", stateOfMatter[2]);


                }
                else if (Input.GetKey(KeyCode.Alpha2))
                {
                    stateOfMatter[0] = false;
                    stateOfMatter[1] = true;
                    stateOfMatter[2] = false;
                    animate.SetBool("Water", stateOfMatter[0]);
                    animate.SetBool("Ice", stateOfMatter[1]);
                    animate.SetBool("Steam", stateOfMatter[2]);

                }
                else if (Input.GetKey(KeyCode.Alpha3))
                {
                    stateOfMatter[0] = false;
                    stateOfMatter[1] = false;
                    stateOfMatter[2] = true;
                    animate.SetBool("Water", stateOfMatter[0]);
                    animate.SetBool("Ice", stateOfMatter[1]);
                    animate.SetBool("Steam", stateOfMatter[2]);
                    

                }

                if (stateOfMatter[0] == true) // Water State
                {
                    gravityModifier = 1;
                    maxSpeed = 5.0f;
                    //spriteRenderer.color = new Color(255, 0, 0);
                }

                if (stateOfMatter[1] == true) // Ice State
                {
                    gravityModifier = 10;
                    maxSpeed = 0.5f;
                    jumpState = JumpState.Grounded;
                    //spriteRenderer.color = new Color(0, 255, 0);
                }

                if (stateOfMatter[2] == true) // Steam State
                {
                    gravityModifier = 0;
                    maxSpeed = 10.0f;
                    //spriteRenderer.color = new Color(0, 0, 255);
                    transform.Translate(3.0f * Time.deltaTime * Vector2.up);
                }

            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
        /*
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((collision.gameObject.CompareTag("Player")) && (stateOfMatter[1] == true))
            {
                Destroy(collision.gameObject);
            }
        }
        */
    }
}