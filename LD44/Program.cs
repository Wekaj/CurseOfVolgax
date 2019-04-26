using System;

namespace LD44 {
    public static class Program {
        [STAThread]
        static void Main() {
            using (var game = new LD44Game()) {
                game.Run();
            }
        }
    }
}
