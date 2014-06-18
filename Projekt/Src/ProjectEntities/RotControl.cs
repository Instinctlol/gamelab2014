using Engine.MathEx;
using Engine.Renderer;
using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{

    /**
     * Beispiel Klasse wie rotieren von gui Elementen funktioniert
     * Probleme:
     * - Entweder ist die Textur größer als man sie eigentlich setzt oder die Textur ist begrenzt auf die gewünschte Fläche aber bei rotation können Ecken abgeschnitten werden
     * - Das Control element rotiert nicht wirklich! Nur die Textur rotiert. Überprüfungen ob Maus auf der Textur ist müssen selbst angepasst werden!
     * - BackTextureCoord muss (0, 0, 0 ,0) sein. Sonst wird die tatsächliche Textur hinter/vor der rotierten gerendert
     */
    public class RotControl : Control
    {

        //Winkel ZU dem rotiert werden soll.
        [Engine.EntitySystem.EntityType.FieldSerialize]
        private float rotateDegree = 0;

        public float RotateDegree
        {
            get { return rotateDegree; }
            set { rotateDegree = value; }
        }



        //Beim Rendern drehen wir zum gewünschten Winkel
        protected override void OnRenderUI(Engine.Renderer.GuiRenderer renderer)
        {
            //Rechteck der GUI holen
            Rect rect;
            GetScreenRectangle(out rect);

            //Rotieren
            RotateControl(rect, renderer, BackTexture, rotateDegree);
               
            base.OnRenderUI(renderer);
        }

        public void RotateControl(Rect controlRect, GuiRenderer renderer, Texture texture, float rotation)
        {
            if (RenderSystem.Instance.IsDeviceLost())
                return;


            //Eckpunkte rotieren
            Vec2 leftTopUV = RotatePointAroundPivot(new Vec2(0, 0), new Vec2(.5f, .5f), rotation);
            Vec2 leftBotUV = RotatePointAroundPivot(new Vec2(0, 1), new Vec2(.5f, .5f), rotation);
            Vec2 rightTopUV = RotatePointAroundPivot(new Vec2(1, 0), new Vec2(.5f, .5f), rotation);
            Vec2 rightBotUV = RotatePointAroundPivot(new Vec2(1, 1), new Vec2(.5f, .5f), rotation);


            //Zwei dreiecke erstellen die zusammen das Rechteck der Control GUI bilden
            List<GuiRenderer.TriangleVertex> vert = new List<GuiRenderer.TriangleVertex>(6);

            //Dreieck1
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.LeftTop, new ColorValue(1, 1, 1, 1), leftTopUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.LeftBottom, new ColorValue(1, 1, 1, 1), leftBotUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.RightBottom, new ColorValue(1, 1, 1, 1), rightBotUV));

            //Dreieck2
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.RightBottom, new ColorValue(1, 1, 1, 1), rightBotUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.RightTop, new ColorValue(1, 1, 1, 1), rightTopUV));
            vert.Add(new GuiRenderer.TriangleVertex(controlRect.LeftTop, new ColorValue(1, 1, 1, 1), leftTopUV));

            //Dreiecke der Render engine hinzufügen
            renderer.AddTriangles(vert, texture, true);
        }


        //Einfaches Mathe zeugs zum rotieren von einem Punkt um eine Position (pivot)
        Vec2 RotatePointAroundPivot(Vec2 point, Vec2 pivot, float degrees)
        {
            Radian radians = MathFunctions.DegToRad(degrees);
            Vec2 rotatedPoint = new Vec2();
            rotatedPoint.X = (float)(pivot.X + (Math.Cos(radians) * (point.X - pivot.X) - Math.Sin(radians) * (point.Y - pivot.Y)));
            rotatedPoint.Y = (float)(pivot.Y + (Math.Sin(radians) * (point.X - pivot.X) + Math.Cos(radians) * (point.Y - pivot.Y)));
            return rotatedPoint;
        }

    }
}
