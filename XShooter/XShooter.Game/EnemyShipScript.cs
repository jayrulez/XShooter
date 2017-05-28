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
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Audio;

namespace XShooter
{
    public class EnemyShipScript : SyncScript
    {
        private int _hp { get; set; }
        private int _damage { get; set; }
        private float _fireRate { get; set; }
        private bool _configured = false;
        private Vector3 _linearVelocity;
        private Prefab _bulletPrefab;
        private Prefab _explosionPrefab;

        private float _fireCoolDown;

        public Sound DeathExplosion { get; set; }

        public int Damage { get { return _damage; } }

        public override void Start()
        {
            if (!_configured)
            {
                _hp = 1;
                _damage = 1;
                _fireRate = 3;
            }

            _fireCoolDown = 0;

            _bulletPrefab = Content.Load<Prefab>("EnemyBullet");
            _explosionPrefab = Content.Load<Prefab>("EnemyExplosion");
        }

        public override void Update()
        {
            if (GameGlobals.LevelState == LevelState.Playing)
            {
                Entity.Get<RigidbodyComponent>().LinearVelocity = _linearVelocity;

                _fireCoolDown -= (float)Game.UpdateTime.Elapsed.TotalSeconds;

                if (_fireCoolDown <= 0)
                {
                    var random = new Random();

                    if (random.Next(0, 2) == 1)
                    {
                        FireShot();
                    }

                    _fireCoolDown = _fireRate;
                }

                if (Entity.Transform.Position.X < -4f || Entity.Transform.Position.X > 4f || Entity.Transform.Position.Y < -4.5f)
                {
                    Remove();
                }
            }
            else
            {
                Entity.Get<RigidbodyComponent>().LinearVelocity = Vector3.Zero;
            }
        }

        public void Configure(int hp, int damage, float fireRate, Vector3 linearVelocity)
        {
            _hp = hp;
            _damage = damage;
            _fireRate = fireRate;
            _linearVelocity = linearVelocity;
            _configured = true;
        }

        private void FireShot()
        {
            var bulletInstance = _bulletPrefab.Instantiate();

            var bullet = bulletInstance[0];

            var playerSprite = Entity.Get<SpriteComponent>();

            var spawnPosition = new Vector3(Entity.Transform.Position.X, Entity.Transform.Position.Y - 0.35f, Entity.Transform.Position.Z);

            bullet.Transform.Position = spawnPosition;

            bullet.Transform.UpdateWorldMatrix();

            SceneSystem.SceneInstance.RootScene.Entities.Add(bullet);

            bullet.Get<RigidbodyComponent>().IsKinematic = false;
            bullet.Get<RigidbodyComponent>().LinearVelocity = new Vector3(0, -5, 0);
        }

        private void Remove()
        {
            SceneSystem.SceneInstance.RootScene.Entities.Remove(Entity);
        }

        public void Die()
        {
            var explosionInstance = _explosionPrefab.Instantiate()[0];
            explosionInstance.Transform.Position = Entity.Transform.Position;
            explosionInstance.Transform.UpdateWorldMatrix();

            Remove();

            SceneSystem.SceneInstance.RootScene.Entities.Add(explosionInstance);

            var explosionSound = DeathExplosion.CreateInstance();
            explosionSound.Volume = 0.3f;
            explosionSound.Play();

            GameGlobals.EnemyDeathEventKey.Broadcast();
        }

        public void TakeDamage(int damage)
        {
            _hp -= damage;

            if (_hp <= 0)
            {
                Die();
            }
        }
    }
}
