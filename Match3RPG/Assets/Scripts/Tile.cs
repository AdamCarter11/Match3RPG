using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int type;

    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    public void Init(int _x, int _y, int _type)
    {
        x = _x;
        y = _y;
        type = _type;

        SetTileColor();
    }

    private void SetTileColor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        switch (type)
        {
            case 0:
                spriteRenderer.color = Color.green;
                break;
            case 1:
                spriteRenderer.color = Color.blue;
                break;
            case 2:
                spriteRenderer.color = Color.red;
                break;
            case 3:
                spriteRenderer.color = Color.yellow;
                break;
            default:
                break;
        }
    }

    public void Swap(Tile targetTile)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = targetTile.transform.position;

        // Swap positions instantly
        transform.position = endPos;
        targetTile.transform.position = startPos;
    }

    public void AnimateTileFall(Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;

        while (elapsedTime < duration)
        {
            if (this != null)
                transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
        }

        if (this != null)
            transform.position = targetPosition;
    }
}
