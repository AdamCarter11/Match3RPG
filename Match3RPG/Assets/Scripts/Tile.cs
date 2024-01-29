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

        if(type == 0)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else if(type == 1)
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
        else if(type == 2)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if(type == 3)
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }

    public void Swap(Tile targetTile)
    {
        gridManager.StartCoroutine(AnimateSwap(transform.position, targetTile.transform.position, 0.2f, targetTile));
    }

    IEnumerator AnimateSwap(Vector3 startPos, Vector3 endPos, float duration, Tile otherTile)
    {
        if(otherTile != null)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                if(this != null)
                    transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
                if(otherTile != null)
                    otherTile.transform.position = Vector3.Lerp(endPos, startPos, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            if(this != null)
                transform.position = endPos;
            if(otherTile != null)
                otherTile.transform.position = startPos;
        }
    }
    public IEnumerator AnimateTileFall(Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;

        while (elapsedTime < duration)
        {
            if(this != null)
                transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if(this != null)
            transform.position = targetPosition;
    }
}
