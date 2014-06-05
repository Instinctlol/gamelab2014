using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Engine;
using Engine.Renderer;
using Engine.MathEx;
using Engine.SoundSystem;
using Engine.UISystem;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;
using Engine.FileSystem;
using Engine.Utils;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
    class OculusGameWindow : ActionGameWindow
    {
        ////HUD screen
        //Control hudControl;

        protected override void OnAttach()
        {
            base.OnAttach();

            if (OculusManager.Instance == null)
                OculusManager.Init(true);

            ////To load the HUD screen
            //hudControl = ControlDeclarationManager.Instance.CreateControl("Gui\\OculusHUD.gui");
            ////Attach the HUD screen to the this window
            //Controls.Add(hudControl);
        }


        protected override void OnDetach()
        {
            base.OnDetach();

            if (OculusManager.Instance != null)
                OculusManager.Shutdown();

        }


        protected override void OnRenderUI(GuiRenderer renderer)
        {
            base.OnRenderUI(renderer);

            //Spawner vor den Astronauten verstecken
            IEnumerable<AlienSpawner> spawnerList = Map.Instance.SceneGraphObjects.OfType<AlienSpawner>();
            foreach (AlienSpawner spawner in spawnerList)
            {
                spawner.Visible = false;
            }


            if (OculusManager.Instance != null)
                OculusManager.Instance.RenderScreenUI(renderer);

        }

        protected override void OnMouseMove()
        {
            base.OnMouseMove();

            if (OculusManager.Instance != null)
                OculusManager.Instance.OnMouseMove(MousePosition);
        }




        //void UpdateHUD()
        //{
        //    Unit playerUnit = GetPlayerUnit();

        //    hudControl.Visible = EngineDebugSettings.DrawGui;

        //    //Game

        //    hudControl.Controls["Game"].Visible = GetRealCameraType() != CameraType.Free &&
        //        !IsCutSceneEnabled();

        //    //Player
        //    string playerTypeName = playerUnit != null ? playerUnit.Type.Name : "";

        //    UpdateHUDControlIcon(hudControl.Controls["Game/PlayerIcon"], playerTypeName);
        //    hudControl.Controls["Game/Player"].Text = playerTypeName;

        //    //HealthBar
        //    {
        //        float coef = 0;
        //        if (playerUnit != null)
        //            coef = playerUnit.Health / playerUnit.Type.HealthMax;

        //        Control healthBar = hudControl.Controls["Game/HealthBar"];
        //        Vec2 originalSize = new Vec2(256, 32);
        //        Vec2 interval = new Vec2(117, 304);
        //        float sizeX = (117 - 82) + coef * (interval[1] - interval[0]);
        //        healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
        //        healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
        //    }

        //    //EnergyBar
        //    {
        //        float coef = 0;// .3f;

        //        Control energyBar = hudControl.Controls["Game/EnergyBar"];
        //        Vec2 originalSize = new Vec2(256, 32);
        //        Vec2 interval = new Vec2(117, 304);
        //        float sizeX = (117 - 82) + coef * (interval[1] - interval[0]);
        //        energyBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
        //        energyBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
        //    }

        //    //Weapon
        //    {
        //        string weaponName = "";
        //        string magazineCountNormal = "";
        //        string bulletCountNormal = "";
        //        string bulletCountAlternative = "";

        //        Weapon weapon = null;
        //        {
        //            //PlayerCharacter specific
        //            PlayerCharacter playerCharacter = playerUnit as PlayerCharacter;
        //            if (playerCharacter != null)
        //                weapon = playerCharacter.ActiveWeapon;
        //        }

        //        if (weapon != null)
        //        {
        //            weaponName = weapon.Type.FullName;

        //            Gun gun = weapon as Gun;
        //            if (gun != null)
        //            {
        //                if (gun.Type.NormalMode.BulletType != null)
        //                {
        //                    //magazineCountNormal
        //                    if (gun.Type.NormalMode.MagazineCapacity != 0)
        //                    {
        //                        magazineCountNormal = gun.NormalMode.BulletMagazineCount.ToString() + "/" +
        //                            gun.Type.NormalMode.MagazineCapacity.ToString();
        //                    }
        //                    //bulletCountNormal
        //                    if (gun.Type.NormalMode.BulletExpense != 0)
        //                    {
        //                        bulletCountNormal = (gun.NormalMode.BulletCount -
        //                            gun.NormalMode.BulletMagazineCount).ToString() + "/" +
        //                            gun.Type.NormalMode.BulletCapacity.ToString();
        //                    }
        //                }

        //                if (gun.Type.AlternativeMode.BulletType != null)
        //                {
        //                    //bulletCountAlternative
        //                    if (gun.Type.AlternativeMode.BulletExpense != 0)
        //                        bulletCountAlternative = gun.AlternativeMode.BulletCount.ToString() + "/" +
        //                            gun.Type.AlternativeMode.BulletCapacity.ToString();
        //                }
        //            }
        //        }

        //        hudControl.Controls["Game/Weapon"].Text = weaponName;
        //        hudControl.Controls["Game/WeaponMagazineCountNormal"].Text = magazineCountNormal;
        //        hudControl.Controls["Game/WeaponBulletCountNormal"].Text = bulletCountNormal;
        //        hudControl.Controls["Game/WeaponBulletCountAlternative"].Text = bulletCountAlternative;

        //        UpdateHUDControlIcon(hudControl.Controls["Game/WeaponIcon"], weaponName);
        //    }

        //    //CutScene
        //    {
        //        hudControl.Controls["CutScene"].Visible = IsCutSceneEnabled();

        //        if (CutSceneManager.Instance != null)
        //        {
        //            //CutSceneFade
        //            float fadeCoef = 0;
        //            if (CutSceneManager.Instance != null)
        //                fadeCoef = CutSceneManager.Instance.GetFadeCoefficient();
        //            hudControl.Controls["CutSceneFade"].BackColor = new ColorValue(0, 0, 0, fadeCoef);

        //            //Message
        //            {
        //                string text;
        //                ColorValue color;
        //                CutSceneManager.Instance.GetMessage(out text, out color);
        //                if (text == null)
        //                    text = "";

        //                TextBox textBox = (TextBox)hudControl.Controls["CutScene/Message"];
        //                textBox.Text = text;
        //                textBox.TextColor = color;
        //            }
        //        }
        //    }
        //}

        //Unit GetPlayerUnit()
        //{
        //    if (PlayerIntellect.Instance == null)
        //        return null;
        //    return PlayerIntellect.Instance.ControlledObject;
        //}



        //CameraType GetRealCameraType()
        //{
        //    //Replacement the camera type depending on a current unit.
        //    Unit playerUnit = GetPlayerUnit();
        //    if (playerUnit != null)
        //    {
        //        //Turret specific
        //        if (playerUnit as Turret != null)
        //        {
        //            if (cameraType == CameraType.FPS)
        //                return CameraType.TPS;
        //        }

        //        //Crane specific
        //        if (playerUnit as Crane != null)
        //        {
        //            if (cameraType == CameraType.TPS)
        //                return CameraType.FPS;
        //        }

        //        //Tank specific
        //        if (playerUnit as Tank != null)
        //        {
        //            if (cameraType == CameraType.FPS)
        //                return CameraType.TPS;
        //        }
        //    }

        //    return cameraType;
        //}


        //bool IsCutSceneEnabled()
        //{
        //    return CutSceneManager.Instance != null && CutSceneManager.Instance.CutSceneEnable;
        //}


        //void UpdateHUDControlIcon(Control control, string iconName)
        //{
        //    if (!string.IsNullOrEmpty(iconName))
        //    {
        //        string fileName = string.Format("Gui\\HUD\\Icons\\{0}.png", iconName);

        //        bool needUpdate = false;

        //        if (control.BackTexture != null)
        //        {
        //            string current = control.BackTexture.Name;
        //            current = current.Replace('/', '\\');

        //            if (string.Compare(fileName, current, true) != 0)
        //                needUpdate = true;
        //        }
        //        else
        //            needUpdate = true;

        //        if (needUpdate)
        //        {
        //            if (VirtualFile.Exists(fileName))
        //                control.BackTexture = TextureManager.Instance.Load(fileName, Texture.Type.Type2D, 0);
        //            else
        //                control.BackTexture = null;
        //        }
        //    }
        //    else
        //        control.BackTexture = null;
        //}


        static Vec2 SnapToPixel(Vec2 value, Vec2 viewportSize)
        {
            Vec2 result = value;
            result *= viewportSize;
            result = new Vec2((int)result.X, (int)result.Y);
            result /= viewportSize;
            return result;
        }


        void DrawObjectSelectionBorder(Bounds bounds)
        {
            Camera camera = RendererWorld.Instance.DefaultCamera;
            GuiRenderer renderer = EngineApp.Instance.ScreenGuiRenderer;

            Texture texture = TextureManager.Instance.Load("Gui\\Textures\\ObjectSelectionBorder.png");
            Vec2 viewportSize = renderer.ViewportForScreenGuiRenderer.DimensionsInPixels.Size.ToVec2();

            float sizeY = .08f;
            Vec2 size = SnapToPixel(new Vec2(sizeY / camera.AspectRatio, sizeY), viewportSize);
            float alpha = MathFunctions.Sin(Time * MathFunctions.PI) * .5f + .5f;

            Rect screenRectangle = Rect.Cleared;
            {
                Vec3[] points = null;
                bounds.ToPoints(ref points);
                foreach (Vec3 point in points)
                {
                    Vec2 screenPoint;
                    if (camera.ProjectToScreenCoordinates(point, out screenPoint))
                    {
                        screenPoint.Clamp(new Vec2(0, 0), new Vec2(1, 1));
                        screenRectangle.Add(screenPoint);
                    }
                }

                Vec2[] screenPositions = new Vec2[] { 
                    new Vec2( 0, 0 ), 
                    new Vec2( 1, 0 ), 
                    new Vec2( 0, 1 ), 
                    new Vec2( 1, 1 ) };
                foreach (Vec2 screenPosition in screenPositions)
                {
                    Ray ray = camera.GetCameraToViewportRay(screenPosition);
                    if (bounds.RayIntersection(ray))
                        screenRectangle.Add(screenPosition);
                }

                if (screenRectangle.GetSize().X < size.X * 2)
                {
                    screenRectangle = new Rect(
                        screenRectangle.GetCenter().X - size.X, screenRectangle.Top,
                        screenRectangle.GetCenter().X + size.X, screenRectangle.Bottom);
                }
                if (screenRectangle.GetSize().Y < size.Y * 2)
                {
                    screenRectangle = new Rect(
                        screenRectangle.Left, screenRectangle.GetCenter().Y - size.Y,
                        screenRectangle.Right, screenRectangle.GetCenter().Y + size.Y);
                }
            }

        }
    }
}
