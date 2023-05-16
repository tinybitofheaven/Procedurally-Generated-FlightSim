using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    enum birdBehaviors
    {
        sing,
        preen,
        ruffle,
        peck,
        hopForward,
        hopBackward,
        hopLeft,
        hopRight,
    }

    public AudioClip song1;
    public AudioClip song2;
    public AudioClip flyAway1;
    public AudioClip flyAway2;

    Animator anim;

    BoxCollider birdCollider;
    Vector3 bColCenter;
    Vector3 bColSize;
    SphereCollider solidCollider;
    float distanceToTarget = 0.0f;
    float originalAnimSpeed = 1.0f;
    Vector3 originalVelocity = Vector3.zero;

    //hash variables for the animation states and animation properties
    int idleAnimationHash;
    int singAnimationHash;
    int ruffleAnimationHash;
    int preenAnimationHash;
    int peckAnimationHash;
    int hopForwardAnimationHash;
    int hopBackwardAnimationHash;
    int hopLeftAnimationHash;
    int hopRightAnimationHash;
    int worriedAnimationHash;
    int landingAnimationHash;
    int flyAnimationHash;
    int hopIntHash;
    int flyingBoolHash;
    int peckBoolHash;
    int ruffleBoolHash;
    int preenBoolHash;
    int landingBoolHash;
    int singTriggerHash;
    int flyingDirectionHash;
    int dieTriggerHash;

    public float scale = 1f;
    Rigidbody rb;
    Vector3 previousHeight;
    float turnTotalTimer = 0.5f;
    float turnTimer = 0f;

    public bool jumping = false;
    public bool onGround = true;
    public bool descend = false;

    // float flyingForce = 50.0f;
    float Angle = 0f;
    public Vector3 localRotationAxis = new Vector3(0, 1, 0);
    float turn = 0f;
    float animationTurnTimer = 0f;

    float speed = 0.1f;

    public GameObject groundRaycast;
    ConstantForce _constantForce;
    Vector3 flyingForce;
    public GameObject mesh;
    public GameObject trails;

    float ascendTimer = 0f;
    float descnedTimer = 0f;
    float neutralTimer = 0f;

    void Awake()
    {
        trails.SetActive(false);
        birdCollider = gameObject.GetComponent<BoxCollider>();
        rb = gameObject.GetComponent<Rigidbody>();
        bColCenter = birdCollider.center;
        bColSize = birdCollider.size;
        solidCollider = gameObject.GetComponent<SphereCollider>();
        anim = gameObject.GetComponent<Animator>();
        _constantForce = gameObject.GetComponent<ConstantForce>();

        idleAnimationHash = Animator.StringToHash("Base Layer.Idle");
        flyAnimationHash = Animator.StringToHash("Base Layer.fly");
        hopIntHash = Animator.StringToHash("hop");
        flyingBoolHash = Animator.StringToHash("flying");
        peckBoolHash = Animator.StringToHash("peck");
        ruffleBoolHash = Animator.StringToHash("ruffle");
        preenBoolHash = Animator.StringToHash("preen");
        landingBoolHash = Animator.StringToHash("landing");
        singTriggerHash = Animator.StringToHash("sing");
        flyingDirectionHash = Animator.StringToHash("flyingDirectionX");
        dieTriggerHash = Animator.StringToHash("die");

        flyingForce = (transform.forward * speed * scale) - (transform.up * speed / 10f * scale);
    }


    //when in air, after localForwardVelocity < 0.5f, set flying = true
    //if flying, always move forward a set speed
    //press A or D will turn in that direction
    //start banking until let go
    //no backwards
    //if near enough to ground (raycast?), change to not flying
    private void Update()
    {
        // CheckLanding();
        if (onGround)
        {
            trails.SetActive(false);
        }
        else
        {
            trails.SetActive(true);
        }

        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 1f, 40f), transform.position.z);
        //going up animation
        float localUpwardVelocity = Vector3.Dot(rb.velocity, gameObject.transform.up);
        if (localUpwardVelocity > 0f)
        {
            neutralTimer = 0f;
            UpdateVerticalRotation(-85f, ref ascendTimer);
            anim.SetFloat("flapSpeed", 2.5f);
        }

        //if flying
        if (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == flyAnimationHash && !descend)
        {
            _constantForce.relativeForce = flyingForce;
        }

        //dive
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!onGround)
            {
                CheckLanding();
                descend = true;
                anim.SetFloat("flapSpeed", 0.1f);

                // Vector3 eulers = mesh.transform.rotation.eulerAngles;
                // mesh.transform.rotation = Quaternion.Euler(new Vector3(-40f, eulers.y, eulers.z));
                neutralTimer = 0f;
                UpdateVerticalRotation(-40f, ref descnedTimer);

                Dive();
            }
        }
        else
        {
            descend = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (onGround)
            {
                Jump();
            }
            else
            {
                Ascend();
            }
        }

        turn = 0f;
        turn += (Input.GetKey(KeyCode.A) ? -1f : 0);
        turn += (Input.GetKey(KeyCode.D) ? 1f : 0);

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (!onGround)
            {
                anim.SetFloat("flapSpeed", 0.5f);
                float targetAngle = turn * 90f + Angle;
                animationTurnTimer += Time.deltaTime;

                anim.SetFloat(flyingDirectionHash, (targetAngle - Angle) / 90f * animationTurnTimer);
                Angle = Mathf.MoveTowardsAngle(Angle, targetAngle, 20f * Time.fixedDeltaTime); //rotation speed = 0.5f
                gameObject.transform.localRotation = Quaternion.Euler(Angle * localRotationAxis.x,
                                                                 Angle * localRotationAxis.y,
                                                                 Angle * localRotationAxis.z);
            }
            else
            {
                float targetAngle = turn * 90f + Angle;
                animationTurnTimer += Time.deltaTime;
                // Debug.Log(targetAngle);

                Angle = Mathf.MoveTowardsAngle(Angle, targetAngle, 20f * Time.fixedDeltaTime); //rotation speed = 0.5f
                gameObject.transform.localRotation = Quaternion.Euler(Angle * localRotationAxis.x,
                                                                 Angle * localRotationAxis.y,
                                                                 Angle * localRotationAxis.z);
            }
        }
        else
        {
            animationTurnTimer = 0f;
            anim.SetFloat(flyingDirectionHash, 0f);
        }

        //reset flying animations
        if (!onGround)
        {
            if (!(localUpwardVelocity > 0f) && !(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && !descend)
            {
                ascendTimer = 0f;
                descnedTimer = 0f;
                // Vector3 eulers = mesh.transform.rotation.eulerAngles;
                // mesh.transform.rotation = Quaternion.Euler(new Vector3(-64.55f, eulers.y, eulers.z));
                UpdateVerticalRotation(-64.55f, ref neutralTimer);
                anim.SetFloat("flapSpeed", 1f);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        CheckLanding();
    }

    void UpdateVerticalRotation(float x, ref float timer)
    {
        if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            // Debug.Log(x);
            Vector3 eulers = mesh.transform.rotation.eulerAngles;
            Quaternion ideal = Quaternion.Euler(new Vector3(x, eulers.y, eulers.z));
            timer += Time.deltaTime;
            mesh.transform.rotation = Quaternion.Lerp(mesh.transform.rotation, ideal, timer);
        }
    }

    private void CheckLanding()
    {
        bool landing = (Physics.Raycast(groundRaycast.transform.position, Vector3.down, 3f));
        if (landing == true)
        {
            // print("grounded!");
            landing = true;

            anim.SetFloat(flyingDirectionHash, 0);
            //initiate the landing for the bird to finally reach the target
            Vector3 vel = Vector3.zero;
            // solidCollider.enabled = false;
            // anim.SetFloat("flapSpeed", 1f);
            anim.SetBool(landingBoolHash, true);
            anim.SetBool(flyingBoolHash, false);
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            bool grounded = (Physics.Raycast(groundRaycast.transform.position, Vector3.down, 2f));
            if (grounded == true)
            {
                // trails.SetActive(false);
                UpdateVerticalRotation(-64.55f, ref neutralTimer);
                // rb.velocity = Vector3.zero;
                // rb.angularVelocity = Vector3.zero;

                GetComponent<Rigidbody>().drag = .5f;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                anim.SetBool(landingBoolHash, false);
                landing = false;
                anim.applyRootMotion = true;
                onGround = true;
                anim.SetBool(idleAnimationHash, true);
            }
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == landingAnimationHash)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.drag = 0.5f;
            anim.applyRootMotion = false;
            anim.SetBool(flyingBoolHash, true);
            anim.SetBool(landingBoolHash, false);
            landing = false;
        }
    }

    private void Dive()
    {
        Vector3 diveForce = (transform.forward * speed / 5f * scale) - (transform.up * speed * scale);
        _constantForce.relativeForce = diveForce;
    }

    private void Descend()
    {
        jumping = false;
        descend = true;

        rb.velocity = (transform.forward * 0.5f * scale) + (transform.up * -1f * scale);
    }

    private void Ascend()
    {
        rb.AddForce((transform.forward * speed * 10f * scale) + (transform.up * speed * 20f * scale));
    }

    private void Jump()
    {
        rb.constraints = ~RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (Random.value < .5)
        {
            GetComponent<AudioSource>().PlayOneShot(flyAway1, .1f);
        }
        else
        {
            GetComponent<AudioSource>().PlayOneShot(flyAway2, .1f);
        }
        onGround = false;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.drag = 0.5f;
        anim.applyRootMotion = false;
        anim.SetBool(flyingBoolHash, true);
        anim.SetBool(landingBoolHash, false);

        rb.AddForce((transform.forward * speed * 5f * scale) + (transform.up * speed * 10f * scale));
    }

    //Sets a variable between -1 and 1 to control the left and right banking animation
    float FindBankingAngle(Vector3 birdForward, Vector3 dirToTarget)
    {
        Vector3 cr = Vector3.Cross(birdForward, dirToTarget);
        float ang = Vector3.Dot(cr, Vector3.up);
        return ang;
    }
}
