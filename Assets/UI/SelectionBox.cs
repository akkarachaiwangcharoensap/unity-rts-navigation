using System.Collections;
using System.Collections.Generic;
using Navigation;
using Unit;
using UnityEngine;

public class SelectionBox : MonoBehaviour
{
    [Header("Selection Box")]
    public RectTransform selectionRect;
    public Canvas canvas;

    [Header("Selection Pointer")]
    public RectTransform pointerRect;

    // Starting positions.
    private Vector2 startScreenPos;
    private Vector2 startLocalPos;

    // Track selected objects and store original colors.
    private HashSet<GameObject> currentSelectedObjects = new HashSet<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    void Start()
    {
        if (selectionRect != null)
            selectionRect.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdatePointerPosition();

        // Left mouse down: record starting position and show selection rectangle.
        if (Input.GetMouseButtonDown(0))
        {
            startScreenPos = Input.mousePosition;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, startScreenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out startLocalPos);

            if (selectionRect != null)
            {
                selectionRect.gameObject.SetActive(true);
                selectionRect.pivot = new Vector2(0, 1);
                selectionRect.anchoredPosition = startLocalPos;
                selectionRect.sizeDelta = Vector2.zero;
            }
        }

        // While dragging the left mouse button.
        if (Input.GetMouseButton(0))
        {
            Vector2 currentScreenPos = Input.mousePosition;
            Vector2 currentLocalPos;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, currentScreenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out currentLocalPos);

            float left   = Mathf.Min(startLocalPos.x, currentLocalPos.x);
            float right  = Mathf.Max(startLocalPos.x, currentLocalPos.x);
            float top    = Mathf.Max(startLocalPos.y, currentLocalPos.y);
            float bottom = Mathf.Min(startLocalPos.y, currentLocalPos.y);

            float width  = right - left;
            float height = top - bottom;

            if (selectionRect != null)
            {
                selectionRect.anchoredPosition = new Vector2(left, top);
                selectionRect.sizeDelta = new Vector2(width, height);
            }
        }

        // Left mouse up: hide selection rectangle and select objects.
        if (Input.GetMouseButtonUp(0))
        {
            if (selectionRect != null)
                selectionRect.gameObject.SetActive(false);

            SelectVisibleObjects();
        }

        // Right mouse button down: instruct selected units to move.
        if (Input.GetMouseButtonDown(1))
        {
            Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                ? Camera.main : canvas.worldCamera;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Use the clicked destination directly as the formation center.
                Vector3 formationCenter = hit.point;
                // Adjust formationSpacing until each unit gets a distinct destination.
                float formationSpacing = 10.0f;
                Vector3[] formationPositions = this.CalculateFormationPositions(formationCenter, currentSelectedObjects.Count, formationSpacing);
                int index = 0;
                foreach (GameObject obj in currentSelectedObjects)
                {
                    Unit.Unit unit = obj.GetComponent<Unit.Unit>();
                    if (unit != null)
                    {
                        // Option 1: Snap to grid if needed:
                        // Node formationTargetNode = navSystem.GetClosestNode(formationPositions[index]);
                        // unit.SetDestination(formationTargetNode.WorldPosition);
                        
                        // Option 2: Directly assign the computed formation position.
                        unit.SetDestination(formationPositions[index]);
                        index++;
                    }
                }
            }
        }
    }

    private void UpdatePointerPosition()
    {
        if (pointerRect == null)
            return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 pointerLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out pointerLocalPos);

        pointerRect.anchoredPosition = pointerLocalPos;
    }

    /// <summary>
    /// Selects objects whose screen positions fall within the selection rectangle.
    /// Changes their color to light green and reverts color for deselected objects.
    /// </summary>
    private void SelectVisibleObjects()
    {
        Vector2 endScreenPos = Input.mousePosition;
        Vector2 minScreen = Vector2.Min(startScreenPos, endScreenPos);
        Vector2 maxScreen = Vector2.Max(startScreenPos, endScreenPos);

        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? Camera.main : canvas.worldCamera;

        HashSet<GameObject> newSelectedObjects = new HashSet<GameObject>();

        foreach (GameObject obj in VisibilityTracker.VisibleObjects)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(obj.transform.position);
            if (screenPos.z > 0 &&
                screenPos.x >= minScreen.x && screenPos.x <= maxScreen.x &&
                screenPos.y >= minScreen.y && screenPos.y <= maxScreen.y)
            {
                newSelectedObjects.Add(obj);
            }
        }

        // Revert color for objects no longer selected.
        foreach (GameObject obj in currentSelectedObjects)
        {
            if (!newSelectedObjects.Contains(obj))
            {
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null && originalColors.ContainsKey(obj))
                {
                    r.material.color = originalColors[obj];
                    originalColors.Remove(obj);
                }
            }
        }

        // Set new color for newly selected objects.
        foreach (GameObject obj in newSelectedObjects)
        {
            if (!currentSelectedObjects.Contains(obj))
            {
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null)
                {
                    if (!originalColors.ContainsKey(obj))
                        originalColors[obj] = r.material.color;
                    r.material.color = new Color(0.5f, 1f, 0.5f, 1f);
                }
            }
        }

        currentSelectedObjects = newSelectedObjects;
    }

    /// <summary>
    /// Calculates a set of positions in a square formation centered on 'center'
    /// with spacing between positions to avoid overlapping.
    /// </summary>
    private Vector3[] CalculateFormationPositions(Vector3 center, int count, float spacing)
    {
        Vector3[] positions = new Vector3[count];
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        // Calculate an offset so that the formation is centered.
        float offsetStart = -((gridSize - 1) * spacing) / 2.0f;
        int index = 0;
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (index < count)
                {
                    float offsetX = offsetStart + col * spacing;
                    float offsetZ = offsetStart + row * spacing;
                    positions[index] = center + new Vector3(offsetX, 0, offsetZ);
                    index++;
                }
            }
        }
        return positions;
    }
}