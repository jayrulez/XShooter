// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Events;
using SiliconStudio.Xenko.Audio;
using System.Collections;
using System.Diagnostics;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Engine.Events;

namespace XShooter
{
    public class LevelManagerScript : SyncScript
    {
        public UIPage GameHUD { get; set; }

        private Button PlayButton { get; set; }
        private Button PauseButton { get; set; }
        private Button MuteButton { get; set; }
        private Button UnmuteButton { get; set; }

        public Sound BackgroundMusic { get; set; }
        private SoundInstance BackgroundMusicInstance { get; set; }

        public TransformComponent PlayerSpawn;
        public TransformComponent TopSpawn;

        private Prefab _playerPrefab;
        private Entity _playerEntity;
        private TextBlock _scoreText;
        private Prefab _enemyPrefab;
        private Prefab _healthUpPrefab;

        private TextBlock _healthText { get; set; }

        private bool _gameOver;

        private int _currentScore;

        private int _currentLevel = 1;

        public float EnemyWaveSpawnWait { get; set; } = 3f;
        private bool _enemyWaveReady = true;

        private EventReceiver WaveClearedListener = new EventReceiver(GameGlobals.WaveClearedEventKey);
        private EventReceiver EnemyDeathListener = new EventReceiver(GameGlobals.EnemyDeathEventKey);
        private EventReceiver PlayerDeathListener = new EventReceiver(GameGlobals.PlayerDeathEventKey);

        public override void Start()
        {
            BackgroundMusicInstance = BackgroundMusic.CreateInstance();

            BackgroundMusicInstance.Volume = 0.25f;

            PlayButton = GameHUD?.RootElement.FindVisualChildOfType<Button>("PlayButton");

            PlayButton.Click += (object sender, RoutedEventArgs routedEventArgs) =>
            {
                GameGlobals.LevelState = LevelState.Playing;
            };

            PauseButton = GameHUD?.RootElement.FindVisualChildOfType<Button>("PauseButton");

            PauseButton.Click += (object sender, RoutedEventArgs routedEventArgs) =>
            {
                GameGlobals.LevelState = LevelState.Paused;
            };

            MuteButton = GameHUD?.RootElement.FindVisualChildOfType<Button>("MuteButton");

            MuteButton.Click += (object sender, RoutedEventArgs routedEventArgs) =>
            {
                GameGlobals.SoundState = SoundState.Muted;
            };

            UnmuteButton = GameHUD?.RootElement.FindVisualChildOfType<Button>("UnmuteButton");

            UnmuteButton.Click += (object sender, RoutedEventArgs routedEventArgs) =>
            {
                GameGlobals.SoundState = SoundState.Unmuted;
            };

            _playerPrefab = Content.Load<Prefab>("Player");
            _enemyPrefab = Content.Load<Prefab>("EnemyShip");
            _healthUpPrefab = Content.Load<Prefab>("HealthUp");

            _scoreText = GameHUD?.RootElement.FindVisualChildOfType<TextBlock>("ScoreText");

            _healthText = GameHUD?.RootElement.FindVisualChildOfType<TextBlock>("HealthText");

            //var camera = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(e => e.Name.Equals("Camera"));

            Script.AddTask(async () =>
            {
                await PlayerDeathListener.ReceiveAsync();

                _gameOver = true;

                BackgroundMusicInstance.Stop();

                SceneSystem.SceneInstance.RootScene = Content.Load<Scene>("MainScene");

            });

            Script.AddTask(async () =>
            {
                await EnemyDeathListener.ReceiveAsync();

                _playerEntity.Get<PlayerScript>()?.AddScore(1);
            });

            Initialize();
        }

        private void Initialize()
        {
            _gameOver = false;
            _currentScore = 0;
            SpawnPlayer();

            GameGlobals.LevelState = LevelState.Playing;
            GameGlobals.SoundState = SoundState.Unmuted;

            Task.Run(() =>
            {
                SpawnEnemyWaves();
            });

            Task.Run(() =>
            {
                SpawnHealthUps();
            });
        }

        private LevelConfig GetCurrentLevelConfig()
        {
            if (_currentLevel <= 0)
            {
                _currentLevel = 1;
            }

            var levelConfig = GameGlobals.LevelConfigs.FirstOrDefault(level => level.Level == _currentLevel);

            if (levelConfig == null)
            {
                throw new Exception($"No configuration was found for level '{_currentLevel}'.");
            }

            return levelConfig;
        }

        private void SpawnPlayer()
        {
            if (!SceneSystem.SceneInstance.RootScene.Entities.Any(e => e.Name == "Player"))
            {
                var playerInstance = _playerPrefab.Instantiate();

                _playerEntity = playerInstance[0];

                _playerEntity.Transform.Position = PlayerSpawn.Position;

                _playerEntity.Transform.UpdateWorldMatrix();

                SceneSystem.SceneInstance.RootScene.Entities.Add(_playerEntity);
            }
        }

        private void SpawnEnemyWaves()
        {
            while (!_gameOver)
            {
                if (GameGlobals.LevelState == LevelState.Playing && _enemyWaveReady)
                {
                    WaitForSeconds(EnemyWaveSpawnWait);

                    var levelConfig = GetCurrentLevelConfig();

                    var spawnedEnemies = 0;

                    while (spawnedEnemies < levelConfig.EnemyCount)
                    {
                        if (GameGlobals.LevelState == LevelState.Playing)
                        {
                            _enemyWaveReady = false;

                            SpawnEnemy(levelConfig);

                            spawnedEnemies++;

                            WaitForSeconds(levelConfig.EnemySpawnRate);
                        }
                    }

                    spawnedEnemies = 0;
                }
            }
        }

        private void SpawnHealthUps()
        {
            while (!_gameOver)
            {
                if (GameGlobals.LevelState == LevelState.Playing && !_enemyWaveReady)
                {
                    var levelConfig = GetCurrentLevelConfig();

                    WaitForSeconds(levelConfig.HealthUpSpawnRate);

                    var spawnedHealthUps = 0;

                    while (spawnedHealthUps < levelConfig.HealthUpCount)
                    {
                        if (GameGlobals.LevelState == LevelState.Playing && !_enemyWaveReady)
                        {
                            SpawnHealthUp();

                            spawnedHealthUps++;

                            WaitForSeconds(levelConfig.HealthUpSpawnRate);
                        }
                    }

                    spawnedHealthUps = 0;
                }
            }
        }

        private float WaitForSeconds(float seconds)
        {
            var stopWatch = Stopwatch.StartNew();

            while (stopWatch.Elapsed.TotalMilliseconds < (seconds * 1000)) { }

            return seconds;
        }

        private Task SpawnEnemy(LevelConfig levelConfig)
        {
            var enemyInstance = _enemyPrefab.Instantiate()[0];

            var random = new Random();

            enemyInstance.Transform.Position = new Vector3(random.Next(-3, 3), TopSpawn.Position.Y, TopSpawn.Position.Z);

            var script = enemyInstance.Get<EnemyShipScript>();

            script.Configure(levelConfig.EnemyHP, levelConfig.EnemyDamage, levelConfig.EnemyFireRate, new Vector3((float)random.Next(-1, 2), -1f, 0) * levelConfig.EnemySpeed);

            enemyInstance.Transform.UpdateWorldMatrix();

            SceneSystem.SceneInstance.RootScene.Entities.Add(enemyInstance);

            return Task.FromResult(0);
        }

        public void SpawnHealthUp()
        {
            var healthUpInstance = _healthUpPrefab.Instantiate()[0];

            var random = new Random();

            var spawnPosition = new Vector3(random.Next(-3, 3), 4.5f, 0);

            healthUpInstance.Transform.Position = spawnPosition;

            healthUpInstance.Transform.UpdateWorldMatrix();

            SceneSystem.SceneInstance.RootScene.Entities.Add(healthUpInstance);

            healthUpInstance.Get<RigidbodyComponent>().LinearVelocity = new Vector3(0, -0.5f, 0);
        }

        public override void Update()
        {
            UpdateGameHUD();

            UpdateBackgroundMusicSate();

            if (Input.IsKeyPressed(Keys.Space) && _enemyWaveReady == false)
            {
                GameGlobals.WaveClearedEventKey.Broadcast();
            }

            if (WaveClearedListener.TryReceive())
            {
                var nextLevel = GameGlobals.LevelConfigs.FirstOrDefault(level => level.Level == _currentLevel + 1);

                if (nextLevel != null)
                {
                    _currentLevel = nextLevel.Level;
                }

                _enemyWaveReady = true;
            }
        }

        private void UpdateBackgroundMusicSate()
        {
            if (GameGlobals.LevelState == LevelState.Paused)
            {
                BackgroundMusicInstance.Pause();
            }
            else
            {
                if (BackgroundMusicInstance.PlayState != SoundPlayState.Playing)
                {
                    if (GameGlobals.SoundState == SoundState.Unmuted)
                    {
                        BackgroundMusicInstance.Play();
                    }
                }
                else
                {
                    if (GameGlobals.SoundState == SoundState.Muted)
                    {
                        BackgroundMusicInstance.Pause();
                    }
                }
            }
        }

        private void UpdateGameHUD()
        {
            PlayButton.Visibility = GameGlobals.LevelState == LevelState.Playing ? Visibility.Hidden : Visibility.Visible;
            PauseButton.Visibility = GameGlobals.LevelState == LevelState.Paused ? Visibility.Hidden : Visibility.Visible;

            MuteButton.Visibility = GameGlobals.SoundState == SoundState.Muted ? Visibility.Hidden : Visibility.Visible;
            UnmuteButton.Visibility = GameGlobals.SoundState == SoundState.Unmuted ? Visibility.Hidden : Visibility.Visible;

            _scoreText.Text = _currentScore.ToString();

            _healthText.Text = _playerEntity.Get<PlayerScript>()?.HitPoints.ToString();
            _scoreText.Text = _playerEntity.Get<PlayerScript>()?.Score.ToString();
        }
    }
}
