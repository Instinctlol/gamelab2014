using Engine.MathEx;
using Engine.Renderer;
using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class RotButton : Button
    {
        public float rotateDegree=90;

        protected override void OnRenderUI(Engine.Renderer.GuiRenderer renderer)
        {



            bool backColorZero = BackColor == new ColorValue(0, 0, 0, 0);

            if (BackColor.Alpha > 0 || (BackTexture != null && backColorZero))
            {
                Rect texCoord = BackTextureCoord;

                if (BackTextureTile && BackTexture != null)
                {
                    float baseHeight = ControlsWorld.Instance.ScaleByResolutionBaseHeight;
                    Vec2 tileCount = new Vec2(baseHeight * renderer.AspectRatio, baseHeight) /
                        BackTexture.SourceSize.ToVec2() * GetScreenSize();
                    texCoord = new Rect(-tileCount * .5f, tileCount * .5f) + new Vec2(.5f, .5f);
                }

                ColorValue color = (backColorZero ? new ColorValue(1, 1, 1) : BackColor) *
                    GetTotalColorMultiplier();
                //color.Clamp( new ColorValue( 0, 0, 0, 0 ), new ColorValue( 1, 1, 1, 1 ) ); //Disabled by John

                Rect rect;
                GetScreenRectangle(out rect);

                if (BackTextureFiltering != GuiRenderer.TextureFilteringModes.Linear)
                    renderer.PushTextureFilteringMode(GuiRenderer.TextureFilteringModes.Point);


                Vec2 oldSize = rect.Size;
                rect.Size *= 1.5f;
                Vec2 diffSize = rect.Size - oldSize;
                rect.LeftTop -= diffSize * .5f;
                rect.RightBottom -= diffSize * .5f;
                RotatingUI(rect, renderer, BackTexture, rotateDegree);
                rotateDegree = 0;
               
            }
            base.OnRenderUI(renderer);
        }

        public static void RotatingUI(Rect controlRect, GuiRenderer renderer, Texture texture, float rotation)
        {
            if (RenderSystem.Instance.IsDeviceLost())
                return;

            Vec2 leftTopUV = RotatePointAroundPivot(new Vec2(-0.2f, -0.2f), new Vec2(.5f, .5f), rotation);
            Vec2 leftBotUV = RotatePointAroundPivot(new Vec2(-0.2f, +1.2f), new Vec2(.5f, .5f), rotation);
            Vec2 rightTopUV = RotatePointAroundPivot(new Vec2(+1.2f, -0.2f), new Vec2(.5f, .5f), rotation);
            Vec2 rightBotUV = RotatePointAroundPivot(new Vec2(+1.2f, +1.2f), new Vec2(.5f, .5f), rotation);

            List<GuiRenderer.TriangleVertex> vert = new List<GuiRenderer.TriangleVertex>(6);
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.LeftTop, new ColorValue(1, 1, 1, 1), leftTopUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.LeftBottom, new ColorValue(1, 1, 1, 1), leftBotUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.RightBottom, new ColorValue(1, 1, 1, 1), rightBotUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.RightBottom, new ColorValue(1, 1, 1, 1), rightBotUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.RightTop, new ColorValue(1, 1, 1, 1), rightTopUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.LeftTop, new ColorValue(1, 1, 1, 1), leftTopUV));

            renderer.AddTriangles(vert, texture, true);

            //EngineConsole console = EngineConsole.Instance;
            //console.Print("Rotating " + rotation);
        }

        static Vec2 RotatePointAroundPivot(Vec2 point, Vec2 pivot, float degrees)
        {
            Radian radians = MathFunctions.DegToRad(degrees);
            Vec2 rotatedPoint = new Vec2();
            rotatedPoint.X = (float)(pivot.X + (Math.Cos(radians) * (point.X - pivot.X) - Math.Sin(radians) * (point.Y - pivot.Y)));
            rotatedPoint.Y = (float)(pivot.Y + (Math.Sin(radians) * (point.X - pivot.X) + Math.Cos(radians) * (point.Y - pivot.Y)));
            return rotatedPoint;
        }

    }
}
