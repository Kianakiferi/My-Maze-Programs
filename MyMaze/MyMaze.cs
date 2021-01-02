using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AStarMaze
{
	public enum Directions
	{
		Up = 0,
		Right = 1,
		Down = 2,
		Left = 3
	};

	public struct Point
	{
		public int X;
		public int Y;
		public double Weight;
		public Directions Direction;

		public Point(int x, int y)
		{
			X = x;
			Y = y;
			Weight = 0.0;
			Direction = Directions.Up;
		}

		public Point(int x, int y, double weight) : this(x, y)
		{
			Weight = weight;
		}

		public Point(int x, int y, Directions direction) : this(x, y)
		{
			Direction = direction;
		}
	}

	class Maze
	{
		public int SIZE = 11;

		public int[,] Map;
		private readonly int[,] _mapBackup;

		public bool CanMapInitialization
		{
			get
			{
				//TODO 更加准确的判断

				if (_mapBackup != null)
				{
					if (_mapBackup.GetLength(0) == SIZE)
					{
						return true;
					}
				}

				return false;
			}
		}

		public Maze(int size)
		{
			if (size <= 0)
			{
				this.SIZE = 1;
			}
			this.SIZE = size;
		}

		public Maze(int[,] map)
		{
			SIZE = map.GetLength(0);
			Map = new int[SIZE, SIZE];
			_mapBackup = new int[SIZE, SIZE];
			Array.Copy(map, Map, map.Length);
			Array.Copy(map, _mapBackup, map.Length);
		}

		public void RecoverMaze()
		{
			if (CanMapInitialization)
			{
				Map = new int[SIZE, SIZE];
				Array.Copy(_mapBackup, Map, _mapBackup.Length);
			}
		}

		public void ShowMaze()
		{
			ShowMaze(Map);
		}

		static public void ShowMaze(int[,] map)
		{
			int Size = map.GetLength(0);
			for (int i = 0; i < Size; i++)
			{
				StringBuilder oneline = new StringBuilder();
				for (int j = 0; j < Size; j++)
				{
					if (map[i, j] == 0)
					{
						oneline.Append("  ");
					}
					else
					{
						oneline.Append("■");
					}
				}
				Console.WriteLine(oneline.ToString());
			}
			Console.WriteLine();
		}


	}

	class MazeGenerator
	{
		private static RNGCryptoServiceProvider rngCSP = new RNGCryptoServiceProvider();
		public int SIZE = 11;

		static readonly int ROAD = 0;
		static readonly int WALL = 1;
		static readonly int BORDER = 9;

		static readonly int VISITED = 1;
		int[,] map;
		int[,] _visited;


		public List<Point> BuildInitialMaze_H()
		{
			map = new int[SIZE, SIZE];
			_visited = new int[SIZE, SIZE];
			List<Point> result = new List<Point>();

			for (int X = 0; X < SIZE; X++)
			{
				for (int Y = 0; Y < SIZE; Y++)
				{
					if ((X == 0) || (X == SIZE - 1) || (Y == 0) || (Y == SIZE - 1))
					{
						map[X, Y] = BORDER;
					}
					else
					{
						map[X, Y] = WALL;
					}
					if ((X + 1) % 2 == 0 && (Y + 1) % 2 == 0)
					{
						map[X, Y] = ROAD;
						result.Add(new Point(X, Y));
					}
				}
			}

			return result;
		}

		private void SetPoint_Road(Point point)
		{
			map[point.X, point.Y] = ROAD;
		}
		private void SetPoint_Visited(Point point)
		{
			_visited[point.X, point.Y] = VISITED;
		}

		private bool IsWall(Point point)
		{
			//if (map[x, y] != BORDER)
			if (map[point.X, point.Y] == WALL)
			{
				return true;
			}

			return false;
		}
		private bool IsWall(int x, int y)
		{
			//if (map[x, y] != BORDER)
			if (map[x, y] == WALL)
			{
				return true;
			}

			return false;
		}

		private bool CanWallBreak(Point wall, Point next)
		{
			Point front = GetPrevPoint(wall);

			if ((_visited[front.X, front.Y] == VISITED) && (_visited[next.X, next.Y] == VISITED))
			{
				return false;
			}
			return true;
		}

		private T GetRandomOne<T>(List<T> list)
		{
			byte[] SeedByte = new byte[16];
			rngCSP.GetBytes(SeedByte);

			Random random = new Random(BitConverter.ToInt32(SeedByte));
			return list.ElementAt(random.Next(0, list.Count));
		}

		private Point GetPrevPoint(Point wall)
		{
			Point result = default;
			switch (wall.Direction)
			{
				case Directions.Up:
					{
						result = new Point(wall.X, wall.Y + 1);
						break;
					}
				case Directions.Right:
					{
						result = new Point(wall.X - 1, wall.Y);
						break;
					}
				case Directions.Down:
					{
						result = new Point(wall.X, wall.Y - 1);
						break;
					}
				case Directions.Left:
					{
						result = new Point(wall.X + 1, wall.Y);
						break;
					}
				default:
					break;
			}
			return result;
		}
		private Point GetNextPoint(Point wall)
		{
			Point result = default;
			switch (wall.Direction)
			{
				case Directions.Up:
					{
						result = new Point(wall.X, wall.Y - 1);
						break;
					}
				case Directions.Right:
					{
						result = new Point(wall.X + 1, wall.Y);
						break;
					}
				case Directions.Down:
					{
						result = new Point(wall.X, wall.Y + 1);
						break;
					}
				case Directions.Left:
					{
						result = new Point(wall.X - 1, wall.Y);
						break;
					}
				default:
					break;
			}

			return result;
		}

		private List<Point> GetNearWalls(Point point)
		{
			List<Point> walls = new List<Point>();
			int X = point.X;
			int Y = point.Y;

			//Up
			if (Y - 1 > 0)
			{
				if (IsWall(X, Y - 1))
				{
					walls.Add(new Point(X, Y - 1, Directions.Up));
				}
			}

			//Right
			if (X + 1 < SIZE - 1)
			{
				if (IsWall(X + 1, Y))
				{
					walls.Add(new Point(X + 1, Y, Directions.Right));
				}
			}

			//Down
			if (Y + 1 < SIZE - 1)
			{
				if (IsWall(X, Y + 1))
				{
					walls.Add(new Point(X, Y + 1, Directions.Down));
				}
			}

			//Left
			if (X - 1 > 0)
			{
				if (IsWall(X - 1, Y))
				{
					walls.Add(new Point(X - 1, Y, Directions.Left));
				}
			}
			return walls;
		}

		public Maze GeneratorMaze()
		{
			List<Point> _roadList = BuildInitialMaze_H();

			Point startPoint = GetRandomOne(_roadList);
			SetPoint_Road(startPoint);
			SetPoint_Visited(startPoint);

			List<Point> wallList = GetNearWalls(startPoint);

			while (wallList.Count != 0)
			{
				Point wall = GetRandomOne(wallList);
				Point next = GetNextPoint(wall);

				if (CanWallBreak(wall, next))
				{
					//Console.WriteLine(wall.X + ", " + wall.Y);
					SetPoint_Road(wall);
					SetPoint_Visited(next);
					wallList.Remove(wall);

					wallList.AddRange(GetNearWalls(next));
				}
				else
				{
					wallList.Remove(wall);
				}
			}

			return new Maze(map);
		}

		public void ShowMaze()
		{
			Maze.ShowMaze(map);
		}
	}

	class MazeSolver
	{
		private static RNGCryptoServiceProvider rngCSP = new RNGCryptoServiceProvider();

		public const int SIZE = 11;
		public const double constK = 1.0;

		private static readonly int[] directionOrder = { 0, 1, 2, 3 };
		private static readonly int[,] direction = { { -1, 0 }, { 1, 0 }, { 0, 1 }, { 0, -1 } };

		private int StartX;
		private int StartY;
		public Point StartPoint { get => new Point(StartX, StartY); }
		private int TargetX;
		private int TargetY;

		Maze maze;
		int[,] _visited = new int[SIZE, SIZE];

		private bool IsFinded { get; set; }

		public MazeSolver(Maze map)
		{
			this.maze = map;
		}
		public MazeSolver(Maze map, int startX, int startY, int targetX, int targetY)
		{
			this.maze = map;
			this.StartX = startX;
			this.StartY = startY;
			this.TargetX = targetX;
			this.TargetY = targetY;
		}

		public void SetStartPoint(int x, int y)
		{
			this.StartX = x;
			this.StartY = y;

			Console.WriteLine("Set start point at: " + StartX + ", " + StartY + "\n");
		}
		public void SetStartPoint(Point point)
		{
			this.StartX = point.X;
			this.StartY = point.Y;
		}

		List<Point> roads = new List<Point>();
		public void SetStartPointRandom()
		{
			byte[] SeedByte = new byte[16];
			rngCSP.GetBytes(SeedByte);
			Random random = new Random(BitConverter.ToInt32(SeedByte));

			if (roads.Count == 0)
			{
				roads = GetRoads(maze.Map);
			}

			SetStartPoint(roads.ElementAt(random.Next(0, roads.Count)));

			Console.WriteLine("Set start point at: " + StartX + ", " + StartY + "\n");
		}

		public void SetTargetPoint(int x, int y)
		{
			this.TargetX = x;
			this.TargetY = y;
			Console.WriteLine("Set target point at: " + TargetX + ", " + TargetY + "\n");
		}
		public void SetTargetPoint(Point point)
		{
			this.TargetX = point.X;
			this.TargetY = point.Y;
		}
		public void SetTatgetPointRandom()
		{
			byte[] SeedByte = new byte[16];
			rngCSP.GetBytes(SeedByte);
			Random random = new Random(BitConverter.ToInt32(SeedByte));

			if (roads.Count == 0)
			{
				roads = GetRoads(maze.Map);
			}

			SetTargetPoint(roads.ElementAt(random.Next(0, roads.Count)));

			Console.WriteLine("Set target point at: " + TargetX + ", " + TargetY + "\n");
		}

		private bool IsPointAvailable(Point point)
		{
			if (maze.Map[point.X, point.Y] == 0 && _visited[point.X, point.Y] != 1)
			{
				return true;
			}
			return false;
		}
		private bool IsTarget(Point point)
		{
			if (point.X == TargetX && point.Y == TargetY)
			{
				Console.WriteLine("Find the target");
				return true;
			}
			return false;
		}
		private void Visited(Point point)
		{
			_visited[point.X, point.Y] = 1;
		}
		public static int[] ShuffleArray(int[] array)
		{
			int[] result = new int[array.Length];
			Array.Copy(array, result, array.Length);

			Random random = new Random(new RNGCryptoServiceProvider().GetHashCode());
			int temp;
			for (int i = 0; i < result.Length; i++)
			{
				int index = random.Next(0, result.Length - 1);
				if (index != i)
				{
					temp = result[i];
					result[i] = result[index];
					result[index] = temp;
				}
			}
			return result;
		}

		private List<Point> GetRoads(int[,] map)
		{
			List<Point> result = new List<Point>();
			int Size = map.GetLength(0);

			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
					if (map[i, j] == 0)
					{
						result.Add(new Point(i, j));
					}
				}
			}

			return result;
		}


		private List<Point> GetNearPoints(Point point)
		{
			int X = point.X;
			int Y = point.Y;

			List<Point> points = new List<Point>();

			if (X >= 0 && X < SIZE && Y >= 0 && Y < SIZE)
			{

				foreach (var order in ShuffleArray(directionOrder))
				{
					Point next = new Point
					(
						X + direction[order, 0],
						Y + direction[order, 1]
					);
					if (IsPointAvailable(next))
					{
						points.Add(next);
						Console.WriteLine(next.X + ", " + next.Y);
					}
				}
			}

			return points;
		}

		public void SearchBFS()
		{
			IsFinded = false;
			_visited = new int[SIZE, SIZE];
			if (!IsPointAvailable(StartPoint))
			{
				Console.WriteLine("[Error] Unavailable start point at" + StartX + ", " + StartY);
				return;
			}
			Console.WriteLine("BFS: Try to find the " + TargetX + ", " + TargetY);

			IsFinded = false;

			Queue<Point> pointQueue = new Queue<Point>();

			pointQueue.Enqueue(StartPoint);

			while (pointQueue.Count != 0)
			{
				//取队列中的第一个点
				Point point = pointQueue.Dequeue();

				//标记该点
				Visited(point);

				//判断该点是否终点
				if (IsFinded = IsTarget(point))
				{
					return;
				}

				//查找该点的后续节点，并添加到队列中
				List<Point> pointNext = GetNearPoints(point);
				foreach (var item in pointNext)
				{
					pointQueue.Enqueue(item);
				}
			}
		}

		public void SearchDFS()
		{
			IsFinded = false;
			_visited = new int[SIZE, SIZE];

			if (!IsPointAvailable(StartPoint))
			{
				Console.WriteLine("[Error] Unavailable start point at" + StartX + ", " + StartY);
				return;
			}
			Console.WriteLine("DFS: Try to find the " + TargetX + ", " + TargetY);

			DFS(StartPoint);


		}
		private void DFS(Point point)
		{
			if (IsFinded)
			{
				return;
			}

			//标记该点
			Visited(point);

			//判断该点是否终点
			if (IsFinded = IsTarget(point))
			{
				return;
			}

			//查找该点的后续节点，并添加到队列中
			List<Point> pointNext = GetNearPoints(point);
			for (int i = 0; i < pointNext.Count; i++)
			{
				//递归查找该点
				DFS(pointNext[i]);
			}
		}

		private int CalculateDistance_Manhatton(Point point)
		{
			int X = point.X;
			int Y = point.Y;

			return Math.Abs(TargetX - X) + Math.Abs(TargetY - Y);
		}
		private double CalculateDistance_Euclidean(Point point)
		{
			int X = point.X;
			int Y = point.Y;

			return Math.Sqrt(Math.Pow(TargetX - X, 2) + Math.Pow(TargetY - Y, 2));
		}

		private List<Point> GetNextPoints_Weight(Point point)
		{
			int X = point.X;
			int Y = point.Y;

			List<Point> points = new List<Point>();

			if (X >= 0 && X < SIZE && Y >= 0 && Y < SIZE)
			{
				foreach (var order in ShuffleArray(directionOrder))
				{
					Point next = new Point
					(
						X + direction[order, 0],
						Y + direction[order, 1]
					);

					if (IsPointAvailable(next))
					{
						Point next_weight = new Point
						(
							next.X,
							next.Y,
							CalculateDistance_Manhatton(point)// * constK
															  //CalculateDistance_Euclidean(point) * constK
						);
						points.Add(next_weight);
						Console.WriteLine(next_weight.X + ", " + next_weight.Y + ", |" + next_weight.Weight);
					}
				}
			}

			return points;
		}

		public void SearchAStar()
		{
			IsFinded = false;
			_visited = new int[SIZE, SIZE];
			if (!IsPointAvailable(StartPoint))
			{
				Console.WriteLine("[Error] Unavailable start point at" + StartX + ", " + StartY);
				return;
			}
			Console.WriteLine("A*: Try to find the " + TargetX + ", " + TargetY);

			List<Point> pointList = new List<Point>
			{
				StartPoint
			};

			while (pointList.Count != 0)
			{
				//排序
				pointList = pointList.OrderBy(p => p.Weight).ToList();

				//取队列中的第一个点
				Point point = pointList.First();
				pointList.RemoveAt(0);

				//标记该点
				Visited(point);

				//判断该点是否终点
				if (IsFinded = IsTarget(point))
				{
					return;
				}

				//查找该点的后续节点，并添加到队列中
				pointList.AddRange(GetNextPoints_Weight(point));
			}
		}

		
	}
}
