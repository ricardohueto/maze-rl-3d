from maze_generator.maze import Maze


class MazeVisualizer:
    """Renders a Maze instance as ASCII art in the terminal."""

    def __init__(self, maze: Maze):
        self.maze = maze

    def render(self) -> str:
        """Converts the maze grid into an ASCII string representation."""
        lines = []

        for row in range(self.maze.height):
            top_line = ""
            middle_line = ""

            for col in range(self.maze.width):
                cell = self.maze.grid[row][col]

                # Top wall of the cell
                top_line += "+" + ("--" if cell.walls["N"] else "  ")

                # Left wall and cell interior
                middle_line += ("|" if cell.walls["W"] else " ") + "  "

            # Close the rightmost edge
            top_line += "+"
            middle_line += "|"

            lines.append(top_line)
            lines.append(middle_line)

        # Bottom border of the entire maze
        bottom_line = ""
        for col in range(self.maze.width):
            cell = self.maze.grid[self.maze.height - 1][col]
            bottom_line += "+" + ("--" if cell.walls["S"] else "  ")
        bottom_line += "+"
        lines.append(bottom_line)

        return "\n".join(lines)

    def print(self):
        """Prints the maze to the terminal."""
        print(self.render())