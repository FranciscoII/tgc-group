﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Core.Text;
using TGC.Examples.Camara;
using TGC.Group.Model.Clases2D;
using TGC.Group.Model.Mundo;

namespace TGC.Group.Model.Meta
{
    internal class EntornoGameOver : Entorno
    {
        private MenuGameOver menuGameOver;
        private TgcCamera camaraDeMenu;
        private NaveGameOver nave;
        private TgcMp3Player tgcMp3Player;

        public EntornoGameOver(GameModel gameModel, string mediaDir, InputDelJugador input, string shaderDir) : base(gameModel, mediaDir, input, shaderDir)
        {
            menuGameOver = new MenuGameOver(mediaDir);
            camaraDeMenu = new TgcCamera();
            camaraDeMenu.SetCamera(new TGCVector3(0, 20, 250), new TGCVector3(0, 20, 0));
        }

        public override void Init()
        {
            var posicionInicialDeNave = new TGCVector3(0, 0, 0);
            nave = new NaveGameOver(mediaDir, posicionInicialDeNave);
            GameManager.Instance.AgregarRenderizable(new SkyboxMenu(mediaDir, new TGCVector3(0, 20, 0)));
            GameManager.Instance.AgregarRenderizable(nave);

            gameModel.CambiarCamara(camaraDeMenu);
            GameManager.Instance.PosicionSol = new TGCVector3(3, 15, -1);
            tgcMp3Player = new TgcMp3Player();
            tgcMp3Player.FileName = mediaDir + "death-music.mp3";
            tgcMp3Player.play(false);
        }
        
        public override void Update(float elapsedTime)
        {
            if (input.HayInputDePausa())//Enter
            {
                nave.IniciarAnimacion();
            }

            GameManager.Instance.Update(elapsedTime);
            if (nave.GetPosicion().X > 300f) //la nave se va de la pantalla.
                CambiarEntorno(new EntornoJuego(gameModel,mediaDir,input,shaderDir));
        }

        public override void Render()
        {
            gameModel.AntesDelRender();
            GameManager.Instance.Render();
            menuGameOver.DibujarMenu();
            gameModel.DespuesDelRender();

        }
        public override void Dispose()
        {
            GameManager.Instance.Dispose();
        }
        public override void CambiarEntorno(Entorno nuevoEntorno)
        {
            tgcMp3Player.stop();
            tgcMp3Player.closeFile();
            base.CambiarEntorno(nuevoEntorno);
        }
    }
}
