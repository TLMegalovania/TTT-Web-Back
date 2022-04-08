namespace TTTService;

internal class GoBangBoard
{
    private readonly GoBangTurnType[,] _board;

    /// <summary>
    /// 用户看到的row。
    /// </summary>
    public int Row { get; }

    /// <summary>
    /// 用户看到的column。
    /// </summary>
    public int Column { get; }

    public GoBangTurnType NextTurnType { get; private set; }

    /// <summary>
    /// 传入用户看到的行列数。
    /// 实际应使用下标为 2 until n+2 。
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public GoBangBoard(int row, int column)
    {
        if (row <= 0) throw new ArgumentOutOfRangeException(nameof(row));
        if (column <= 0) throw new ArgumentOutOfRangeException(nameof(column));
        Row = row;
        Column = column;
        //多开4行列为了判断好写点
        row += 4;
        column += 4;
        _board = new GoBangTurnType[row,column];
        NextTurnType = GoBangTurnType.Black;
    }

    /// <summary>
    /// 实际应使用下标为 2 until n+2 。
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    public GoBangTurnType this[int i, int j] => _board[i,j];

    //public GoBangTurnType this[Index x, Index y] => _board[x][y];
    //public GoBangTurnType[][] this[Range x, Range y] => _board[x][y];

    /// <summary>
    /// 记得转换为 2 until n+2 。
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns>Move successfully.</returns>
    public bool Move(int row, int column)
    {
        if (row >= Row + 2 || row <= 1 || column >= Column + 2 || column <= 1 || _board[row,column] != GoBangTurnType.Null) return false;
        _board[row,column] = NextTurnType;
        NextTurnType = NextTurnType.Opposite();
        return true;
    }
}

