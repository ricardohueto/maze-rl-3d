import heapq
from maze_generator.maze import Maze


def heuristic(cell: tuple, end: tuple) -> int:
    """Manhattan distance heuristic."""
    return abs(cell[0] - end[0]) + abs(cell[1] - end[1])


def astar(maze: Maze, start: tuple, end: tuple) -> list | None:
    """
    Solves a maze using the A* algorithm.

    Args:
        maze:  the Maze instance to solve
        start: (row, col) tuple for the starting cell
        end:   (row, col) tuple for the exit cell

    Returns:
        A list of (row, col) tuples representing the path,
        or None if no path exists.
    """
    open_set = []
    heapq.heappush(open_set, (0, [start]))
    visited = set()

    directions = {
        "N": (-1, 0),
        "S": (1,  0),
        "E": (0,  1),
        "W": (0, -1)
    }

    while open_set:
        cost, path = heapq.heappop(open_set)
        row, col = path[-1]

        if (row, col) == end:
            return path

        if (row, col) in visited:
            continue
        visited.add((row, col))

        for direction, (dr, dc) in directions.items():
            if not maze.grid[row][col].walls[direction]:
                new_row = row + dr
                new_col = col + dc
                neighbor = (new_row, new_col)
                if neighbor not in visited:
                    g = len(path)
                    h = heuristic(neighbor, end)
                    f = g + h
                    heapq.heappush(open_set, (f, path + [neighbor]))

    return None
