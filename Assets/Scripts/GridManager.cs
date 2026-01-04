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
    
    private Block[,] grid;
    private Block firstBlock;
    public int Score { get; private set; }

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
        grid = new Block[height, width];

        for(int i=0; i<height; i++)
        {
            for(int j=0; j<width; j++)
            {
                Vector3 pos = transform.position;
                pos.y += i * 1.1f;
                pos.x += j * 1.1f;
                Block curBlock = Instantiate(block, transform);

                curBlock.name = "Block[" + i + "][" + j + "]";
                curBlock.transform.position = pos;
                curBlock.y = i;
                curBlock.x = j;
                curBlock.effect = Instantiate(effect, transform);

                grid[i, j] = curBlock;
            }
        }
    }

    public void ResetGrid()
    {
        for(int i=0; i<height; i++)
        {
            for(int j=0; j<width; j++)
            {
                grid[i, j].SetColor();
            }
        }
        Score = 0;
        GameEvents.RaiseScoreChanged(Score);
        StartCoroutine(AutoPopRoutine());
    }

    private void SwapBlock(Block a, Block b)
    {
        int tmp = a.colorIdx;
        a.SetColor(b.colorIdx);
        b.SetColor(tmp);
    }

    private bool PopBlock(int y, int x)
    {
        int popCount = 0;
        List<(int y, int x)> col = new List<(int y, int x)>();
        List<(int y, int x)> row = new List<(int y, int x)>();
        int colorIdx = grid[y, x].colorIdx;

        Queue<(int y, int x, bool d)> q = new Queue<(int y, int x, bool d)>();
        bool[,] visited = new bool[height, width];
        q.Enqueue((y, x, false));
        q.Enqueue((y, x, true));
        visited[y, x] = true;
        col.Add((y, x));
        row.Add((y, x));

        while(q.Count > 0)
        {
            var yx = q.Dequeue();

            if(!yx.d && yx.y + 1 < height && !visited[yx.y + 1, yx.x] && grid[yx.y + 1, yx.x].colorIdx == colorIdx)
            {
                q.Enqueue((yx.y + 1, yx.x, false));
                visited[yx.y + 1, yx.x] = true;
                col.Add((yx.y + 1, yx.x));
            }
            if(!yx.d && yx.y - 1 >= 0 && !visited[yx.y - 1, yx.x] && grid[yx.y - 1, yx.x].colorIdx == colorIdx)
            {
                q.Enqueue((yx.y - 1, yx.x, false));
                visited[yx.y - 1, yx.x] = true;
                col.Add((yx.y - 1, yx.x));
            }
            if(yx.d && yx.x + 1 < width && !visited[yx.y, yx.x + 1] && grid[yx.y, yx.x + 1].colorIdx == colorIdx)
            {
                q.Enqueue((yx.y, yx.x + 1, true));
                visited[yx.y, yx.x + 1] = true;
                row.Add((yx.y, yx.x + 1));
            }
            if(yx.d && yx.x - 1 >= 0 && !visited[yx.y, yx.x - 1] && grid[yx.y, yx.x - 1].colorIdx == colorIdx)
            {
                q.Enqueue((yx.y, yx.x - 1, true));
                visited[yx.y, yx.x - 1] = true;
                row.Add((yx.y, yx.x - 1));
            }
        }

        if(col.Count >= 3)
        {
            foreach(var yx in col)
            {
                grid[yx.y, yx.x].EffectPlay();
                grid[yx.y, yx.x].gameObject.SetActive(false);
            }
            popCount += col.Count;
        }
        if(row.Count >= 3)
        {
            foreach(var yx in row)
            {
                grid[yx.y, yx.x].EffectPlay();
                grid[yx.y, yx.x].gameObject.SetActive(false);
            }
            popCount += row.Count;
        }

        Score += popCount;
        GameEvents.RaiseScoreChanged(Score);

        return popCount > 0;
    }

    private bool DownBlock()
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
                            isDown = true;
                            grid[i, j].SetColor(grid[k, j].colorIdx);
                            grid[i, j].gameObject.SetActive(true);
                            grid[k, j].gameObject.SetActive(false);
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
                }
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        if(DownBlock()) yield return new WaitForSeconds(0.5f);
        MakeBlock();
    }

    private bool AutoPop()
    {
        bool isPop = false;
        for(int i=0; i<height; i++)
        {
            for(int j=0; j<width; j++)
            {
                if(grid[i, j].gameObject.activeSelf && PopBlock(i, j))
                    isPop = true;
            }
        }

        return isPop;
    }

    private IEnumerator AutoPopRoutine()
    {
        ChangeState(GameState.Popping);
        yield return new WaitForSeconds(0.5f);
        while(AutoPop()) yield return RespawnRoutine();
        ChangeState(GameState.Playing);
    }

    private void ChangeState(GameState state)
    {
        GameEvents.RaiseGameStateChanged(state);
    }
    
    private void HandleBlockSwapRequested(Block a, Block b)
    {
        SwapBlock(a, b);
        StartCoroutine(AutoPopRoutine());
    }
}
