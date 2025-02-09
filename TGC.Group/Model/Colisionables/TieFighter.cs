﻿using TGC.Core.Mathematica;
using TGC.Core.Text;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using TGC.Core.BoundingVolumes;
using System.Drawing.Text;

namespace TGC.Group.Model
{
    class TieFighter : Destruible
    {
        private TGCVector3 posicion;
        private ModeloCompuesto modeloNave;
        private string mediaDir;
        private TGCMatrix matrizEscala;
        private TGCMatrix matrizPosicion;
        private TGCMatrix matrizRotacion;
        private float coolDownDisparo;
        private TieFighterSpawner spawner;

        //private TgcBoundingAxisAlignBox boundingBox;

        public TieFighter(string mediaDir, TGCVector3 posicionInicial,Nave jugador, TieFighterSpawner spawner) : base(jugador)
        {
            this.mediaDir = mediaDir;
            this.posicion = posicionInicial;
            this.modeloNave = new ModeloCompuesto(mediaDir + "XWing\\xwing-TgcScene.xml", posicion);
            coolDownDisparo = 0f;
            this.spawner = spawner;
        }


        public override void Init()
        {

            matrizEscala = TGCMatrix.Scaling(.5f, .5f, .5f);
            matrizPosicion = TGCMatrix.Translation(posicion);
            TGCQuaternion rotacionInicial = TGCQuaternion.RotationAxis(new TGCVector3(0.0f, 1.0f, 0.0f), Geometry.DegreeToRadian(90f));
            matrizRotacion = TGCMatrix.RotationTGCQuaternion(rotacionInicial);
            mainMesh = modeloNave.GetMesh();
        }
        
        public override void Update(float elapsedTime)
        {
            if (GameManager.Instance.estaPausado)
                return;
            base.Update(elapsedTime);

            modeloNave.CambiarRotacion(new TGCVector3(0f, Geometry.DegreeToRadian(0f), 0f));
            if (NaveEstaMuyLejos())
            {
                Acercarse(elapsedTime);
            }
            else
            {
                IrALaVelocidadDeLaNave(elapsedTime);
                Disparar(naveDelJugador.GetPosicion(), elapsedTime);
            }
            //matrizEscala * matrizRotacion *matrizPosicion;
            modeloNave.UpdateShader(GameManager.Instance.PosicionSol, GameManager.Instance.EyePosition(),0f);
        }
        private bool NaveEstaMuyLejos()
        {
            return posicion.Z - naveDelJugador.GetPosicion().Z >= 100f;
        }
        public override void Render()
        {
            modeloNave.AplicarTransformaciones();
            modeloNave.Render();
            //new TgcText2D().drawText("COOLDOWN: " + coolDownDisparo.ToString(), 5, 250, Color.White);

        }

        public override void Dispose()
        {
            liberarPosicionEnSpawner();
            modeloNave.Dispose();
        }

        private void Acercarse(float elapsedTime)
        {
            TGCVector3 versorDirector = new TGCVector3(0, 0, -1);
            TGCVector3 movimientoDelFrame = new TGCVector3(0, 0, 0);
            TGCVector3 movimientoAdelante = new TGCVector3(0, 0, 1);
            movimientoDelFrame += versorDirector + movimientoAdelante;
            movimientoDelFrame *= 10f * elapsedTime * 8f;
            posicion += movimientoDelFrame;
            modeloNave.CambiarPosicion(posicion);
        }
        private void IrALaVelocidadDeLaNave(float elapsedTime)
        {
            TGCVector3 versorDirector = new TGCVector3(0, 0, 1);
            TGCVector3 movimientoDelFrame = new TGCVector3(0, 0, 0);
            TGCVector3 movimientoAdelante = new TGCVector3(0, 0, 1);
            movimientoDelFrame += versorDirector + movimientoAdelante;
            movimientoDelFrame *= 10f * elapsedTime * naveDelJugador.GetVelocidad();
            posicion += movimientoDelFrame;
            modeloNave.CambiarPosicion(posicion);
        }
        private void Disparar(TGCVector3 posicionNave,float tiempoTranscurrido)
        {
            coolDownDisparo += tiempoTranscurrido;
            posicionNave.Z += 15f; //TODO modificar este valor respecto la velocidad de la nave
            TGCVector3 direccionDisparo = posicionNave - posicion;
            if(coolDownDisparo > 4f)
            {
                GameManager.Instance.AgregarRenderizable(new LaserEnemigo(mediaDir, posicion, direccionDisparo, naveDelJugador));
                coolDownDisparo = 0f;
            }
            

        }

        public override TgcBoundingAxisAlignBox GetBoundingBox()
        {
            return modeloNave.BoundingBoxesDelModelo()[0]; //TODO: Hacer que esto sea mas decente 
        }

        private void liberarPosicionEnSpawner()
        {
            TGCVector2 posicionLibre = new TGCVector2(this.posicion.X, this.posicion.Y);
            spawner.AgregarPosicionLibre(posicionLibre);
        }

    }
}
