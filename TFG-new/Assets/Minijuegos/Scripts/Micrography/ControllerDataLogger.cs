// ControllerDataLogger.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using System.Globalization;

public class ControllerDataLogger : MonoBehaviour
{
    [Header("Sampling Settings")]
    [Range(0.01f, 2f)]
    public float sampleInterval = 0.1f;

    [Header("Which Controller")]
    public bool rightController = true;

    [HideInInspector] public string userID;
    [HideInInspector] public string gameCode;

    private InputDevice device;
    private StreamWriter writer;
    private float nextSampleTime;
    private bool isLogging = false;
    private int sessionNumber;

    void OnEnable()
    {
        TryInitializeDevice();
        InputDevices.deviceConnected += OnDeviceConnected;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        StopLogging();
    }

    private void OnDeviceConnected(InputDevice d)
    {
        if (!device.isValid)
            TryInitializeDevice();
    }

    private void TryInitializeDevice()
    {
        var desired = InputDeviceCharacteristics.Controller
                    | (rightController ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left);

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desired, devices);

        if (devices.Count > 0)
            device = devices[0];
        else
            Debug.LogWarning($"[ControllerDataLogger] No se encontró {(rightController ? "controlador derecho" : "controlador izquierdo")}.");
    }

    /// <summary>
    /// Inicializa y abre un nuevo fichero:
    /// CONTROLLER_LOG_{userID}_{gameCode}_{CR|CL}_{nSesion:D2}.txt
    /// </summary>
    public void Initialize()
    {
        userID = string.IsNullOrWhiteSpace(userID) ? "NOID" : userID.Trim().Replace("/", "_").Replace("\\", "_");
        string ctrlCode = rightController ? "CR" : "CL";

        string basePath = Application.persistentDataPath;
        string folderPath = Path.Combine(basePath, userID);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePrefix = $"CONTROLLER_LOG_{userID}_{gameCode}_{ctrlCode}_";
        string[] existing = Directory.GetFiles(folderPath, filePrefix + "*.txt");
        sessionNumber = existing.Length + 1;

        string fileName = $"{filePrefix}{sessionNumber:D2}.txt";
        string fullPath = Path.Combine(folderPath, fileName);

        writer = new StreamWriter(fullPath, false, Encoding.UTF8);
        Debug.Log($"[ControllerDataLogger] Fichero creado: {fullPath}");

        var header = new StringBuilder("timestamp,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z,rot_w");
        header.Append(",vel_x,vel_y,vel_z,angVel_x,angVel_y,angVel_z");
        header.Append(",trigger,grip,primaryButton,secondaryButton,2DAxisX,2DAxisY,2DAxisClick,2DAxisTouch");
        writer.WriteLine(header.ToString());
        writer.Flush();
    }

    public void StartLogging()
    {
        if (!device.isValid) TryInitializeDevice();
        if (!device.isValid)
        {
            Debug.LogError("[ControllerDataLogger] Dispositivo inválido.");
            return;
        }
        if (writer == null)
        {
            Debug.LogError("[ControllerDataLogger] Llama a Initialize() antes de StartLogging().");
            return;
        }

        isLogging = true;
        nextSampleTime = Time.time;
        Debug.Log($"[ControllerDataLogger] ▶ StartLogging (Controlador {(rightController ? "D" : "I")}, Sesión={sessionNumber:D2})");
    }

    public void StopLogging()
    {
        if (!isLogging) return;

        isLogging = false;
        writer?.Flush();
        writer?.Close();
        writer = null;
        Debug.Log("[ControllerDataLogger] ■ StopLogging() y fichero cerrado.");
    }

    void Update()
    {
        if (!isLogging) return;

        if (Time.time >= nextSampleTime)
        {
            SampleAndWrite();
            nextSampleTime = Time.time + sampleInterval;
        }
    }

    private void SampleAndWrite()
    {
        string ts = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

        device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos);
        device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot);
        device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 vel);
        device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 angVel);
        device.TryGetFeatureValue(CommonUsages.trigger, out float trigger);
        device.TryGetFeatureValue(CommonUsages.grip, out float grip);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool btnA);
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool btnB);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis2D);
        device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool axisClick);
        device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool axisTouch);

        var line = new StringBuilder(ts);
        line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F4},{1:F4},{2:F4}", pos.x, pos.y, pos.z);
        line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F4},{1:F4},{2:F4},{3:F4}", rot.x, rot.y, rot.z, rot.w);
        line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F4},{1:F4},{2:F4}", vel.x, vel.y, vel.z);
        line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F4},{1:F4},{2:F4}", angVel.x, angVel.y, angVel.z);
        line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F3},{1:F3}", trigger, grip);
        line.Append($",{(btnA ? 1 : 0)},{(btnB ? 1 : 0)}");
        line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F4},{1:F4},{2},{3}", axis2D.x, axis2D.y, axisClick ? 1 : 0, axisTouch ? 1 : 0);

        writer.WriteLine(line.ToString());
        writer.Flush();
    }

    void OnDestroy()
    {
        StopLogging();
    }
}
