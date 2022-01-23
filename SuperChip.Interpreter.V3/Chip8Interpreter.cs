﻿using System.Diagnostics;

namespace SuperChip11Interpreter.V3;

public partial class Chip8Interpreter
{

    public event EventHandler<Chip8DrawingEventArgs> Drawing;
    public event EventHandler SoundOn;
    public event EventHandler SoundOff;

    private Random randomGenerator = new Random();
    private Dictionary<char, byte> keyMappings;
    private readonly int instructionsPerSecond;
    int IndexRegister = 0;
    byte[] Memory = new byte[4096]; // This is 12 bits.
    byte[] RPL = new byte[7];
    Dictionary<byte, byte> Registers = new Dictionary<byte, byte>(16);
    Stack<ushort> Stack = new Stack<ushort>();
    public bool[] Display = new bool[128 * 64];
    ushort ProgramCounter = 0;

    byte DelayTimer = 0;
    byte SoundTimer = 0;
    private Chip8Timer instructionTimer;
    private Chip8Timer sixtyHertzTimer;

    public byte? CurrentKey { get; private set; }

    public ChipMachineOptions options { get; private set; }
    public bool ExtendedScreenMode { get; private set; }

    public Chip8Interpreter(int instructionsPerSecond, bool enableLoadQuirks, bool enableShiftquirks, bool enableJumpQuirk)
    {
        var startTime = DateTime.Now;
        int millisecondsToNextInstruction = (int)Math.Abs(1000f / instructionsPerSecond);
        int sixtyCycleHum = (int)Math.Abs(1000f / 60);
        this.instructionTimer = new Chip8Timer(startTime, millisecondsToNextInstruction);
        this.sixtyHertzTimer = new Chip8Timer(startTime, sixtyCycleHum);
        this.instructionsPerSecond = instructionsPerSecond;

        this.options = new ChipMachineOptions
        {
            LoadStoreQuirks = enableLoadQuirks,
            ShiftQuirks = enableShiftquirks,
            JumpQuirk = enableJumpQuirk
        };

        Initialise();

    }

    public Chip8Interpreter(int instructionsPerSecond, bool enableSuperChip) : this(instructionsPerSecond, false, false, false)
    {
        this.SuperChipEnabled = enableSuperChip;
    }

    /// <summary>
    /// This should run 1 instruction at a time
    /// (instruction timer dependent)
    /// </summary>
    public void Tick()
    {
        this.instructionTimer.Update(DateTime.Now);
        this.sixtyHertzTimer.Update(DateTime.Now);

        if (this.instructionTimer.GetTicked())
        {
            this.FetchDecodeExecute();
        }

        if (soundTickAccumulator == 0) soundtickduration = new TimeSpan(DateTime.Now.Ticks);


        if (this.sixtyHertzTimer.GetTicked())
        {
            soundTickAccumulator += 1;

            var sTimer = SoundTimer;
            if (SoundTimer > 0) SoundTimer -= 1;
            if (DelayTimer > 0) DelayTimer -= 1;
            if (SoundTimer == 0 && sTimer != SoundTimer)
            {
                this.OnSoundOffEventRasied();
                Debug.WriteLine($"Sound On Duration: ${SoundOnLength}");

            }

            if (soundTickAccumulator == 60)
            {
                soundTickAccumulator = 0;
                soundtickduration = new TimeSpan(DateTime.Now.Ticks) - soundtickduration;
                Debug.WriteLine($"Sound 60 tick duration: ${soundtickduration}");
            }
        }
    }
    private TimeSpan soundtickduration;
    private TimeSpan SoundOnLength;
    private int soundTickAccumulator = 0;
    private bool SuperChipEnabled;

    public void Load(byte[] instructions)
    {
        Array.Copy(instructions, 0, Memory, 0x200, instructions.Length);
        this.ProgramCounter = 0x200; // Currently executing instruction.
    }

    /// <summary>
    /// runs the complete interepreter from start to finish, with n though to anything else.
    /// Really not very good, and can't process user input.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="OutOfMemoryException"></exception>
    public void Interpreter()
    {

        while (ProgramCounter < Memory.Length)
        {
            this.FetchDecodeExecute();
            if (this.SuperChipEnabled && ProgramCounter == 0xFFF)
                throw new OutOfMemoryException("Over the lonie");
        }
    }

    private void FetchDecodeExecute()
    {
        // Fetch
        ushort opCode = this.Memory.ReadOpcode(this.ProgramCounter);
        // Immediatly increment the program counter.
        this.ProgramCounter += 2;
        //Decode

        // get the actual command.
        byte operation = opCode.GetOperation();

        // Mode decode and Execute
        switch (operation)
        {
            case 0x0:
                // The scrolling commmand is a pickle becuase we care about the final nibble.
                if ((opCode & 0xC0) == 192)
                {
                    // Scroll N lines Down
                    var rows = opCode.GetLastNibble();
                    // Starting from the bottom,
                    // a certain set of rows are to be cleared first.

                    // Now copy rows to the new position (again go from the bottom up)

                }
                else
                    switch (opCode)
                    {
                        case 0xE0: /// ClS
                            this.ClearDisplay();
                            break;
                        case 0xEE: // Return from subroutine
                            var returnAddress = this.Stack.Pop();
                            this.ProgramCounter = returnAddress;
                            break;
                        case 0xFB: // Scroll display 4 pixels right
                            {
                                // blank right 4 'columns'
                                var blankSize = 4; ;
                                var blankMask = Enumerable.Range(0, blankSize).Select(a => false).ToArray();
                                // var startNumber = this.ExtendedScreenMode ? 128 - blankSize : 64 - blankSize;
                                // Step through each row
                                //blanking out the end.
                                for (var y = 0; y < 64; ++y)
                                {
                                    var startIdx = (y * 128);
                                    var endIdx = startIdx + (128 - blankSize);
                                    // should always be 128
                                    var rowData = blankMask.Concat(Display[startIdx..endIdx]).ToArray();
                                    rowData.CopyTo(Display, startIdx);
                                }
                            }
                            break;
                        case 0xFC:
                            { // Scroll display 4 pixels left
                              // blank right 4 'columns'
                                var blankSize = 4;// this.ExtendedScreenMode ? 4 : 8;
                                var blankMask = Enumerable.Range(0, blankSize).Select(a => false).ToArray();
                                // var startNumber = this.ExtendedScreenMode ? 128 - blankSize : 64 - blankSize;
                                // Step through each row
                                //blanking out the end.
                                for (var y = 0; y < 64; ++y)
                                {
                                    var startIdx = (y * 128) + blankSize;
                                    var endIdx = (y * 128) + 128;
                                    // should always be 128
                                    var rowData = Display[startIdx..endIdx].Concat(blankMask).ToArray();
                                    rowData.CopyTo(Display, (y * 128));
                                }
                            }
                            break;
                        case 0xFD: // Exit interpreter
                            return;
                            break;
                        case 0xFE: // Disable extended screen
                            this.ExtendedScreenMode = false;
                            break;
                        case 0xFF: // Enabled extended sceen mode;
                            this.ExtendedScreenMode = true;
                            break;

                    }
                break;
            case 0x1: // JUMP!
                {
                    var address = opCode.Get12BitNumber();
                    this.ProgramCounter = address;
                }
                break;
            case 0x2: // JUMP (to subroutine, with return address)
                {
                    var address = opCode.Get12BitNumber();
                    this.Stack.Push(ProgramCounter);
                    this.ProgramCounter = address;
                }
                break;
            case 0x3: // A == B (Register value)
                {
                    var registerPos = opCode.GetXRegister();
                    var comparision = opCode.Get8BitConstant();
                    if (this.Registers[registerPos] == comparision)
                        this.ProgramCounter += 2;
                }
                break;
            case 0x4: // A != B (Register value)
                {
                    var registerPos = opCode.GetXRegister();
                    var comparision = opCode.Get8BitConstant();
                    if (this.Registers[registerPos] != comparision)
                        this.ProgramCounter += 2;
                }
                break;
            case 0x5: // A=B Register
                {
                    var registerPosX = opCode.GetXRegister();
                    var registerPosY = opCode.GetYRegister();
                    var comparision = opCode.Get8BitConstant();
                    if (this.Registers[registerPosX] == this.Registers[registerPosY])
                        this.ProgramCounter += 2;
                }
                break;
            case 0x6: // SET register
                {
                    var registerPosX = opCode.GetXRegister();
                    var number = opCode.Get8BitConstant();
                    this.Registers[registerPosX] = number;
                }
                break;
            case 0x7: // ADD (No  carry check handled)
                {
                    var registerPosX = opCode.GetXRegister();
                    var number = opCode.Get8BitConstant();
                    this.Registers[registerPosX] += number;
                }
                break;
            case 0x75:
                {
                    var registerPosX = opCode.GetXRegister();
                    for (byte x = 0; x <= registerPosX; ++x)
                    {
                        RPL[x] = this.Registers[x];
                    }
                }
                break;
            case 0x85:
                {
                    var registerPosX = opCode.GetXRegister();
                    for (byte x = 0; x <= registerPosX; ++x)
                    {
                        this.Registers[x] = RPL[x];
                    }
                }
                break;

            case 0x8:
                { // Logical Operators
                    var registerPosX = opCode.GetXRegister();
                    var registerPosY = opCode.GetYRegister();
                    var vx = this.Registers[registerPosX];
                    var vy = this.Registers[registerPosY];

                    switch (opCode.GetLastNibble())
                    {
                        case 0: // Set x->Y
                            this.Registers[registerPosX] = this.Registers[registerPosY];
                            break;
                        case 1:
                            {
                                this.Registers[registerPosX] = (byte)(vx | vy);
                            }
                            break;
                        case 2:
                            {
                                this.Registers[registerPosX] = (byte)(vx & vy);
                            }
                            break;
                        case 3:
                            {
                                this.Registers[registerPosX] = (byte)(vx ^ vy);
                            }
                            break;
                        case 4:
                            {
                                var total = (vx + vy);
                                if (total > 255) this.Registers[0xF] = 1;
                                else this.Registers[0xF] = 0;
                                this.Registers[registerPosX] = (byte)total;
                            }
                            break;

                        case 5:
                            {
                                var total = (vx - vy);
                                if (vx > vy) this.Registers[0xF] = 1;
                                else if (vy > vx) this.Registers[0xF] = 0;
                                this.Registers[registerPosX] = (byte)total;
                            }
                            break;
                        case 6: // Left Shift
                            {
                                var tempVx = this.Registers[registerPosX];
                                var firstBit = tempVx & 0x01;
                                this.Registers[registerPosX] = (byte)(tempVx >> 1);
                                this.Registers[0xF] = (byte)firstBit;
                            }
                            break;
                        case 7:
                            {
                                var total = (vy - vx);
                                if (vx > vy) this.Registers[0xF] = 1;
                                else if (vy > vx) this.Registers[0xF] = 0;
                                this.Registers[registerPosX] = (byte)total;
                            }
                            break;
                        case 0xE: // Right shift
                            {
                                var tempVx = this.Registers[registerPosX];
                                var lastBit = (tempVx & (1 << 0x11)) != 0 ? 1 : 0;  // Does the bit at position 12  =0
                                this.Registers[registerPosX] = (byte)(tempVx << 1);
                                this.Registers[0xF] = (byte)lastBit;

                                if (this.options.ShiftQuirks) this.Registers[registerPosY] = this.Registers[registerPosX];
                            }
                            break;
                    }
                }
                break;
            case 0x9: // A!=B Register
                {
                    var registerPosX = opCode.GetXRegister();
                    var registerPosY = opCode.GetYRegister();
                    var comparision = opCode.GetTop8BitNumber();
                    if (this.Registers[registerPosX] != this.Registers[registerPosY])
                        this.ProgramCounter += 2;
                }
                break;
            case 0xA: // set address register
                {
                    var address = opCode.Get12BitNumber();
                    this.IndexRegister = address;
                }
                break;
            case 0xB:
                {
                    // original
                    if (!this.options.JumpQuirk)
                    {
                        var jumpAddr = opCode.Get12BitNumber() + this.Registers[0];
                        this.ProgramCounter = (ushort)jumpAddr;
                    }
                    else
                    { // modern
                        var registerPosX = opCode.GetXRegister();
                        var address = opCode.Get12BitNumber();
                        this.ProgramCounter = (ushort)(this.Registers[registerPosX] + address);
                    }
                }
                break;
            case 0xC: // Random
                {
                    var num = randomGenerator.Next(0, this.Memory.Length);
                    var registerPosX = opCode.GetXRegister();
                    var result = num & opCode.Get8BitConstant();
                    this.Registers[registerPosX] = (byte)result;
                }
                break;
            case 0xD:
                {
                    // get register ids
                    var registerPosX = opCode.GetXRegister();
                    var registerPosY = opCode.GetYRegister();
                    var pixelHeight = opCode.GetLastNibble();

                    var setPixelHeight = this.SuperChipEnabled && this.ExtendedScreenMode && pixelHeight == 0 ? (byte)16 : pixelHeight;
                    this.Draw(this.Registers[registerPosX], this.Registers[registerPosY], setPixelHeight);
                }
                break;
            case 0xE: // Skip if key
                {
                    var cmd = opCode.Get8BitConstant();
                    var registerPosX = opCode.GetXRegister();
                    var keyPressed = this.Registers[registerPosX];
                    if (cmd == 0x9E)
                    {
                        if (this.IsKeyPressed(keyPressed))
                            this.ProgramCounter += 2;
                    }
                    else if (cmd == 0xA1)
                    {
                        if (!this.IsKeyPressed(keyPressed))
                            this.ProgramCounter += 2;
                    }
                }
                break;
            case 0xF:
                {
                    var cmd = opCode.Get8BitConstant();
                    var registerPosX = opCode.GetXRegister();
                    switch (cmd)
                    {
                        case 0x07:
                            this.Registers[registerPosX] = this.DelayTimer;
                            break;
                        case 0x15:
                            this.DelayTimer = this.Registers[registerPosX];
                            break;
                        case 0x18: // Set Sound timer duration and raise the event
                            this.SoundTimer = this.Registers[registerPosX];
                            this.OnSoundOnEventRasied();
                            break;
                        case 0x1E:  // Add to index register
                            this.IndexRegister += this.Registers[registerPosX];
                            break;
                        case 0x0A: // Get Key
                            {
                                // hmm... we can pause here with a console.readkey.
                                // but instead, I am directed to createing an infinite loop until a key is pressed
                                byte? pressedKey = this.PressedKey();
                                if (pressedKey is byte hexKey)
                                {
                                    if (hexKey > 0xF) throw new ArgumentOutOfRangeException($"This {hexKey}, key is very out of range.");
                                    this.Registers[registerPosX] = hexKey;
                                }
                                else
                                {
                                    this.ProgramCounter -= 2; // No key pressed, repeat this instruction until all is well.
                                }
                            }
                            break;
                        case 0x29: // Font Character
                            {
                                // To be pedantic, we only should use final nibble of the register value
                                this.IndexRegister = this.Registers[registerPosX] * 5;
                            }
                            break;
                        case 0x30: // Super 8 Character
                            {
                                // To be pedantic, we only should use final nibble of the register value
                                this.IndexRegister = this.Registers[registerPosX] * 10;
                            }
                            break;
                        case 0x33: // BCD
                            {
                                var indexAddress = this.IndexRegister;
                                var bcdValue = this.Registers[registerPosX];
                                this.Memory[indexAddress] = (byte)(bcdValue / 100);
                                this.Memory[indexAddress + 1] = (byte)((bcdValue / 10) % 10);
                                this.Memory[indexAddress + 2] = (byte)(bcdValue % 10);
                            }
                            break;
                        case 0x55: // Store Memory
                            {

                                for (byte registerIdx = 0; registerIdx <= registerPosX; ++registerIdx)
                                {
                                    this.Memory[this.IndexRegister + registerIdx] = this.Registers[registerIdx];
                                }

                                if (this.options.LoadStoreQuirks) this.IndexRegister += registerPosX;
                            }
                            break;
                        case 0x65: // Load Memory
                            {
                                for (byte registerIdx = 0; registerIdx <= registerPosX; ++registerIdx)
                                {
                                    this.Registers[registerIdx] = this.Memory[this.IndexRegister + registerIdx];
                                }
                                if (this.options.LoadStoreQuirks) this.IndexRegister += registerPosX;
                            }
                            break;

                    }
                }
                break;
        }


        if (ProgramCounter > this.Memory.Length)
            throw new OutOfMemoryException("Interpreter memory borked");

    }

    private byte? PressedKey()
    {
        return this.CurrentKey;
    }

    private bool IsKeyPressed(byte keyPressed)
    {
        return this.CurrentKey == null ? false : CurrentKey == keyPressed;
    }
}
