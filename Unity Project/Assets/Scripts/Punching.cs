using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punching : MonoBehaviour
{
    private List<Joycon> joycons;

    // Values made available via Unity
    public float[] stick;
    public Vector3 gyro;
    public Vector3 accel;
    // jc_ind = 1:left = 0:Right
    public int jc_ind = 0;
    public Quaternion orientation;


    private float rightTime;
    private float leftTime;
    private bool ableRightHit;
    private bool ableLeftHit;
    private bool rightFlag;
    private bool leftFlag;

    public GameObject Player;
    // パンチのクールタイム
    public float CoolTime = 0.3f;

    void Start()
    {
        gyro = new Vector3(0, 0, 0);
        accel = new Vector3(0, 0, 0);
        // get the public Joycon array attached to the JoyconManager in scene
        // シーン内のジョイコンマネージャーにアタッチされているジョイコン配列を取得
        joycons = JoyconManager.Instance.j;
        if (joycons.Count < jc_ind + 1)
        {
            Destroy(gameObject);
        }

        ableLeftHit = true;
        ableRightHit = true;
        rightFlag = false;
        leftFlag = false;
    }

    // Update is called once per frame
    void Update()
    { 
        // make sure the Joycon only gets checked if attached
        // ジョイコンが接続されている時だけチェックする
        if (joycons.Count > 0)
        {
            Joycon j = joycons[jc_ind];
            // GetButtonDown checks if a button has been pressed (not held)
            if (j.GetButtonDown(Joycon.Button.SHOULDER_2))
            {
                Debug.Log("Shoulder button 2 pressed");
                // GetStick returns a 2-element vector with x/y joystick components
                Debug.Log(string.Format("Stick x: {0:N} Stick y: {1:N}", j.GetStick()[0], j.GetStick()[1]));

                // Joycon has no magnetometer, so it cannot accurately determine its yaw value. Joycon.Recenter allows the user to reset the yaw value.
                j.Recenter();
            }
            // GetButtonDown checks if a button has been released
            if (j.GetButtonUp(Joycon.Button.SHOULDER_2))
            {
                Debug.Log("Shoulder button 2 released");
            }
            // GetButtonDown checks if a button is currently down (pressed or held)
            if (j.GetButton(Joycon.Button.SHOULDER_2))
            {
                Debug.Log("Shoulder button 2 held");
            }

            if (j.GetButtonDown(Joycon.Button.DPAD_DOWN))
            {
                Debug.Log("Rumble");
                // 傾き（多分）
                // Rumble for 200 milliseconds, with low frequency rumble at 160 Hz and high frequency rumble at 320 Hz. For more information check:
                // https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering/blob/master/rumble_data_table.md

                j.SetRumble(160, 320, 0.6f, 200);

                // The last argument (rightTime) in SetRumble is optional. Call it with three arguments to turn it on without telling it when to turn off.
                // (Useful for dynamically changing rumble values.)
                // Then call SetRumble(0,0,0) when you want to turn it off.
            }

            stick = j.GetStick();

            // Gyro values: x, y, z axis values (in radians per second)
            gyro = j.GetGyro();

            // Accel values:  x, y, z axis values (in Gs)
            // 加速度
            accel = j.GetAccel();

            // ポジション取得
            Transform ArmTransform = this.transform;
            Vector3 ArmPos = ArmTransform.localPosition;
            ArmTransform.parent=Player.transform;
            Transform PlayerTransform = Player.transform;
            Vector3 PlayerPos = PlayerTransform.localPosition;

            // 
            Rigidbody rb = this.GetComponent<Rigidbody>();
            Vector3 Punch = 0.3f * ArmTransform.forward;

            // Right
            if (jc_ind == 0)
            {
                if (ableRightHit)
                {
                    ArmPos.x = -1.0f;
                    ArmPos.y = 0f;
                    ArmPos.z = 0f;
                    ArmTransform.localPosition = ArmPos;


                    if (j.GetVector().x < 0f && j.GetAccel().x < 0)
                    {
                        rightFlag = true;
                        ableRightHit = false;
                    }
                }


                if (rightFlag)
                {
                    rightTime += Time.deltaTime;

                    rb.AddForce(transform.forward * 0.3f, ForceMode.Force);

                    if (CoolTime <= rightTime)
                    {
                        rightFlag = false;
                        ableRightHit = true;

                        rightTime = 0f;
                    }
                }
            }

            // Left
            if (jc_ind == 1)
            {
                if (ableLeftHit)
                {
                    ArmPos.x = 1.0f;
                    ArmPos.y = 0f;
                    ArmPos.z = 0f;
                    ArmTransform.localPosition = ArmPos;

                    if (j.GetVector().x < 0f && j.GetAccel().x < 0)
                    {
                        leftFlag = true;
                        ableLeftHit = false;
                    }
                }


                if (leftFlag)
                {
                    leftTime += Time.deltaTime;

                    rb.AddForce(Punch);

                    if (CoolTime < leftTime)
                    {
                        leftFlag = false;
                        ableLeftHit = true;

                        leftTime = 0f;
                    }
                }
            }

            /*orientation = j.GetVector();
            if (j.GetButton(Joycon.Button.DPAD_UP))
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
            }
            gameObject.transform.rotation = orientation;*/
        }
    }
}