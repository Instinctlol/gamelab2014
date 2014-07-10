﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine.EntitySystem;
using Engine;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.MapSystem;
using Engine.Renderer;
using Engine.Utils;
using System.Reflection;
using System.Drawing.Design;
using System.Diagnostics;
using Engine.SoundSystem;
using Engine.FileSystem;
using ProjectCommon;


namespace ProjectEntities
{
    /// <summary>
	/// Defines the <see cref="Alien"/> entity type.
	/// </summary>
	public class AlienType : AlienUnitType
	{
        const float heightDefault = 1.8f;

        [FieldSerialize]
        float height = heightDefault;

        const float radiusDefault = .4f;
        [FieldSerialize]
        float radius = radiusDefault;

        const float maxVelocityDefault = 5;
        [FieldSerialize]
        float maxVelocity = maxVelocityDefault;
        
        [DefaultValue(heightDefault)]
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        [DefaultValue(radiusDefault)]
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        [DefaultValue(maxVelocityDefault)]
        public float MaxVelocity
        {
            get { return maxVelocity; }
            set { maxVelocity = value; }
        }
	}

    /// <summary>
    /// The Alien character.
    /// </summary>
    public class Alien : AlienUnit, GridBasedNavigationSystem.IOverrideObjectBehavior
    {
        Body mainBody;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        Vec2 pathFoundedToPosition = new Vec2(float.NaN, float.NaN);

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        List<Vec2> path = new List<Vec2>();

        float pathFindWaitTime;

        Vec3 oldMainBodyPosition;
        Vec3 mainBodyVelocity;
        // Channel zum abspielen des Default-Sounds für die kleinen Aliens
        Sound alienSound;
        VirtualChannel alienChannel;
        
        AlienType _type = null; public new AlienType Type { get { return _type; } }

        [FieldSerialize]
        private MapObject movRoute; //will hold a MapCurve that we place on the map as patrol route


        public MapObject MovementRoute //accessor for the MapCurve. Lets create some logic for the 'set'
        {
            get { return movRoute; }
            set
            {
                if (value is MapCurve) //accept only certain MapObjects
                {
                    movRoute = value;
                }
                else movRoute = null;
            }
        }


      
        enum NetworkMessages
        {
            MainBodyVelocityToClient
        }

        /// <summary>
        /// Wenn ein Alien stirbt, dann muss das dem Computer mitgeteilt werden
        /// </summary>
        /// <param name="prejudicial"></param>
        protected override void OnDie(MapObject prejudicial)
        {
            Computer.DecrementUsedAliens();
            base.OnDie(prejudicial);
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            pathFindWaitTime = World.Instance.Random.NextFloat();

            CreatePhysicsModel();

            Body body = PhysicsModel.CreateBody();
            mainBody = body;
            body.Name = "main";
            body.Static = true;
            body.Position = Position;
            body.Rotation = Rotation;

            // Sound erstellen
            if (this._type.SoundCollision != null)
            {
                alienSound = SoundWorld.Instance.SoundCreate(this._type.SoundCollision, SoundMode.Record);
            }

            float length = Type.Height - Type.Radius * 2;
            if (length < 0)
            {
                Log.Error("Length < 0");
                return;
            }
            CapsuleShape shape = body.CreateCapsuleShape();
            shape.Length = length;
            shape.Radius = Type.Radius;
            shape.ContactGroup = (int)ContactGroup.Dynamic;

            SubscribeToTickEvent();

            PhysicsModel.PushToWorld();

            if (mainBody != null)
                oldMainBodyPosition = mainBody.Position;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            if (MoveEnabled)
                TickMove();
            else
                path.Clear();


            CalculateMainBodyVelocity();

            oldMainBodyPosition = mainBody.Position;
        }

        private void CalculateMainBodyVelocity()
        {
            mainBodyVelocity = (mainBody.Position - oldMainBodyPosition) * EntitySystemWorld.Instance.GameFPS;

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendMainBodyVelocityToAllClients();
        }

        private void Server_SendMainBodyVelocityToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Alien),
                (ushort)NetworkMessages.MainBodyVelocityToClient);

            writer.Write(mainBodyVelocity);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.MainBodyVelocityToClient)]
        void Client_ReceiveMainBodyVelocity(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            Vec3 velocity = reader.ReadVec3();
            if (!reader.Complete())
                return;

            mainBodyVelocity = velocity;
        }

        public void SetLookDirection(Vec3 pos)
        {
            Vec2 diff = pos.ToVec2() - Position.ToVec2();

            if (diff == Vec2.Zero)
                return;

            Radian dir = MathFunctions.ATan(diff.Y, diff.X);

            float halfAngle = dir * 0.5f;
            Quat rot = new Quat(new Vec3(0, 0, MathFunctions.Sin(halfAngle)), MathFunctions.Cos(halfAngle));
            Rotation = rot;
        }

        bool DoPathFind()
        {
            Dynamic targetObj = null;
            {
                AlienAI ai = Intellect as AlienAI;
                if (ai != null)
                    targetObj = ai.CurrentTask.Entity;
            }

            //remove this unit from the pathfinding grid
            GetNavigationSystem().RemoveObjectFromMotionMap(this);

            float radius = Type.Radius;
            Rect targetRect = new Rect(
                MovePosition.ToVec2() - new Vec2(radius, radius),
                MovePosition.ToVec2() + new Vec2(radius, radius));

            //remove target unit from the pathfinding grid
            if (targetObj != null && targetObj != this)
                GetNavigationSystem().RemoveObjectFromMotionMap(targetObj);

            //TO DO: really need this call?
            GetNavigationSystem().AddTempClearMotionMap(targetRect);

            const int maxFieldsDistance = 1000;
            const int maxFieldsToCheck = 100000;
            bool found = GetNavigationSystem().FindPath(
                Type.Radius * 2 * 1.1f,
                Position.ToVec2(),
                MovePosition.ToVec2(),
                maxFieldsDistance,
                maxFieldsToCheck,
                true,
                false,
                path);

            GetNavigationSystem().DeleteAllTempClearedMotionMap();

            //add target unit to the pathfinding grid
            if (targetObj != null && targetObj != this)
                GetNavigationSystem().AddObjectToMotionMap(targetObj);

            //add this unit the the pathfinding grid
            GetNavigationSystem().AddObjectToMotionMap(this);

            return found;
        }

        /// <summary>Overridden from <see cref="Engine.MapSystem.MapObject.OnRender(Camera)"/>.</summary>
        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);

            if (path.Count != 0 && GetNavigationSystem().AlwaysDrawGrid)
                GetNavigationSystem().DebugDrawPath(camera, Position.ToVec2(), path);
        }

        void TickMove()
        {
            //path find control
            {
                if (pathFindWaitTime != 0)
                {
                    pathFindWaitTime -= TickDelta;
                    if (pathFindWaitTime < 0)
                        pathFindWaitTime = 0;
                }

                if (pathFoundedToPosition != MovePosition.ToVec2() && pathFindWaitTime == 0)
                    path.Clear();

                if (path.Count == 0)
                {
                    if (pathFindWaitTime == 0)
                    {
                        if (DoPathFind())
                        {
                            pathFoundedToPosition = MovePosition.ToVec2();
                            pathFindWaitTime = .5f;
                        }
                        else
                        {
                            pathFindWaitTime = 1.0f;
                        }
                    }
                }
            }

            if (path.Count == 0)
                return;

            //line movement to path[ 0 ]
            {
                Vec2 destPoint = path[0];

                Vec2 diff = destPoint - Position.ToVec2();

                if (diff == Vec2.Zero)
                {
                    path.RemoveAt(0);
                    return;
                }

                Radian dir = MathFunctions.ATan(diff.Y, diff.X);

                float halfAngle = dir * 0.5f;
                Quat rot = new Quat(new Vec3(0, 0, MathFunctions.Sin(halfAngle)),
                    MathFunctions.Cos(halfAngle));
                Rotation = rot;

                Vec2 dirVector = diff.GetNormalize();
                Vec2 dirStep = dirVector * (Type.MaxVelocity * TickDelta);

                Vec2 newPos = Position.ToVec2() + dirStep;

                if (Math.Abs(diff.X) <= Math.Abs(dirStep.X) && Math.Abs(diff.Y) <= Math.Abs(dirStep.Y))
                {
                    //unit at point
                    newPos = path[0];
                    path.RemoveAt(0);
                }

                GetNavigationSystem().RemoveObjectFromMotionMap(this);

                bool free;
                {
                    float radius = Type.Radius;
                    Rect targetRect = new Rect(newPos - new Vec2(radius, radius), newPos + new Vec2(radius, radius));
                    free = GetNavigationSystem().IsFreeInMapMotion(targetRect);
                }

                GetNavigationSystem().AddObjectToMotionMap(this);

                if (free)
                {
                    float newZ = GetNavigationSystem().GetMotionMapHeight(newPos) + Type.Height * .5f;
                    Position = new Vec3(newPos.X, newPos.Y, newZ);
                }
                else
                    path.Clear();
            }
        }

        protected override void OnRenderFrame()
        {
            //update animation tree
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                AnimationTree tree = GetFirstAnimationTree();

                if (tree != null)
                    UpdateAnimationTree(tree);
            }

            base.OnRenderFrame();
        }

        // TODO kann gelöscht werden
        public void testDeath()
        {
            AnimationTree tree = GetFirstAnimationTree();
            if (tree != null)
            {
                tree.ActivateTrigger("death");
            }
        }

        void UpdateAnimationTree(AnimationTree tree)
        {
            bool move = false;
            //Degree moveAngle = 0;
            float moveSpeed = 0;

            if (mainBodyVelocity.ToVec2().Length() > .1f)
            {
                move = true;
                moveSpeed = (Rotation.GetInverse() * mainBodyVelocity).X;
                if ( alienChannel != null)
                {
                    alienChannel.Stop();
                }
                // Play sound
                if (alienSound != null)
                {
                    this.alienChannel = SoundWorld.Instance.SoundPlay(alienSound, EngineApp.Instance.DefaultSoundChannelGroup, 1f, false);
                }
            }

            tree.SetParameterValue("move", move ? 1 : 0);
            //tree.SetParameterValue( "moveAngle", moveAngle );
            tree.SetParameterValue("moveSpeed", moveSpeed);
        }

        public override bool IsAllowToChangeScale(out string reason)
        {
            reason = ToolsLocalization.Translate("Various", "Characters do not support scaling.");
            return false;
        }

        public GridBasedNavigationSystem GetNavigationSystem()
        {
            //get the first instance on the map
            return GridBasedNavigationSystem.Instances[0];
        }

        /// <summary>
        /// Implements GridBasedNavigationSystem.IOverrideObjectBehavior
        /// </summary>
        /// <param name="navigationSystem"></param>
        /// <param name="obj"></param>
        /// <param name="rectangles"></param>
        public void GetMotionMapRectanglesForObject(GridBasedNavigationSystem navigationSystem, MapObject obj, List<Rect> rectangles)
        {
            rectangles.Add(new Rect(
                obj.Position.ToVec2() - new Vec2(Type.Radius, Type.Radius),
                obj.Position.ToVec2() + new Vec2(Type.Radius, Type.Radius)));
        }
    }
}
