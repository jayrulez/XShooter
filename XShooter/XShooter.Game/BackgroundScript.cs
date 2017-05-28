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
using SiliconStudio.Xenko.Audio;

namespace XShooter
{
    public class BackgroundScript : SyncScript
    {
        private Vector3 _startPosition;

        public float ScrollSpeed { get; set; } = 0.0025f;
        public float TileSize { get; set; } = 8f;

        public override void Start()
        {
            _startPosition = Entity.Transform.Position;
            
            var spriteComponent = Entity.Get<SpriteComponent>();

            TileSize = spriteComponent.CurrentSprite.Size.Y;

            ScrollSpeed = 0.5f;
        }

        private float Repeat(float t, float length)
        {
            return (t - (float)Math.Floor(t / length) * length);
        }

        public override void Update()
        {
            if (GameGlobals.LevelState == LevelState.Playing)
            {
                var newPosition = Repeat((float)Game.UpdateTime.Total.TotalSeconds * ScrollSpeed, TileSize);

                Entity.Transform.Position = _startPosition - Vector3.UnitY * newPosition;
            }
        }
    }
}
