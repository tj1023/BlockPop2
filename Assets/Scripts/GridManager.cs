using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("----------[ BASE ]----------")]
    [SerializeField] private Block block;
    [SerializeField] private ParticleSystem effect;
    [SerializeField] private int height;
    [SerializeField] private int width;
    
    [Header("----------[ DEBUG ]----------")]
    [SerializeField] public bool autoMod;
    
    public int Score { get; private set; }
    
    private Block[,] grid;
    private Block firstBlock;
    private GameState currentState = GameState.Playing;

    private void OnEnable()
    {
        GameEvents.OnBlockSwapRequested += HandleBlockSwapRequested;
    }

    private void OnDisable()
    {
        GameEvents.OnBlockSwapRequested -= HandleBlockSwapRequested;
    }

    private void Start()
    {
        MakeGrid();
        StartCoroutine(AutoPopRoutine());
    }

    private void MakeGrid()
    {
        transform.position -= new Vector3(height / 2f, width / 2f, 0);
        grid = new Block[height, width];

        for(int i=0; i<height; i++)
        {
            for(int j=0; j<width; j++)
            {
                Vector3 pos = transform.position + new Vector3(j * 1.1f, i * 1.1f, 0);
                Block curBlock = Instantiate(block, transform);

                curBlock.name = $"Block[{i}][{j}]";
                curBlock.transform.position = pos;
                curBlock.y = i;
                curBlock.x = j;
                curBlock.effect = Instantiate(effect, transform);

                grid[i, j] = curBlock;
            }
        }
    }

    public void ResetGrid(int score = 0)
    {
        for(int i=0; i<height; i++)
        {
            for(int j=0; j<width; j++)
            {
                grid[i, j].SetColor();
            }
        }
        Score = score;
        GameEvents.RaiseScoreChanged(Score);
        StartCoroutine(AutoPopRoutine());
    }

    private static void SwapBlockColor(Block a, Block b)
    {
        int tmp = a.colorIdx;
        a.SetColor(b.colorIdx);
        b.SetColor(tmp);
    }

    private static void SwapBlockPosition(Block a, Block b)
    {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;
        a.transform.position = posB;
        b.transform.position = posA;
        a.MoveToTarget(posA, 0.4f);
        b.MoveToTarget(posB, 0.4f);
    }
    
    private static void SwapBlock(Block a, Block b)
    {
        SwapBlockColor(a, b);
        SwapBlockPosition(a, b);
    }

    private HashSet<(int, int)> GetMatchedBlocks()
    {
        HashSet<(int, int)> matchedBlocks = new HashSet<(int, int)>();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width - 2; j++)
            {
                if (grid[i, j].gameObject.activeSelf && grid[i, j + 1].gameObject.activeSelf && grid[i, j + 2].gameObject.activeSelf
                    && grid[i, j].colorIdx == grid[i, j + 1].colorIdx && grid[i, j].colorIdx == grid[i, j + 2].colorIdx)
                {
                    matchedBlocks.Add((i, j));
                    matchedBlocks.Add((i, j + 1));
                    matchedBlocks.Add((i, j + 2));
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height - 2; j++)
            {
                if (grid[j, i].gameObject.activeSelf && grid[j + 1, i].gameObject.activeSelf && grid[j + 2, i].gameObject.activeSelf
                    && grid[j, i].colorIdx == grid[j + 1, i].colorIdx && grid[j, i].colorIdx == grid[j + 2, i].colorIdx)
                {
                    matchedBlocks.Add((j, i));
                    matchedBlocks.Add((j + 1, i));
                    matchedBlocks.Add((j + 2, i));
                }
            }
        }

        return matchedBlocks;
    }

    private static void PopBlock(Block b)
    {
        b.EffectPlay();
        b.gameObject.SetActive(false);
    }
    
    private bool PopMatchedBlocks()
    {
        HashSet<(int y, int x)> matchedBlocks = GetMatchedBlocks();
        int popCount = matchedBlocks.Count;
        if (popCount == 0) return false;
        
        foreach (var yx in matchedBlocks)
            PopBlock(grid[yx.y, yx.x]);
        
        Score += popCount;
        GameEvents.RaiseScoreChanged(Score);

        return true;
    }
    
    private bool DropBlock()
    {
        bool isDown = false;
        for(int i=0; i<height; i++)
        {
            for(int j=0; j<width; j++)
            {
                if(!grid[i, j].gameObject.activeSelf)
                {
                    for(int k=i+1; k<height; k++)
                    {
                        if(grid[k, j].gameObject.activeSelf)
                        {
                            grid[i, j].SetColor(grid[k, j].colorIdx);
                            grid[i, j].gameObject.SetActive(true);
                            grid[k, j].gameObject.SetActive(false);
                            
                            grid[i, j].transform.position = grid[k, j].transform.position;
                            Vector3 targetPos = transform.position + new Vector3(j * 1.1f, i * 1.1f, 0);
                            grid[i, j].MoveToTarget(targetPos, 0.4f);
                            
                            isDown = true;
                            break;
                        }
                    }
                }
            }
        }

        return isDown;
    }

    private void MakeBlock()
    {
        for(int i=0; i<height; i++)
        {
            for(int j=0; j<width; j++)
            {
                if(!grid[i, j].gameObject.activeSelf)
                {
                    grid[i, j].SetColor();
                    grid[i, j].gameObject.SetActive(true);
                    
                    Vector3 targetPos = transform.position + new Vector3(j * 1.1f, i * 1.1f, 0);
                    grid[i, j].transform.position = targetPos + Vector3.up * 2f;
                    grid[i, j].MoveToTarget(targetPos, 0.5f);
                }
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        if(DropBlock()) yield return new WaitForSeconds(0.5f);
        MakeBlock();
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator AutoPopRoutine()
    {
        ChangeState(GameState.Popping);
        yield return new WaitForSeconds(0.5f);
        while(PopMatchedBlocks()) yield return RespawnRoutine();
        
        if(!HasAnyPossibleSwap()) ResetGrid(Score);
        ChangeState(GameState.Playing);
    }

    private void ChangeState(GameState state)
    {
        currentState = state;
        GameEvents.RaiseGameStateChanged(state);
    }
    
    private void HandleBlockSwapRequested(Block a, Block b)
    {
        if(currentState == GameState.Playing)
            StartCoroutine(SwapBlockRoutine(a, b));
    }

    IEnumerator SwapBlockRoutine(Block a, Block b)
    {
        SwapBlock(a, b);
        if (GetMatchedBlocks().Count > 0)
        {
            yield return StartCoroutine(AutoPopRoutine());
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
            SwapBlock(a, b);
        }
    }

    private bool HasAnyPossibleSwap()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j + 1 < width; j++)
            {
                Block a = grid[i, j];
                Block b = grid[i, j + 1];
                SwapBlockColor(a, b);
                if (GetMatchedBlocks().Count > 0)
                {
                    SwapBlockColor(a, b);
                    if(autoMod) StartCoroutine(SwapBlockRoutine(a, b));
                    return true;
                }
                SwapBlockColor(a, b);
            }
        }
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j + 1 < height; j++)
            {
                Block a = grid[j, i];
                Block b = grid[j + 1, i];
                SwapBlockColor(a, b);
                if (GetMatchedBlocks().Count > 0)
                {
                    SwapBlockColor(a, b);
                    if(autoMod) StartCoroutine(SwapBlockRoutine(a, b));
                    return true;
                }
                SwapBlockColor(a, b);
            }
        }
        
        return false;
    }
}
