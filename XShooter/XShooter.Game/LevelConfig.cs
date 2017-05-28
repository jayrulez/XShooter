using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XShooter
{
    public class LevelConfig
    {
        public int Level { get; set; }
        public int EnemyCount { get; set; }
        public int EnemyHP { get; set; }
        public int EnemyDamage { get; set; }
        public float EnemySpeed { get; set; }
        public float EnemyFireRate { get; set; }
        public float EnemySpawnRate { get; set; }
        public float HealthUpCount { get; set; }
        public float HealthUpSpawnRate { get; set; }
    }
}
