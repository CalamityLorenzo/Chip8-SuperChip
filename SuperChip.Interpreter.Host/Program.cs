namespace SuperChip.Interpreter.Host
{

    using System.Text.Json;
    using System.IO;
    using SuperChip.Interpreter.UI;

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            var settings = new SuperChipSettings();
#if WINBUILD

            using FileStream fs = new FileStream("./settings/config.win.json", FileMode.Open);
            settings= System.Text.Json.JsonSerializer.Deserialize<SuperChipSettings>(fs);
#else
            using FileStream fs = new FileStream("./Settings/config.linux.json", FileMode.Open);
            settings= System.Text.Json.JsonSerializer.Deserialize<SuperChipSettings>(fs);
#endif
            using (var gmw = new SuperChipGame(settings))
            // using (var gmw = new UITest(settings))
            {
                gmw.Run();
            }
        }
    }
}