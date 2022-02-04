using System.Diagnostics;

namespace SuperChip11Interpreter.V3;
internal class Chip8Timer
{
    private DateTime startTime;
    private readonly int millisecondDiff;
    private DateTime previousTime;

    public string Name { get; }
    private bool ticked { get; set; }

    public Chip8Timer(DateTime dateTime, int milliseconds, string name ="")
    {
        this.startTime = dateTime;
        this.millisecondDiff = milliseconds;
        this.previousTime = startTime;
        this.Name = name;
    }

    public bool GetTicked()
    {
        var tmp = ticked;
        //if (ticked == true) ticked = false;
        Debug.WriteLine($"{Name} : {tmp}");
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

