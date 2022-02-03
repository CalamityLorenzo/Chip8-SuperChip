using SuperChip11Interpreter.V3;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Chip8.Test
{
    public partial class SuperChipTests
    {
        private readonly ITestOutputHelper output;

        public SuperChipTests(ITestOutputHelper output){
            this.output = output;
        }

        [Fact(DisplayName = "00FD: Exit interpreter")]
        public void ExitInterpreter()
        {
            byte[] program = new byte[]
            {
                0x00, 0xFD
            };

            var chip8 = new SuperChipInterpreter(0, false, false, false);
            chip8.Load(program);
            Assert.Throws<Exception>(() => chip8.Tick());


            var super8 = new SuperChipInterpreter(0, true);
            super8.Load(program);
            super8.Tick();
        }

        [Fact(DisplayName = "00FE: Disable high-resolution mode")]
        public void Disable_Hires()
        {
            byte[] program = new byte[]
            {
                0x00, 0xFE
            };

            var chip8 = new SuperChipInterpreter(0, false, false, false);
            chip8.Load(program);
            Assert.Throws<Exception>(() => chip8.Tick());


            var super8 = new SuperChipInterpreter(0, true);
            super8.Load(program);
            super8.Tick();

            Assert.False(super8.HighResolutionMode);
        }

        [Fact(DisplayName = "00FF: Enable high-resolution mode")]
        public void Enable_Hires()
        {
            byte[] program = new byte[]
            {
                0x00, 0xFF
            };

            var chip8 = new SuperChipInterpreter(0, false, false, false);
            chip8.Load(program);
            Assert.Throws<Exception>(() => chip8.Tick());


            var super8 = new SuperChipInterpreter(0, true);
            super8.Load(program);
            super8.Tick();

            Assert.True(super8.HighResolutionMode);
        }


        [Fact(DisplayName = "FX75: Store V0..VX in RPL user flags (X <= 7)")]
        public void Store_V0_Vx_Rpl()
        {
            byte[] program = new byte[]
                  {
                        0x60, 0xFF,  // 255
                        0x61, 0x17,   // 23
                        0x62, 0x1F,
                        0xF3, 0x75
                  };

            byte[] programA = new byte[]
                  {
                        0x60, 0xFF,  // 255
                        0x61, 0x17,   // 23
                        0x62, 0x1F,
                        0xF9, 0x75 // Out of bounds for RPL
                  };

            var chip8 = new SuperChipInterpreter(0, false, false, false);
            chip8.Load(new byte[] { 0xFA, 0x75 });
            Assert.Throws<Exception>(() => chip8.Tick());

            var super8 = new SuperChipInterpreter(0, true);
            super8.Load(program);
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();

            Assert.True(super8.RPLFlags[0] == 0xFF);
            Assert.True(super8.RPLFlags[1] == 0x17);
            Assert.True(super8.RPLFlags[2] == 0x1F);
            Assert.True(super8.RPLFlags[3] == 0x00);

            super8.Load(programA);
            super8.Tick();
            super8.Tick();
            super8.Tick();
            Assert.Throws<ArgumentOutOfRangeException>(() => super8.Tick());

        }


        [Fact(DisplayName = "FX85: Restore V0..VX from RPL user flags (X <= 7))")]
        public void Store_Rpl_V0_Vx()
        {
            byte[] program = new byte[]
                  {
                        0x60, 0xFF,  // 255
                        0x61, 0x17,   // 23
                        0x62, 0x1F,
                        0xF3, 0x75,
                        0x60, 0x00,  // 
                        0x61, 0x01,   // 1
                        0x62, 0x02,   // 2
                        0x63, 0x03,   // 3
                        0xF3, 0x85,

                  };

            byte[] programA = new byte[]
                  {
                        0x60, 0xFF,  // 255
                        0x61, 0x17,   // 23
                        0x62, 0x1F,
                        0xF3, 0x75,
                        0x60, 0x00,  // 
                        0x61, 0x01,   // 1
                        0x62, 0x02,   // 2
                        0x63, 0x03,   // 3
                        0xFA, 0x85,

                  };
            var chip8 = new SuperChipInterpreter(0, false, false, false);
            chip8.Load(new byte[] { 0xFA, 0x85 });
            Assert.Throws<Exception>(() => chip8.Tick());

            var super8 = new SuperChipInterpreter(0, true);
            super8.Load(program);
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            Assert.True(super8.Registers[0] == 0x00);
            Assert.True(super8.Registers[1] == 0x01);
            Assert.True(super8.Registers[2] == 0x02);
            Assert.True(super8.Registers[3] == 0x03);
            super8.Tick();

            Assert.True(super8.Registers[0] == 0xFF);
            Assert.True(super8.Registers[1] == 0x17);
            Assert.True(super8.Registers[2] == 0x1F);
            Assert.True(super8.Registers[3] == 0x00);
            super8.Load(programA);
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            super8.Tick();
            Assert.Throws<ArgumentOutOfRangeException>(() => super8.Tick());

        }
    
    
        [Fact(DisplayName = "DXYN: Draw a sprite at position VX, VY with N bytes of sprite data starting at the address stored in I. Set VF to 01 if any set pixels are changed to unset, and 00 otherwise (Hi-Res enabled)")]
        public void Draw()
        {

            byte[] program = new byte[]
              {
                    0x00,0xF,  // Enable hi-res
                    0x60,0x05,
                    0x61,0x05, // Position 25
                    0xA0,000,
                    0xD0,0x11,
                    0x60,0x07,
                    0xD0,0x11,
               };
            SuperChipInterpreter chip8 = new(0, true);
            chip8.Load(program);
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            chip8.Tick();
            output.WriteLine((chip8.Display[650] == true).ToString());
            output.WriteLine((chip8.Display[651] == true).ToString());
            output.WriteLine((chip8.Display[652] == true).ToString());
            Assert.True(chip8.Display[650] == true && chip8.Display[651] == true && chip8.Display[652] == true && chip8.Display[653] == true);
            Assert.True(chip8.Registers[0xf] == 0);

            chip8.Tick();
            chip8.Tick();
            Assert.True(chip8.Display[654] == false && chip8.Display[655] == false && chip8.Display[656] == true && chip8.Display[657] == true);
            Assert.True(chip8.Registers[0xf] == 1);

        }

    }
}
