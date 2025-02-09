﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BoundingVolumes;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class ObstaculoMapa : Colisionable
    {
        public ObstaculoMapa(Nave naveDelJugador, TgcMesh mesh) : base(naveDelJugador)
        {
            this.mainMesh = mesh;
        }

        public override void Init(){}

        public override void Render()
        {
            /*
            if (ColisionaConNave())
            {
                mainMesh.BoundingBox.setRenderColor(Color.Red);//para "debuggear"
            }
            mainMesh.BoundingBox.Render();
        */
        }

        public override void Dispose(){}

        public override void Update(float elapsedTime){}
    }
}
