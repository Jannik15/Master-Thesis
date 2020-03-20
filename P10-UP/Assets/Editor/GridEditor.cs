using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.UIElements;
using Random = UnityEngine.Random;

public class GridEditor : EditorWindow
{
    // TODO:    - Add bool for changing materials with selection from the container, or just the entire library (nice to have)
    // TODO:    - Window resizing (empty space on the right side)
    // TODO:    - Loading a grid??? monkaS

    // Class references
    private GridGeneration grid;
    private static TileGeneration selectedTile;
    private TileGeneration[,] gridTiles;
    private List<TileGeneration> gridTilesList = new List<TileGeneration>();
    private WallGeneration[,] gridWalls;
    private List<WallGeneration> wallGenerationList = new List<WallGeneration>();
    private TileGeneration.TileType tileType;
    private TileGeneration.TileType leftClickDragTileType;

    // Basic types
    private int selectedTileNum = 0;
    private int leftClickDragSelection, leftClickDragTileTypeIndex, leftClickDragIsWalkableSelection;
    private readonly string[] leftClickDragLabels = { "Do nothing", "Tile Type", "Walls", "Ceiling", "Is Walkable", "Material" };
    private readonly string[] tileTypeLabels = Enum.GetNames(typeof(TileGeneration.TileType));
    private readonly string[] boolValuesAsLabels = { "True", "False" };
    private bool _gridDesign, _drawGridOnce, _guiInit, _reset, _leftClickDragIsWalkable;
    private float cellSize = 0.5f, tileSize;
    private static Vector2 buttonSize = new Vector2(150, 30);
    private Vector2 windowSize, screenMiddle, tileSizeVector;
    private Vector2Int gridDimensions = new Vector2Int(6, 6);
    private Event currentEvent;

    // Materials
    private MaterialContainer baseMaterialContainer, manualMaterialContainer;
    private List<Material> materials;
    private Material leftClickDragMaterial;
    private Texture texture;

    // GUI variables
    private List<Rect> tileAreas = new List<Rect>(), wallAreas = new List<Rect>();
    private Rect optionsPanel, designerPanel;
    private static GUILayoutOption[] buttonParams = new GUILayoutOption[2];
    private static GUIStyle textCenteringStyle;

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
            grid = new GridGeneration(gridDimensions.x, gridDimensions.y, cellSize, 2f, 0.01f);
            materials = new List<Material>();
            tileSize = cellSize * 100;
            tileSizeVector = new Vector2(tileSize, tileSize);
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
                    gridWalls = grid.GetAllWalls();
                }
                for (int y = 0; y < gridTiles.GetLength(1); y++)
                {
                    for (int x = 0; x < gridTiles.GetLength(0); x++) 
                    {
                        gridTilesList.Add(gridTiles[x, y]);
                        wallGenerationList.Add(gridWalls[x, y]);
                        wallGenerationList.Add(gridWalls[x, y + 1]);
                        if (x == gridTiles.GetLength(0) - 1)
                        {
                            wallGenerationList.Add(gridWalls[x + 1, y + 1]);
                        }

                        if (y == gridTiles.GetLength(1) - 1)
                        {
                            wallGenerationList.Add(gridWalls[x + 1, y + 2]);
                        }

                        gridTiles[x, y].AssignMaterial(randomMat);
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

    private void DrawGrid()
    {
        // Detect if a tile has been clicked
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
                break;
            case 3:
                break;
            case 4:
                leftClickDragIsWalkableSelection = GUILayout.SelectionGrid(leftClickDragIsWalkableSelection, boolValuesAsLabels, boolValuesAsLabels.Length, EditorStyles.radioButton);
                _leftClickDragIsWalkable = leftClickDragIsWalkableSelection == 0;
                break;
            case 5:
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
            string savePrefabPath = EditorUtility.SaveFilePanelInProject("Save Grid as Prefab", "UnnamedGrid.prefab", "prefab", "Please select file name to save the grid to:","Assets/Prefabs/Grids/");
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
        tileAreas.Clear();
        for (int y = 0; y < gridTiles.GetLength(1); y++)
        {
            for (int x = 0; x < gridTiles.GetLength(0); x++)
            {
                Vector2 rectCenter = new Vector2(screenMiddle.x - gridTiles.GetLength(0) / 2.0f * tileSize + x * tileSize + x * 5, screenMiddle.y - gridTiles.GetLength(1) / 2.0f * tileSize + y * tileSize + y * 5);
                Rect tileRect = new Rect(rectCenter, tileSizeVector);

                EditorGUI.DrawPreviewTexture(tileRect, gridTiles[x, y].GetMaterialTexture());
                EditorGUI.LabelField(tileRect, gridTiles[x,y].GetTileType().ToString(), textCenteringStyle);
                if (!gridTiles[x, y].IsWalkable())
                {
                    EditorGUI.DrawRect(new Rect(rectCenter + new Vector2(tileSize * 0.4f, tileSize * 0.2f), tileSizeVector * 0.2f), Color.red);
                }
                tileAreas.Add(tileRect);

                Rect wallRect = new Rect(rectCenter + new Vector2(1, -5), tileSizeVector - new Vector2(1, tileSize * 0.9f));
                EditorGUI.DrawRect(wallRect, Color.red);
                wallAreas.Add(wallRect);

                wallRect = new Rect(rectCenter + new Vector2(-5, 1), tileSizeVector - new Vector2(tileSize * 0.9f, 1));
                EditorGUI.DrawRect(wallRect, Color.blue);
                wallAreas.Add(wallRect);

                if (x == gridTiles.GetLength(0) - 1)
                {
                    wallRect = new Rect(rectCenter + new Vector2(tileSize, 1), tileSizeVector - new Vector2(tileSize * 0.9f, 1));
                    EditorGUI.DrawRect(wallRect, Color.blue);
                    wallAreas.Add(wallRect);
                }

                if (y == gridTiles.GetLength(1) - 1)
                {
                    wallRect = new Rect(rectCenter + new Vector2(1, tileSize), tileSizeVector - new Vector2(1, tileSize * 0.9f));
                    EditorGUI.DrawRect(wallRect, Color.red);
                    wallAreas.Add(wallRect);
                }
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
        if (leftClickDragSelection != 2)
        {
            for (int i = 0; i < tileAreas.Count; i++)
            {
                if (tileAreas[i].Contains(mouseDragEvent.mousePosition))
                {
                    switch (leftClickDragSelection)
                    {
                        case 0:
                            break;
                        case 1:
                            gridTilesList[i].SetTileType(leftClickDragTileType);
                            break;
                        case 3:
                            break;
                        case 4:
                            gridTilesList[i].SetWalkable(_leftClickDragIsWalkable);
                            break;
                        case 5:
                            gridTilesList[i].AssignMaterial(leftClickDragMaterial);
                            break;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < wallAreas.Count; i++)
            {
                if (wallAreas[i].Contains(mouseDragEvent.mousePosition))
                {
                    Debug.Log("Hovering wall area " + i);
                    //wallGenerationList[i].SetHeight();
                }
            }
        }
    }

    private void OnMouseClick(MouseUpEvent mouseEvent)
    {
        if (mouseEvent.button == 0) // Left click to select the tile
        {
            for (int i = 0; i < tileAreas.Count; i++)
            {
                if (tileAreas[i].Contains(mouseEvent.mousePosition))
                {
                    selectedTileNum = i;
                    selectedTile = gridTilesList[i];
                }
            }
        }
        else if (mouseEvent.button == 1) // Right click to create dropdown menu of tile options for that tile
        {
            for (int i = 0; i < tileAreas.Count; i++)
            {
                if (tileAreas[i].Contains(mouseEvent.mousePosition))
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
        tileAreas.Clear();
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
