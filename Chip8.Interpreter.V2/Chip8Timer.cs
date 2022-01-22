namespace Chip8.Interpreter.V2;
internal class Chip8Timer
{
    private DateTime startTime;
    private readonly int millisecondDiff;
    private DateTime previousTime;

    private bool ticked { get; set; }

    public Chip8Timer(DateTime dateTime, int milliseconds)
    {
        this.startTime = dateTime;
        this.millisecondDiff = milliseconds;
        this.previousTime = startTime;
    }

    public bool GetTicked()
    {
        var tmp = ticked;
        //if (ticked == true) ticked = false;
        return tmp;
    }

    public void Update(DateTime current)
    {
        var currentTime = current;
        if (millisecondDiff > 0){

            var diff = currentTime - previousTime;
            this.ticked = diff.TotalMilliseconds >= millisecondDiff ? true : false;
            if (ticked)
            {
                previousTime = currentTime;
            }
        }
        else
        {
            this.ticked = true;
            previousTime = currentTime;

        }
    }
}

