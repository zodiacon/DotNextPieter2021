using System;

namespace SpaceDotNet.Desktop {
    public static class Program {
        [STAThread]
        static void Main() {
            using (var game = new SpaceDotNetGame())
                game.Run();
        }
    }
}
