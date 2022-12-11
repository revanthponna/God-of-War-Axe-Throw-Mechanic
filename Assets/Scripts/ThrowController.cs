using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;

public class ThrowController : MonoBehaviour
{
    private Animator anim;
    private MovementInput input;
    private Rigidbody weaponRb;
    private Weapon weaponScript;
    private float returnTime;

    private Vector3 origLocPos;
    private Vector3 origLocRot;
    private Vector3 pullPosition;

    [Space]
    [Header("Public References")]
    public Transform weapon;
    public Transform hand;
    public Transform spine;
    public Transform curverPoint;

    [Space]
    [Header("Parameters")]
    public float throwPower = 30;
    public float cameraZoomOffset = 0.3f;

    [Space]
    [Header("Booleans")]
    public bool walking = true;
    public bool aiming = false;
    public bool hasWeapon = true;
    public bool pulling = false;

    [Space]
    [Header("Particles and Trails")]
    public ParticleSystem glowParticle;
    public ParticleSystem catchParticle;
    public ParticleSystem trailParticle;
    public TrailRenderer trailRenderer;

    [Space]
    [Header("UI")]
    public Image reticle;

    [Space]
    // For third person Cinemachine
    public CinemachineFreeLook virtualCamera;
    public CinemachineImpulseSource impulseSource;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        // Getting respective components and scripts
        anim = GetComponent<Animator>();
        input = GetComponent<MovementInput>();
        weaponRb = weapon.GetComponent<Rigidbody>();
        weaponScript = weapon.GetComponent<Weapon>();
        origLocPos = weapon.localPosition;
        origLocRot = weapon.localEulerAngles;
        reticle.DOFade(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (aiming) {
            input.RotateToCamera(transform);
        }
        else {
            transform.eulerAngles = new Vector3(Mathf.LerpAngle(transform.eulerAngles.x, 0, 0.2f), transform.eulerAngles.y, transform.eulerAngles.z);
        }
        // Setting respective animations
        anim.SetBool("pulling", pulling);

        walking = input.Speed > 0;
        anim.SetBool("walking", walking);    
        
        // Input Controls
        if (Input.GetMouseButtonDown(1) && hasWeapon) {
            Aim(true, true, 0); // Aiming true when right mouse button is held down
        }

        if (Input.GetMouseButtonUp(1) && hasWeapon) {
            Aim(false, true, 0); // Aiming false when right mouse button is released
        }

        if (hasWeapon) {
            if (aiming && Input.GetMouseButtonDown(0)) {
                anim.SetTrigger("throw"); // Setting the animation throw trigger when aiming and left mouse button is clicked
            }
        } else {
            if(Input.GetMouseButtonDown(0)) {
                WeaponStartPull(); // Start pulling when weapon is thrown, not aiming and left mouse button is clicked
            }
        }

        if (pulling) {
            if(returnTime < 1) {
                weapon.position = GetQuadraticCurvePoint(returnTime, pullPosition, curverPoint.position, hand.position); // Getting the middle curve point of the quadratic bezier curve for the axe return 
                returnTime += Time.deltaTime * 1.5f;
            } else {
                WeaponCatch();
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name); // Restart scene
        }
    }
    
    void Aim(bool state, bool changeCamera, float delay)
    {
        if (walking)
            return;
        
        // Setting the aiming state and respective animation
        aiming = state;

        anim.SetBool("aiming", aiming);

        // UI
        float fade = state ? 1 : 0;
        reticle.DOFade(fade, 0.2f);

        if (!changeCamera)
            return;

        // Camera Offset
        float newAim = state ? cameraZoomOffset : 0;
        float originalAim = !state ? cameraZoomOffset : 0;
        DOVirtual.Float(originalAim, newAim, 0.5f, CameraOffset).SetDelay(delay);

        // Particle Effects
        if (state) {
            glowParticle.Play();
        } else {
            glowParticle.Stop();
        }
    }

    public void WeaponThrow()
    {
        Aim(false, true, 1f);

        hasWeapon = false;

        weaponScript.activated = true;
        weaponRb.isKinematic = false; // setting isKinematic to false when the axe is detached from the player
        weaponRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        weapon.parent = null;
        weapon.eulerAngles = new Vector3(0, -90 + transform.eulerAngles.y, 0); // Changing Euler angles of the axe for rotation
        weapon.transform.position += transform.right / 5;

        weaponRb.AddForce(Camera.main.transform.forward * throwPower + transform.up * 2, ForceMode.Impulse); // adding force for the throw

        // Trail
        trailRenderer.emitting = true;
        trailParticle.Play();
    }

    public void WeaponStartPull()
    {
        pullPosition = weapon.position;

        weaponRb.Sleep();
        weaponRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        weaponRb.isKinematic = true;

        weapon.DORotate(new Vector3(-90, -90, 0), 0.2f).SetEase(Ease.InOutSine);
        weapon.DOBlendableLocalRotateBy(Vector3.right * 90, 0.5f);

        weaponScript.activated = true;
        pulling = true;
    }

    public void WeaponCatch()
    {
        returnTime = 0;
        pulling = false;

        weapon.parent = hand;
        weaponScript.activated = false;
        weapon.localEulerAngles = origLocRot;
        weapon.localPosition = origLocPos;
        hasWeapon = true;

        // Particle and Trail
        catchParticle.Play();
        trailRenderer.emitting = false;
        trailParticle.Stop();

        // Camera Shake
        impulseSource.GenerateImpulse(Vector3.right);
    }

    public Vector3 GetQuadraticCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return (uu * p0) + (2 * u * t * p1) + (tt * p2); // using the quadratic bezier formula to get the middle curve point of the return arc
    }

    void CameraOffset(float offset)
    {
        virtualCamera.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(offset, 1.5f, 0);
        virtualCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(offset, 1.5f, 0);
        virtualCamera.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(offset, 1.5f, 0);
    }
}
