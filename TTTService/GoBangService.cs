namespace TTTService;

public class GoBangService
{
    private readonly int[,,,] _directions;
    private readonly GoBangBoard board;
    public GoBangService(int row, int column) : this(new(row, column)) { }
    private GoBangService(GoBangBoard board)
    {
        _directions = new int[,,,]
        {
                //竖
                {
                    {{-1, 0}, {1, 0}},
                    {{-2, 0}, {-1, 0}},
                    {{1, 0}, {2, 0}}
                },
                //横
                {
                    {{0, -1}, {0, 1}},
                    {{0, -2}, {0, -1}},
                    {{0, 1}, {0, 2}}
                },
                //斜杠
                {
                    {{1, -1}, {-1, 1}},
                    {{2, -2}, {1, -1}},
                    {{-1, 1}, {-2, 2}}
                },
                //反斜杠
                {
                    {{-1, -1}, {1, 1}},
                    {{-2, -2}, {-1, -1}},
                    {{1, 1}, {2, 2}}
                }
        };
        this.board = board;
    }

    /// <summary>
    /// 返回胜利方。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="board"></param>
    /// <returns>Winning side.</returns>
    public GoBangTurnType? Judge(int x, int y)
    {
        x += 2;
        y += 2;
        if (!board.Move(x, y)) return null;
        var turn = board[x, y];
        int lines = 0;
        for (int direction = 0; direction < 4; direction++)
        {
            bool isLine = false;
            for (int condition = 0; condition < 3; condition++)
            {
                int goodSteps = 0;
                for (int step = 0; step < 2; step++)
                    if (board[_directions[direction, condition, step, 0] + x,
                        _directions[direction, condition, step, 1] + y] == turn)
                        goodSteps++;
                if (goodSteps == 2)
                {
                    isLine = true;
                    break;
                }
            }

            if (isLine && ++lines >= 2)
                break;
        }

        if (lines == 0)
        {
            /// TODO : refactor
            for (int row = 2; row <= board.Row + 1; row++)
            {
                for (int column = 2; column <= board.Column + 1; column++)
                {
                    if (board[row, column] == GoBangTurnType.Null) return GoBangTurnType.Null;
                }
            }
            return GoBangTurnType.Tie;
        }
        else if (lines == 1)
        {
            return turn.Opposite();
        }
        return turn;
    }

    public IEnumerable<IEnumerable<GoBangTurnType>> GetBoard() => board.GetBoard();

    public GoBangTurnType NextTurnType => board.NextTurnType;
}

