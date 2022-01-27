using Chip8.Interpreter.V2;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Chip8.Tests.V2
{
    public partial class V2Tests
    {
        [Fact(DisplayName = "00E0: Clear the screen")]
        public void Cls()
        {
            // Create a program
            // set V[1], V[2]
            // Draw a sprite (from Font)
            // Clear the screen
            // Check to see if the value of the display =0;

            byte[] program = new byte[]
            {
                0x61,0x0A,
                0x62,0x0A,    // Set registers
                0xA0,00,     // Set I to 0 [our first font character]
                0xD1,0x21,   // Draw the specified character
            };
            // Bog standard setup
            Chip8Interpreter chip8 = new(0, false, false, false);

            chip8.Load(program);

            while (chip8.ProgramCounter < chip8.Memory.Length)
            {
                chip8.Tick();
            }
            // The only data should be:
            // First row of 0 char = 0xF0 = 1111 0000 = 4
            var displaySum = chip8.Display.Sum(a => a == true ? 1 : 0);
            Assert.True(displaySum == 4);


            byte[] programA = new byte[]
            {
                0x00E0
            };

            chip8.Load(program);
            while (chip8.ProgramCounter < chip8.Memory.Length)
            {
                chip8.Tick();
            }

            // No data should be held.
            displaySum = chip8.Display.Sum(a => a == true ? 1 : 0);
            Assert.True(displaySum == 0);
        }

        [Fact(DisplayName = "00EE: Return from subroutin")]
        public void return_From_Subroutine()
        {
            // start a program, jump to an address
            // Check to see if the program counter is in the correct place.
            byte[] program = new byte[4096 - 0x200];

            program[0] = 0x22;
            program[1] = 0x08;  // Jump to location 520.

            byte[] subroutine = new byte[]
            {
                0x61, 0x0F, // Stick a value in V[1]
                0x00, 0xEE // return program counter
            };
            // Logical offset
            subroutine.CopyTo(program, 8);

            // Bog standard setup
            Chip8Interpreter chip8 = new(0, false, false, false);

            chip8.Load(program);
            chip8.Tick();
            Debug.WriteLine(chip8.ProgramCounter);
            // Memory wise offset.
            Assert.True(chip8.ProgramCounter == 8 + 0x200);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.ProgramCounter == 2 + 0x200);
            // Make sure the activity did something too.
            Assert.True(chip8.Registers[1] == 15);

        }

        [Fact(DisplayName = "1NNN: Jump to address NNN")]
        public void JumpToAddress()
        {
            // start a program, jump to an address
            // Check to see if the program counter is in the correct place.
            byte[] program = new byte[]
                {
                    0x17,0xD0   // The number 2000 is spread over 3 nibbles.
                };
            // Bog standard setup
            Chip8Interpreter chip8 = new(0, false, false, false);

            chip8.Load(program);
            chip8.Tick();

            Assert.True(chip8.ProgramCounter == 2000);

        }

        [Fact(DisplayName = "2NNN: Call subroutine at address and return")]
        public void CallSubAtAddressAndReturn()
        {
            // start a program, jump to an address
            // Check to see if the program counter is in the correct place.
            byte[] program = new byte[4096 - 0x200];

            program[0] = 0x22;
            program[1] = 0x08;  // Jump to location 520.

            byte[] subroutine = new byte[]
            {
                0x61, 0x0F, // Stick a value in V[1]
                0x00, 0xEE // return program counter
            };
            // Logical offset
            subroutine.CopyTo(program, 8);

            // Bog standard setup
            Chip8Interpreter chip8 = new(0, false, false, false);

            chip8.Load(program);
            chip8.Tick();
            Debug.WriteLine(chip8.ProgramCounter);
            // Memory wise offset.
            Assert.True(chip8.ProgramCounter == 8 + 0x200);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.ProgramCounter == 2 + 0x200);
            // Make sure the activity did something too.
            Assert.True(chip8.Registers[1] == 15);

        }

        [Fact(DisplayName = "3XNN: Skip the following instruction if the value of register VX equals NN")]
        public void SkipTheFollowingIftheValueofX_Equals()
        {
            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x61,0x0E,   // V[1] =15
                    0x31,0x0E,   // Skip if v1=15
                    0x61,0x00,    // V1=0
                    0x61,0x02    // V1 ==2
                };
            byte[] programA = new byte[]
                {
                    0x61,0x0E,   // V[1] =15
                    0x31,0x02,   // Skip if v1=2
                    0x61,0x00,   // V1=0
                    0x61,0x02    // V1 ==2
                };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[1] == 2);
            chip8.Load(programA);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[1] == 0);

        }

        [Fact(DisplayName = "4XNN: Skip the following instruction if the value of register VX is not equal to NN")]
        public void SkipTheFollowingIftheValueofX_NotEquals()
        {
            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x61,0x0E,   // V[1] =15
                    0x41,0x0E,   // Do not Skip if v1=15
                    0x61,0x00,    // V1=0
                    0x61,0x02    // V1 ==2
                };
            byte[] programA = new byte[]
                {
                    0x61,0x0E,   // V[1] =15
                    0x41,0x02,   // Do not Skip if v1=2
                    0x61,0x00,   // V1=0
                    0x61,0x02    // V1 ==2
                };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.False(chip8.Registers[1] == 2);
            chip8.Load(programA);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.False(chip8.Registers[1] == 0);

        }

        [Fact(DisplayName = "5XY0: Skip the following instruction if the value of register VX is equal to the value of register VY")]
        public void SkipTheFollowingvX_and_vY_Equals()
        {
            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x61,0x0E,   // V[1] =15
                    0x62,0x0E,   // V[2] =15
                    0x52,0x10,   // X==Y?
                    0x61,0x00,   // V1=0 False
                    0x61,0x02    // V1 ==2  true
                };
            byte[] programA = new byte[]
              {
                    0x61,0x0E,   // V[1] =15
                    0x62,0x07,   // V[2] =15
                    0x52,0x10,   // X==Y?
                    0x61,0x00,   // V1=0 False
                    0x61,0x02    // V1 ==2  true
              };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[1] == chip8.Registers[2]);
            Assert.True(chip8.ProgramCounter == 0x200 + 8);

            chip8.Load(programA);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[1] != chip8.Registers[2]);
            Assert.True(chip8.ProgramCounter == 0x200 + 6);

        }
        [Fact(DisplayName = "6XNN: Set VX to NN")]
        public void Set_vx_NN()
        {

            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x61,0x0E,   // V[1] =15
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.Registers[1] == 14);
        }

        [Fact(DisplayName = "7XNN: Add NN to VX")]
        public void Add_NN_Vx()
        {

            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x61,0x0E,   // V[1] =15
                    0x71,0x22
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[1] == 0x0E + 0x22);
        }

        [Fact(DisplayName = "8XY0: Set VX to the value in VY")]
        public void Set_X_Y()
        {

            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x62,0x0E,   // V[1] =15
                    0x64,0x08,
                    0x82,0x40
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 14);

            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[2] == chip8.Registers[4]);
        }

        [Fact(DisplayName = "8XY1: Set VX to VX OR VY")]
        public void Set_X_Y_Or()
        {

            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x62,0x0C,   // V[1] =15
                    0x64,0x0A,
                    0x82,0x41
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 12);

            chip8.Tick();
            chip8.Tick();
            Assert.True((chip8.Registers[2] | chip8.Registers[4]) == (12 | 10));
        }

        [Fact(DisplayName = "8XY2: Set VX to VX AND VY")]
        public void Set_X_Y_AND()
        {

            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x62,0x0C,   // V[1] =15
                    0x64,0x0A,
                    0x82,0x42
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 12);

            chip8.Tick();
            chip8.Tick();
            Assert.True((chip8.Registers[2] & chip8.Registers[4]) == (12 & 10));
        }

        [Fact(DisplayName = "8XY3: Set VX to VX XOR VY")]
        public void Set_X_Y_xor()
        {

            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x62,0x0C,   // V[1] =15
                    0x64,0x0A,
                    0x82,0x43
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 12);

            chip8.Tick();
            chip8.Tick();
            Assert.True((chip8.Registers[2]) == 6);
        }


        [Fact(DisplayName = "8XY4: Add the value of register VY to register VX. Set VF to 01 if a carry occurs. Set VF to 00 if a carry does not occur")]
        public void Add_Y_X_WithCarry()
        {

            // Assign a value to X
            // then check same value (We want it to skip)
            byte[] program = new byte[]
                {
                    0x62,0xFF,   // V[1] =15
                    0x64,0x0A,
                    0x82,0x44
                };
            byte[] programA = new byte[]
                    {
                    0x62,0x07,   // V[1] =15
                    0x64,0x0A,
                    0x82,0x44
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 255);

            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[0xF] == 1);


            chip8.Load(programA);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 7);

            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[0xF] == 0);
        }

        [Fact(DisplayName = "8XY5: Subtract the value of register VY from register VX. Set VF to 00 if a borrow occurs. Set VF to 01 if a borrow does not occur")]
        public void Subtract_Y_X_WithCarry()
        {

            // Assign a value to X
            byte[] program = new byte[]
                {
                    0x62,0x09,   // V[1] =15
                    0x64,0x0A,
                    0x82,0x45
                };
            byte[] programA = new byte[]
                    {
                    0x62,0x0B,   // V[1] =15
                    0x64,0x0A,
                    0x82,0x45
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 9);

            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[0xF] == 0);


            chip8.Load(programA);
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 11);

            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[0xF] == 1);
        }

        [Fact(DisplayName = "8XY6: Store the value of register VY shifted right one bit in register VX. Set register VF to the least significant bit prior to the shift")]
        public void VY_VX_ShiftRight()
        {
            // Assign a value to X
            byte[] program = new byte[]
                {
                    0x62,0x09,   // V[1] =15
                    0x64,0x0C,
                    0x82,0x46
                };
            byte[] programA = new byte[]
                    {
                    0x62,0x0B,   // V[1] =15
                    0x64,0x0B,
                    0x82,0x46
                };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 6);
            Assert.True(chip8.Registers[0xF] == 0);

            chip8.Load(programA);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 5);
            Assert.True(chip8.Registers[0xF] == 1);
        }

        [Fact(DisplayName = "8XY7: Set register VX to the value of VY minus VX. Set VF to 00 if a borrow occurs. Set VF to 01 if a borrow does not occur")]
        public void Subtract_Vy_fromVX_WithCarry()
        {

            // Assign a value to X
            byte[] program = new byte[]
                {
                    0x62,0x0A,   // V[1] =
                    0x64,0x09,   // 9-10
                    0x82,0x47
                };
            byte[] programA = new byte[]
                    {
                    0x62,0x04,   // V[1] =15
                    0x64,0x0A, // 10-4
                    0x82,0x47
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 255);
            Assert.True(chip8.Registers[0xf] == 0);



            chip8.Load(programA);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 6);
            Assert.True(chip8.Registers[0xF] == 1);
        }

        [Fact(DisplayName = "8XYE: Store the value of register VY shifted left one bit in register VX. Set register VF to the most significant bit prior to the shift")]
        public void VY_VX_ShiftLeft()
        {
            // Assign a value to X
            byte[] program = new byte[]
                {
                    0x62,0x09,   // V[1] =15
                    0x64,0x09,
                    0x82,0x4E
                };
            byte[] programA = new byte[]
                    {
                    0x62,0x0B,   // V[1] =15
                    0x64,0xFB,
                    0x82,0x4E
                };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 18);
            Assert.True(chip8.Registers[0xF] == 0);

            chip8.Load(programA);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[2] == 246);
            Assert.True(chip8.Registers[0xF] == 1);
        }

        [Fact(DisplayName = "9XY0: Skip the following instruction if the value of register VX is not equal to the value of register VY")]
        public void Skip_VX_VY_Not_Equal()
        {        // Assign a value to X
                 // then check same value (We want it to skip)
            byte[] program = new byte[]
                    {
                    0x61,0x0E,   // V[1] =14
                    0x62,0x0E,   // V[2] =14
                    0x91,0x20,   // Skip if v1=v2
                    0x61,0x00,    // V1=0
                    0x61,0x02    // V1 ==2
                    };
            byte[] programA = new byte[]
                {
                    0x61,0x0E,   // V[1] =14
                    0x62,0x08,   // V[2] =8
                    0x91,0x20,   // Skip if v1=v2
                    0x61,0x00,   // V1=0
                    0x61,0x02    // V1 ==2
                };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[1] == 0);
            Assert.True(chip8.ProgramCounter == 0x200 + 8);
            chip8.Load(programA);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[1] == 2);
            Assert.True(chip8.ProgramCounter == 0x200 + 10);

        }

        [Fact(DisplayName = "ANNN: Store memory address NNN in register I")]
        public void SetIndexRegister()
        {

            byte[] program = new byte[]
                {
                    0xA1,0x23
                 };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            Assert.True(chip8.IndexRegister == 291);
        }

        [Fact(DisplayName = "BNNN: Jump to address NNN + V0")]
        public void JumpToAddress_V0_Add()
        {
            byte[] program = new byte[]
               {
                    0x60,0x05,
                    0xB8,0x00
                };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.ProgramCounter == 2048 + 5);
        }

        [Fact(DisplayName = "CXNN: Set VX to a random number with a mask of NN")]
        public void RandomNumGer()
        {
            byte[] program = new byte[]
              {
                    0x60,0x05,
                    0xC0,0x05
               };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.False(chip8.Registers[0] != 5);

        }

        [Fact(DisplayName = "DXYN: Draw a sprite at position VX, VY with N bytes of sprite data starting at the address stored in I. Set VF to 01 if any set pixels are changed to unset, and 00 otherwise")]
        public void Draw()
        {

            byte[] program = new byte[]
              {
                    0x60,0x05,
                    0x61,0x05, // Position 25
                    0xA0,000,
                    0xD0,0x11,
                    0x60,0x07,
                    0xD0,0x11,
               };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Display[325] = true && chip8.Display[326] == true && chip8.Display[327] == true && chip8.Display[328] == true);
            Assert.True(chip8.Registers[0xf] == 0);

            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Display[327] == false && chip8.Display[328] == false && chip8.Display[329] == true && chip8.Display[330] == true);
            Assert.True(chip8.Registers[0xf] == 1);

        }

        [Fact(DisplayName = "EX9E: Skip the following instruction if the key corresponding to the hex value currently stored in register VX is pressed")]
        public void skip_if_Vx_key_pressed()
        {
            byte[] program = new byte[]
           {
                    0x65,0x09,   // V[1] =15
                    0xE5,0x9E,
                    0x65,0x02,   // V[1] =15
                    0x65,0xFE,   // V[1] =15

           };
            byte[] programA = new byte[]
                    {
                    0x65,0x09,   // V[1] =15
                    0xE5,0x9E,
                    0x65,0x02,   // V[1] =15
                    0x65,0xFE,   // V[1] =15
                };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.PressedKey('D'); // Obviously the is 9...
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[5] == 0xFE);

            chip8.Load(program);
            chip8.Tick();
            chip8.PressedKey('A'); // and this 7 ...
            chip8.Tick();
            chip8.Tick();
            Assert.False(chip8.Registers[5] == 0xFE);
            Assert.True(chip8.Registers[5] == 0x02);

        }

        [Fact(DisplayName = "EXA1: Skip the following instruction if the key corresponding to the hex value currently stored in register VX is not pressed")]
        public void skip_if_Vx_key_not_pressed()
        {
            byte[] program = new byte[]
           {
                    0x65,0x09,   // V[1] =15
                    0xE5,0xA1,
                    0x65,0x02,   // V[1] =15
                    0x65,0xFE,   // V[1] =15

           };
            byte[] programA = new byte[]
                    {
                    0x65,0x09,   // V[1] =15
                    0xE5,0xA1,
                    0x65,0x02,   // V[1] =15
                    0x65,0xFE,   // V[1] =15
                };

            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.PressedKey('D'); // Obviously the is 9...
            chip8.Tick();
            chip8.Tick();
            Assert.False(chip8.Registers[5] == 0xFE);

            chip8.Load(program);
            chip8.Tick();
            chip8.PressedKey('A'); // and this 7 ...
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Registers[5] == 0xFE);
            Assert.False(chip8.Registers[5] == 0x02);

        }

        [Fact(DisplayName = "FX07: Store the current value of the delay timer in register VX")]
        public void storeDelayTimerInVx()
        {

            byte[] program = new byte[]
           {
                    0x65,0x09,   // V[1] =15
                    0xF5,0x15, // Set the delaytimer to the value of 5
                    0xF8,0x07 //Store the delay timer in 8

               //0x65,0x02,   // V[1] =15
               //0x65,0xFE,   // V[1] =15

           };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.False(chip8.Registers[8] == 0x09);

        }

        [Fact(DisplayName = "FX15: Set the delay timer to the value of register VX")]
        public void Set_delay_toVx()
        {

            byte[] program = new byte[]
           {
                    0x65,0x09,   // V[1] =15
                    0xF5,0x15, // Set the delaytimer to the value of 5
                    0xF8,0x07 //Store the delay timer in 8

               //0x65,0x02,   // V[1] =15
               //0x65,0xFE,   // V[1] =15

           };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.DelayTimer == 0x09 || chip8.DelayTimer == 0x08);

        }

        [Fact(DisplayName = "FX0A: Wait for a keypress and store the result in register VX")]
        public void Wait_KeyPress_Store_VX()
        {
            byte[] program = new byte[]
         {
                    0xF9,0x0A,   // V[1] =15

             //0x65,0x02,   // V[1] =15
             //0x65,0xFE,   // V[1] =15

         };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.ProgramCounter == 0x200);
            chip8.PressedKey('A');
            chip8.Tick();
            Assert.True(chip8.Registers[9] == 0x07);

        }

        [Fact(DisplayName = "FX18: Set the sound timer to the value of register VX")]
        public void Set_sound_toVx()
        {

            byte[] program = new byte[]
           {
                    0x65,0x09,   // V[1] =15
                    0xF5,0x18, // Set the sound to the value of 5

               //0x65,0x02,   // V[1] =15
               //0x65,0xFE,   // V[1] =15

           };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.SoundTimer == 0x09 || chip8.SoundTimer == 0x08);

        }


        [Fact(DisplayName = "FX1E: Add the value stored in register VX to register I")]
        public void Set_index_toVS()
        {

            byte[] program = new byte[]
           {
                    0x65,0x09,   // V[1] =15
                    0xF5,0x1e, // Set the indexto the value of 5

           };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.IndexRegister == 09);

        }

        [Fact(DisplayName = "FX29: Set I to the memory address of the sprite data corresponding to the hexadecimal digit stored in register VX")]
        public void Set_index_toFontChar()
        {

            byte[] program = new byte[]
           {
                    0x65,0x09,   // V[1] =15
                    0xF5,0x29, // Set the indexto the font char (by number)

           };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.IndexRegister == 09 * 5);

        }

        [Fact(DisplayName = "FX33: Store the binary-coded decimal equivalent of the value stored in register VX at addresses I, I+1, and I+2")]
        public void Set_bcd()
        {

            byte[] program = new byte[]
           {
                    0x61, 0x99,
                    0x62, 0x21,
                    0xA2,0x12,
                    0xF1,0x33,   // V[1] =15
                    0xF2,0x33, // Set the indexto the value of 5

           };
            Chip8Interpreter chip8 = new(0, false, false, false);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Memory[0x212] == 1);
            Assert.True(chip8.Memory[0x213] == 5);
            Assert.True(chip8.Memory[0x214] == 3);

            chip8.Tick();

            Assert.True(chip8.Memory[0x212] == 0);
            Assert.True(chip8.Memory[0x213] == 3);
            Assert.True(chip8.Memory[0x214] == 3);

        }

        [Fact(DisplayName = "FX55: Store the values of registers V0 to VX inclusive in memory starting at address I. I is set to I + X + 1 after operation")]
        public void store_registers_memory()
        {
            byte[] program = new byte[]
                    {
                    0x61, 0x99,
                    0x62, 0x21,
                    0x63, 0x0A,
                    0x64, 0x2A,

                    0x6A, 0x01,
                    0x6B, 0x02,
                    0x6C, 0x0C,
                    0x6D, 0x0D,
                    0xA2, 0x22,
                    0xF4, 0x55,
                    0xA3, 0x55,
                    0xFD, 0x55

                };
            Chip8Interpreter chip8 = new(0, true, false, false);
            chip8.Load(program);

            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();

            Assert.True(chip8.IndexRegister == 0x222 + 4);

            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.IndexRegister == 0x355 + 0xD);

            Assert.True(chip8.Memory[0x222] == 0);
            Assert.True(chip8.Memory[0x222 + 1] == 0x99);
            Assert.True(chip8.Memory[0x222 + 2] == 0x21);
            Assert.True(chip8.Memory[0x222 + 3] == 0x0A);
            Assert.True(chip8.Memory[0x222 + 4] == 0x2A);
            Assert.True(chip8.Memory[0x222 + 5] == 0);

        }

    }

}