// Generated by https://quicktype.io

namespace SuperChip.Interpreter.Host
{
    public partial class SuperChipSettings
    {
        public SuperChipSettings(){ this.Switches=new Switches();}

        public string Sound { get; set; }="";
        public string Rom { get; set; } ="";
        public bool SuperChipEnabled { get; set; }
        public Switches Switches { get; set; }
        public string FilePickerDirectory { get; internal set; }
    }

    public partial class Switches
    {
        public bool LoadStoreQuirk { get; set; }
        public bool ShiftQuirk { get; set; }
        public bool JumpQuirk { get; set; }
    }

  
}
