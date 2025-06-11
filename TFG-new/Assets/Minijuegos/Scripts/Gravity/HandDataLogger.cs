// HandDataLogger.cs
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
    [Range(0.01f, 2f)]
    public float sampleInterval = 0.1f;

    [Header("Which Hand")]
    public bool rightHand = true;

    [Header("Joints to Log")]
    public XRHandJointID[] jointsToLog = new XRHandJointID[]
    {
        XRHandJointID.Wrist,
        XRHandJointID.ThumbTip,
        XRHandJointID.IndexTip,
        XRHandJointID.MiddleTip,
        XRHandJointID.RingTip,
        XRHandJointID.LittleTip
    };

    [HideInInspector] public string userID;
    [HideInInspector] public string gameCode;

    private XRHandSubsystem handSubsystem;
    private float nextSampleTime;
    private StreamWriter writer;
    private bool isLogging = false;

    void Awake()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0)
            handSubsystem = subsystems[0];
        else
            Debug.LogError("[HandDataLogger] ¡No se encontró XRHandSubsystem!");
    }

    void Update()
    {
        if (!isLogging || handSubsystem == null || !handSubsystem.running)
            return;

        if (Time.time >= nextSampleTime)
        {
            SampleAndWrite();
            nextSampleTime = Time.time + sampleInterval;
        }
    }

    /// <summary>
    /// Inicializa y abre un nuevo fichero:
    /// HAND_LOG_{userID}_{gameCode}_{L|R}_{nSesion:D2}.txt
    /// </summary>
    public void Initialize()
    {
        userID = string.IsNullOrWhiteSpace(userID) ? "NOID" : userID.Trim().Replace("/", "_").Replace("\\", "_");
        string handCode = rightHand ? "R" : "L";

        string basePath = Application.persistentDataPath;
        string folderPath = Path.Combine(basePath, userID);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePrefix = $"HAND_LOG_{userID}_{gameCode}_{handCode}_";
        string[] existing = Directory.GetFiles(folderPath, filePrefix + "*.txt");
        int nSesion = existing.Length + 1;

        string fileName = $"{filePrefix}{nSesion:D2}.txt";
        string fullPath = Path.Combine(folderPath, fileName);

        writer = new StreamWriter(fullPath, false, Encoding.UTF8);
        Debug.Log($"[HandDataLogger] Fichero creado: {fullPath}");

        var header = new StringBuilder("timestamp");
        foreach (var id in jointsToLog)
            header.Append($", {id}_x, {id}_y, {id}_z");

        writer.WriteLine(header.ToString());
        writer.Flush();
    }

    public void StartLogging()
    {
        if (handSubsystem == null)
        {
            Debug.LogError("[HandDataLogger] handSubsystem es null.");
            return;
        }
        if (writer == null)
        {
            Debug.LogError("[HandDataLogger] Llama a Initialize() antes de StartLogging().");
            return;
        }

        isLogging = true;
        nextSampleTime = Time.time;
        Debug.Log($"[HandDataLogger] ▶ StartLogging (ID={userID}, Juego={gameCode}, Mano={(rightHand ? "R" : "L")})");
    }

    /// <summary>
    /// Para el logging, cierra el StreamWriter y libera el recurso.
    /// </summary>
    public void StopLogging()
    {
        if (!isLogging) return;

        isLogging = false;
        writer?.Flush();
        writer?.Close();
        writer = null;
        Debug.Log($"[HandDataLogger] ■ StopLogging() y fichero cerrado.");
    }

    private void SampleAndWrite()
    {
        XRHand hand = rightHand ? handSubsystem.rightHand : handSubsystem.leftHand;
        if (!hand.isTracked) return;

        var line = new StringBuilder(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
        foreach (var id in jointsToLog)
        {
            XRHandJoint joint = hand.GetJoint(id);
            if (joint.TryGetPose(out Pose pose))
            {
                Vector3 p = pose.position;
                line.AppendFormat(CultureInfo.InvariantCulture, ",{0:F4},{1:F4},{2:F4}", p.x, p.y, p.z);
            }
            else
            {
                line.Append(",,,");

            }
        }

        writer.WriteLine(line.ToString());
        writer.Flush();
    }

    void OnDestroy()
    {
        writer?.Close();
        writer = null;
    }
}
