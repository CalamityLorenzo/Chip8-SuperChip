using Chip8.Interpreter.V1;
using System.Text;

Console.WriteLine("Hello, World!");

Console.SetBufferSize(500, 500);

var chip = new Chip8Interpreter(1, ReadConsoleKey);

bool[] copyDisplay = new bool[64 * 32];
chip.Drawing += HandleDrawingEvent2;


var instructions = new byte[]
{
    0x00, 0xE0,
    0x61, 0xF,
    0x62, 0xA,

    0xA0, 0x00,
    0xD1, 0x25,

    0xA0, 0x05,
    0x61, 0x14,
    0x62, 0xA,
    0xD1, 0x25,
};
var ibmBits = File.ReadAllBytes("progs/IBM Logo Hack.ch8");

try
{

chip.Interpreter(ibmBits);
}
catch(Exception ex)
{
    ;
}

for (var y = 0; y < 32; ++y)
{
    var row = y * 64;
    var displayChunk = copyDisplay[row..(row + 64)];
    var result = string.Join("", displayChunk.Select(a => a ? '*' : ' '));
    Console.WriteLine(result + "n");
}

StringBuilder sb = new StringBuilder();
for(var x = 0; x < copyDisplay.Length; ++x)
{
    if (copyDisplay[x])
        sb.AppendLine($"chip8board[{x}]=true;");
}

File.WriteAllText("../../../boolBits.txt", sb.ToString());


byte? ReadConsoleKey()
{
    var chip8Keyboard = new Dictionary<char, byte>
    {
        {'1', 1 },
        {'2', 2 },
        {'3', 3 },
        {'4', 0xC },
        {'Q', 4 },
        {'W', 5 },
        {'E', 6 },
        {'R', 0xD },
        {'A', 7 },
        {'S', 8 },
        {'D', 9 },
        {'F', 0xE },
        {'Z', 0xA },
        {'X', 0x0 },
        {'C', 0xB },
        {'V', 0xF },
    };

    var keyChar = Console.ReadKey(true).KeyChar;
    return chip8Keyboard.ContainsKey(keyChar) ? chip8Keyboard[keyChar] : null;

}


void HandleDrawingEvent(object sender, Chip8DrawingEventArgs args)
{
    var truth = args.Display.Select(a => a == true).Count();
    var acc = 0;
    for (var x = 0; x < args.Display.Length; ++x)
    {
        if (args.Display[x] == true)
            acc++;
    }
    args.Display.CopyTo(copyDisplay, 0);
    Console.WriteLine(acc);
}

void HandleDrawingEvent2(object sender, Chip8DrawingEventArgs args)
{
    var truth = args.Display.Select(a => a == true).Count();
    var acc = 0;
    for (var x = 0; x < args.Display.Length; ++x)
    {
        if (args.Display[x] == true)
            acc++;
    }
    args.Display.CopyTo(copyDisplay, 0);

    Console.Clear();
    Console.SetCursorPosition(0, 100);
    Console.WriteLine(acc);
    for (var y = 0; y < 32; ++y)
    {
        for (var x = 0; x < 64; ++x)
        {
            Console.SetCursorPosition(x, y);
            var s = x + (y * 64);
            Console.Write(copyDisplay[s] ? '*' : ' ');
        }
        //Console.WriteLine(s);
        //Console.WriteLine();
    }

    //Console.WriteLine(acc);
}