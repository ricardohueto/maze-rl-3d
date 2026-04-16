import random


class MazeCell:
    """Represents a single cell in the maze grid."""

    def __init__(self):
        self.walls = {
            "N": True,
            "S": True,
            "E": True,
            "W": True
        }
        self.visited = False


class Maze:
    """Generates a perfect maze using the Recursive Backtracker algorithm."""

    def __init__(self, width: int, height: int, seed: int = None):
        self.width = width
        self.height = height
        self.seed = seed
        self.grid = [[MazeCell() for _ in range(width)] for _ in range(height)]
        self._generate()

    def _generate(self):
        """Runs the Recursive Backtracker algorithm to carve the maze."""
        rng = random.Random(self.seed)
        start_row = rng.randint(0, self.height - 1)
        start_col = rng.randint(0, self.width - 1)
        self._visit(start_row, start_col, rng)

    def _visit(self, row: int, col: int, rng: random.Random):
        """Recursively visits cells and carves passages."""
        self.grid[row][col].visited = True
        neighbors = self._get_unvisited_neighbors(row, col)
        rng.shuffle(neighbors)
        for neighbor_row, neighbor_col, direction in neighbors:
            if not self.grid[neighbor_row][neighbor_col].visited:
                self._remove_wall(row, col, neighbor_row, neighbor_col, direction)
                self._visit(neighbor_row, neighbor_col, rng)

    def _get_unvisited_neighbors(self, row: int, col: int) -> list:
        """Returns a list of unvisited neighbors as (row, col, direction)."""
        neighbors = []
        directions = [
            (-1, 0, "N"),
            (1,  0, "S"),
            (0,  1, "E"),
            (0, -1, "W")
        ]
        for dr, dc, direction in directions:
            new_row = row + dr
            new_col = col + dc
            if 0 <= new_row < self.height and 0 <= new_col < self.width:
                if not self.grid[new_row][new_col].visited:
                    neighbors.append((new_row, new_col, direction))
        return neighbors

    def _remove_wall(self, row: int, col: int,
                     neighbor_row: int, neighbor_col: int,
                     direction: str):
        """Removes the wall between two adjacent cells."""
        opposite = {"N": "S", "S": "N", "E": "W", "W": "E"}
        self.grid[row][col].walls[direction] = False
        self.grid[neighbor_row][neighbor_col].walls[opposite[direction]] = False