using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Audio;

namespace XShooter
{
    public class PlayerScript : SyncScript
    {
        [Flags]
        private enum InputState
        {
            None = 0,
            Shoot = 1 << 0,
            Up = 1 << 1,
            Down = 1 << 2,
            Left = 1 << 3,
            Right = 1 << 4
        }

        private InputState _inputState;
        private Vector3 _velocity;
        private float _speed;

        public int HitPoints { get; private set; }
        public Sound ShotSound { get; set; }
        public Sound HealthUpSound { get; set; }
        private SoundInstance ShotSoundInstance;
        private SoundInstance HealthUpSoundInstance;

        private Prefab _playerBulletPrefab;

        public float ShotReloadTime { get; set; } = 0.17f;
        private float _shotCoolDownTime;

        public int Score { get; private set; }

        public override void Start()
        {
            _inputState = InputState.None;
            _speed = 300;

            HitPoints = 10;
            ShotSoundInstance = ShotSound.CreateInstance();
            HealthUpSoundInstance = HealthUpSound.CreateInstance();
            _playerBulletPrefab = Content.Load<Prefab>("PlayerBullet");

            _shotCoolDownTime = 0;
            Score = 0;
        }

        public override void Update()
        {
            if (GameGlobals.LevelState == LevelState.Playing)
            {
                ProcessInput();

                UpdateTransform();

                _shotCoolDownTime -= (float)Game.UpdateTime.Elapsed.TotalSeconds;

                if (_inputState.HasFlag(InputState.Shoot))
                {
                    if(_shotCoolDownTime <= 0)
                    {
                        FireShot();
                        _shotCoolDownTime = ShotReloadTime;
                    }
                }
            }
        }

        private void ProcessInput()
        {
            _inputState = InputState.None;

            if (Input.IsKeyDown(Keys.Space))
            {
                _inputState |= InputState.Shoot;
            }

            if (Input.IsKeyDown(Keys.Left))
            {
                _inputState |= InputState.Left;
            }

            if (Input.IsKeyDown(Keys.Right))
            {
                _inputState |= InputState.Right;
            }

            if (Input.IsKeyDown(Keys.Up))
            {
                _inputState |= InputState.Up;
            }

            if (Input.IsKeyDown(Keys.Down))
            {
                _inputState |= InputState.Down;
            }
        }

        private void UpdateTransform()
        {
            var position = new Vector3(Entity.Transform.Position.X, Entity.Transform.Position.Y, Entity.Transform.Position.Z);

            var deltaTime = (float)Game.UpdateTime.Elapsed.TotalMilliseconds / 1000;

            if (_inputState.HasFlag(InputState.Left))
            {
                position.X -= _speed * deltaTime;
            }

            if (_inputState.HasFlag(InputState.Up))
            {
                position.Y += _speed * deltaTime;
            }

            if (_inputState.HasFlag(InputState.Right))
            {
                position.X += _speed * deltaTime;
            }

            if (_inputState.HasFlag(InputState.Down))
            {
                position.Y -= _speed * deltaTime;
            }

            _velocity = position - Entity.Transform.Position;

            Entity.Get<CharacterComponent>().SetVelocity(_velocity);
        }

        private void FireShot()
        {
            var bulletInstance = _playerBulletPrefab.Instantiate();

            var bullet = bulletInstance[0];

            var playerSprite = Entity.Get<SpriteComponent>();

            //var gunPosition = Entity.GetChildren().FirstOrDefault(e => e.Name.Equals("GunPosition"));

            bullet.Transform.Position = new Vector3(Entity.Transform.Position.X, Entity.Transform.Position.Y + 0.35f, Entity.Transform.Position.Z);// gunPosition.Transform.Position;

            bullet.Transform.UpdateWorldMatrix();

            SceneSystem.SceneInstance.RootScene.Entities.Add(bullet);

            bullet.Get<RigidbodyComponent>().LinearVelocity = new Vector3(0, 20, 0);

            ShotSoundInstance.Stop();
            ShotSoundInstance.Volume = 0.5f;
            ShotSoundInstance.Play();
        }

        public void TakeDamage(int damage)
        {
            HitPoints -= damage;

            GameGlobals.PlayerDamageEventKey.Broadcast();

            if (HitPoints <= 0)
            {
                HitPoints = 0;

                GameGlobals.PlayerDeathEventKey.Broadcast();
            }
        }

        public void AddHealth(int health)
        {
            HealthUpSoundInstance.Play();
            HitPoints += health;
        }

        public void AddScore(int score)
        {
            Score += score;
        }
    }
}
