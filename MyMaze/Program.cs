using System;
using System.Diagnostics;

namespace AStarMaze
{
	class Program
	{
		static void Main(string[] args)
		{
			MazeGenerator generator = new MazeGenerator();

			Maze maze1 = generator.GeneratorMaze();
			maze1.ShowMaze();

			MazeSolver solver = new MazeSolver(maze1);

			solver.SetStartPoint(1, 1);
			solver.SetTargetPoint(5, 5);

			solver.SearchBFS();
			solver.SearchDFS();
			solver.SearchAStar();

			solver.SetStartPointRandom();
			solver.SetTatgetPointRandom();
			solver.SearchBFS();
			solver.SearchDFS();
			solver.SearchAStar();



		}
	}
}
