using SiliconStudio.Xenko.Engine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XShooter
{
    public enum LevelState
    {
        Paused,
        Playing
    }

    public enum SoundState
    {
        Muted,
        Unmuted
    }

    public static class GameGlobals
    {
        public static LevelState LevelState { get; set; } = LevelState.Paused;
        public static SoundState SoundState { get; set; } = SoundState.Muted;

        public static List<LevelConfig> LevelConfigs { get; } = new List<LevelConfig>
        {
            new LevelConfig
            {
                Level = 1,
                EnemyCount = 20,
                EnemyDamage = 1,
                EnemyHP = 1,
                EnemySpeed = 0.9f,
                EnemyFireRate = 0.5f,
                EnemySpawnRate = 3,
                HealthUpCount = 10,
                HealthUpSpawnRate = 4
            },
            new LevelConfig
            {
                Level = 2,
                EnemyCount = 25,
                EnemyDamage = 2,
                EnemyHP = 2,
                EnemySpeed = 1,
                EnemyFireRate = 0.3f,
                EnemySpawnRate = 2,
                HealthUpCount = 9,
                HealthUpSpawnRate = 6
            }
        };

        public static EventKey WaveClearedEventKey = new EventKey("Global", "WaveCleared");
        public static EventKey PlayerDeathEventKey = new EventKey("Global", "PlayerDeath");
        public static EventKey PlayerDamageEventKey = new EventKey("Global", "PlayerDamage");
        public static EventKey EnemyDeathEventKey = new EventKey("Global", "EnemyDeath");
        public static EventKey EnemyDamageEventKey = new EventKey("Global", "EnemyDamage");
    }
}
