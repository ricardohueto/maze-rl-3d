from collections import deque
from maze_generator.maze import Maze


def bfs(maze: Maze, start: tuple, end: tuple) -> list | None:
    """
    Solves a maze using Breadth-First Search.

    Args:
        maze:  the Maze instance to solve
        start: (row, col) tuple for the starting cell
        end:   (row, col) tuple for the exit cell

    Returns:
        A list of (row, col) tuples representing the path,
        or None if no path exists.
    """
    queue = deque()
    queue.append([start])
    visited = set()
    visited.add(start)

    directions = {
        "N": (-1, 0),
        "S": (1,  0),
        "E": (0,  1),
        "W": (0, -1)
    }

    while queue:
        path = queue.popleft()
        row, col = path[-1]

        if (row, col) == end:
            return path

        for direction, (dr, dc) in directions.items():
            if not maze.grid[row][col].walls[direction]:
                new_row = row + dr
                new_col = col + dc
                neighbor = (new_row, new_col)
                if neighbor not in visited:
                    visited.add(neighbor)
                    queue.append(path + [neighbor])

    return None