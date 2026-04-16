import pytest
from maze_generator.maze import Maze
from maze_generator.solvers.bfs import bfs
from maze_generator.solvers.astar import astar


class TestMazeDimensions:
    """Tests that verify the maze is created with correct dimensions."""

    def test_maze_has_correct_width(self):
        maze = Maze(width=10, height=5, seed=1)
        assert maze.width == 10

    def test_maze_has_correct_height(self):
        maze = Maze(width=10, height=5, seed=1)
        assert maze.height == 5

    def test_maze_grid_has_correct_number_of_rows(self):
        maze = Maze(width=10, height=5, seed=1)
        assert len(maze.grid) == 5

    def test_maze_grid_has_correct_number_of_cols(self):
        maze = Maze(width=10, height=5, seed=1)
        assert len(maze.grid[0]) == 10

class TestMazeGeneration:
    """Tests that verify the maze is correctly generated."""

    def test_all_cells_are_visited(self):
        maze = Maze(width=10, height=10, seed=1)
        for row in range(maze.height):
            for col in range(maze.width):
                assert maze.grid[row][col].visited, \
                    f"Cell ({row}, {col}) was not visited"

    def test_walls_are_symmetric(self):
        maze = Maze(width=10, height=10, seed=1)
        opposite = {"N": "S", "S": "N", "E": "W", "W": "E"}
        directions = [
            (-1, 0, "N"),
            (1,  0, "S"),
            (0,  1, "E"),
            (0, -1, "W")
        ]
        for row in range(maze.height):
            for col in range(maze.width):
                for dr, dc, direction in directions:
                    new_row = row + dr
                    new_col = col + dc
                    if 0 <= new_row < maze.height and 0 <= new_col < maze.width:
                        cell_wall = maze.grid[row][col].walls[direction]
                        neighbor_wall = maze.grid[new_row][new_col].walls[opposite[direction]]
                        assert cell_wall == neighbor_wall, \
                            f"Wall mismatch at ({row},{col}) direction {direction}"

    def test_border_walls_always_exist(self):
        maze = Maze(width=10, height=10, seed=1)
        for col in range(maze.width):
            assert maze.grid[0][col].walls["N"], "Top border wall missing"
            assert maze.grid[maze.height - 1][col].walls["S"], "Bottom border wall missing"
        for row in range(maze.height):
            assert maze.grid[row][0].walls["W"], "Left border wall missing"
            assert maze.grid[row][maze.width - 1].walls["E"], "Right border wall missing"


class TestMazeReproducibility:
    """Tests that verify seed-based reproducibility."""

    def test_same_seed_produces_same_maze(self):
        maze1 = Maze(width=10, height=10, seed=42)
        maze2 = Maze(width=10, height=10, seed=42)
        for row in range(maze1.height):
            for col in range(maze1.width):
                assert maze1.grid[row][col].walls == maze2.grid[row][col].walls, \
                    f"Mismatch at ({row},{col}) with same seed"

    def test_different_seeds_produce_different_mazes(self):
        maze1 = Maze(width=10, height=10, seed=42)
        maze2 = Maze(width=10, height=10, seed=99)
        differences = 0
        for row in range(maze1.height):
            for col in range(maze1.width):
                if maze1.grid[row][col].walls != maze2.grid[row][col].walls:
                    differences += 1
        assert differences > 0, "Different seeds produced identical mazes"


class TestMazeEdgeCases:
    """Tests for edge cases and boundary conditions."""

    def test_minimal_maze_1x1(self):
        maze = Maze(width=1, height=1, seed=1)
        assert maze.grid[0][0].visited

    def test_single_row_maze(self):
        maze = Maze(width=5, height=1, seed=1)
        assert len(maze.grid) == 1
        assert len(maze.grid[0]) == 5

    def test_single_col_maze(self):
        maze = Maze(width=1, height=5, seed=1)
        assert len(maze.grid) == 5
        assert len(maze.grid[0]) == 1

#TEST SOLVERS
class TestBFS:
    """Tests for the BFS solver."""

    def test_bfs_finds_path(self):
        maze = Maze(width=10, height=10, seed=42)
        path = bfs(maze, (0, 0), (9, 9))
        assert path is not None

    def test_bfs_path_starts_at_start(self):
        maze = Maze(width=10, height=10, seed=42)
        path = bfs(maze, (0, 0), (9, 9))
        assert path[0] == (0, 0)

    def test_bfs_path_ends_at_end(self):
        maze = Maze(width=10, height=10, seed=42)
        path = bfs(maze, (0, 0), (9, 9))
        assert path[-1] == (9, 9)

    def test_bfs_path_is_continuous(self):
        maze = Maze(width=10, height=10, seed=42)
        path = bfs(maze, (0, 0), (9, 9))
        for i in range(len(path) - 1):
            row1, col1 = path[i]
            row2, col2 = path[i + 1]
            assert abs(row1 - row2) + abs(col1 - col2) == 1, \
                f"Path jump detected between {path[i]} and {path[i+1]}"

    def test_bfs_finds_shortest_path(self):
        maze = Maze(width=10, height=10, seed=42)
        path_bfs = bfs(maze, (0, 0), (9, 9))
        path_astar = astar(maze, (0, 0), (9, 9))
        assert len(path_bfs) == len(path_astar)


class TestAstar:
    """Tests for the A* solver."""

    def test_astar_finds_path(self):
        maze = Maze(width=10, height=10, seed=42)
        path = astar(maze, (0, 0), (9, 9))
        assert path is not None

    def test_astar_path_starts_at_start(self):
        maze = Maze(width=10, height=10, seed=42)
        path = astar(maze, (0, 0), (9, 9))
        assert path[0] == (0, 0)

    def test_astar_path_ends_at_end(self):
        maze = Maze(width=10, height=10, seed=42)
        path = astar(maze, (0, 0), (9, 9))
        assert path[-1] == (9, 9)

    def test_astar_path_is_continuous(self):
        maze = Maze(width=10, height=10, seed=42)
        path = astar(maze, (0, 0), (9, 9))
        for i in range(len(path) - 1):
            row1, col1 = path[i]
            row2, col2 = path[i + 1]
            assert abs(row1 - row2) + abs(col1 - col2) == 1, \
                f"Path jump detected between {path[i]} and {path[i+1]}"

    def test_astar_same_seed_same_path(self):
        maze1 = Maze(width=10, height=10, seed=42)
        maze2 = Maze(width=10, height=10, seed=42)
        path1 = astar(maze1, (0, 0), (9, 9))
        path2 = astar(maze2, (0, 0), (9, 9))
        assert path1 == path2
