namespace SuperChip11Interpreter.V3
{
    //  https://github.com/Diesel-Net/kiwi-8/issues/9
    // https://github.com/Diesel-Net/kiwi-8/issues/9#issuecomment-552126906
    public record ChipMachineOptions
    {
        // 0xFX55/0xFX65, value of I index changes when enabled.
        public bool LoadStoreQuirks { get; set; }
        // 8XY6/8XYE Enabling this quirk causes VX to become shifted and VY to be changed.
        public bool ShiftQuirks { get; set; }
        public bool JumpQuirk { get; internal set; }
    }
}