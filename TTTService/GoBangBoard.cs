namespace TTTService;

public class GoBangBoard
{
    private readonly GoBangTurnType[][] _board;

    /// <summary>
    /// 用户看到的row。
    /// </summary>
    public byte Row { get; }

    /// <summary>
    /// 用户看到的column。
    /// </summary>
    public byte Column { get; }

    public GoBangTurnType NextTurnType { get; private set; }

    /// <summary>
    /// 传入用户看到的行列数。
    /// 实际应使用下标为 2 until n+2 。
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public GoBangBoard(byte row, byte column, GoBangTurnType nextTurn = GoBangTurnType.Black)
    {
        Row = row;
        Column = column;
        //多开4行列为了判断好写点
        row += 4;
        column += 4;
        _board = new GoBangTurnType[row][];
        for (int i = 0; i < row; i++)
        {
            _board[i] = new GoBangTurnType[column];
        }
        NextTurnType = nextTurn;
    }

    /// <summary>
    /// 实际应使用下标为 2 until n+2 。
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    //public GoBangTurnType this[int i, int j] => _board[i][j];

    //public GoBangTurnType this[Index x, Index y] => _board[x][y];
    //public GoBangTurnType[][] this[Range x, Range y] => _board[x][y];

    /// <summary>
    /// 记得转换为 2 until n+2 。
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns>Move successfully.</returns>
    private bool Move(byte row, byte column)
    {
        if (row >= Row + 2 || row <= 1 || column >= Column + 2 || column <= 1 || _board[row][column] != GoBangTurnType.Null) return false;
        _board[row][column] = NextTurnType;
        NextTurnType = NextTurnType.Opposite();
        return true;
    }

    public IEnumerable<IEnumerable<GoBangTurnType>> GetBoard() => _board;

    public byte[] Serialize()
    {
        var bytes = new byte[Row * Column + 3];
        bytes[0] = (byte)Row;
        bytes[1] = (byte)Column;
        bytes[2] = (byte)NextTurnType;
        for (int i = 0; i < Row; i++)
        {
            for (int j = 0; j < Column; j++)
            {
                bytes[i * Column + j + 3] = (byte)_board[i + 2][j + 2];
            }
        }
        return bytes;
    }

    public static GoBangBoard Deserialize(byte[] bytes)
    {
        var row = bytes[0];
        var column = bytes[1];
        var nextTurnType = (GoBangTurnType)bytes[2];
        var board = new GoBangBoard(row, column, nextTurnType);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                board._board[i + 2][j + 2] = (GoBangTurnType)bytes[i * column + j + 3];
            }
        }
        return board;
    }

    private static readonly int[,,,] _directions = new int[,,,]
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

    /// <summary>
    /// 返回胜利方。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="board"></param>
    /// <returns>Winning side.</returns>
    public GoBangTurnType? Judge(byte x, byte y)
    {
        x += 2;
        y += 2;
        if (!Move(x, y)) return null;
        var turn = _board[x][y];
        int lines = 0;
        for (int direction = 0; direction < 4; direction++)
        {
            bool isLine = false;
            for (int condition = 0; condition < 3; condition++)
            {
                int goodSteps = 0;
                for (int step = 0; step < 2; step++)
                    if (_board[_directions[direction, condition, step, 0] + x]
                    [_directions[direction, condition, step, 1] + y] == turn)
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
            for (int row = 2; row <= Row + 1; row++)
            {
                for (int column = 2; column <= Column + 1; column++)
                {
                    if (_board[row][column] == GoBangTurnType.Null) return GoBangTurnType.Null;
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
}

