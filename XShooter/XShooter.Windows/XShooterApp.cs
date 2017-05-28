using SiliconStudio.Xenko.Engine;

namespace XShooter
{
    class XShooterApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
