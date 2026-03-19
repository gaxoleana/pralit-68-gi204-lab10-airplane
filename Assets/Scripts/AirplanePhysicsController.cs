using UnityEngine;
using UnityEngine.InputSystem;

public class AirplanePhysicsController : MonoBehaviour
{
    [Header("Engine")]
    public float thrust = 40f; // แรงขับของเครื่องยนต์

    [Header("Aerodynamics")]
    public float liftCoefficient = 0.02f; // ค่าความแรงของ Lift
    public float dragCoefficient = 0.02f; // แรงต้านอากาศ
    public float sideDrag = 2f;           // ลดการไถลด้านข้าง

    [Header("BANK TURN")]
    public float turnStrength = 0.5f; // ความแรงการเลี้ยวเมื่อเอียงปีก

    [Header("STALL")]
    public float stallAngle = 35f;        // มุมที่เริ่ม stall
    public float stallLiftMultiplier = 0.3f; // ลด lift เมื่อ stall

    [Header("Control")]
    public float pitchPower = 15f; // เชิดหัว
    public float rollPower = 20f;  // เอียง
    public float yawPower = 15f;   // เลี้ยว turn

    Rigidbody rb;
    bool engineOn = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ทำให้เครื่องบินเสถียร
        // rb.centerOfMass = new Vector3(0, -0.5f, 0);
        rb.centerOfMass = new Vector3(0, -0.6f, -0.2f);
    }

    void FixedUpdate()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // -------- THRUST --------
        if (kb.spaceKey.isPressed)
        {
            engineOn = true;

            // แรงขับไปข้างหน้า
            rb.AddRelativeForce(Vector3.forward * thrust, ForceMode.Acceleration);
        }

        // --- SPEED - หาความเร็วตามทิศหน้า -------
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        // --- LIFT - Lift depends on forward speed -
        // The faster the plane moves → the stronger the lift.
        if (engineOn && forwardSpeed > 5f)
        {
            // Lift ∝ velocity²
            float lift = forwardSpeed * forwardSpeed * liftCoefficient;

            // ตรวจมุมเครื่องบินเพื่อจำลอง stall
            float pitchAngle = Vector3.Angle(transform.forward,
                                             Vector3.ProjectOnPlane(transform.forward,
                                             Vector3.up));

            if (pitchAngle > stallAngle)
            {
                // ลด lift เมื่อ stall
                lift *= stallLiftMultiplier;
            }

            // ใส่แรงยก Upward force lifts the airplane.
            rb.AddForce(transform.up * lift, ForceMode.Acceleration);

            // แสดง vector ของ Lift
            Debug.DrawRay(transform.position, transform.up * 5f, Color.green);
        }

        // --- DRAG (Air Resistance) -------
        Vector3 drag = -rb.linearVelocity * dragCoefficient;
        rb.AddForce(drag);

        // -------- SIDE DRAG (ลด drift ซ้ายขวา) --------
        Vector3 sideVel = Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        rb.AddForce(-sideVel * sideDrag);

        // --- CONTROL Input --------
        float pitch = 0;
        float roll = 0;
        float yaw = 0;

        if (kb.sKey.isPressed) pitch = 1;
        if (kb.wKey.isPressed) pitch = -1;

        if (kb.aKey.isPressed) roll = 1;
        if (kb.dKey.isPressed) roll = -1;

        if (kb.qKey.isPressed) yaw = -1;
        if (kb.eKey.isPressed) yaw = 1;

        // --- TORQUE CONTROL --------
        rb.AddRelativeTorque(new Vector3(pitch * pitchPower, yaw * yawPower, -roll * rollPower));

        // -------- BANKED TURN --------
        // เมื่อเครื่องบินเอียงปีก Lift จะเอียง ทำให้เครื่องบินเลี้ยว
        float bankAmount = Vector3.Dot(transform.right, Vector3.up);

        rb.AddForce(transform.right * bankAmount * forwardSpeed * turnStrength);

        // แสดงทิศ forward
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.blue);
    }
}