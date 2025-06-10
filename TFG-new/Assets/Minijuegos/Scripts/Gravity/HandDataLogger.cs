// HandDataLogger.cs
// ------------------------------------------------------------
// Registra posiciones de joints de la mano con XR Hands 1.5.1
// Ahora genera nombre de fichero: HandLog_<IDUsuario>_<JUEGO>_<MANO>_<nºSesión>.txt
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.SubsystemsImplementation;
using System.Globalization;

public class HandDataLogger : MonoBehaviour
{
    [Header("Sampling Settings")]
    [Tooltip("Intervalo entre muestras en segundos (ej: 0.1–0.5).")]
    [Range(0.01f, 2f)]
    public float sampleInterval = 0.1f;

    [Header("Which Hand")]
    [Tooltip("Marca true para la mano derecha, false para la izquierda.")]
    public bool rightHand = true;

    [Header("Joints to Log")]
    [Tooltip("Lista de joints (en orden) que quieres muestrear.")]
    public XRHandJointID[] jointsToLog = new XRHandJointID[]
    {
        XRHandJointID.Wrist,
        XRHandJointID.ThumbTip,
        XRHandJointID.IndexTip,
        XRHandJointID.MiddleTip,
        XRHandJointID.RingTip,
        XRHandJointID.LittleTip
    };

    // Estos valores los debe asignar SessionManager justo antes de llamar a Initialize():
    [HideInInspector] public string userID;    // Por ejemplo "LGM21A" o "NOID"
    [HideInInspector] public string gameCode;  // "01", "02" o "03"
    // rightHand ya existe: si es true → "R", si es false → "L".

    // Estado interno
    private XRHandSubsystem handSubsystem;
    private float nextSampleTime;
    private StreamWriter writer;
    private bool isLogging = false;

    void Awake()
    {
        // 1) Obtener la instancia del subsistema de manos
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
            Debug.Log($"[HandDataLogger] Encontrado XRHandSubsystem ({subsystems.Count} instancia(s))");
        }
        else
        {
            Debug.LogError("[HandDataLogger] ¡No se encontró XRHandSubsystem!");
        }
        // NOTA: ya no creamos fichero ni directorio aquí. Esto se hará en Initialize().
    }

    void Update()
    {
        if (!isLogging)
            return;

        if (handSubsystem == null)
        {
            Debug.LogWarning("[HandDataLogger] handSubsystem es null. Abortando muestreo.");
            return;
        }

        if (!handSubsystem.running)
        {
            Debug.LogWarning("[HandDataLogger] handSubsystem no está en ejecución. Abortando muestreo.");
            return;
        }

        if (Time.time >= nextSampleTime)
        {
            SampleAndWrite();
            nextSampleTime = Time.time + sampleInterval;
        }
    }

    /// <summary>
    /// Inicializa el logger:
    /// - Si userID está vacío o null, se usa "NOID".
    /// - Calcula el número de sesión contando archivos previos.
    /// - Crea carpeta userID dentro de persistentDataPath (si no existe).
    /// - Genera nombre de fichero: HandLog_<userID>_<gameCode>_<handCode>_<nSesion>.txt
    /// - Escribe cabecera: timestamp, <joint_x>, <joint_y>, <joint_z>, ...
    /// Debe llamarse antes de StartLogging().
    /// </summary>
    public void Initialize()
    {
        // 1) Determinar userID real
        string realID = string.IsNullOrWhiteSpace(userID) ? "NOID" : userID.Trim();
        realID = realID.Replace("/", "_").Replace("\\", "_"); // sanitizar
        userID = realID; // guardar la versión final

        // 2) Determinar handCode
        string handCode = rightHand ? "R" : "L";

        // 3) Carpeta base: persistentDataPath/userID
        string basePath = Application.persistentDataPath;
        string folderPath = Path.Combine(basePath, userID);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"[HandDataLogger] Carpeta creada: {folderPath}");
        }
        else
        {
            Debug.Log($"[HandDataLogger] Carpeta ya existe: {folderPath}");
        }

        // 4) Calcular número de sesión (contar ficheros que empiecen por "HandLog_<userID>_<gameCode>_<handCode>_")
        string filePrefix = $"HandLog_{userID}_{gameCode}_{handCode}_";
        string[] existing = Directory.GetFiles(folderPath, $"{filePrefix}*.txt");
        int nSesion = existing.Length + 1; // si hay 0 archivos → sesión 1, etc.

        // 5) Construir nombre de fichero completo
        string fileName = $"{filePrefix}{nSesion}.txt";
        string fullPath = Path.Combine(folderPath, fileName);

        // 6) Abrir StreamWriter
        writer = new StreamWriter(fullPath, false, Encoding.UTF8);
        Debug.Log($"[HandDataLogger] Fichero creado: {fullPath}");

        // 7) Escribir cabecera: timestamp, luego cada joint (_x, _y, _z)
        var header = new StringBuilder("timestamp");
        foreach (var id in jointsToLog)
        {
            header.Append($", {id}_x, {id}_y, {id}_z");
        }
        writer.WriteLine(header.ToString());
        writer.Flush();
        Debug.Log($"[HandDataLogger] Cabecera escrita: {header}");
    }

    /// <summary>
    /// Comienza la grabación de datos.
    /// </summary>
    public void StartLogging()
    {
        if (handSubsystem == null)
        {
            Debug.LogError("[HandDataLogger] No puedes arrancar el logging: handSubsystem es null.");
            return;
        }

        if (writer == null)
        {
            Debug.LogError("[HandDataLogger] No se ha llamado a Initialize() antes de StartLogging().");
            return;
        }

        isLogging = true;
        nextSampleTime = Time.time;
        Debug.Log($"[HandDataLogger] ▶ StartLogging() a t={Time.time:F3} (ID={userID}, Game={gameCode}, Hand={(rightHand ? "R" : "L")})");
    }

    /// <summary>
    /// Para la grabación y vacía el buffer.
    /// </summary>
    public void StopLogging()
    {
        if (!isLogging) return;

        isLogging = false;
        writer?.Flush();
        Debug.Log($"[HandDataLogger] ■ StopLogging() a t={Time.time:F3}");
    }

    private void SampleAndWrite()
    {
        XRHand hand = rightHand ? handSubsystem.rightHand : handSubsystem.leftHand;
        if (!hand.isTracked)
            return;

        int got = 0, missed = 0;
        // Iniciamos línea con timestamp
        var line = new StringBuilder(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));

        // Append de cada joint: ,x,y,z o ,,, si falta
        foreach (var id in jointsToLog)
        {
            XRHandJoint joint = hand.GetJoint(id);
            if (joint.TryGetPose(out Pose pose))
            {
                got++;
                Vector3 p = pose.position;
                line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F4},{1:F4},{2:F4}", p.x, p.y, p.z);
            }
            else
            {
                missed++;
                line.Append(",,,");
            }
        }

        writer.WriteLine(line.ToString());
        writer.Flush();
        Debug.Log($"[HandDataLogger] Línea escrita → jointsOK={got}, jointsMissed={missed}");
    }

    void OnDestroy()
    {
        writer?.Close();
    }
}
