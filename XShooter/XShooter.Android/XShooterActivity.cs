using Android.App;
using Android.OS;
using Android.Content.PM;

using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Starter;

namespace XShooter
{
    [Activity(MainLauncher = true, 
              Icon = "@drawable/icon", 
              ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.UiMode | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class XShooterActivity : AndroidXenkoActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Game = new Game();
            Game.Run(GameContext);
        }

        protected override void OnDestroy()
        {
            Game.Dispose();

            base.OnDestroy();
        }
    }
}
