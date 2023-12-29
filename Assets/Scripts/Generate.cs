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
    public Tilemap dec_map;
    public Tilemap rivers_map;
    public TileBase[] tiles;
    public TileBase[] dec_tiles;
    public TileBase[] rivers_tiles;
    public int height;
    public int width;
    public int iter;
    public int center;
    int[,] map_matrix;
    int[,] dec_map_matrix;
    int[,] rivers_map_matrix;
    Vector2Int[] center_points;
    Vector2Int[] offsets_0 = {
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1), 
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 1),
    };
    Vector2Int[] offsets_1 = {
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(0, 1),
    };
    
    Vector2Int[] mini_offsets_horizontal = {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 1),
    };
    Vector2Int[] mini_offsets_right = {
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
    };
    Vector2Int[] mini_offsets_left = {
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
    };
    Vector2Int[] mini_offsets_horizontal_center = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
    };
    Vector2Int[] mini_offsets_right_center = {
        new Vector2Int(1, -1),
        new Vector2Int(0, 1),
    };
    Vector2Int[] mini_offsets_left_center = {
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
    };
    enum Tiles
    {
        Land,
        Sea,
        Meadows,
        Rock,
        Sand,
        Shallow,
    }
    enum DecTiles
    {
        None,
        Forest,
        Forest2,
        Forest3,
        Hills,
        Oasis,
        Lake,
        Dune,
    }
    enum RiversTiles
    {
        None,
        Source,
        Horizontal,
        Left,
        Right
    }
    // Start is called before the first frame update
    void Start()
    {
        center = UnityEngine.Random.Range(3, 8);
        map_matrix = new int[height, width];
        dec_map_matrix = new int[height, width];
        rivers_map_matrix = new int[height * 2 + 1, width * 2 + 1];
        //Array.Clear(rivers_map_matrix, 0, rivers_map_matrix.Length);
        //Debug.Log(rivers_map_matrix[0,3]);
        center_points = new Vector2Int[center];
        //float w = map.GetComponent<TilemapRenderer>.rect.width;
        //cam.transform.position =  new Vector2(w, 0);
        cam.orthographicSize = width /4;
        //test();
        drawRivers();
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

    Vector2Int[] getOffsets(int y)
    {
        return y % 2 == 0 ? offsets_0 : offsets_1;
    }

    Vector2Int[] getMiniOffsets(int x, int y)
    {
        /*if (y % 3 == 0 && x % 2 == 0 || y % 3 != 0 && x % 2 == 1)
        {*/
            if (y % 2 == 0) return mini_offsets_horizontal;
            else if (y % 4 == 1)
            {
                if ((x + y) % 2 == 0) return mini_offsets_right;
                return mini_offsets_left;
            }
            else
            {
                if ((x + y) % 2 == 0) return mini_offsets_left;
                return mini_offsets_right;
            }
        /*}
        else return offsets_0;*/
    }
    Vector2Int[] getMiniCenterOffsets(int x, int y)
    {
        /*if (y % 3 == 0 && x % 2 == 0 || y % 3 != 0 && x % 2 == 1)
        {*/
        if (y % 2 == 0) return mini_offsets_horizontal_center;
        else if (y % 4 == 1)
        {
            if ((x + y) % 2 == 0) return mini_offsets_right_center;
            return mini_offsets_left_center;
        }
        else
        {
            if ((x + y) % 2 == 0) return mini_offsets_left_center;
            return mini_offsets_right_center;
        }
        /*}
        else return offsets_0;*/
    }

    int getRiverTile(int x, int y)
    {
        if (y % 2 == 0) return (int)RiversTiles.Horizontal;
        else if (y % 4 == 1)
        {
            if ((x + y) % 2 == 0) return (int)RiversTiles.Right;
            return (int)RiversTiles.Left;
        }
        else
        {
            if ((x + y) % 2 == 0) return (int)RiversTiles.Left;
            return (int)RiversTiles.Right;
        }
    }
    void start_points()
    {
        for (int i = 0; i < center; i++)
        {
            float h = 0.9f; // 0.4
            float w = 0.9f; // 0.3
            center_points[i] = new Vector2Int(UnityEngine.Random.Range((int)(height * h), (int)((height + 1) * (1-h))), UnityEngine.Random.Range((int)(width * w), (int)((width+1) * (1-w))));
            //Debug.Log(center_points[i]);
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
                    map_matrix[x, y] = (int)Tiles.Land;
                else
                    map_matrix[x, y] = (int)Tiles.Sea;
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
                Vector2Int[] offsets = yc % 2 == 0 ? offsets_0 : offsets_1;
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Sea)
                        counter_sea += 1;
                    else
                        counter_land += 1;
                }
                if (map_matrix[xc, yc] == (int)Tiles.Sea)
                {
                    if (counter_land == 3 || counter_land == 4 || counter_land == 5 || counter_land == 6)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Land;
                        continue;
                    }
                }
                if (map_matrix[xc, yc] == (int)Tiles.Land)
                {
                    if (counter_sea == 3 || counter_sea == 5 || counter_sea == 6 || (counter_sea == 4 && UnityEngine.Random.Range(0, 21) == 1)) //
                    {
                        map_matrix[xc, yc] = (int)Tiles.Sea;
                        continue;
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
                if (map_matrix[xc, yc] == (int)Tiles.Land)
                {
                    int counter_sea = 0;
                    Vector2Int[] offsets = getOffsets(yc);
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height) continue;
                        if (y < 0 || y >= width) continue;
                        if (map_matrix[x, y] == (int)Tiles.Sea)
                            counter_sea += 1;
                    }
                    if (map_matrix[xc, yc] == (int)Tiles.Land)
                    {
                        if (counter_sea > 0 && UnityEngine.Random.Range(0, 11) == 1)
                        {
                            map_matrix[xc, yc] = (int)Tiles.Sand;
                            continue;
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
                if (map_matrix[xc, yc] == (int)Tiles.Land)
                {
                    int counter_sea = 0;
                    int counter_sand = 0;
                    Vector2Int[] offsets = getOffsets(yc);
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height) continue;
                        if (y < 0 || y >= width) continue;
                        if (map_matrix[x, y] == (int)Tiles.Sea)
                            counter_sea += 1;
                        else if (map_matrix[x, y] == (int)Tiles.Sand)
                            counter_sand += 1;
                    }
                    if (map_matrix[xc, yc] == (int)Tiles.Land)
                    {
                        if (counter_sea > 0 && counter_sand > 0 && UnityEngine.Random.Range(0, 2) == 1)
                        {
                            map_matrix[xc, yc] = (int)Tiles.Sand;
                            continue;
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
                if (map_matrix[xc, yc] == (int)Tiles.Sea)
                {
                    int counter_sea = 0;
                    int counter_land = 0;
                    Vector2Int[] offsets = getOffsets(yc);
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height) continue;
                        if (y < 0 || y >= width) continue;
                        if (map_matrix[x, y] == (int)Tiles.Sea || map_matrix[x, y] == (int)Tiles.Shallow)
                            counter_sea++;
                        else if (map_matrix[x, y] == (int)Tiles.Land || map_matrix[x, y] == (int)Tiles.Sand)
                            counter_land++;
                    }
                    if (counter_sea > 0 && counter_land > 0)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Shallow;
                        continue;
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
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Sea)
                        counter_sea += 1;
                    else if (map_matrix[x, y] == (int)Tiles.Shallow)
                        counter_shallow += 1;
                    
                }
                if (map_matrix[xc, yc] == (int)Tiles.Sea)
                {
                    if (counter_shallow == 3 || counter_shallow == 5 || counter_shallow == 6) // 
                    {
                        map_matrix[xc, yc] = (int)Tiles.Shallow;
                        continue;
                    }
                }
            }
        }
        drawMap();
    }


    void meadows_generation() 
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (map_matrix[x, y] == (int)Tiles.Land && UnityEngine.Random.Range(0, 3) == 1)
                {
                    map_matrix[x, y] = (int)Tiles.Meadows;
                }
            }
        }
        drawMap();
    }

    void next_meadows_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_hills = 0;
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Meadows)
                        counter_hills += 1;
                    else if (map_matrix[x, y] == (int)Tiles.Land)
                        counter_land += 1;
                }
                if (map_matrix[xc, yc] == (int)Tiles.Land)
                {
                    if (counter_hills == 3 || counter_hills == 4 || counter_hills == 5 || counter_hills == 6)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Meadows;
                        continue;
                    }
                }
                if (map_matrix[xc, yc] == (int)Tiles.Meadows)
                {
                    if (counter_land == 3 || counter_land == 5 || counter_land == 6) //
                    {
                        map_matrix[xc, yc] = (int)Tiles.Land;
                        continue;
                    }
                }
            }
        }
        drawMap();
    }

    void forest_generation(int forest) 
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if ((map_matrix[x, y] == (int)Tiles.Land || map_matrix[x, y] == (int)Tiles.Meadows) && dec_map_matrix[x, y] == (int)DecTiles.None && UnityEngine.Random.Range(0, 3) == 1)
                {
                    dec_map_matrix[x, y] = forest;
                }
            }
        }
        drawMap();
    }

    void next_forest_generation(int forest)
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_forest = 0;
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (dec_map_matrix[x, y] == forest)
                        counter_forest += 1;
                    else if (dec_map_matrix[x, y] == (int)DecTiles.None)
                        counter_land += 1;
                }
                if (map_matrix[xc, yc] == (int)DecTiles.None)
                {
                    if (counter_forest == 3 || counter_forest == 4 || counter_forest == 5 || counter_forest == 6)
                    {
                        dec_map_matrix[xc, yc] = forest;
                        continue;
                    }
                }
                if (dec_map_matrix[xc, yc] == forest)
                {
                    if (counter_land == 3 || counter_land == 4 || counter_land == 5 || counter_land == 6) //
                    {
                        dec_map_matrix[xc, yc] = (int)DecTiles.None;
                        continue;
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
                if (map_matrix[x, y] == (int)Tiles.Land && UnityEngine.Random.Range(0, 4) == 1)
                {
                    map_matrix[x, y] = (int)Tiles.Rock;
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
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Land)
                        counter_land += 1;
                    else if (map_matrix[x, y] == (int)Tiles.Rock)
                        counter_rock += 1;
                    if (map_matrix[x, y] == (int)DecTiles.Forest)
                        counter_forest += 1;
                }
                if (map_matrix[xc, yc] == (int)Tiles.Rock)
                {
                    if ((counter_land + counter_forest) == 3 || (counter_land + counter_forest) == 4 || (counter_land + counter_forest) == 5 || (counter_land + counter_forest) == 6)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Land;
                        continue;
                    }
                }
                if (map_matrix[xc, yc] == (int)Tiles.Land)
                {
                    if (counter_rock == 3 || counter_rock == 4 || counter_rock == 5 || counter_rock == 6)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Rock;
                        continue;
                    }
                }
            }
        }
        drawMap();
    }

    void rivers_generation()
    {
        int height_ = height * 2 + 1;
        int width_ = width * 2 + 1;
        int sum = 0;

        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_rock = 0;
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Rock)
                        counter_rock += 1;
                }
                if (map_matrix[xc, yc] == (int)Tiles.Rock || counter_rock > 2)
                {
                    if (UnityEngine.Random.Range(0, 11) == 1)
                    {
                        Vector2Int vm = Big2SmallCoord(xc, yc);
                        vm += offsets[UnityEngine.Random.Range(0, 6)];
                        if (vm.x < 0 || vm.x >= height*2+1) continue;
                        if (vm.y < 0 || vm.y >= width*2+1) continue;
                        /*if (vm.y % 2 == 0)
                        {
                            rivers_map_matrix[vm.x, vm.y] = (int)RiversTiles.Horizontal;
                        }
                        else if (vm.y % 4 == 1)
                        {
                            if ((vm.x + vm.y) % 2 == 0)
                            {
                                rivers_map_matrix[vm.x, vm.y] = (int)RiversTiles.Right;
                            }
                            else
                            {
                                rivers_map_matrix[vm.x, vm.y] = (int)RiversTiles.Left;
                            }
                        }
                        else
                        {
                            if ((vm.x + vm.y) % 2 == 0)
                            {
                                rivers_map_matrix[vm.x, vm.y] = (int)RiversTiles.Left;
                            }
                            else
                            {
                                rivers_map_matrix[vm.x, vm.y] = (int)RiversTiles.Right;
                            }
                        }*/
                        rivers_map_matrix[vm.x, vm.y] = getRiverTile(vm.x, vm.y);
                        sum++;
                    }
                }
            }
        }
        Debug.Log(sum);
        drawRivers();
    }

    void next_rivers_generation()
    {
        int height_ = height * 2 + 1;
        int width_ = width * 2 + 1;
        int sum = 0;
        for (int xc = 0; xc < height_; xc++)
        {
            int last_x = xc;
            int last_y = 0;
            for (int yc = 0; yc < width_; yc++)
            {
                if (rivers_map_matrix[xc, yc] != (int)RiversTiles.None)
                {
                    int counter_rivers = 0;
                    int counter_none = 0;
                    Vector2Int[] offsets = getMiniOffsets(xc, yc);
                    //if (offsets == offsets_0) continue;
                    sum++;
                    
                    //rivers_map_matrix[xc, yc] = (int)RiversTiles.Source;
                    //Debug.Log(xc.ToString() + " " + yc.ToString());
                    
                    /*rivers_map_matrix[xc + offsets[0].x, yc + offsets[0].y] = (int)RiversTiles.Source;
                    rivers_map_matrix[xc + offsets[1].x, yc + offsets[1].y] = (int)RiversTiles.Source;
                    rivers_map_matrix[xc + offsets[2].x, yc + offsets[2].y] = (int)RiversTiles.Source;
                    rivers_map_matrix[xc + offsets[3].x, yc + offsets[3].y] = (int)RiversTiles.Source;*/
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height_) continue;
                        if (y < 0 || y >= width_) continue;
                        if (rivers_map_matrix[x, y] != (int)RiversTiles.None)
                            counter_rivers++;
                        else
                            counter_none++;
                    }
                    if (counter_rivers == 0)
                    {
                        Vector2Int offset = offsets[UnityEngine.Random.Range(0, 4)];
                        Vector2Int vm = new Vector2Int(xc, yc);
                        vm += offset;
                        /*int x_ = xc + offset.x;
                        int y_ = yc + offset.y;*/
                        if (vm.x < 0 || vm.x >= height_) continue;
                        if (vm.y < 0 || vm.y >= width_) continue;
                        rivers_map_matrix[vm.x, vm.y] = getRiverTile(vm.x, vm.y);
                        Debug.Log(rivers_map_matrix[vm.x, vm.y]);
                    }
                    else if (counter_rivers == 1)
                    {
                        Vector2Int offset = new Vector2Int(0, 0);
                        Vector2Int v1 = new Vector2Int(xc + offsets[0].x, yc + offsets[0].y);
                        Vector2Int v2 = new Vector2Int(xc + offsets[1].x, yc + offsets[1].y);
                        Vector2Int v3 = new Vector2Int(xc + offsets[2].x, yc + offsets[2].y);
                        Vector2Int v4 = new Vector2Int(xc + offsets[3].x, yc + offsets[3].y);

                        if (v1.x < 0 || v1.x >= height_) continue;
                        if (v1.y < 0 || v1.y >= width_) continue;
                        /*if (rivers_map_matrix[v1.x, v1.y] != (int)RiversTiles.None ||
                            rivers_map_matrix[v2.x, v2.y] != (int)RiversTiles.None)
                        {
                            offset = offsets[UnityEngine.Random.Range(2, 4)];
                        }
                        else
                        {
                            offset = offsets[UnityEngine.Random.Range(0, 2)];
                        }*/
                        try
                        {
                            int rand = UnityEngine.Random.Range(0, 8);
                            if (rivers_map_matrix[v1.x, v1.y] != (int)RiversTiles.None)
                            {
                                offset = rand != 0 ? offsets[2] : offsets[3];
                            }
                            else if (rivers_map_matrix[v2.x, v2.y] != (int)RiversTiles.None)
                            {
                                offset = rand != 0 ? offsets[3] : offsets[2];
                            }
                            else if (rivers_map_matrix[v3.x, v3.y] != (int)RiversTiles.None)
                            {
                                offset = rand != 0 ? offsets[0] : offsets[1];
                            }
                            else if (rivers_map_matrix[v4.x, v4.y] != (int)RiversTiles.None)
                            {
                                offset = rand != 0 ? offsets[1] : offsets[0];
                            }
                        }
                        catch
                        {
                            continue;
                        }
                        Vector2Int vm = new Vector2Int(xc, yc);
                        vm += offset;
                        Vector2Int[] center_offsets = getMiniCenterOffsets(vm.x, vm.y);
                        bool end = false;
                        foreach (Vector2Int vc in center_offsets)
                        {
                            Vector2Int bvc = Small2BigCoord(vm.x + vc.x, vm.y + vc.y);
                            if (bvc.x < 0 || bvc.x >= height) continue;
                            if (bvc.y < 0 || bvc.y >= width) continue;
                            if (map_matrix[bvc.x, bvc.y] == (int)Tiles.Shallow)
                            {
                                end = true;
                            }
                        }
                        if (end) continue;
                            /*int x_ = xc + offset.x;
                            int y_ = yc + offset.y;*/
                        if (vm.x < 0 || vm.x >= height_) continue;
                        if (vm.y < 0 || vm.y >= width_) continue;
                        rivers_map_matrix[vm.x, vm.y] = getRiverTile(vm.x, vm.y);
                        Debug.Log(rivers_map_matrix[vm.x, vm.y]);
                    }
                }
            }
        }
        Debug.Log(sum);
        drawRivers();
    }

    void big_rivers_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_rock = 0;
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Rock)
                        counter_rock += 1;
                }
                if (map_matrix[xc, yc] == (int)Tiles.Rock || counter_rock > 2)
                {
                    if (UnityEngine.Random.Range(0, 11) == 1)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Shallow;
                        continue;
                    }
                }
            }
        }
        drawMap();
    }

    void next_big_rivers_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_sea = 0;
                int counter_shallow = 0;
                int counter_land = 0;
                int[] water = new int[6];
                int i = -1;
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    i++;
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Sea)
                        counter_sea++;
                    else if (map_matrix[x, y] == (int)Tiles.Shallow)
                    {
                        counter_shallow++;
                        water[i] = 1;
                    }
                    else
                        counter_land++;
                }
                if (map_matrix[xc, yc] == (int)Tiles.Shallow)
                {
                    //if (counter_land == 5) Debug.Log(counter_shallow);
                    if (counter_land == 6)
                    {
                        while (true)
                        {
                            Vector2Int offset = offsets[UnityEngine.Random.Range(0, offsets.Length)];
                            int x_ = xc + offset.x;
                            int y_ = yc + offset.y;
                            if (x_ < 0 || x_ >= height) continue;
                            if (y_ < 0 || y_ >= width) continue;
                            map_matrix[x_, y_] = (int)Tiles.Shallow;
                            break;
                        }
                    }
                    else if (counter_shallow > 0 && counter_land > 3)
                    {
                        if (counter_shallow == 1) {
                            int j = -1;
                            foreach (int d in water) {
                                j++;
                                if (d == 1)
                                {
                                    int r = j + UnityEngine.Random.Range(2, 5);
                                    if (r > 5) r = r - 6;
                                    Vector2Int offset = offsets[r];
                                    int x_ = xc + offset.x;
                                    int y_ = yc + offset.y;
                                    if (x_ < 0 || x_ >= height) continue;
                                    if (y_ < 0 || y_ >= width) continue;
                                    map_matrix[x_, y_] = (int)Tiles.Shallow;
                                }
                            }
                        }
                       /* else if (counter_shallow == 2)
                        {

                        }*/
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
                if ((map_matrix[x, y] == (int)Tiles.Land || map_matrix[x, y] == (int)Tiles.Meadows) && dec_map_matrix[x, y] == (int)DecTiles.None && UnityEngine.Random.Range(0, 3) == 1)
                {
                    dec_map_matrix[x, y] = (int)DecTiles.Hills;
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
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (dec_map_matrix[x, y] == (int)DecTiles.Hills)
                        counter_hills += 1;
                    else if (dec_map_matrix[x, y] == (int)DecTiles.None)
                        counter_land += 1;
                }
                if (map_matrix[xc, yc] == (int)DecTiles.None)
                {
                    if (counter_hills == 3 || counter_hills == 4 || counter_hills == 5 || counter_hills == 6)
                    {
                        dec_map_matrix[xc, yc] = (int)DecTiles.Hills;
                        continue;
                    }
                }
                if (dec_map_matrix[xc, yc] == (int)DecTiles.Hills)
                {
                    if (counter_land == 5 || counter_land == 6) //
                    {
                        dec_map_matrix[xc, yc] = (int)DecTiles.None;
                        continue;
                    }
                }
            }
        }
        drawMap();
    }
    void dune_generation()
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if ((map_matrix[x, y] == (int)Tiles.Sand) && dec_map_matrix[x, y] == (int)DecTiles.None && UnityEngine.Random.Range(0, 3) == 1)
                {
                    dec_map_matrix[x, y] = (int)DecTiles.Dune;
                }
            }
        }
        drawMap();
    }
    void next_dune_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_dune = 0;
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (dec_map_matrix[x, y] == (int)DecTiles.Dune)
                        counter_dune += 1;
                    else if (dec_map_matrix[x, y] == (int)DecTiles.None)
                        counter_land += 1;
                }
                if (map_matrix[xc, yc] == (int)DecTiles.None)
                {
                    if (counter_dune == 3 || counter_dune == 4 || counter_dune == 5 || counter_dune == 6)
                    {
                        dec_map_matrix[xc, yc] = (int)DecTiles.Dune;
                        continue;
                    }
                }
                if (dec_map_matrix[xc, yc] == (int)DecTiles.Dune)
                {
                    if (counter_land == 5 || counter_land == 6) //
                    {
                        dec_map_matrix[xc, yc] = (int)DecTiles.None;
                        continue;
                    }
                }
            }
        }
        drawMap();
    }

    void deserts_generation()
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if ((map_matrix[x, y] == (int)Tiles.Land || map_matrix[x, y] == (int)Tiles.Meadows) && dec_map_matrix[x, y] == (int)DecTiles.None && UnityEngine.Random.Range(0, 1029) == 1)
                {
                    map_matrix[x, y] = (int)Tiles.Sand;
                }
            }
        }
        drawMap();
    }

    void next_deserts_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_sand = 0;
                Vector2Int[] offsets = getOffsets(yc);
                foreach (Vector2Int v in offsets)
                {
                    int x = xc + v.x;
                    int y = yc + v.y;
                    if (x < 0 || x >= height) continue;
                    if (y < 0 || y >= width) continue;
                    if (map_matrix[x, y] == (int)Tiles.Sand)
                        counter_sand++;
                    else if (map_matrix[x, y] == (int)Tiles.Land)
                        counter_land++;
                }
                if (map_matrix[xc, yc] == (int)Tiles.Land || map_matrix[xc, yc] == (int)Tiles.Meadows)
                {
                    if (counter_sand == 6 || counter_sand == 5)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Sand;
                        continue;
                    }
                    else if (counter_sand == 4 && UnityEngine.Random.Range(0, 2) == 1)
                    {

                    }
                    else if (counter_sand > 0 && UnityEngine.Random.Range(0, 6) == 1)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Sand;
                        continue;
                    }
                }
/*                if (map_matrix[xc, yc] == (int)Tiles.Meadows)
                {
                    if (counter_land == 3 || counter_land == 5 || counter_land == 6) //
                    {
                        map_matrix[xc, yc] = (int)Tiles.Land;
                        continue;
                    }
                }*/
            }
        }
        drawMap();
    }

    void last_deserts_generation()
    {
        for (int xc = 0; xc < height; xc++)
        {
            for (int yc = 0; yc < width; yc++)
            {
                int counter_land = 0;
                int counter_sand = 0;
                Vector2Int[] offsets = getOffsets(yc);
                if (map_matrix[xc, yc] == (int)Tiles.Sand)
                {
                    foreach (Vector2Int v in offsets)
                    {
                        int x = xc + v.x;
                        int y = yc + v.y;
                        if (x < 0 || x >= height) continue;
                        if (y < 0 || y >= width) continue;
                        if (map_matrix[x, y] == (int)Tiles.Sand)
                            counter_sand++;
                        else if (map_matrix[x, y] == (int)Tiles.Land)
                            counter_land++;
                    }
                    if (counter_land > 3)
                    {
                        map_matrix[xc, yc] = (int)Tiles.Land;
                        continue;
                    }
                }
            }
        }
        drawMap();
    }

    void oasis_generation()
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if ((map_matrix[x, y] == (int)Tiles.Sand) && dec_map_matrix[x, y] == (int)DecTiles.None && UnityEngine.Random.Range(0, 69) == 1)
                {
                    dec_map_matrix[x, y] = (int)DecTiles.Oasis;
                }
                if ((map_matrix[x, y] == (int)Tiles.Meadows) && dec_map_matrix[x, y] == (int)DecTiles.None && UnityEngine.Random.Range(0, 69) == 1)
                {
                    dec_map_matrix[x, y] = (int)DecTiles.Lake;
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
                map.SetTile(new Vector3Int(x - height / 2, y - width / 2, 0), tiles[map_matrix[x, y]]);
                dec_map.SetTile(new Vector3Int(x - height / 2, y - width / 2, 0), dec_tiles[dec_map_matrix[x, y]]);
                /*map.SetTile(new Vector3Int(x, y, 0), tiles[map_matrix[x, y]]);
                dec_map.SetTile(new Vector3Int(x, y, 0), dec_tiles[dec_map_matrix[x, y]]);*/
            }
        }
    }
    void drawRivers()
    {
        for (int x = 0; x < height * 2 + 1; x++)
        {
            for (int y = 0; y < width * 2 + 1; y++)
            {
                rivers_map.SetTile(new Vector3Int(x - height, y - width, 0), rivers_tiles[rivers_map_matrix[x, y]]);
                //rivers_map.SetTile(new Vector3Int(x, y, 0), rivers_tiles[rivers_map_matrix[x, y]]);
            }
        }
    }

    Vector2Int Big2SmallCoord(int x, int y)
    {
        x = y % 2 == 0 ? x * 2 : x * 2 + 1;
        y = y * 2;
        return new Vector2Int(x, y);
    }
    Vector2Int Small2BigCoord(int x, int y)
    {
        x = y % 2 == 0 ? x / 2 : (x - 1) / 2 ;
        y = y / 2;
        return new Vector2Int(x, y);
    }

    void test()
    {
        /*Vector2Int[] offsets = mini_offsets_horizontal;
        foreach (Vector2Int v in offsets)
        {
            rivers_map.SetTile(new Vector3Int(v.x+1, v.y, 0), rivers_tiles[(int)RiversTiles.Source]);
        }*/
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
                        next_base_generation();
                        r++;
                        if (r == iter)
                        {
                            step++;
                            r = 0;
                        }
                    }
                    break;
                case 1:
                    deserts_generation();
                    step++;
                    break;
                case 2:
                    if (r < 20)
                    {
                        next_deserts_generation();
                        r++;
                        if (r == 20)
                        {
                            last_deserts_generation();
                            step++;
                            r = 0;
                        }
                    }
                    break;
                case 3:
                    sand_generation();
                    step++;
                    break;
                case 4:
                    next_sand_generation();
                    step++;
                    break;
                case 5:
                    shallow_generation();
                    step++;
                    break;
                case 6:
                    next_shallow_generation();
                    step++;
                    break;
                case 7:
                    meadows_generation();
                    step++;
                    break;
                case 8:
                    next_meadows_generation();
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
                case 11:
                    rivers_generation();
                    step++;
                    break;
                case 12:
                    if (r < 30)
                    {
                        next_rivers_generation();
                        r++;
                        if (r == 30)
                        {
                            step++;
                            r = 0;
                        }
                    }
                    break;
                case 13:
                    forest_generation((int)DecTiles.Forest);
                    step++;
                    break;
                case 14:
                    next_forest_generation((int)DecTiles.Forest);
                    step++;
                    break;
                case 15:
                    forest_generation((int)DecTiles.Forest2);
                    step++;
                    break;
                case 16:
                    next_forest_generation((int)DecTiles.Forest2);
                    step++;
                    break;
                case 17:
                    forest_generation((int)DecTiles.Forest3);
                    step++;
                    break;
                case 18:
                    next_forest_generation((int)DecTiles.Forest3);
                    step++;
                    break;
                case 19:
                    hills_generation();
                    step++;
                    break;
                case 20:
                    next_hills_generation();
                    step++;
                    break;
                case 21:
                    dune_generation();
                    step++;
                    break;
                case 22:
                    next_dune_generation();
                    step++;
                    break;
                case 23:
                    oasis_generation();
                    step++;
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            step = 0;
            r = 0;
            center = UnityEngine.Random.Range(3, 8);
            center_points = new Vector2Int[center];
            dec_map_matrix = new int[height, width];
            rivers_map_matrix = new int[height * 2 + 1, width * 2 + 1];
            drawRivers();
            base_generation();
        }
    }
}