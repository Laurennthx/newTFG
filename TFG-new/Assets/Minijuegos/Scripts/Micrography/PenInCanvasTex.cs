using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PenInCanvasTex : MonoBehaviour
{
    [Header("Pen Settings")]
    public Transform tip;
    public Material drawingMaterial;
    public Material tipMaterial;
    [Range(0.001f, 0.05f)] public float penWidth = 0.005f;

    [Header("Input Actions")]
    public InputActionReference drawAction;

    [Header("Texture Painting")]
    public Canvas3DController canvasController;
    public int brushRadiusPx = 8;

    // Estado interno
    bool isTouchingCanvas = false;
    Transform currentCanvas = null;

    XRGrabInteractable grabInteractable;
    LineRenderer currentLine;
    int pointCount = 0;
    Color currentColor;

    [Header("Ajustes de calibración")]
    [Tooltip("Desplaza el origen del rayo relativo a tip.position (en metros)")]
    public Vector3 rayOriginOffset = Vector3.zero;
    [Tooltip("Desplaza la dirección del rayo (no olvides normalizar)")]
    public Vector3 rayDirectionOffset = Vector3.zero;


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
        Debug.Log($"[Pen] OnTriggerEnter con: {other.name}, tag: {other.tag}");
        if (other.CompareTag("Canvas3D"))
        {
            Debug.Log("[Pen] ¡Entré al Canvas3D!");
            isTouchingCanvas = true;
            currentCanvas = other.transform;
        }
        var cc = other.GetComponent<ColorCapsule>();
        if (cc != null)
            SetDrawingColor(cc.capsuleColor);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Pen] OnTriggerExit con: {other.name}");
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
        if (isTouchingCanvas && drawAction.action.ReadValue<float>() > 0.1f)
            DrawStroke();
        else
            currentLine = null;
    }

    void DrawStroke()
    {
        /*
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
        */
        PaintOnTextureAtTip();
    }

    void PaintOnTextureAtTip()
    {
        // 1) Referencia al controlador del canvas
        var cc = canvasController;
        if (cc == null)
        {
            Debug.LogError("[Pen] canvasController es NULL");
            return;
        }
        if (cc.canvasCollider == null)
        {
            Debug.LogError("[Pen] cc.canvasCollider es NULL");
            return;
        }

        // 2) Origen calibrable: tip.position + offset
        Vector3 origin = tip.position + rayOriginOffset;

        // 3) Dirección base (-tip.forward) + offset, luego normalizar
        Vector3 dir = (-tip.forward + rayDirectionOffset).normalized;

        // Debug: dibuja el rayo en Scene View
        Debug.DrawRay(origin, dir * 0.5f, Color.red, 0.1f);
        Debug.Log($"[Pen] Lanzando raycast desde {origin:F2} en dirección {dir:F2}");

        // 4) Raycast genérico
        if (Physics.Raycast(origin, dir, out RaycastHit hit, 0.5f))
        {
            // 5) Filtra solo el collider de tu DrawingPlane
            if (hit.collider == cc.canvasCollider)
            {
                Vector2 uv = hit.textureCoord;
                int x = Mathf.Clamp(Mathf.RoundToInt(uv.x * cc.drawingTexture.width), 0, cc.drawingTexture.width - 1);
                int y = Mathf.Clamp(Mathf.RoundToInt(uv.y * cc.drawingTexture.height), 0, cc.drawingTexture.height - 1);
                Debug.Log($"[Pen] Raycast HIT en UV {uv} → píxel ({x},{y})");

                // 6) Pinta un círculo de radio brushRadiusPx
                for (int dy = -brushRadiusPx; dy <= brushRadiusPx; dy++)
                    for (int dx = -brushRadiusPx; dx <= brushRadiusPx; dx++)
                        if (dx * dx + dy * dy <= brushRadiusPx * brushRadiusPx)
                        {
                            int px = x + dx, py = y + dy;
                            if (px >= 0 && px < cc.drawingTexture.width &&
                                py >= 0 && py < cc.drawingTexture.height)
                            {
                                cc.drawingTexture.SetPixel(px, py, currentColor);
                            }
                        }

                // 7) Aplica cambios a la textura
                cc.drawingTexture.Apply();
                Debug.Log($"[Pen] setPixel(s) alrededor de ({x},{y})");
            }
            else
            {
                Debug.Log("[Pen] Raycast golpeó otro collider: " + hit.collider.name);
            }
        }
        else
        {
            Debug.Log("[Pen] Raycast NO hit");
        }
    }










    void SetDrawingColor(Color col)
    {
        currentColor = col;
        if (tipMaterial != null)
            tipMaterial.color = col;
    }
}
