using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int offsetX, offsetY;
    [SerializeField] GameObject tilePrefab;

    private Tile[,] grid;
    private Tile selectedTile;

    void Start()
    {
        InitializeGrid();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile clickedTile = hit.collider.GetComponent<Tile>();

                if (clickedTile != null)
                {
                    // If no tile is selected, select the clicked tile
                    if (selectedTile == null)
                    {
                        selectedTile = clickedTile;
                    }
                    else
                    {
                        // If a tile is already selected, swap positions
                        if (AreTilesAdjacent(selectedTile, clickedTile))
                        {
                            SwapTiles(selectedTile, clickedTile);
                            selectedTile = null;
                        }
                        else
                        {
                            // Deselect if clicking on a non-adjacent tile
                            selectedTile = clickedTile;
                        }
                    }
                }
            }
        }
    }

    void CheckMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile currentTile = grid[x, y];
                if (currentTile != null)
                {
                    // Check horizontally
                    int horizontalMatches = GetMatches(currentTile, 1, 0);

                    // Check vertically
                    int verticalMatches = GetMatches(currentTile, 0, 1);

                    // Check if there are enough matches
                    if (horizontalMatches >= 2 || verticalMatches >= 2)
                    {
                        // Perform match logic (destroy tiles, score points, etc.)
                        HandleMatches(currentTile, horizontalMatches, verticalMatches);
                    }
                }
            }
        }
    }

    int GetMatches(Tile startTile, int dx, int dy)
    {
        int matches = 0;
        int targetType = startTile.type;

        // Check in positive direction
        int currentX = startTile.x + dx;
        int currentY = startTile.y + dy;

        while (currentX >= 0 && currentX < width && currentY >= 0 && currentY < height)
        {
            Tile currentTile = grid[currentX, currentY];

            if (currentTile == null || currentTile.type != targetType)
            {
                break;
            }

            matches++;
            currentX += dx;
            currentY += dy;
        }

        // Check in negative direction
        currentX = startTile.x - dx;
        currentY = startTile.y - dy;

        while (currentX > 0 && currentX < width && currentY > 0 && currentY < height)
        {
            Tile currentTile = grid[currentX, currentY];

            if (currentTile == null || currentTile.type != targetType)
            {
                break;
            }

            matches++;
            currentX -= dx;
            currentY -= dy;
        }

        return matches;
    }

    void HandleMatches(Tile startTile, int horizontalMatches, int verticalMatches)
    {
        print("new match");
        // Determine the total number of matches
        int totalMatches = horizontalMatches + verticalMatches + 1; // +1 for the startTile itself

        // Destroy the matched tiles
        DestroyMatches(startTile, horizontalMatches, verticalMatches);

        // Update the score or perform any other relevant actions
        UpdateScore(totalMatches);

        // Play a sound effect or trigger other game events
        PlayMatchSoundEffect();

        // Check for new matches
        CheckMatches();
    }
    void DestroyMatches(Tile startTile, int horizontalMatches, int verticalMatches)
    {
        // Destroy the horizontally matched tiles
        for (int i = 0; i < horizontalMatches; i++)
        {
            int tileX = startTile.x + i;
            int tileY = startTile.y;

            // Check if the indices are within the valid range
            if (tileX >= 0 && tileX < width && tileY >= 0 && tileY < height)
            {
                Tile matchedTile = grid[tileX, tileY];
                if (matchedTile != null && matchedTile.gameObject != null)
                {
                    Destroy(matchedTile.gameObject);
                    grid[tileX, tileY] = null;
                }
            }
        }

        // Destroy the vertically matched tiles
        for (int i = 0; i < verticalMatches; i++)
        {
            int tileX = startTile.x;
            int tileY = startTile.y + i;

            // Check if the indices are within the valid range
            if (tileX >= 0 && tileX < width && tileY >= 0 && tileY < height)
            {
                Tile matchedTile = grid[tileX, tileY];
                if (matchedTile != null && matchedTile.gameObject != null)
                {
                    Destroy(matchedTile.gameObject);
                    grid[tileX, tileY] = null;
                }
            }
        }

        // Destroy the starting tile itself
        Destroy(startTile.gameObject);
        grid[startTile.x, startTile.y] = null;

        // After destroying matches, spawn new tiles at the top
        FallTilesDownAndCheckMatches();
    }

    void FallTilesDownAndCheckMatches()
    {
        // Fall tiles down
        FallTilesDown();

        // Check for new matches
        CheckMatches();
    }

    void FallTilesDown()
    {
        // Loop through each column
        for (int x = 0; x < width; x++)
        {
            int newY = 0; // Variable to keep track of the new Y position

            // Shift non-null tiles down to fill empty spaces
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    Tile tile = grid[x, y];

                    // Move the tile down visually
                    tile.transform.position = new Vector3(x, newY, 0);

                    // Update the grid
                    grid[x, y] = null;
                    grid[x, newY] = tile;
                    tile.y = newY;

                    newY++; // Increment the new Y position
                }
            }

            // If there's an empty space after the fall, spawn a new tile at the top
            while (newY < height)
            {
                CreateTile(x, newY, Random.Range(0, 3)); // Adjust the range based on the number of tile types/colors
                newY++;
            }
        }
    }

    void SpawnNewTilesAtTop()
    {
        // Loop through each column
        for (int x = 0; x < width; x++)
        {
            // Check for empty spaces in the column
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    // Spawn a new tile at the top of the visible grid
                    CreateTile(x, y, Random.Range(0, 3)); // Adjust the range based on the number of tile types/colors
                }
            }
        }
    }

    int CountEmptySpacesAbove(int x, int y)
    {
        int count = 0;

        for (int i = y - 1; i >= 0; i--)
        {
            if (grid[x, i] == null)
            {
                count++;
            }
        }

        return count;
    }

    void UpdateScore(int totalMatches)
    {
        // score logic
    }

    void PlayMatchSoundEffect()
    {
        // sound effects
    }

    bool AreTilesAdjacent(Tile tile1, Tile tile2)
    {
        // Check if two tiles are adjacent
        int dx = Mathf.Abs(tile1.x - tile2.x);
        int dy = Mathf.Abs(tile1.y - tile2.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    void SwapTiles(Tile tile1, Tile tile2)
    {
        // Swap the positions of two tiles
        int tempX = tile1.x;
        int tempY = tile1.y;

        tile1.x = tile2.x;
        tile1.y = tile2.y;

        tile2.x = tempX;
        tile2.y = tempY;

        // Call the Swap method on the Tile script for visual animation
        tile1.Swap(tile2);
        CheckMatches();
    }

    void InitializeGrid()
    {
        grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y, Random.Range(0, 3)); // Change 3 to the number of different tile types/colors
            }
        }
    }

    void CreateTile(int x, int y, int type)
    {
        GameObject tileGO = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
        Tile tile = tileGO.GetComponent<Tile>();
        tile.Init(x, y, type);
        grid[x, y] = tile;
    }
}
