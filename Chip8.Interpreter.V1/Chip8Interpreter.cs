namespace Chip8.Interpreter.V1
{
    public class Chip8DrawingEventArgs : EventArgs
    {
        public bool[] Display;
        public Chip8DrawingEventArgs(bool[] display)
        {
            Display = display;
        }
    }

    public partial class Chip8Interpreter
    {

        public event EventHandler<Chip8DrawingEventArgs> Drawing;
        private Random randomGenerator = new Random();

        private readonly int instructionsPerSecond;
        private readonly Func<byte?> readConsoleKey;
        byte[] Memory = new byte[4096]; // This is 12 bits.
        Dictionary<byte, byte> Registers = new Dictionary<byte, byte>(16);
        Stack<ushort> Stack = new Stack<ushort>();
        int IndexRegister = 0;
        public bool[] Display = new bool[64 * 32];
        ushort ProgramCounter = 0;

        byte DelayTimer = 0;
        byte SountTimer = 0;
        private Chip8Timer instructionTimer;
        private Chip8Timer sixtyHertzTimer;

        public Chip8Interpreter(int instructionsPerSecond, Func<byte?> readConsoleKey)
        {
            var startTime = DateTime.Now;
            int millisecondsToNextInstruction = (int)Math.Abs(1000f / instructionsPerSecond);

            this.instructionTimer = new Chip8Timer(startTime, millisecondsToNextInstruction);
            this.sixtyHertzTimer = new Chip8Timer(startTime, 60);

            // BuIld out the font.
            // This can be stored anywhere in the first 512 bytes.

            //Memory[0] =

            var zero = new byte[] {
                0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                };

            var one = new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 }; // 1
            var two = new byte[] { 0xF0, 0x10, 0xF0, 0x80, 0xF0 }; // 2
            var three = new byte[] { 0xF0, 0x10, 0xF0, 0x10, 0xF0 }; // 3
            var four = new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 }; // 4
            var five = new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 }; // 5
            var six = new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 }; // 6
            var seven = new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 }; // 7
            var eight = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 }; // 8
            var nine = new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 }; // 9
            var ten = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 }; // A
            var Aa = new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 }; // B
            var be = new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 }; // C
            var ce = new byte[] { 0xE0, 0x90, 0x90, 0x90, 0xE0 }; // D
            var de = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0xF0 }; // E
            var eff = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 };  // F

            Array.Copy(zero, 0, Memory, 0, zero.Length);
            Array.Copy(one, 0, Memory, 5, one.Length);
            Array.Copy(two, 0, Memory, 10, two.Length);
            Array.Copy(three, 0, Memory, 15, three.Length);
            Array.Copy(four, 0, Memory, 20, four.Length);
            Array.Copy(five, 0, Memory, 25, five.Length);
            Array.Copy(six, 0, Memory, 30, six.Length);
            Array.Copy(seven, 0, Memory, 35, seven.Length);
            Array.Copy(eight, 0, Memory, 40, eight.Length);
            Array.Copy(nine, 0, Memory, 45, nine.Length);
            Array.Copy(ten, 0, Memory, 50, ten.Length);
            Array.Copy(Aa, 0, Memory, 55, Aa.Length);
            Array.Copy(be, 0, Memory, 60, be.Length);
            Array.Copy(ce, 0, Memory, 65, ce.Length);
            Array.Copy(de, 0, Memory, 70, de.Length);
            Array.Copy(eff, 0, Memory, 75, eff.Length);
            this.instructionsPerSecond = instructionsPerSecond;
            this.readConsoleKey = readConsoleKey;
        }

        public void Interpreter(byte[] instructions)
        {

            Array.Copy(instructions, 0, Memory, 0x200, instructions.Length);
            this.ProgramCounter = 0x200; // Currently executing instruction.



            while (ProgramCounter < Memory.Length)
            {
                // Fetch
                ushort opCode = this.Memory.ReadOpcode(this.ProgramCounter);
                // Immediatly increment the program counter.
                this.ProgramCounter += 2;
                //Decode

                // get the actual command.

                byte operation = opCode.GetOperation();

                switch (operation)
                {
                    case 0x0:
                        if (opCode == 0xE0) // CLear the screen
                        {
                            this.ClearDisplay();
                        }
                        else if (opCode == 0xEE) // Return from a subroutine.
                        {
                            var returnAddress = this.Stack.Pop();
                            this.ProgramCounter = returnAddress;
                        }
                        break;
                    case 0x1: // JUMP!
                        {
                            var address = opCode.Get12BitNumber();
                            this.ProgramCounter = address;
                        }
                        break;
                    case 0x2:
                        {
                            var address = opCode.Get12BitNumber();
                            this.Stack.Push(ProgramCounter);
                            this.ProgramCounter = address;
                        }
                        break;
                    case 0x3: // A == B (Register value)
                        {
                            var registerPos = opCode.GetXRegister();
                            var comparision = opCode.GetTop8BitNumber();
                            if (this.Registers[registerPos] == comparision)
                                this.ProgramCounter += 2;
                        }
                        break;
                    case 0x4: // A != B (Register value)
                        {
                            var registerPos = opCode.GetXRegister();
                            var comparision = opCode.GetTop8BitNumber();
                            if (this.Registers[registerPos] != comparision)
                                this.ProgramCounter += 2;
                        }
                        break;
                    case 0x5: // A=B Register
                        {
                            var registerPosX = opCode.GetXRegister();
                            var registerPosY = opCode.GetYRegister();
                            var comparision = opCode.GetTop8BitNumber();
                            if (this.Registers[registerPosX] == this.Registers[registerPosY])
                                this.ProgramCounter += 2;
                        }
                        break;
                    case 0x6: // SET register
                        {
                            var registerPosX = opCode.GetXRegister();
                            var number = opCode.GetBottom8BitNumber();
                            this.Registers[registerPosX] = number;
                        }
                        break;
                    case 0x7: // ADD (No  carry check handled)
                        {
                            var registerPosX = opCode.GetXRegister();
                            var number = opCode.GetBottom8BitNumber();
                            this.Registers[registerPosX] += number;
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
                                case 7:
                                    {
                                        var total = (vy - vx);
                                        if (vx > vy) this.Registers[0xF] = 1;
                                        else if (vy > vx) this.Registers[0xF] = 0;
                                        this.Registers[registerPosX] = (byte)total;
                                    }
                                    break;
                                case 8:
                                    {

                                        this.Registers[registerPosX] = vy;
                                        if (opCode.GetLastNibble() == 6) // Right shift
                                        {
                                            var tempVx = this.Registers[registerPosX];
                                            var firstBit = tempVx & 0x01;
                                            this.Registers[registerPosX] = (byte)(tempVx >> 1);
                                            this.Registers[0xF] = (byte)firstBit;
                                        }
                                        else
                                        {
                                            var tempVx = this.Registers[registerPosX];
                                            var lastBit = (tempVx & (1 << 0x11)) != 0 ? 1 : 0;  // Does the bit at position 12  =0
                                            this.Registers[registerPosX] = (byte)(tempVx << 1);
                                            this.Registers[0xF] = (byte)lastBit;
                                        }
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
                            {
                                var jumpAddr = opCode.Get12BitNumber() + this.Registers[0];
                                this.ProgramCounter = (ushort)jumpAddr;
                            }

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
                            var result = num & opCode.GetBottom8BitNumber();
                            this.Registers[registerPosX] = (byte)result;
                        }
                        break;
                    case 0xD:
                        {
                            // get register ids
                            var registerPosX = opCode.GetXRegister();
                            var registerPosY = opCode.GetYRegister();
                            var pixelHeight = opCode.GetLastNibble();
                            this.Draw(this.Registers[registerPosX], this.Registers[registerPosY], pixelHeight);
                        }
                        break;
                    case 0xE: // Skip if key
                        {
                            var cmd = opCode.GetBottom8BitNumber();
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
                            var cmd = opCode.GetBottom8BitNumber();
                            var registerPosX = opCode.GetXRegister();
                            switch (cmd)
                            {
                                case 0x07:
                                    this.Registers[registerPosX] = this.DelayTimer;
                                    break;
                                case 0x15:
                                    this.DelayTimer = this.Registers[registerPosX];
                                    break;
                                case 0x18:
                                    this.SountTimer = this.Registers[registerPosX];
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
                                    }
                                    break;
                                case 0x29: // Font Character
                                    {
                                        // To be pedantic, we only should use final nibble of the register value
                                        this.IndexRegister = this.Registers[registerPosX];
                                    }
                                    break;
                                case 0x33: // BCD
                                    {
                                        var indexAddress = this.IndexRegister;
                                        var bcdValue = this.Registers[registerPosX];
                                        this.Memory[indexAddress] = (byte)(bcdValue / 100);
                                        this.Memory[indexAddress + 1] = (byte)((bcdValue / 10) % 10);
                                        this.Memory[indexAddress + 2] = (byte)(bcdValue % 10); ;
                                    }
                                    break;
                                case 0x55: // Store Memory
                                    {
                                        if (registerPosX == 0x0)
                                        {
                                            this.Memory[this.IndexRegister] = this.Registers[registerPosX];
                                        }
                                        else
                                        {
                                            var indexAdder = 0;
                                            for (byte register = registerPosX; register < 0xF; ++register)
                                            {
                                                this.Memory[this.IndexRegister + indexAdder] = this.Registers[register];
                                                indexAdder += 1;
                                            }
                                        }
                                    }
                                    break;
                                case 0x65: // Load Memory
                                    {
                                        if (registerPosX == 0x0)
                                        {
                                            this.Registers[registerPosX] = this.Memory[this.IndexRegister];
                                        }
                                        else
                                        {
                                            var indexAdder = 0;
                                            for (byte register = registerPosX; register < 0xF; ++register)
                                            {
                                                this.Registers[register] = this.Memory[this.IndexRegister + indexAdder];
                                                indexAdder += 1;
                                            }
                                        }
                                    }
                                    break;

                            }
                        }
                        break;
                }

                // Execute

                if (ProgramCounter > this.Memory.Length)
                    throw new OutOfMemoryException("Interpreter memory borked");
            }
        }

        private byte? PressedKey()
        {
            return this.readConsoleKey();
        }

        private bool IsKeyPressed(byte keyPressed)
        {
            throw new NotImplementedException();
        }
    }
}