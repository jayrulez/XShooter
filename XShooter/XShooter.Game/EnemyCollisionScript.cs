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

namespace XShooter
{
    public class EnemyCollisionScript : AsyncScript
    {
        public override async Task Execute()
        {
            if (Game.IsRunning)
            {
                var rigidBody = Entity.Get<RigidbodyComponent>();

                var enemyScript = Entity.Get<EnemyShipScript>();

                while (Game.IsRunning)
                {
                    await Script.NextFrame();

                    var collision = await rigidBody.NewCollision();

                    if (collision.ColliderA.Entity.Name == "PlayerBullet" && rigidBody.IsTrigger)
                    {
                        var script = collision.ColliderA.Entity.Get<PlayerBulletScript>();

                        script.Die();

                        enemyScript.TakeDamage(1);
                    }
                    else if (collision.ColliderB.Entity.Name == "PlayerBullet" && rigidBody.IsTrigger)
                    {
                        var script = collision.ColliderB.Entity.Get<PlayerBulletScript>();

                        script.Die();

                        enemyScript.TakeDamage(1);
                    }
                    else if (collision.ColliderA.Entity.Name == "PlayerShip")
                    {
                        var script = collision.ColliderA.Entity.Get<PlayerScript>();

                        script.TakeDamage(enemyScript.Damage);

                        enemyScript.TakeDamage(1);
                    }
                    else if (collision.ColliderB.Entity.Name == "PlayerShip")
                    {
                        var script = collision.ColliderB.Entity.Get<PlayerScript>();

                        script.TakeDamage(enemyScript.Damage);

                        enemyScript.TakeDamage(1);
                    }
                }
            }
        }
    }
}
