// Assets/Scripts/ControllerDataLogger.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using System.Globalization;

public class ControllerDataLogger : MonoBehaviour
{
    [Header("Sampling Settings")]
    [Tooltip("Intervalo entre muestras en segundos (ej: 0.01–0.5).")]
    [Range(0.01f, 2f)]
    public float sampleInterval = 0.1f;

    [Header("Which Controller")]
    [Tooltip("Marca true para el controlador derecho; false para el izquierdo.")]
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
        // Intentamos inicializar ahora y cada vez que llegue un nuevo dispositivo
        TryInitializeDevice();
        InputDevices.deviceConnected += OnDeviceConnected;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void OnDeviceConnected(InputDevice connectedDevice)
    {
        // Si aún no tenemos uno, reintenta
        if (!device.isValid)
            TryInitializeDevice();
    }

    /// <summary>
    /// Busca el controlador izquierdo/derecho y lo asigna a `device`.
    /// </summary>
    private void TryInitializeDevice()
    {
        var desired = InputDeviceCharacteristics.Controller
                    | (rightController
                        ? InputDeviceCharacteristics.Right
                        : InputDeviceCharacteristics.Left);

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desired, devices);

        if (devices.Count > 0)
        {
            device = devices[0];
            Debug.Log($"[ControllerDataLogger] Dispositivo encontrado: {device.name}");
        }
        else
        {
            Debug.LogWarning($"[ControllerDataLogger] No encontrado {(rightController ? "Controlador Derecho" : "Controlador Izquierdo")}");
        }
    }

    public void Initialize()
    {
        // Sanitizar userID
        userID = string.IsNullOrWhiteSpace(userID) ? "NOID" : userID.Trim().Replace("/", "_").Replace("\\", "_");

        // Carpeta usuario
        string folder = Path.Combine(Application.persistentDataPath, userID);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // Prefijo y recuento de sesiones
        string handCode = rightController ? "CR" : "CL";
        string prefix = $"ControllerLog_{userID}_{gameCode}_{handCode}_";
        var files = Directory.GetFiles(folder, prefix + "*.txt");
        sessionNumber = files.Length + 1;

        string fileName = $"{prefix}{sessionNumber:D2}.txt";
        string fullPath = Path.Combine(folder, fileName);
        writer = new StreamWriter(fullPath, false, Encoding.UTF8);
        Debug.Log($"[ControllerDataLogger] Creando log: {fullPath}");

        // Cabecera
        var hdr = new StringBuilder("timestamp,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z,rot_w");
        hdr.Append(",vel_x,vel_y,vel_z,angVel_x,angVel_y,angVel_z");
        hdr.Append(",trigger,grip,primaryButton,secondaryButton,2DAxisX,2DAxisY,2DAxisClick,2DAxisTouch");
        writer.WriteLine(hdr.ToString());
        writer.Flush();
    }

    public void StartLogging()
    {
        // Si todavía no detectamos el controlador, reinténtalo
        if (!device.isValid)
            TryInitializeDevice();

        if (!device.isValid)
        {
            Debug.LogError("[ControllerDataLogger] No se puede empezar: dispositivo inválido.");
            return;
        }
        if (writer == null)
        {
            Debug.LogError("[ControllerDataLogger] Llama a Initialize() antes de StartLogging().");
            return;
        }

        isLogging = true;
        nextSampleTime = Time.time;
        Debug.Log($"[ControllerDataLogger] ▶ StartLogging (Hand={(rightController ? "D" : "I")}, Session={sessionNumber})");
    }

    void Update()
    {
        if (!isLogging) return;

        // Si perdemos el dispositivo en mitad de la sesión, intentamos recuperarlo
        if (!device.isValid)
        {
            TryInitializeDevice();
            if (!device.isValid) return;
        }

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

        string line = string.Join(",",
            ts,
            pos.x.ToString("F4", CultureInfo.InvariantCulture),
            pos.y.ToString("F4", CultureInfo.InvariantCulture),
            pos.z.ToString("F4", CultureInfo.InvariantCulture),
            rot.x.ToString("F4", CultureInfo.InvariantCulture),
            rot.y.ToString("F4", CultureInfo.InvariantCulture),
            rot.z.ToString("F4", CultureInfo.InvariantCulture),
            rot.w.ToString("F4", CultureInfo.InvariantCulture),
            vel.x.ToString("F4", CultureInfo.InvariantCulture),
            vel.y.ToString("F4", CultureInfo.InvariantCulture),
            vel.z.ToString("F4", CultureInfo.InvariantCulture),
            angVel.x.ToString("F4", CultureInfo.InvariantCulture),
            angVel.y.ToString("F4", CultureInfo.InvariantCulture),
            angVel.z.ToString("F4", CultureInfo.InvariantCulture),
            trigger.ToString("F3", CultureInfo.InvariantCulture),
            grip.ToString("F3", CultureInfo.InvariantCulture),
            btnA ? "1" : "0",
            btnB ? "1" : "0",
            axis2D.x.ToString("F4", CultureInfo.InvariantCulture),
            axis2D.y.ToString("F4", CultureInfo.InvariantCulture),
            axisClick ? "1" : "0",
            axisTouch ? "1" : "0"
        );

        writer.WriteLine(line);
        writer.Flush();
    }

    public void StopLogging()
    {
        if (!isLogging) return;
        isLogging = false;
        writer?.Flush();
        Debug.Log($"[ControllerDataLogger] ■ StopLogging()");
    }

    void OnDestroy()
    {
        writer?.Close();
    }
}
