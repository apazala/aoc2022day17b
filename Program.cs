internal class Program
{
    const int CHAMBER_WIDTH = 7;
    class Rock
    {
        int h;
        public int H { get => h; }
        int w;
        public int W { get => w; }
        byte[][] shapeX;

        public Rock(byte[] shape)
        {
            h = shape.Length;
            w = ComputeWidth(shape);

            int xmax = CHAMBER_WIDTH - w + 1;
            shapeX = new byte[xmax][];
            for (int x = 0; x < xmax; x++)
            {
                shapeX[x] = new byte[this.h];
                for (int y = 0; y < h; y++)
                {
                    shapeX[x][y] = (byte)(shape[y] << x);
                }
            }
        }

        private static int ComputeWidth(byte[] shape)
        {
            int maxW = 0;
            for (int y = 0; y < shape.Length; y++)
            {
                int thisW = 0;
                for (int x = 0; x < CHAMBER_WIDTH; x++)
                {
                    if (((1 << x) & shape[y]) != 0)
                        thisW = x + 1;
                }
                if (thisW > maxW)
                    maxW = thisW;
            }
            return maxW;
        }

        public byte[]? GetXShape(int x)
        {
            if (x < 0 || x >= shapeX.Length)
                return null;
            return shapeX[x];
        }
    }

    static List<Rock> rocks;
    private static void Init()
    {
        rocks = new List<Rock>();

        //-
        rocks.Add(new Rock(new[] { (byte)0b1111 }));

        //+
        rocks.Add(new Rock(new[] { (byte)0b010, (byte)0b111, (byte)0b010 }));

        //L -> byte 0 is rightmost: inverted shape
        rocks.Add(new Rock(new[] { (byte)0b111, (byte)0b100, (byte)0b100 }));

        //|
        rocks.Add(new Rock(new[] { (byte)0b1, (byte)0b1, (byte)0b1, (byte)0b1 }));

        //square
        rocks.Add(new Rock(new[] { (byte)0b11, (byte)0b11 }));

    }

    private static int rockInd;
    private static int dirInd;

    private static int highestRock;

    private static List<byte> chamber = new List<byte>();

    private static string directions = LoadDirections("input1.txt");
    private static void FallNextPiece()
    {
        Rock currRock = rocks[rockInd];
        rockInd = (rockInd == rocks.Count - 1 ? 0 : rockInd + 1);
        int x = 2;
        int y = highestRock + 4;
        while (y + currRock.H > chamber.Count)
            chamber.Add(0);

        bool stillFalling = true;
        while (stillFalling)
        {
            char dir = directions[dirInd];
            dirInd = (dirInd == directions.Length - 1 ? 0 : dirInd + 1);
            if (dir == '>')
            {
                if (ValidPos(currRock, chamber, x + 1, y))
                    x++;

            }
            else
            {
                if (ValidPos(currRock, chamber, x - 1, y))
                    x--;
            }
            stillFalling = ValidFallAndInsertIfNot(currRock, chamber, x, ref y);
        }

        y = y + currRock.H - 1;
        if (y > highestRock)
            highestRock = y;
    }

    private static void Reset()
    {
        chamber.Clear();
        highestRock = -1;
        rockInd = 0;
        dirInd = 0;
    }
    private static void Main(string[] args)
    {
        int target = 18029;
        Init();
        Reset();

        for (int i = 0; i < target; i++)
        {
            FallNextPiece();
        }

        Console.WriteLine(highestRock + 1);

        ulong eightLines = 0;
        for (int i = highestRock / 2, limit = i + 8; i < limit; i++)
            eightLines = (eightLines << 8) | chamber[i];

        ulong matchEight = 0;
        List<int> matchheightsList = new List<int>();
        int cycleLen = -1, prevDist = -1, prevMatch = 0;
        bool cycleFound = false;
        for (int i = 0; !cycleFound && i < chamber.Count; i++)
        {
            matchEight = (matchEight << 8) | chamber[i];
            if (matchEight == eightLines)
            {
                matchheightsList.Add(i);
                prevDist = cycleLen;
                cycleLen = i - prevMatch;
                prevMatch = i;
                //Console.WriteLine(cycleLen);
                cycleFound = (cycleLen == prevDist);
            }
        }

        if (!cycleFound)
        {
            Console.WriteLine("NO CYCLE FOUND YET :(. Maybe we need more lines");
            return;
        }

        //Cicle found! Several assumptions: answer may be a bit off, we need a little luck      

        Reset();
        int nPieces = 0;
        int[] nPiecesMatch = new int[2];
        for (int k = 0; k < 2; k++)
        {
            for (; highestRock < matchheightsList[k]; nPieces++)
            {
                FallNextPiece();
            }

            eightLines = EightLines(matchheightsList[k]);
            for (; eightLines != matchEight; nPieces++)
            {
                FallNextPiece();
                eightLines = EightLines(matchheightsList[k]);
            }
            nPiecesMatch[k] = nPieces;
        }

        int nPiecesCycle = nPiecesMatch[1]-nPiecesMatch[0];
        int remainingPieces =(int) ((1_000_000_000_000L - nPiecesMatch[0])%nPiecesCycle);
        int currHeight = highestRock;
        for(int k = 0; k < remainingPieces; k++)
        {
            FallNextPiece();
        }

        long answer = matchheightsList[0]+1;
        answer += ((1_000_000_000_000L - nPiecesMatch[0])/nPiecesCycle)*cycleLen;
        answer += highestRock-currHeight+1;
        //PlotChamber(chamber);
        //Console.WriteLine(highestRock+1);
        Console.WriteLine(answer+1);
    }

    private static ulong EightLines(int ip7)
    {
        ulong eightLines = 0;
        for (int i = ip7 - 7; i <= ip7; i++)
        {
            eightLines = (eightLines << 8) | chamber[i];
        }
        return eightLines;
    }

    private static void PlotChamber(List<byte> chamber)
    {
        for (int i = chamber.Count - 1; i >= 0; i--)
        {
            int line = chamber[i];
            Console.Write('|');
            for (int j = 0; j < CHAMBER_WIDTH; j++)
            {
                Console.Write(((line & (1 << j)) == 0 ? '.' : '#'));
            }
            Console.WriteLine('|');
        }
        Console.Write('+');
        for (int j = 0; j < CHAMBER_WIDTH; j++)
        {
            Console.Write('-');
        }

        Console.WriteLine('+');
    }

    private static bool ValidFallAndInsertIfNot(Rock currRock, List<byte> chamber, int x, ref int y)
    {
        y--;
        bool valid = (y >= 0);
        byte[]? shape = currRock.GetXShape(x);
        for (int i = 0; valid && i < shape.Length; i++)
        {
            valid = ((chamber[y + i] & shape[i]) == 0);
        }
        if (!valid) //Insert
        {
            y++;
            for (int i = shape.Length - 1; i >= 0; i--)
            {
                chamber[y + i] |= shape[i];
            }
        }
        return valid;
    }

    private static bool ValidPos(Rock currRock, List<byte> chamber, int x, int y)
    {
        if (y < 0) return false;
        byte[]? shape = currRock.GetXShape(x);
        if (shape == null) return false;
        bool valid = true;
        for (int i = 0; valid && i < shape.Length; i++)
        {
            valid = ((chamber[y + i] & shape[i]) == 0);
        }
        return valid;
    }

    private static string LoadDirections(string filename)
    {
        string[] lines = File.ReadAllLines(filename);
        return lines[0];
    }
}