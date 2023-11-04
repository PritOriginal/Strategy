using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Generate : MonoBehaviour
{
    public Camera cam;
    public Tilemap map;
    public TileBase[] tile;
    public int height;
    public int width;
    public int iter;
    public int center;
    int[,] map_matrix;
    Vector2Int[] center_points;
    Vector2Int[] offsets = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(1, 1),
        new Vector2Int(0, 0)
    };
    // Start is called before the first frame update
    void Start()
    {
        map_matrix = new int[height, width];
        center_points = new Vector2Int[center];
        //float w = map.GetComponent<TilemapRenderer>.rect.width;
        //cam.transform.position =  new Vector2(w, 0);
        cam.orthographicSize = width /4;
        base_generation();
        /*
        for (int i = 0; i < iter; i++)
        {
            //StartCoroutine(waiter());
            next_base_generation();
            //Invoke("next_base_generation", 2f);
        }
        sand_generation();
        next_sand_generation();
        forest_generation();
        next_forest_generation();
        rock_generation();
        next_rock_generation();
        */
    }

    IEnumerator waiter()
    {
        //Wait for 4 seconds
        yield return new WaitForSecondsRealtime(4);
    }

    void start_points()
    {
        for (int i = 0; i < center; i++)
        {
            float h = 0.9f; // 0.4
            float w = 0.9f; // 0.3
            center_points[i] = new Vector2Int(UnityEngine.Random.Range((int)(height * h), (int)((height + 1) * (1-h))), UnityEngine.Random.Range((int)(width * w), (int)((width+1) * (1-w))));
            Debug.Log(center_points[i]);
        }
    }

    void base_generation()
    {
        start_points();
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                double distance = 0;
                //float[] d = new float[center];
                int i = 0;
                foreach (Vector2Int cent in center_points)
                {

                    double d = (float)Math.Sqrt(Math.Pow(cent.x - x, 2) + Math.Pow(cent.y - y, 2));
                    if (d < distance || (distance == 0 && d != 0))
                        distance = d;
                    
                    //d[i] = (float)Math.Sqrt(Math.Pow(cent.x - x, 2) + Math.Pow(cent.y - y, 2));
                    //i++;                   
                }
                //distance = d.Min();
                if (x + 10 == center_points[0].x + 10 && y+10 == center_points[0].y + 10)
                {
                  //  Debug.Log((int)(distance / width * 100));
                }
                // 15
                int chance = (int)(distance / width * 100) / 5;
                if (UnityEngine.Random.Range(0,chance / 2 + 1) == 0 && UnityEngine.Random.Range(0, 2) == 0)
                    map_matrix[x, y] = 0;
                else
                    map_matrix[x, y] = 1;
                //map_matrix[x, y] = UnityEngine.Random.Range(0, 2);
            }
        }
        drawMap();
    }

    void next_base_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_sea = 0;
                int counter_land = 0;
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (x == xc && y == yc)
                    {
                        if (map_matrix[x, y] == 1)
                        {
                            if (counter_land == 3 || counter_land == 4 || counter_land == 5 || counter_land == 6)
                            {
                                map_matrix[x, y] = 0;
                                continue;
                            }
                        }
                        if (map_matrix[x, y] == 0)
                        {
                            if (counter_sea == 3 || counter_sea == 5 || counter_sea == 6) //
                            {
                                map_matrix[x, y] = 1;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (map_matrix[x, y] == 1)
                            counter_sea += 1;
                        else
                            counter_land += 1;
                    }
                }
            }
        }
        drawMap();
    }

    void sand_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                if (map_matrix[xc, yc] == 0)
                {
                    int counter_sea = 0;
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height) continue;
                        if (y < 0 || y >= width) continue;
                        if (x == xc && y == yc)
                        {
                            if (map_matrix[x, y] == 0)
                            {
                                if (counter_sea > 0 && UnityEngine.Random.Range(0, 11) == 1)
                                {
                                    map_matrix[x, y] = 4;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (map_matrix[x, y] == 1)
                                counter_sea += 1;
                        }
                    }
                }
            }
        }
        drawMap();
    }

    void next_sand_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                if (map_matrix[xc, yc] == 0)
                {
                    int counter_sea = 0;
                    int counter_sand = 0;
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height) continue;
                        if (y < 0 || y >= width) continue;
                        if (x == xc && y == yc)
                        {
                            if (map_matrix[x, y] == 0)
                            {
                                if (counter_sea > 0 && counter_sand > 0 && UnityEngine.Random.Range(0, 2) == 1)
                                {
                                    map_matrix[x, y] = 4;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (map_matrix[x, y] == 1)
                                counter_sea += 1;
                            else if (map_matrix[x, y] == 4)
                                counter_sand += 1;
                        }
                    }
                }
            }
        }
        drawMap();
    }

    void shallow_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                if (map_matrix[xc, yc] == 1)
                {
                    int counter_sea = 0;
                    int counter_land = 0;
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height) continue;
                        if (y < 0 || y >= width) continue;
                        if (x == xc && y == yc)
                        {
                            if (counter_sea > 0 && counter_land > 0)
                            {
                                map_matrix[x, y] = 5;
                                continue;
                            }
                        }
                        else
                        {
                            if (map_matrix[x, y] == 1 || map_matrix[x, y] == 5)
                                counter_sea++;
                            else if (map_matrix[x, y] == 0 || map_matrix[x, y] == 4)
                                counter_land++;
                        }
                    }
                }
            }
        }
        drawMap();
    }

    void next_shallow_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_sea = 0;
                int counter_shallow = 0;
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (x == xc && y == yc)
                    {
                        if (map_matrix[x, y] == 1)
                        {
                            if (counter_shallow == 3 || counter_shallow == 4 || counter_shallow == 5 || counter_shallow == 6)
                            {
                                map_matrix[x, y] = 5;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (map_matrix[x, y] == 1)
                            counter_sea += 1;
                        else if (map_matrix[x, y] == 5)
                            counter_shallow += 1;
                    }
                }
            }
        }
        drawMap();
    }


    void hills_generation() 
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (map_matrix[x, y] == 0 && UnityEngine.Random.Range(0, 3) == 1)
                {
                    map_matrix[x, y] = 6;
                }
            }
        }
        drawMap();
    }

    void next_hills_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_hills = 0;
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (x == xc && y == yc)
                    {
                        if (map_matrix[x, y] == 0)
                        {
                            if (counter_hills == 3 || counter_hills == 4 || counter_hills == 5 || counter_hills == 6)
                            {
                                map_matrix[x, y] = 6;
                                continue;
                            }
                        }
                        if (map_matrix[x, y] == 6)
                        {
                            if (counter_land == 3 || counter_land == 5 || counter_land == 6) //
                            {
                                map_matrix[x, y] = 0;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (map_matrix[x, y] == 6)
                            counter_hills += 1;
                        else if (map_matrix[x, y] == 0)
                            counter_land += 1;
                    }
                }
            }
        }
        drawMap();
    }

    void forest_generation() 
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (map_matrix[x, y] == 0 && UnityEngine.Random.Range(0, 2) == 1)
                {
                    map_matrix[x, y] = 2;
                }
            }
        }
        drawMap();
    }

    void next_forest_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_forest = 0;
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (x == xc && y == yc)
                    {
                        if (map_matrix[x, y] == 0)
                        {
                            if (counter_forest == 3 || counter_forest == 4 || counter_forest == 5 || counter_forest == 6)
                            {
                                map_matrix[x, y] = 2;
                                continue;
                            }
                        } 
                        if (map_matrix[x, y] == 2)
                        {
                            if (counter_land == 3 || counter_land == 5 || counter_land == 6) //
                            {
                                map_matrix[x, y] = 0;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (map_matrix[x, y] == 2)
                            counter_forest += 1;
                        else if (map_matrix[x, y] == 0)
                            counter_land += 1;
                    }
                }
            }
        }
        drawMap();
    }
    void rock_generation()
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if ((map_matrix[x, y] == 0 || map_matrix[x, y] == 2) && UnityEngine.Random.Range(0, 4) == 1)
                {
                    map_matrix[x, y] = 3;
                }
            }
        }
        drawMap();
    }

    void next_rock_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_forest = 0;
                int counter_rock = 0;
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (x == xc && y == yc)
                    {
                        if (map_matrix[x, y] == 3)
                        {
                            if ((counter_land + counter_forest) == 3 || (counter_land + counter_forest) == 4 || (counter_land + counter_forest) == 5 || (counter_land + counter_forest) == 6)
                            {
                                if (counter_land >= counter_forest)
                                    map_matrix[x, y] = 0;
                                else
                                    map_matrix[x, y] = 2;
                                continue;
                            }
                        }
                        if (map_matrix[x, y] == 0 || map_matrix[x, y] == 2)
                        {
                            if (counter_rock == 3 || counter_rock == 4 || counter_rock == 5 || counter_rock == 6)
                            {
                                map_matrix[x, y] = 3;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (map_matrix[x, y] == 0)
                            counter_land += 1;
                        else if (map_matrix[x, y] == 2)
                            counter_forest += 1;
                        else if (map_matrix[x, y] == 3)
                            counter_rock += 1;
                    }
                }
            }
        }
        drawMap();
    }

    void drawMap()
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                map.SetTile(new Vector3Int(x-height/2, y-width/2, 0), tile[map_matrix[x, y]]);
            }
        }
    }

    bool frame = false;
    int step = 0;
    int r = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
        {
            switch (step)
            {
                case 0:
                    if (r < iter)
                    {
                        //StartCoroutine(waiter());
                        next_base_generation();
                        //Invoke("next_base_generation", 2f);
                        r++;
                        if (r == iter)
                        {
                            step++;
                            r = 0;
                        }
                    }
                    break;
                case 1:
                    sand_generation();
                    step++;
                    break;
                case 2:
                    next_sand_generation();
                    step++;
                    break;
                case 3:
                    shallow_generation();
                    step++;
                    break;
                case 4:
                    next_shallow_generation();
                    step++;
                    break;
                case 5:
                    hills_generation();
                    step++;
                    break;
                case 6:
                    next_hills_generation();
                    step++;
                    break;
                case 7:
                    forest_generation();
                    step++;
                    break;
                case 8:
                    next_forest_generation();
                    step++;
                    break;
                case 9:
                    rock_generation();
                    step++;
                    break;
                case 10:
                    next_rock_generation();
                    step++;
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            step = 0;
            r = 0;
            base_generation();
        }
    }
}