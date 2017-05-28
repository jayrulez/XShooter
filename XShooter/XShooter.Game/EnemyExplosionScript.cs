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
using SiliconStudio.Xenko.Animations;

namespace XShooter
{
    public class EnemyExplosionScript : SyncScript
    {
        private SpriteComponent SpriteComponent;

        public override void Start()
        {
            SpriteComponent = Entity.Get<SpriteComponent>();

            SpriteAnimation.Play(SpriteComponent, 0, SpriteComponent.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.PlayOnce);
        }

        public override void Update()
        {
            if (SpriteComponent.CurrentFrame == SpriteComponent.SpriteProvider.SpritesCount - 1)
            {
                SceneSystem.SceneInstance.RootScene.Entities.Remove(Entity);
            }
        }
    }
}
