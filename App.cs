using System;

public class App
{
    public static void Main(string[] args)
    {
        int width = 40;
        int height = 15;
        int cursorX = 0;
        int cursorY = 0;

        GridDeliveryPathManager manager = new GridDeliveryPathManager();

        Console.Clear();
        Console.CursorVisible = false;
        bool done = false;
        while (!done) {
            Console.SetCursorPosition(0, 0);
            Console.Write($"({cursorX}, {cursorY}) - Arrows=Move - q=Quit - x=Delete");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($" - f=FuelPump - p=Pipe");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" - m=Mine - c=Conveyor");
            Console.ResetColor();

            List<GridDeliveryPathAttachablePart> paths = manager.GetAllPaths();
            int pathNum = 0;
            foreach (GridDeliveryPathAttachablePart path in paths) {
                // Console.SetCursorPosition(width + 1, pathNum + 1);
                // Console.Write(path);
                GridDeliveryPathAttachablePart? part = path;
                while (part != null) {
                    if (part.Part != null && part.Part is IDrawable) {
                        IDrawable drawable = (IDrawable)part.Part;
                        drawable.Draw(0, 1);
                    }
                    part = part.NextPart;
                }
                pathNum++;
            }

            List<IGridDeliveryPathSource> sources = manager.GetAllSources();
            foreach (IGridDeliveryPathSource source in sources) {
                if (source != null && source is IDrawable) {
                    IDrawable drawable = (IDrawable)source;
                    drawable.Draw(0, 1);
                }
            }

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(cursorX, cursorY + 1);
            Console.Write("X");
            Console.ResetColor();

            ConsoleKeyInfo key = Console.ReadKey();
            Console.Clear();
            Console.SetCursorPosition(0, height + 1);
            if (key.KeyChar == 'q') {
                done = true;
            } else if (key.Key == ConsoleKey.UpArrow && cursorY > 0) {
                cursorY -= 1;
            } else if (key.Key == ConsoleKey.DownArrow && cursorY < height - 1) {
                cursorY += 1;
            } else if (key.Key == ConsoleKey.LeftArrow && cursorX > 0) {
                cursorX -= 1;
            } else if (key.Key == ConsoleKey.RightArrow && cursorX < width - 1) {
                cursorX += 1;
            } else if (key.KeyChar == 'x') {
                manager.RemovePart(new Vector2Int(cursorX, cursorY));
                manager.RemoveSource(new Vector2Int(cursorX, cursorY));
            } else if (key.KeyChar == 'f') {
                FuelPump fuelPump = new FuelPump(new Vector2Int(cursorX, cursorY));
                manager.AddSource(fuelPump);
            } else if (key.KeyChar == 'p') {
                Pipe pipe = new Pipe(new Vector2Int(cursorX, cursorY));
                manager.AddPart(pipe);
            } else if (key.KeyChar == 'm') {
                Mine mine = new Mine(new Vector2Int(cursorX, cursorY));
                manager.AddSource(mine);
            } else if (key.KeyChar == 'c') {
                Conveyor conveyor = new Conveyor(new Vector2Int(cursorX, cursorY));
                manager.AddPart(conveyor);
            }
        }
    }
}

interface IDrawable {
    void Draw(int x, int y);
}

public class Pipe : IDrawable, IGridDeliveryPathPart
{
    private static Dictionary<string, string> Icons = new Dictionary<string, string> {
        { "--", "═" },

        { "U-", "║" },
        { "D-", "║" },
        { "L-", "═" },
        { "R-", "═" },

        { "UD", "║" },
        { "UL", "╝" },
        { "UR", "╚" },

        { "DU", "║" },
        { "DL", "╗" },
        { "DR", "╔" },

        { "LU", "╝" },
        { "LR", "═" },
        { "LD", "╗" },

        { "RU", "╚" },
        { "RL", "═" },
        { "RD", "╔" },

        { "-U", "║" },
        { "-D", "║" },
        { "-L", "═" },
        { "-R", "═" }
    };

    public string VisualType;
    public bool Active;
    public bool Flipped;
    public string Icon;

    public int GridDeliveryPathGroupType { get; } = 1; // In the Pipe group
    public HashSet<int> AttachableGridDelivaryPathSourceGroupTypes { get; } = [1]; // Can only attach to a Fuel Pump
    public Vector2Int GridCell { set; get; }

    public Pipe(Vector2Int gridCell)
    {
        GridCell = gridCell;
        VisualType = "";
        Active = false;
        Flipped = false;
        Icon = Icons.Values.First();
    }

    public void GridDeliveryPathPartUpdateVisualType(string visualType)
    {
        VisualType = visualType;
        Icon = Icons[VisualType];
    }

    public void GridDeliveryPathPartUpdateActive(bool active)
    {
        Active = active;
    }

    public void GridDeliveryPathPartUpdateFlipped(bool flipped)
    {
        Flipped = flipped;
    }

    public void Draw(int x, int y)
    {
        Console.SetCursorPosition(GridCell.x + x, GridCell.y + y);
        if (VisualType.StartsWith("-"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (VisualType.EndsWith("-"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Blue;
        }
        if (Active) {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
        } else {
            Console.BackgroundColor = ConsoleColor.Black;
        }
        Console.Write(Icon);
        Console.ResetColor();
    }
}

public class FuelPump : IDrawable, IGridDeliveryPathSource
{
    private static string Icon = "F";
    public bool Attached;

    public int GridDeliveryPathSourceGroupType { get; } = 1; // In the Fuel Pump group
    public GridDeliveryPathAttachablePart? GridDeliveryPathPart { set; get; }
    public Vector2Int GridCell { set; get; }

    public FuelPump(Vector2Int gridCell)
    {
        GridCell = gridCell;
        Attached = false;
    }

    public void GridDeliveryPathSourceUpdateAttached(bool attached)
    {
        Attached = attached;
    }

    public void Draw(int x, int y)
    {
        Console.SetCursorPosition(GridCell.x + x, GridCell.y + y);
        Console.ForegroundColor = ConsoleColor.Gray;
        if (Attached) {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
        } else {
            Console.BackgroundColor = ConsoleColor.Black;
        }
        Console.Write(Icon);
        Console.ResetColor();
    }
}

public class Conveyor : IDrawable, IGridDeliveryPathPart
{
    private static Dictionary<string, string> Icons = new Dictionary<string, string> {
        { "--", "═" },

        { "U-", "║" },
        { "D-", "║" },
        { "L-", "═" },
        { "R-", "═" },

        { "UD", "║" },
        { "UL", "╝" },
        { "UR", "╚" },

        { "DU", "║" },
        { "DL", "╗" },
        { "DR", "╔" },

        { "LU", "╝" },
        { "LR", "═" },
        { "LD", "╗" },

        { "RU", "╚" },
        { "RL", "═" },
        { "RD", "╔" },

        { "-U", "║" },
        { "-D", "║" },
        { "-L", "═" },
        { "-R", "═" }
    };

    public string VisualType;
    public bool Active;
    public bool Flipped;
    public string Icon;

    public int GridDeliveryPathGroupType { get; } = 2; // In the Conveyor group
    public HashSet<int> AttachableGridDelivaryPathSourceGroupTypes { get; } = [2]; // Can only attach to a Mine
    public Vector2Int GridCell { set; get; }

    public Conveyor(Vector2Int gridCell)
    {
        GridCell = gridCell;
        VisualType = "";
        Active = false;
        Flipped = false;
        Icon = Icons.Values.First();
    }

    public void GridDeliveryPathPartUpdateVisualType(string visualType)
    {
        VisualType = visualType;
        Icon = Icons[VisualType];
    }

    public void GridDeliveryPathPartUpdateActive(bool active)
    {
        Active = active;
    }

    public void GridDeliveryPathPartUpdateFlipped(bool flipped)
    {
        Flipped = flipped;
    }

    public void Draw(int x, int y)
    {
        Console.SetCursorPosition(GridCell.x + x, GridCell.y + y);
        if (VisualType.StartsWith("-"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (VisualType.EndsWith("-"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        if (Active) {
            Console.BackgroundColor = ConsoleColor.Green;
        } else {
            Console.BackgroundColor = ConsoleColor.Black;
        }
        Console.Write(Icon);
        Console.ResetColor();
    }
}

public class Mine : IDrawable, IGridDeliveryPathSource
{
    private static string Icon = "M";
    public bool Attached;

    public int GridDeliveryPathSourceGroupType { get; } = 2; // In the Mine group
    public GridDeliveryPathAttachablePart? GridDeliveryPathPart { set; get; }
    public Vector2Int GridCell { set; get; }

    public Mine(Vector2Int gridCell)
    {
        GridCell = gridCell;
    }

    public void GridDeliveryPathSourceUpdateAttached(bool attached)
    {
        Attached = attached;
    }

    public void Draw(int x, int y)
    {
        Console.SetCursorPosition(GridCell.x + x, GridCell.y + y);
        Console.ForegroundColor = ConsoleColor.Gray;
        if (Attached) {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
        } else {
            Console.BackgroundColor = ConsoleColor.Black;
        }
        Console.Write(Icon);
        Console.ResetColor();
    }
}