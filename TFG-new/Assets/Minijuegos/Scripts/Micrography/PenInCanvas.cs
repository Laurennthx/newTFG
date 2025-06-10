using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PenInCanvas : MonoBehaviour
{
    [Header("Pen Settings")]
    public Transform tip;
    public Material drawingMaterial;
    public Material tipMaterial;
    [Range(0.001f, 0.05f)] public float penWidth = 0.005f;

    [Header("Input Actions")]
    public InputActionReference drawAction; // gatillo para dibujar

    // Estado interno
    bool isTouchingCanvas = false;
    Transform currentCanvas = null;

    XRGrabInteractable grabInteractable;
    LineRenderer currentLine;
    int pointCount = 0;
    Color currentColor;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        currentColor = tipMaterial != null ? tipMaterial.color : Color.black;
    }

    void OnEnable()
    {
        drawAction.action.Enable();
    }

    void OnDisable()
    {
        drawAction.action.Disable();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Canvas3D"))
        {
            isTouchingCanvas = true;
            currentCanvas = other.transform;
        }

        var cc = other.GetComponent<ColorCapsule>();
        if (cc != null)
            SetDrawingColor(cc.capsuleColor);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Canvas3D") && other.transform == currentCanvas)
        {
            isTouchingCanvas = false;
            currentCanvas = null;
            currentLine = null;
        }
    }

    void Update()
    {
        if (!grabInteractable.isSelected)
        {
            currentLine = null;
            return;
        }

        // Dibujar con gatillo manteniendo el comportamiento original
        if (isTouchingCanvas && drawAction.action.ReadValue<float>() > 0.1f)
            DrawStroke();
        else
            currentLine = null;
    }

    void DrawStroke()
    {
        if (currentLine == null)
        {
            GameObject go = new GameObject("Stroke");
            currentLine = go.AddComponent<LineRenderer>();
            currentLine.material = drawingMaterial;
            currentLine.startWidth = currentLine.endWidth = penWidth;
            currentLine.startColor = currentLine.endColor = currentColor;
            currentLine.positionCount = 1;
            currentLine.useWorldSpace = false;

            go.transform.SetParent(currentCanvas, false);
            Vector3 localPos = currentCanvas.InverseTransformPoint(tip.position);
            currentLine.SetPosition(0, localPos);
            pointCount = 0;
        }
        else
        {
            Vector3 localPos = currentCanvas.InverseTransformPoint(tip.position);
            if (Vector3.Distance(currentLine.GetPosition(pointCount), localPos) > penWidth)
            {
                pointCount++;
                currentLine.positionCount = pointCount + 1;
                currentLine.SetPosition(pointCount, localPos);
            }
        }
    }

    void SetDrawingColor(Color col)
    {
        currentColor = col;
        if (tipMaterial != null)
            tipMaterial.color = col;
    }
}
