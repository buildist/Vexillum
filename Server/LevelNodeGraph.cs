using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.util;
using Vexillum;
using System.Threading;
using Vexillum.Entities;

namespace Server
{
    public class LevelNodeGraph
    {
        private static ServerLevel level;
        private static Node[,] nodes;
        public const int gridSize = 4;
        const int platformThreshold = 4 * 4;
        const int ladderThreshold = 4 * 0;
        const int MAX_PATH_SIZE = 1000;

        public LevelNodeGraph(ServerLevel level)
        {
            LevelNodeGraph.level = level;
            nodes = new Node[(int) level.Size.X, (int) level.Size.Y];
            for (int x = 0; x < level.Size.X; x += gridSize)
            {
                for (int y = 0; y < level.Size.Y; y += gridSize)
                {
                    if (level.terrain.GetTerrain(x, y) || level.terrain.GetTerrain(x - gridSize, y) || level.terrain.GetTerrain(x + gridSize, y) || level.terrain.GetTerrain(x - gridSize * 2, y) || level.terrain.GetTerrain(x + gridSize * 2, y) || level.terrain.GetTerrain(x - gridSize*3, y) || level.terrain.GetTerrain(x + gridSize*3, y) || level.terrain.GetTerrain(x-gridSize,y) || level.terrain.GetTerrain(x+gridSize, y))
                        continue;
                    /*if (y % 16 == 0)
                    {
                        PixelEntity en = new PixelEntity();
                        en.Position = new Vec2(x, y);
                        level.AddEntity(en);
                    }*/
                    Node n = new Node(x, y);
                    nodes[x, y] = n;
                    n.bottom = !level.terrain.GetTerrain(x, y - gridSize);
                    bool ground = false;
                    for(int dx = x - platformThreshold; dx <= x + platformThreshold; dx++)
                        ground = ground || (level.terrain.GetTerrain(dx, y - gridSize) || level.terrain.GetLadder(dx, y - gridSize));
                    n.left = !level.terrain.GetTerrain(x-gridSize, y) && ground;
                    n.right = !level.terrain.GetTerrain(x + gridSize, y) && ground;
                    bool ladder = false;
                    for (int dx = x - ladderThreshold; dx <= x + ladderThreshold; dx++)
                        ladder = ladder || level.terrain.GetLadder(dx, y) || level.terrain.GetLadder(dx, y+gridSize) || level.terrain.GetLadder(dx, y-gridSize);
                    n.top = !level.terrain.GetTerrain(x, y + gridSize) && (ladder || (ground && (!level.terrain.GetTerrain(x + gridSize, y + gridSize) || !level.terrain.GetTerrain(x - gridSize, y + gridSize))));
                }
            }
        }
        private static int round(int num)
        {
            return (int)(Math.Round((float)num / gridSize) * gridSize);
        }
        private static int floor(int num)
        {
            return (int)(Math.Floor((float)num / gridSize) * gridSize);
        }
        public class Path
        {
            public int index = 0;
            public List<Node> points = new List<Node>();
        }
        public class Node
        {
            public int f;
            public int g;
            public int h;
            public int x;
            public int y;
            public bool left;
            public bool right;
            public bool top;
            public bool bottom;
            public Node(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public List<Node> getNeighbors(Node[,] nodes)
            {
                List<Node> neighbors = new List<Node>(4);
                if (left && nodes[x - gridSize, y] != null)
                    neighbors.Add(nodes[x - gridSize, y]);
                if (right && nodes[x + gridSize, y] != null)
                    neighbors.Add(nodes[x + gridSize, y]);
                if (top && nodes[x, y + gridSize] != null)
                    neighbors.Add(nodes[x, y+gridSize]);
                if (bottom && nodes[x, y - gridSize] != null)
                    neighbors.Add(nodes[x, y-gridSize]);
                return neighbors;
            }
            public int dist(Node n2)
            {
                return Math.Abs(n2.x - this.x) - Math.Abs(n2.y - this.y);
            }
        }
        public void FindPath(int x1, int y1, int x2, int y2, AIPlayer player)
        {
            FindPathTask t = new FindPathTask(x1, y1, x2, y2, player);
            ThreadPool.QueueUserWorkItem(t.ThreadPoolCallback);
        }
        public class FindPathTask
        {
            private int x1;
            private int x2;
            private int y1;
            private int y2;
            private AIPlayer player;
            private Path path;

            List<Node> closedSet = new List<Node>(MAX_PATH_SIZE);
            PrioQueue openSet = new PrioQueue();
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>(MAX_PATH_SIZE);
            Dictionary<Node, int> gScore = new Dictionary<Node, int>(MAX_PATH_SIZE);
            Dictionary<Node, int> hScore = new Dictionary<Node, int>(MAX_PATH_SIZE);
            Dictionary<Node, int> fScore = new Dictionary<Node, int>(MAX_PATH_SIZE);
            private Vec2[] pathArray = new Vec2[MAX_PATH_SIZE];


            public FindPathTask(int x1, int y1, int x2, int y2, AIPlayer player)
            {
                this.x1 = x1;
                this.x2 = x2;
                this.y1 = y1;
                this.y2 = y2;
                this.player = player;
            }
            public void ThreadPoolCallback(Object threadContext)
            {
                Path p = FindPath(x1, y1, x2, y2);
                player.targetPath = p;
                player.OnTargetPathChanged();
            }
            public Path FindPath(int x1, int y1, int x2, int y2)
            {
                if (true)
                    return null;
                openSet.Clear();
                closedSet.Clear();
                cameFrom.Clear();

                y1 = round(y1);
                y2 = round(y2);

                int y = y1;
                while (y > 0)
                {
                    if (level.terrain.GetTerrain(x1, y))
                        break;
                    y -= gridSize;

                }
                y1 = y + gridSize;

                y = y2;
                while (y > 0)
                {
                    if (level.terrain.GetTerrain(x2, y))
                        break;
                    y -= gridSize;

                }
                y2 = y + gridSize;

                if (y1 >= level.Size.Y)
                    y1 = floor((int)level.Size.Y - 1);
                if (y2 >= level.Size.Y)
                    y2 = floor((int)level.Size.Y - 1);
                if (y1 < 0)
                    y1 = 0;
                if (y2 < 0)
                    y2 = 0;

                if (x1 >= level.Size.X)
                    x1 = floor((int)level.Size.X - 1);
                if (x2 >= level.Size.X)
                    x2 = floor((int)level.Size.X - 1);
                if (x1 < 0)
                    x1 = 0;
                if (x2 < 0)
                    x2 = 0;

                Node start = nodes[round(x1), y1];
                Node end = nodes[round(x2), y2];
                if (start == null || end == null)
                    return null;
                start.g = 0;
                start.h = start.dist(end);
                start.f = start.g + start.h;

                openSet.Enqueue(start, start.f);
                while (!openSet.IsEmpty())
                {
                    Node x = (Node)openSet.Peek();
                    if (x == end)
                    {
                        Path p = ReconstructPath(cameFrom, x);
                        return p;
                    }
                    openSet.Dequeue();
                    closedSet.Add(x);
                    foreach (Node z in x.getNeighbors(nodes))
                    {
                        if (closedSet.Contains(z))
                            continue;
                        int gTemp = x.g + x.dist(z);
                        bool useTemp = false;
                        if (!openSet.Contains(z))
                        {
                            openSet.Enqueue(z, z.f);
                            useTemp = true;
                        }
                        else if (gTemp < z.g)
                        {
                            useTemp = true;
                        }
                        else
                            useTemp = false;
                        if (!useTemp)
                            continue;
                        if (cameFrom.ContainsKey(z))
                            cameFrom.Remove(z);
                        cameFrom.Add(z, x);
                        z.g = gTemp;
                        z.h = z.dist(end);
                        z.f = z.g + z.h;
                    }
                }
                return null;
            }
            public Path ReconstructPath(Dictionary<Node, Node> cameFrom, Node currentNode)
            {
                Path p = new Path();
                while (cameFrom.ContainsKey(currentNode))
                {
                    currentNode = cameFrom[currentNode];
                    p.points.Insert(0, currentNode);
                }
                return p;
            }
        }
    }
}
