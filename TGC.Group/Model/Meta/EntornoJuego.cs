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
using TGC.Core.Shaders;
using TGC.Core.Sound;
using TGC.Core.Text;
using TGC.Examples.Camara;
using TGC.Group.Model.Clases2D;
using TGC.Group.Model.NaveJugador;

namespace TGC.Group.Model.Meta
{
    internal class EntornoJuego : Entorno
    {
        private EscenarioLoader escenarioLoader;
        private TieFighterSpawner tieFighterSpawner;
        private Nave naveDelJuego;
        private List<Light> lights;
        private TgcMp3Player tgcMp3Player;
        private TgcCamera CamaraActual;
        private TgcCamera CamaraSiguiente;
        private Skybox skybox;
        private float cooldownCambioCamara;

        public EntornoJuego(GameModel gameModel, string mediaDir, InputDelJugador input, string shaderDir) : base(gameModel, mediaDir, input, shaderDir)
        {
        }

        public override void Init()
        {
            var posicionInicialDeNave = new TGCVector3(105, -15, 420);
            naveDelJuego = new Nave(mediaDir, posicionInicialDeNave, input);
            
            GameManager.Instance.AgregarRenderizable(naveDelJuego);
            CamaraJugadorFija camaraFija = new CamaraJugadorFija(posicionInicialDeNave, 10, -50, naveDelJuego);
            gameModel.CambiarCamara(camaraFija);
            skybox = new Skybox(mediaDir, camaraFija);
            CamaraActual = camaraFija;
            cooldownCambioCamara = 0f;
            CamaraJugadorDinamica camaraDinamica = new CamaraJugadorDinamica(posicionInicialDeNave, 10, -50, naveDelJuego);
            CamaraSiguiente = camaraDinamica;
            GameManager.Instance.AgregarRenderizable(skybox);
            escenarioLoader = new EscenarioLoader(mediaDir, naveDelJuego);
            tieFighterSpawner = new TieFighterSpawner(mediaDir, naveDelJuego);
            tieFighterSpawner.SpawnTieFighterDecorativo();
            GameManager.Instance.ReanudarOPausarJuego();
            CreateFullScreenQuad();
            //CreateRenderTarget();
            InitializeFrameBuffers();
            InitializeLights();
            effect = TGCShaders.Instance.LoadEffect(shaderDir + "Bloom.fx");
            effect.SetValue("screen_dx", D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight);
            tgcMp3Player = new TgcMp3Player();
            tgcMp3Player.FileName = mediaDir + "tema.mp3";
            tgcMp3Player.play(true);
        }

        public override void Update(float elapsedTime)
        {
            //UPDATE del Bloom-----------------------
            effect.SetValue("scene", true);
            effect.SetValue("bloom", true);
            effect.SetValue("toneMapping", true);
            effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat3Array(GameManager.Instance.EyePosition()));
            InitializeLights();
            int index = 0;
            lights.ForEach(light => light.SetLight(index++, effect));
            effect.SetValue("lightCount", lights.Count);
            cooldownCambioCamara += elapsedTime;
            //-------------------

            if (input.HayInputDePausa())
            {
                
                GameManager.Instance.ReanudarOPausarJuego();
            }
            if (input.HayInputDeCambioDeCamara())
            {
                CambiarCamara();
            }

            GameManager.Instance.Update(elapsedTime);
            escenarioLoader.Update(elapsedTime);
            tieFighterSpawner.Update(elapsedTime);
            if (!naveDelJuego.estaVivo)
                CambiarEntorno(new EntornoGameOver(gameModel, mediaDir, input,shaderDir));
        }

        private void CambiarCamara()
        {
            if (cooldownCambioCamara > 0.5f)
            {
                TgcCamera CamaraViejaACambiar = CamaraActual;
                CamaraActual = CamaraSiguiente;
                CamaraSiguiente = CamaraViejaACambiar;
                gameModel.CambiarCamara(CamaraActual);
                skybox.CambiarCamara(CamaraActual);
                cooldownCambioCamara = 0f;
            }


        }

        public override void Dispose()
        {
            depthStencil.Dispose();
            effect.Dispose();
            glowyObjectsFrameBuffer.Dispose();
            GameManager.Instance.Dispose();
        }
        public override void Render()
        {
            var device = D3DDevice.Instance.Device;

            var screenSurface = device.GetRenderTarget(0);
            var originalDepthStencil = device.DepthStencilSurface;

            // --------------
            // Guardamos la escena en un framebuffer

            BeginScene(device, sceneFrameBuffer.GetSurfaceLevel(0), depthStencil);

            //ACA RENDEREAR 
            if (GameManager.Instance.estaPausado)
            {
                string textoControles = "Controles:\nWASD: Moverse\nQ: Rollear\nE: Voltearse\nShift: Acelerar\nCtrl: Desacelerar\nEnter: Pausar/Despausar\nTab: Cambiar camara";
                TgcText2D textoDrawer = new TgcText2D();
                textoDrawer.Text = textoControles;
                textoDrawer.changeFont(new System.Drawing.Font("Calibri", 0.009765625f * D3DDevice.Instance.Width));
                textoDrawer.drawText(textoControles, D3DDevice.Instance.Width / 40, D3DDevice.Instance.Height / 20, Color.White);
            }
            ConfigureBlinnForShip();
            naveDelJuego.GetModelo().CambiarShader(effect,"Luzbelito");

            GameManager.Instance.Render();


            device.EndScene();

            // --------------
            // Guardamos lo brillante de la escena en un framebuffer
            
            BeginScene(device, glowyObjectsFrameBuffer.GetSurfaceLevel(0), depthStencil);


            var meshes = naveDelJuego.GetModelo().GetMeshes();
            meshes[0].Technique = "GlowyObjects";
            meshes[1].Technique = "GlowyObjects";
            meshes[2].Technique = "GlowyObjects";

            meshes[0].Render();
            meshes[1].Render();
            meshes[2].Render();

            device.EndScene();
            meshes[0].Technique = "Luzbelito";
            meshes[1].Technique = "Luzbelito";
            meshes[2].Technique = "Luzbelito";
            // --------------
            // Aplicamos una pasada de blur horizontal al framebuffer de los objetos brillantes
            // y una pasada vertical
            var passBuffer = glowyObjectsFrameBuffer;
            for (int index = 0; index < 15; index++)
            {
                BeginScene(device, bloomHorizontalFrameBuffer.GetSurfaceLevel(0), depthStencil);

                effect.Technique = "HorizontalBlur";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, fullScreenQuad, 0);
                effect.SetValue("glowyFrameBuffer", passBuffer);

                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();

                device.EndScene();


                BeginScene(device, bloomVerticalFrameBuffer.GetSurfaceLevel(0), depthStencil);

                effect.Technique = "VerticalBlur";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, fullScreenQuad, 0);
                effect.SetValue("glowyFrameBuffer", bloomHorizontalFrameBuffer);

                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();

                device.EndScene();

                passBuffer = bloomVerticalFrameBuffer;
            }

            // --------------
            // Este postprocesado integra lo blureado a la escena
            
            BeginScene(device, screenSurface, originalDepthStencil);
            
            effect.Technique = "Integrate";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, fullScreenQuad, 0);
            effect.SetValue("verticalBlurFrameBuffer", bloomVerticalFrameBuffer);
            effect.SetValue("sceneFrameBuffer", sceneFrameBuffer);

            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            //gameModel.ShowFPS();

            device.EndScene();
            device.Present();

        }
        private void InitializeLights()
        {
            lights = new List<Light>();
            var posNave = naveDelJuego.GetPosicion();
            // Red
            var red = new Light
            {
                Enabled = true,
                Position = new TGCVector3(posNave.X, posNave.Y, posNave.Z),
                AmbientColor = new TGCVector3(0.2f, 0.1f, 0.1f),
                DiffuseColor = new TGCVector3(1f, 0.0f, 0.0f),
                SpecularColor = new TGCVector3(0.9f, 0.8f, 0.8f),
            };
            var yellow = new Light
            {
                Enabled = true,
                Position = new TGCVector3(posNave.X+0.5f, posNave.Y, posNave.Z),
                AmbientColor = new TGCVector3(0.2f, 0.2f, 0.05f),
                DiffuseColor = new TGCVector3(1f, 1f, 0f),
                SpecularColor = new TGCVector3(0.85f, 0.85f, 0.65f)
            };
            var green = new Light
            {
                Enabled = true,
                Position = new TGCVector3(posNave.X, posNave.Y, posNave.Z),
                AmbientColor = new TGCVector3(0.2f, 0.1f, 0.1f),
                DiffuseColor = new TGCVector3(1f, 0.0f, 0.0f),
                SpecularColor = new TGCVector3(0.9f, 0.8f, 0.8f),
            };
            lights.Add(red);
            //lights.Add(yellow);
            //lights.Add(green);
        }

        private void ConfigureBlinnForShip()
        {
            effect.SetValue("KAmbient", 0.3f);
            effect.SetValue("KDiffuse", 0.6f);
            effect.SetValue("KSpecular", 0.5f);
            effect.SetValue("shininess", 10f);
        }

        public override void CambiarEntorno(Entorno nuevoEntorno)
        {
            tgcMp3Player.stop();
            tgcMp3Player.closeFile();
            base.CambiarEntorno(nuevoEntorno);
        }
    }
}
