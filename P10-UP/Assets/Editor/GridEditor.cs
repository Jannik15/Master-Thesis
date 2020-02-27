using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GridEditor : EditorWindow
{
    // TODO:    - Walkable bool (how to represent?)
    // TODO:    - Add bool for changing materials with selection from the container, or just the entire library
    // TODO:    - Button for creating a new grid while in the grid designer (Go back functionality)
    // TODO:    - Window resizing (empty space on the right side)
    // TODO:    - Loading a grid??? monkaS
    // TODO:    - Cleanup code 

    private Grid grid;
    private TileGeneration selectedTile;
    private int selectedTileNum = 0;
    private TileGeneration[,] gridTiles;
    private List<TileGeneration> gridTilesList = new List<TileGeneration>();
    private List<Rect> tileRectangles = new List<Rect>();

    private Rect optionsPanel, designerPanel;
    private Vector2 windowSize, screenMiddle;
    private Vector2 offset;
    private Vector2 drag;
    private Vector2Int gridDimensions = new Vector2Int(6,6);
    private static Vector2 buttonSize = new Vector2(150, 30);
    private float cellSize = 0.5f;
    private bool _gridDesign, _drawGridOnce, _guiInit, _reset;
    private Texture texture;
    private MaterialContainer baseMaterialContainer, manualMaterialContainer;
    private List<Material> materials;
    private float tileSize;
    private TileGeneration.TileType tileType;
    private static GUILayoutOption[] buttonParams = new GUILayoutOption[2]; 
    private static GUIStyle textCenteringStyle;
    private Event currentEvent;

     [MenuItem("Tools/Grid Editor", false, 10)]
    private static void OpenWindow()
    {
        GridEditor window = GetWindow<GridEditor>();
        window.minSize = new Vector2(250, 240);
        window.titleContent = new GUIContent("Grid Editor");
        buttonParams = new[] {GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)};
    }

    private void OnEnable()
    {
        rootVisualElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
    }

    private void OnInspectorUpdate()
    {
        if (focusedWindow == this && mouseOverWindow == this)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        if (!_guiInit) /// GUI Variables have not been initialized
        {
            textCenteringStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            _guiInit = true;
        }

        /// Variables that can change during use
        screenMiddle = new Vector2(position.width / 2.0f, position.height / 2.0f);
        
        if (!_gridDesign) /// Paint grid generation menu
        {
            DrawGridOptionsPanel();
        }
        else if (grid == null) /// Paint grid editor
        {
            grid = new Grid(gridDimensions.x, gridDimensions.y, cellSize);
            materials = new List<Material>();
            tileSize = cellSize * 100;
            if (baseMaterialContainer != null)
            {
                for (int i = 0; i < baseMaterialContainer.materials.Length; i++)
                {
                    if (baseMaterialContainer.materials[i] != null)
                    {
                        materials.Add(baseMaterialContainer.materials[i]);
                    }
                }
                Material randomMat = materials[Random.Range(0, materials.Count)];
                if (gridTiles == null)
                {
                    gridTiles = grid.GetAllTiles();
                }
                for (int x = 0; x < gridTiles.GetLength(0); x++)
                {
                    for (int y = 0; y < gridTiles.GetLength(1); y++)
                    {
                        gridTilesList.Add(gridTiles[x, y]);

                        gridTiles[x,y].AssignMaterial(randomMat);
                    }
                }
            }
            else
            {
                Debug.Log("No material container assigned!");
            }
        }
        else
        {
            DrawGrid();
        }
    }
    
    private void DrawGridOptionsPanel()
    {
        Vector2 matContainerSize = new Vector2(335, 20);
        baseMaterialContainer = (MaterialContainer)EditorGUI.ObjectField(new Rect(screenMiddle-matContainerSize/2 - new Vector2(0,70), matContainerSize), "Add a Material container:", baseMaterialContainer, typeof(MaterialContainer));
        
        /// Grid input
        gridDimensions = EditorGUI.Vector2IntField(new Rect(screenMiddle - new Vector2(110, 100) / 2, new Vector2(110, 15)), "Grid dimensions:", 
            new Vector2Int(Mathf.Clamp(gridDimensions.x,0,10),Mathf.Clamp(gridDimensions.y,0,10))); // Clamped to max 10

        /// Tile size input
        Vector2 tileSize = new Vector2(110, 30);
        GUILayout.BeginArea(new Rect(screenMiddle - tileSize / 2 + new Vector2(0, tileSize.y / 2), new Vector2(tileSize.x, 160)));
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile size (m):",GUILayout.Width(78));
        cellSize = EditorGUILayout.FloatField(cellSize,GUILayout.Width(30));
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        /// Button
        GUILayout.BeginArea(new Rect(screenMiddle - buttonSize / 2 + new Vector2(0, buttonSize.y / 2), new Vector2(buttonSize.x, 160)));
        GUILayout.Space(30);
        if (baseMaterialContainer != null)
        {
            _gridDesign = GUILayout.Button("Generate Grid", buttonParams);
        }
        else
        {
            GUILayout.Button("Generate Grid", buttonParams);
        }
        GUILayout.EndArea();
    }

    private void DrawGridDesigner(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawGrid()
    {
        /// Detect if a tile has been clicked 
        
        
        currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 || currentEvent.button == 1) // TODO: Make right click work as dropdown
        {
            for (int i = 0; i < tileRectangles.Count; i++)
            {
                if (tileRectangles[i].Contains(currentEvent.mousePosition))
                {
                    selectedTileNum = i;
                    selectedTile = gridTilesList[i];
                    currentEvent.Use();
                }
            }
        }


        Vector2 matContainerSize = new Vector2(335, 20);
        manualMaterialContainer = (MaterialContainer)EditorGUI.ObjectField(new Rect(screenMiddle - matContainerSize / 2 - new Vector2(0, 200), matContainerSize), "Material container:", manualMaterialContainer, typeof(MaterialContainer));

        /// Area for Buttons for saving or instantiating the Grid, and selected tile options
        Vector2 areaSize = new Vector2(200, 300);
        GUILayout.BeginArea(new Rect(screenMiddle - areaSize / 2 - new Vector2(300, 0), areaSize));
        if (selectedTile != null)
        {
            GUILayout.Label("Tile " + selectedTileNum + ": [" + selectedTile.GetCoordinates().x + "," + selectedTile.GetCoordinates().y + "]");
            selectedTile.SetTileType((TileGeneration.TileType)EditorGUILayout.EnumPopup("TileType: ", selectedTile.GetTileType()));
            selectedTile.AssignMaterial((Material)EditorGUILayout.ObjectField("Material:", selectedTile.GetMaterial(), typeof(Material)));

        }
        if (GUILayout.Button("Instantiate Grid", buttonParams))
        {
            grid.InstantiateGrid();
        }
        if (GUILayout.Button("Save Grid as Prefab", buttonParams))
        {
            string savePrefabPath = EditorUtility.SaveFilePanelInProject("Save Grid as Prefab", "UnnamedGrid.prefab", "prefab", "Please select file name to save the grid to:");
            if (!string.IsNullOrEmpty(savePrefabPath))
            {
                string[] fullPathString = savePrefabPath.Split(new char[] { '/' });
                string gridName = "Undefined";
                for (int i = 0; i < fullPathString.Length; i++)
                {
                    if (fullPathString[i].Contains("."))
                    {
                        gridName = fullPathString[i].Substring(0, fullPathString[i].IndexOf('.'));
                    }
                }
                GameObject gridToSaveAsPrefab = grid.GetGrid(gridName);
                PrefabUtility.SaveAsPrefabAsset(gridToSaveAsPrefab, savePrefabPath);
                GameObject.DestroyImmediate(gridToSaveAsPrefab);
            }
        }

        if (GUILayout.Button("Return to Grid Options", buttonParams))
        {
            _reset = true;
        }
        GUILayout.EndArea();

        /// Draw grid tiles with their respective materials and types
        tileRectangles.Clear();
        for (int x = 0; x < gridTiles.GetLength(0); x++)
        {
            for (int y = 0; y < gridTiles.GetLength(1); y++)
            {
                Rect rect = new Rect(screenMiddle.x - (gridTiles.GetLength(0) / 2.0f * tileSize) + x * tileSize, screenMiddle.y - (gridTiles.GetLength(1) / 2.0f * tileSize) + y * tileSize, tileSize, tileSize);
                EditorGUI.DrawPreviewTexture(rect, gridTiles[x, y].GetMaterialTexture());
                EditorGUI.LabelField(rect, gridTiles[x,y].GetTileType().ToString(), textCenteringStyle);

                tileRectangles.Add(rect);
            }
        }

        /// Return to option menu
        if (_reset)
        {
            ReturnToGridOptions();
        }
    }

    private void HandleMouseClick()
    {

    }
    private void HandleLeftClick()
    {

    }

    private void OnMouseDown(MouseDownEvent mouseEvent)
    {
        if (mouseEvent.button == 0) // Left click to select the tile
        {

        }
        else if (mouseEvent.button == 1) // Right click to create dropdown menu of tile options for that tile
        {

        }
    }

    private void HandleRightClick(MouseUpEvent evt)
    {
        if (evt.button != (int)MouseButton.RightMouse)
            return;

        var targetElement = evt.target as VisualElement;
        if (targetElement == null)
            return;

        var menu = new GenericMenu();

        int menuItemValue = 5;

        // Add a single menu item
        bool isSelected = true;
        menu.AddItem(new GUIContent("some menu item name"), isSelected,
            value => ChangeValueFromMenu(value),
            menuItemValue);

        // Get position of menu on top of target element.
        var menuPosition = new Vector2(targetElement.layout.xMin, targetElement.layout.height);
        var menuRect = new Rect(menuPosition, Vector2.zero);

        menu.DropDown(menuRect);
    }

    private void ChangeValueFromMenu(object menuItem)
    {
        //doSomethingWithValue(menuItem as int);
    }

    private void ReturnToGridOptions() // BUG: Event error reappears using this method
    {
        tileRectangles.Clear();
        materials.Clear();
        grid = null;
        gridTiles = null;
        gridTilesList.Clear();
        _gridDesign = false;
        _reset = false;
    }
}
