using UnityEngine;
using UnityEngine.InputSystem; // ต้องใช้ตัวนี้สำหรับการรับค่าจากคีย์บอร์ดแบบใหม่

public class AirplaneFlightPhysicsSimulation : MonoBehaviour
{
    Rigidbody rb;
    bool engineOn = false;

    [Header("Flight Settings")]
    public float thrust = 5000f;
    public float liftCoefficient = 0.5f;
    public float stallAngle = 25f;
    public float stallLiftMultiplier = 0.3f;

    [Header("Drag Settings")]
    public float dragCoefficient = 0.05f;
    public float sideDrag = 2f;

    [Header("Control Power")]
    public float pitchPower = 100f; // เชิดหัว/กดหัว (แกน X)
    public float yawPower = 50f;    // หันซ้าย/ขวา (แกน Y)
    public float rollPower = 100f;  // เอียงปีก (แกน Z)
    public float turnStrength = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ทำให้เครื่องบินเสถียรขึ้น ไม่หัวทิ่มง่ายๆ
        rb.centerOfMass = new Vector3(0, -0.6f, -0.2f);
    }

    void FixedUpdate() // ใช้ FixedUpdate กับเรื่องที่เกี่ยวกับฟิสิกส์เสมอ
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // -------- 1. THRUST (แรงขับไปข้างหน้า) --------
        if (kb.spaceKey.isPressed)
        {
            engineOn = true;
            // ใส่แรงดันไปด้านหน้าของตัวเครื่องบิน
            rb.AddRelativeForce(Vector3.forward * thrust, ForceMode.Acceleration);
        }

        // -------- 2. SPEED (หาความเร็วเฉพาะทิศหน้าตรง) --------
        // Vector3.Dot ช่วยคำนวณว่าเครื่องบินพุ่งไปข้างหน้าเร็วแค่ไหน
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        // -------- 3. LIFT (แรงยก) --------
        if (engineOn && forwardSpeed > 5f)
        {
            // สูตรแรงยก Lift แปลผันตามความเร็วยกกำลังสอง
            float lift = forwardSpeed * forwardSpeed * liftCoefficient;

            // ตรวจสอบมุมเงย (Stall) ถ้าเชิดหน้ามากเกินไป ลมหลุดปีก แรงยกจะหาย
            float pitchAngle = Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(transform.forward, Vector3.up));
            if (pitchAngle > stallAngle)
            {
                lift *= stallLiftMultiplier; // ลดแรงยกลงเมื่อเกิด Stall
            }

            // ใส่แรงยกขึ้นฟ้ายกตัวเครื่องบิน
            rb.AddForce(transform.up * lift, ForceMode.Acceleration);
            Debug.DrawRay(transform.position, transform.up * 5f, Color.green); // เส้นสีเขียวแสดงแรงยก
        }

        // -------- 4. DRAG (แรงต้านอากาศ) --------
        Vector3 drag = -rb.linearVelocity * dragCoefficient;
        rb.AddForce(drag);

        // SIDE DRAG (ลดการไถลออกข้างแบบดริฟต์)
        Vector3 sideVel = Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        rb.AddForce(-sideVel * sideDrag);

        // -------- 5. CONTROL INPUT (บังคับทิศทาง) --------
        float pitch = 0;
        float roll = 0;
        float yaw = 0;

        if (kb.sKey.isPressed) pitch = 1;  // ดึงหัวขึ้น
        if (kb.wKey.isPressed) pitch = -1; // กดหัวลง
        if (kb.aKey.isPressed) roll = 1;   // เอียงซ้าย
        if (kb.dKey.isPressed) roll = -1;  // เอียงขวา
        if (kb.qKey.isPressed) yaw = -1;   // หันซ้าย
        if (kb.eKey.isPressed) yaw = 1;    // หันขวา

        // -------- 6. TORQUE CONTROL (ใส่แรงหมุน) --------
        rb.AddRelativeTorque(new Vector3(pitch * pitchPower, yaw * yawPower, -roll * rollPower));

        // -------- 7. BANKED TURN (ตีวงเลี้ยวเวลาเอียงเครื่อง) --------
        float bankAmount = Vector3.Dot(transform.right, Vector3.up);
        rb.AddForce(transform.right * bankAmount * forwardSpeed * turnStrength);

        Debug.DrawRay(transform.position, transform.forward * 5f, Color.blue); // เส้นสีฟ้าแสดงทิศทางเดินหน้า
    }
}