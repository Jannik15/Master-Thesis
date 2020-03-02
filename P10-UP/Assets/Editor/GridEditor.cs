using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.UIElements;
using Random = UnityEngine.Random;

public class GridEditor : EditorWindow
{
    // TODO:    - Add bool for changing materials with selection from the container, or just the entire library
    // TODO:    - Window resizing (empty space on the right side)
    // TODO:    - Loading a grid??? monkaS
    // TODO:    - Cleanup code 

    private GridGeneration grid;
    private static TileGeneration selectedTile;
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
    private int leftClickDragSelection, leftClickDragTileTypeIndex, leftClickDragIsWalkableSelection;
    private TileGeneration.TileType leftClickDragTileType;
    private bool leftClickDragIsWalkable;
    private Material leftClickDragMaterial;
    private string[] leftClickDragLabels = { "Empty", "Tile Type", "Is Walkable", "Material" };
    private string[] tileTypeLabels = Enum.GetNames(typeof(TileGeneration.TileType));
    private string[] boolValuesAsLabels = { "True", "False"};

    [MenuItem("Tools/Grid Editor", false, 10)]
    private static void OpenWindow()
    {
        GridEditor window = GetWindow<GridEditor>();
        window.minSize = new Vector2(250, 240);
        window.titleContent = new GUIContent("Grid Editor");
        buttonParams = new[] {GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)};
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
        if (!_guiInit) // GUI Variables have not been initialized
        {
            textCenteringStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            _guiInit = true;
        }

        // Variables that can change during use
        screenMiddle = new Vector2(position.width / 2.0f, position.height / 2.0f);
        
        if (!_gridDesign) // Paint grid generation menu
        {
            DrawGridOptionsPanel();
        }
        else if (grid == null) // Paint grid editor
        {
            grid = new GridGeneration(gridDimensions.x, gridDimensions.y, cellSize);
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
        
        // Grid input
        gridDimensions = EditorGUI.Vector2IntField(new Rect(screenMiddle - new Vector2(110, 100) / 2, new Vector2(110, 15)), "Grid dimensions:", 
            new Vector2Int(Mathf.Clamp(gridDimensions.x,0,25),Mathf.Clamp(gridDimensions.y,0,25))); // Clamped to max 10

        // Tile size input
        Vector2 tileSize = new Vector2(110, 30);
        GUILayout.BeginArea(new Rect(screenMiddle - tileSize / 2 + new Vector2(0, tileSize.y / 2), new Vector2(tileSize.x, 160)));
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile size (m):",GUILayout.Width(78));
        cellSize = EditorGUILayout.FloatField(cellSize,GUILayout.Width(30));
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Button
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
        if (Event.current.type == EventType.MouseUp)
        {
            MouseUpEvent mouseEvent = MouseUpEvent.GetPooled(Event.current);
            if (mouseEvent != null)
            {
                OnMouseClick(mouseEvent);
            }
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            MouseMoveEvent mouseDragEvent = MouseMoveEvent.GetPooled(Event.current);
            if (mouseDragEvent != null)
            {
                OnMouseLeftClickDrag(mouseDragEvent);
            }
        }   

        Vector2 matContainerSize = new Vector2(335, 20);
        manualMaterialContainer = (MaterialContainer)EditorGUI.ObjectField(new Rect(screenMiddle - matContainerSize / 2 - new Vector2(0, 200), matContainerSize), "Material container:", manualMaterialContainer, typeof(MaterialContainer));

        Vector2 areaSizeLeft = new Vector2(150, 300);
        GUILayout.BeginArea(new Rect(screenMiddle - areaSizeLeft / 2 - new Vector2(300, 0), areaSizeLeft));
        GUILayout.Label("Left-click drag selection");
        leftClickDragSelection = GUILayout.SelectionGrid(leftClickDragSelection, leftClickDragLabels, 1);

        switch (leftClickDragSelection)
        {
            case 0:
                break;  
            case 1:
                leftClickDragTileTypeIndex = GUILayout.SelectionGrid(leftClickDragTileTypeIndex, tileTypeLabels, 1, EditorStyles.radioButton, new GUILayoutOption[] { GUILayout.Width(75 * tileTypeLabels.Length) });
                leftClickDragTileType = (TileGeneration.TileType)leftClickDragTileTypeIndex;
                break;
            case 2:
                leftClickDragIsWalkableSelection = GUILayout.SelectionGrid(leftClickDragIsWalkableSelection, boolValuesAsLabels, boolValuesAsLabels.Length, EditorStyles.radioButton);
                if (leftClickDragIsWalkableSelection == 0)
                {
                    leftClickDragIsWalkable = true;
                }
                else
                {
                    leftClickDragIsWalkable = false;
                }
                break;
            case 3: // TODO: Also add only select materials from currently selected materialContainer
                EditorGUIUtility.labelWidth = 50;
                leftClickDragMaterial = (Material)EditorGUILayout.ObjectField("Material:", leftClickDragMaterial, typeof(Material), new GUILayoutOption[] { GUILayout.ExpandWidth(true)});
                break;
        }
        GUILayout.EndArea();
        // Area for Buttons for saving or instantiating the Grid, and selected tile options
        Vector2 areaSizeRight = new Vector2(200, 300);
        GUILayout.BeginArea(new Rect(screenMiddle - areaSizeRight / 2 + new Vector2(300, 0), areaSizeRight));
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

        // Draw grid tiles with their respective materials and types
        tileRectangles.Clear();
        for (int x = 0; x < gridTiles.GetLength(0); x++)
        {
            for (int y = 0; y < gridTiles.GetLength(1); y++)
            {
                Rect rect = new Rect(screenMiddle.x - (gridTiles.GetLength(0) / 2.0f * tileSize) + x * tileSize, screenMiddle.y - (gridTiles.GetLength(1) / 2.0f * tileSize) + y * tileSize, tileSize, tileSize);
                EditorGUI.DrawPreviewTexture(rect, gridTiles[x, y].GetMaterialTexture());
                EditorGUI.LabelField(rect, gridTiles[x,y].GetTileType().ToString(), textCenteringStyle);
                if (!gridTiles[x, y].IsWalkable())
                {
                    EditorGUI.DrawRect(new Rect(screenMiddle.x - (gridTiles.GetLength(0) / 2.0f * tileSize) + x * tileSize + tileSize * 0.4f, screenMiddle.y - (gridTiles.GetLength(1) / 2.0f * tileSize) + y * tileSize, tileSize * 0.2f, tileSize * 0.2f), Color.red);
                }
                tileRectangles.Add(rect);
            }
        }

        // Return to options menu
        if (_reset)
        {
            ReturnToGridOptions();
        }
    }

    private void OnMouseLeftClickDrag(MouseMoveEvent mouseDragEvent)
    {
        for (int i = 0; i < tileRectangles.Count; i++)
        {
            if (tileRectangles[i].Contains(mouseDragEvent.mousePosition))
            {
                switch (leftClickDragSelection)
                {
                    case 0:
                        break;
                    case 1:
                        gridTilesList[i].SetTileType(leftClickDragTileType);
                        break;
                    case 2:
                        gridTilesList[i].SetWalkable(leftClickDragIsWalkable);
                        break;
                    case 3:
                        gridTilesList[i].AssignMaterial(leftClickDragMaterial);
                        break;
                }
            }
        }
    }

    private void OnMouseClick(MouseUpEvent mouseEvent)
    {
        if (mouseEvent.button == 0) // Left click to select the tile
        {
            for (int i = 0; i < tileRectangles.Count; i++)
            {
                if (tileRectangles[i].Contains(mouseEvent.mousePosition))
                {
                    selectedTileNum = i;
                    selectedTile = gridTilesList[i];
                }
            }
        }
        else if (mouseEvent.button == 1) // Right click to create dropdown menu of tile options for that tile
        {
            for (int i = 0; i < tileRectangles.Count; i++)
            {
                if (tileRectangles[i].Contains(mouseEvent.mousePosition))
                {
                    selectedTileNum = i;
                    selectedTile = gridTilesList[i];
                }
            }
            CustomDropDown.Show(this, mouseEvent.mousePosition, selectedTile);
        }
    }

    public static void SetSelectedTileType(TileGeneration.TileType newType)
    {
        selectedTile.SetTileType(newType);
    }

    private void ReturnToGridOptions()
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

class CustomDropDown : EditorWindow
{
    static Vector2 s_DefaultSize = new Vector2(100,23 * 5);
    private static TileGeneration.TileType _startingType, _selectedType;
    private static EditorWindow hostWindow;
    public static void Show(EditorWindow host, Vector2 displayPosition, TileGeneration selectedTile)
    {
        _selectedType = selectedTile.GetTileType();
        _startingType = _selectedType;
        hostWindow = host;
        var window = CreateInstance<CustomDropDown>();
        var position = GetPosition(host, displayPosition);
        window.position = new Rect(position + host.position.position, s_DefaultSize);
        window.ShowPopup();
        window.Focus();
    }

    private static Vector2 GetPosition(EditorWindow host, Vector2 displayPosition)
    {
        var x = displayPosition.x;
        var y = displayPosition.y;

        // Searcher overlaps with the right boundary.
        if (x + s_DefaultSize.x >= host.position.size.x)
            x -= s_DefaultSize.x;

        // Searcher overlaps with the bottom boundary.
        if (y + s_DefaultSize.y >= host.position.size.y)
            y -= s_DefaultSize.y;

        return new Vector2(x, y);
    }

    private void OnGUI()
    {
        if (focusedWindow != this)
        {
            this.Close();
        }
        _selectedType = (TileGeneration.TileType)EditorGUILayout.EnumPopup("", _selectedType);

        if (_selectedType != _startingType)
        {
            GridEditor.SetSelectedTileType(_selectedType);
            GetWindow(typeof(GridEditor)).Focus();
            this.Close();
        }
    }
}
