namespace Chip8.Interpreter.V1;
internal class Chip8Timer
{
    private DateTime startTime;
    private readonly int millisecondDiff;
    private DateTime previousTime;

    public bool Ticked { get; private set; }

    public Chip8Timer(DateTime dateTime, int milliseconds)
    {
        this.startTime = dateTime;
        this.millisecondDiff = milliseconds;
        this.previousTime = startTime;
    }

    public void Update()
    {
        var currentTime = DateTime.Now;
        var diff = currentTime - previousTime;

        this.Ticked = diff.TotalMilliseconds >= millisecondDiff ? true : false;
        if (Ticked)
        {
            previousTime = currentTime;
        }
    }
}

