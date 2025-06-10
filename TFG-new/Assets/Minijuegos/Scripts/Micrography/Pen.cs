using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRPen : MonoBehaviour
{
    [Header("Pen Settings")]
    public Transform tip;
    public Material drawingMaterial;
    public Material tipMaterial;
    [Range(0.001f, 0.05f)] public float penWidth = 0.005f;
    public Color[] penColors;  // sigue usándose para cambiar con botón

    [Header("Input Actions")]
    public InputActionReference drawAction;
    public InputActionReference colorAction;

    XRGrabInteractable grabInteractable;
    LineRenderer currentLine;
    int pointCount;
    Color currentColor;
    int colorIndex = 0;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        // Initial color:
        currentColor = penColors[colorIndex];
        tipMaterial.color = currentColor;
        drawAction.action.Enable();
        colorAction.action.Enable();
    }

    void OnEnable()
    {
        drawAction.action.Enable();
        colorAction.action.Enable();
    }

    void OnDisable()
    {
        //drawAction.action.Disable();
        //colorAction.action.Disable();
    }

    void Update()
    {
        if (!grabInteractable.isSelected)
        {
            currentLine = null;
            return;
        }

        // Dibujar con gatillo
        if (drawAction.action.ReadValue<float>() > 0.1f)
            DrawStroke();
        else
            currentLine = null;

        // Cambiar color por botón
        if (colorAction.action.WasPressedThisFrame())
            CycleColor();
    }

    void DrawStroke()
    {
        if (currentLine == null)
        {
            var go = new GameObject("Stroke");
            currentLine = go.AddComponent<LineRenderer>();
            currentLine.material = drawingMaterial;
            currentLine.startWidth = currentLine.endWidth = penWidth;
            currentLine.startColor = currentLine.endColor = currentColor;
            currentLine.positionCount = 1;
            currentLine.SetPosition(0, tip.position);
            pointCount = 0;
        }
        else
        {
            var last = currentLine.GetPosition(pointCount);
            if (Vector3.Distance(last, tip.position) > penWidth)
            {
                pointCount++;
                currentLine.positionCount = pointCount + 1;
                currentLine.SetPosition(pointCount, tip.position);
            }
        }
    }

    void CycleColor()
    {
        colorIndex = (colorIndex + 1) % penColors.Length;
        SetDrawingColor(penColors[colorIndex]);
    }

    /// <summary>
    /// Método que llamamos desde el trigger de la punta
    /// </summary>
    public void SetDrawingColor(Color newColor)
    {
        currentColor = newColor;
        tipMaterial.color = newColor;
    }

    /// <summary>
    /// Capturamos colisiones con las cápsulas de color
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Si choca con algo que tenga ColorCapsule, tomamos su color
        var cc = other.GetComponent<ColorCapsule>();
        if (cc != null)
        {
            SetDrawingColor(cc.capsuleColor);
        }
    }
}
