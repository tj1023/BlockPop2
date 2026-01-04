using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Block firstBlock;
    private bool canInput = true;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        if (!canInput) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            firstBlock = GetBlockAtMouse();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Block secondBlock = GetBlockAtMouse();
            
            if (firstBlock && secondBlock && firstBlock != secondBlock && IsAdj(firstBlock, secondBlock))
            {
                GameEvents.RaiseBlockSwapRequested(firstBlock, secondBlock);
            }
            firstBlock = null;
        }
    }
    
    private Block GetBlockAtMouse()
    {
        Vector2 worldPoint = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        return hit.collider ? hit.collider.GetComponent<Block>() : null;
    }

    private bool IsAdj(Block a, Block b)
    {
        return Mathf.Abs(a.y - b.y) + Mathf.Abs(a.x - b.x) == 1;
    }

    private void HandleGameStateChanged(GameState state)
    {
        canInput = state == GameState.Playing;
    }
}
