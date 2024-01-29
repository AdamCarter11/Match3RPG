using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
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

        // Implement your logic for handling matches here
        // For example, you might destroy matched tiles, update the score, etc.

        // Destroy the matched tiles
        DestroyMatches(startTile, horizontalMatches, verticalMatches);

        // Update the score or perform any other relevant actions
        UpdateScore(totalMatches);

        // Play a sound effect or trigger other game events
        PlayMatchSoundEffect();

        CheckMatches();
    }

    void DestroyMatches(Tile startTile, int horizontalMatches, int verticalMatches)
    {
        // Destroy the horizontally matched tiles
        for (int i = 0; i < horizontalMatches; i++)
        {
            Tile matchedTile = grid[startTile.x + i, startTile.y];
            if (matchedTile != null && matchedTile.gameObject != null)
            {
                Destroy(matchedTile.gameObject);
                grid[startTile.x + i, startTile.y] = null;
            }
        }

        // Destroy the vertically matched tiles
        for (int i = 0; i < verticalMatches; i++)
        {
            Tile matchedTile = grid[startTile.x, startTile.y + i];
            if (matchedTile != null && matchedTile.gameObject != null)
            {
                Destroy(matchedTile.gameObject);
                grid[startTile.x, startTile.y + i] = null;
            }
        }

        // Destroy the starting tile itself
        Destroy(startTile.gameObject);
        grid[startTile.x, startTile.y] = null;

        // After destroying matches, spawn new tiles at the top
        StartCoroutine(FallTilesDownAndCheckMatches());
    }

    IEnumerator FallTilesDownAndCheckMatches()
    {
        // Wait for a short delay before starting the falling effect
        yield return new WaitForSeconds(0.2f);

        // Fall tiles down
        StartCoroutine(FallTilesDown());

        // Wait for a short delay before checking matches again
        yield return new WaitForSeconds(0.5f);

        // Check for new matches
        CheckMatches();
    }
    IEnumerator FallTilesDown()
    {
        // Wait for a short delay before starting the falling effect
        yield return new WaitForSeconds(0.2f);

        // Loop through each column
        for (int x = 0; x < width; x++)
        {
            // Shift non-null tiles down to fill empty spaces
            for (int y = 1; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    Tile tile = grid[x, y];
                    int newY = y;

                    // Check for empty spaces below and shift the tile down
                    while (newY > 0 && grid[x, newY - 1] == null)
                    {
                        newY--;
                    }

                    if (newY != y)
                    {
                        // Move the tile down visually
                        StartCoroutine(tile.AnimateTileFall(new Vector3(x, newY, 0), 0.5f));

                        // Update the grid
                        grid[x, y] = null;
                        grid[x, newY] = tile;
                        tile.y = newY;
                    }
                }
            }
        }

        // Spawn new tiles at the top after the falling effect
        SpawnNewTiles();
    }

    void SpawnNewTiles()
    {
        // Loop through each column
        for (int x = 0; x < width; x++)
        {
            // Check for empty spaces in the column
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    // Determine the number of empty spaces above the current position
                    int emptySpacesAbove = CountEmptySpacesAbove(x, y);

                    // Spawn a new tile at the top of the visible grid
                    CreateTile(x, height - 1 - emptySpacesAbove, Random.Range(0, 3)); // Adjust the range based on the number of tile types/colors

                    // Animate the new tile falling into place
                    StartCoroutine(AnimateTileFall(grid[x, height - 1 - emptySpacesAbove], new Vector3(x, y, 0), 0.5f)); // Adjust the duration as needed
                }
            }
        }
    }

    IEnumerator AnimateTileFall(Tile tile, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = tile.transform.position;

        while (elapsedTime < duration)
        {
            if(tile != null)
                tile.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if(tile != null)
            tile.transform.position = targetPosition;
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
        // Implement your score logic here
        // For example, you might increment a score variable based on the number of matches.
        // You could also update a UI element to display the current score.
    }

    void PlayMatchSoundEffect()
    {
        // Implement your sound effect logic here
        // For example, you might play a sound effect when matches are made.
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
