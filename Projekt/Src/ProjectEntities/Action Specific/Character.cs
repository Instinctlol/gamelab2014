// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.FileSystem;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
	/// <summary>
	/// Defines the <see cref="Character"/> entity type.
	/// </summary>
	public class CharacterType : UnitType
	{
		//physics

		const float heightDefault = 1.8f;
		[FieldSerialize]
		float height = heightDefault;

		const float radiusDefault = .4f;
		[FieldSerialize]
		float radius = radiusDefault;

		const float bottomRadiusDefault = .15f;
		[FieldSerialize]
		float bottomRadius = bottomRadiusDefault;

		const float walkUpHeightDefault = .5f;
		[FieldSerialize]
		float walkUpHeight = walkUpHeightDefault;

		const float massDefault = 70;
		[FieldSerialize]
		float mass = massDefault;

		const float minSpeedToSleepBodyDefault = .5f;
		[FieldSerialize]
		float minSpeedToSleepBody = minSpeedToSleepBodyDefault;

		//walk

		const float walkForwardMaxSpeedDefault = 1.0f;
		[FieldSerialize]
		float walkForwardMaxSpeed = walkForwardMaxSpeedDefault;

		const float walkBackwardMaxSpeedDefault = 1.0f;
		[FieldSerialize]
		float walkBackwardMaxSpeed = walkBackwardMaxSpeedDefault;

		const float walkSideMaxSpeedDefault = .8f;
		[FieldSerialize]
		float walkSideMaxSpeed = walkSideMaxSpeedDefault;

		const float walkForceDefault = 280000;
		[FieldSerialize]
		float walkForce = walkForceDefault;

		//run

		const float runForwardMaxSpeedDefault = 5;
		[FieldSerialize]
		float runForwardMaxSpeed = runForwardMaxSpeedDefault;

		const float runBackwardMaxSpeedDefault = 5;
		[FieldSerialize]
		float runBackwardMaxSpeed = runBackwardMaxSpeedDefault;

		const float runSideMaxSpeedDefault = 5;
		[FieldSerialize]
		float runSideMaxSpeed = runSideMaxSpeedDefault;

		const float runForceDefault = 420000;
		[FieldSerialize]
		float runForce = runForceDefault;

		//fly

		const float flyControlMaxSpeedDefault = 10;
		[FieldSerialize]
		float flyControlMaxSpeed = flyControlMaxSpeedDefault;

		const float flyControlForceDefault = 35000;
		[FieldSerialize]
		float flyControlForce = flyControlForceDefault;

		//jump

		[FieldSerialize]
		bool jumpSupport;

		const float jumpSpeedDefault = 4;
		[FieldSerialize]
		float jumpSpeed = jumpSpeedDefault;

		[FieldSerialize]
		string soundJump;

		////crouching

		//[FieldSerialize]
		//bool crouchingSupport;

		//const float crouchingWalkUpHeightDefault = .1f;
		//[FieldSerialize]
		//float crouchingWalkUpHeight = crouchingWalkUpHeightDefault;

		//const float crouchingHeightDefault = 1f;
		//[FieldSerialize]
		//float crouchingHeight = crouchingHeightDefault;

		//const float crouchingMaxSpeedDefault = .5f;
		//[FieldSerialize]
		//float crouchingMaxSpeed = .5f;

		//const float crouchingForceDefault = 100000;
		//[FieldSerialize]
		//float crouchingForce = crouchingForceDefault;

		//damageFastChangeSpeed

		const float damageFastChangeSpeedMinimalSpeedDefault = 10;
		[FieldSerialize]
		float damageFastChangeSpeedMinimalSpeed = damageFastChangeSpeedMinimalSpeedDefault;

		const float damageFastChangeSpeedFactorDefault = 40;
		[FieldSerialize]
		float damageFastChangeSpeedFactor = damageFastChangeSpeedFactorDefault;

		//cached data

		float distanceFromPositionToFloor;
		//float distanceFromCrouchingPositionToFloor;

		///////////////////////////////////////////

		//physics

		[DefaultValue( heightDefault )]
		public float Height
		{
			get { return height; }
			set { height = value; }
		}

		[DefaultValue( radiusDefault )]
		public float Radius
		{
			get { return radius; }
			set { radius = value; }
		}

		[DefaultValue( bottomRadiusDefault )]
		public float BottomRadius
		{
			get { return bottomRadius; }
			set { bottomRadius = value; }
		}

		[DefaultValue( walkUpHeightDefault )]
		public float WalkUpHeight
		{
			get { return walkUpHeight; }
			set { walkUpHeight = value; }
		}

		[DefaultValue( massDefault )]
		public float Mass
		{
			get { return mass; }
			set { mass = value; }
		}

		[DefaultValue( minSpeedToSleepBodyDefault )]
		public float MinSpeedToSleepBody
		{
			get { return minSpeedToSleepBody; }
			set { minSpeedToSleepBody = value; }
		}

		//walk

		[DefaultValue( walkForwardMaxSpeedDefault )]
		public float WalkForwardMaxSpeed
		{
			get { return walkForwardMaxSpeed; }
			set { walkForwardMaxSpeed = value; }
		}

		[DefaultValue( walkBackwardMaxSpeedDefault )]
		public float WalkBackwardMaxSpeed
		{
			get { return walkBackwardMaxSpeed; }
			set { walkBackwardMaxSpeed = value; }
		}

		[DefaultValue( walkSideMaxSpeedDefault )]
		public float WalkSideMaxSpeed
		{
			get { return walkSideMaxSpeed; }
			set { walkSideMaxSpeed = value; }
		}

		[DefaultValue( walkForceDefault )]
		public float WalkForce
		{
			get { return walkForce; }
			set { walkForce = value; }
		}

		//run

		[DefaultValue( runForwardMaxSpeedDefault )]
		public float RunForwardMaxSpeed
		{
			get { return runForwardMaxSpeed; }
			set { runForwardMaxSpeed = value; }
		}

		[DefaultValue( runBackwardMaxSpeedDefault )]
		public float RunBackwardMaxSpeed
		{
			get { return runBackwardMaxSpeed; }
			set { runBackwardMaxSpeed = value; }
		}

		[DefaultValue( runSideMaxSpeedDefault )]
		public float RunSideMaxSpeed
		{
			get { return runSideMaxSpeed; }
			set { runSideMaxSpeed = value; }
		}

		[DefaultValue( runForceDefault )]
		public float RunForce
		{
			get { return runForce; }
			set { runForce = value; }
		}

		//fly

		[DefaultValue( flyControlMaxSpeedDefault )]
		public float FlyControlMaxSpeed
		{
			get { return flyControlMaxSpeed; }
			set { flyControlMaxSpeed = value; }
		}

		[DefaultValue( flyControlForceDefault )]
		public float FlyControlForce
		{
			get { return flyControlForce; }
			set { flyControlForce = value; }
		}

		//jump

		[DefaultValue( false )]
		public bool JumpSupport
		{
			get { return jumpSupport; }
			set { jumpSupport = value; }
		}

		[DefaultValue( jumpSpeedDefault )]
		public float JumpSpeed
		{
			get { return jumpSpeed; }
			set { jumpSpeed = value; }
		}

		[DefaultValue( "" )]
		[Editor( typeof( EditorSoundUITypeEditor ), typeof( UITypeEditor ) )]
		[SupportRelativePath]
		public string SoundJump
		{
			get { return soundJump; }
			set { soundJump = value; }
		}

		////crouching

		//[DefaultValue( false )]
		//public bool CrouchingSupport
		//{
		//   get { return crouchingSupport; }
		//   set { crouchingSupport = value; }
		//}

		//[DefaultValue( crouchingWalkUpHeightDefault )]
		//public float CrouchingWalkUpHeight
		//{
		//   get { return crouchingWalkUpHeight; }
		//   set { crouchingWalkUpHeight = value; }
		//}

		//[DefaultValue( crouchingHeightDefault )]
		//public float CrouchingHeight
		//{
		//   get { return crouchingHeight; }
		//   set { crouchingHeight = value; }
		//}

		//[DefaultValue( crouchingMaxSpeedDefault )]
		//public float CrouchingMaxSpeed
		//{
		//   get { return crouchingMaxSpeed; }
		//   set { crouchingMaxSpeed = value; }
		//}

		//[DefaultValue( crouchingForceDefault )]
		//public float CrouchingForce
		//{
		//   get { return crouchingForce; }
		//   set { crouchingForce = value; }
		//}

		//damageFastChangeSpeed

		[DefaultValue( damageFastChangeSpeedMinimalSpeedDefault )]
		public float DamageFastChangeSpeedMinimalSpeed
		{
			get { return damageFastChangeSpeedMinimalSpeed; }
			set { damageFastChangeSpeedMinimalSpeed = value; }
		}

		[DefaultValue( damageFastChangeSpeedFactorDefault )]
		public float DamageFastChangeSpeedFactor
		{
			get { return damageFastChangeSpeedFactor; }
			set { damageFastChangeSpeedFactor = value; }
		}

		//cached data

		[Browsable( false )]
		public float DistanceFromPositionToFloor
		{
			get { return distanceFromPositionToFloor; }
		}

		//[Browsable( false )]
		//public float DistanceFromCrouchingPositionToFloor
		//{
		//   get { return distanceFromCrouchingPositionToFloor; }
		//}

		///////////////////////////////////////////

		protected override void OnLoaded()
		{
			base.OnLoaded();
			distanceFromPositionToFloor = ( height - walkUpHeight ) * .5f + walkUpHeight;
			//distanceFromCrouchingPositionToFloor = ( crouchingHeight - crouchingWalkUpHeight ) * .5f + crouchingWalkUpHeight;
		}

		protected override void OnPreloadResources()
		{
			base.OnPreloadResources();

			//it is not known how will be used this sound (2D or 3D?).
			//Sound will preloaded as 2D only here.
			PreloadSound( SoundJump, 0 );
		}
	}

	/// <summary>
	/// Defines the physical characters.
	/// </summary>
	public class Character : Unit
	{
		Body mainBody;

		//on ground and flying states
		[FieldSerialize( FieldSerializeSerializationTypes.World )]
		float mainBodyGroundDistance = 1000;//from center of body
		Body groundBody;
		[FieldSerialize( FieldSerializeSerializationTypes.World )]
		float onGroundTime;
		[FieldSerialize( FieldSerializeSerializationTypes.World )]
		float notOnGroundTime;
		Vec3 lastTickPosition;

		[FieldSerialize( FieldSerializeSerializationTypes.World )]
		float jumpInactiveTime;
		[FieldSerialize( FieldSerializeSerializationTypes.World )]
		float shouldJumpTime;

		Vec3 turnToPosition;
		Radian horizontalDirectionForUpdateRotation;

		//moveVector
		int forceMoveVectorTimer;//if == 0 to disabled
		Vec2 forceMoveVector;
		Vec2 lastTickForceVector;

		[FieldSerialize( FieldSerializeSerializationTypes.World )]
		Vec3 linearVelocityForSerialization;

		Vec3 groundRelativeVelocity;
		Vec3 server_sentGroundRelativeVelocity;
		Vec3[] groundRelativeVelocitySmoothArray;
		Vec3 groundRelativeVelocitySmooth;

		//damageFastChangeSpeed
		Vec3 damageFastChangeSpeedLastVelocity = new Vec3( float.NaN, float.NaN, float.NaN );

		float allowToSleepTime;

		///////////////////////////////////////////

		enum NetworkMessages
		{
			JumpEventToClient,
			GroundRelativeVelocityToClient,
		}

		///////////////////////////////////////////

		CharacterType _type = null; public new CharacterType Type { get { return _type; } }

		public void SetForceMoveVector( Vec2 vec )
		{
			forceMoveVectorTimer = 2;
			forceMoveVector = vec;
		}

		[Browsable( false )]
		public Body MainBody
		{
			get { return mainBody; }
		}

		[Browsable( false )]
		public Vec3 TurnToPosition
		{
			get { return turnToPosition; }
		}

		public void SetTurnToPosition( Vec3 pos )
		{
			turnToPosition = pos;

			Vec3 diff = turnToPosition - Position;
			horizontalDirectionForUpdateRotation = MathFunctions.ATan( diff.Y, diff.X );

			UpdateRotation( true );
		}

        public void OnHit()
        {

        }

		public void UpdateRotation( bool allowUpdateOldRotation )
		{
			float halfAngle = horizontalDirectionForUpdateRotation * .5f;
			Quat rot = new Quat( new Vec3( 0, 0, MathFunctions.Sin( halfAngle ) ),
				MathFunctions.Cos( halfAngle ) );

			const float epsilon = .001f;

			//update Rotation
			if( !Rotation.Equals( rot, epsilon ) )
				Rotation = rot;

			//update OldRotation
			if( allowUpdateOldRotation )
			{
				//disable updating OldRotation property for TPSArcade demo and for PlatformerDemo
				bool updateOldRotation = true;
				if( Intellect != null && PlayerIntellect.Instance == Intellect )
				{
					if( GameMap.Instance != null && (
						GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
						GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo ) )
					{
						updateOldRotation = false;
					}
				}
				if( updateOldRotation )
					OldRotation = rot;
			}
		}

		public bool IsOnGround()
		{
			const float maxThreshold = .2f;
			return mainBodyGroundDistance - maxThreshold < Type.DistanceFromPositionToFloor && groundBody != null;
		}

		public float GetElapsedTimeSinceLastGroundContact()
		{
			return notOnGroundTime;
		}

		protected override void OnSave( TextBlock block )
		{
			if( mainBody != null )
				linearVelocityForSerialization = mainBody.LinearVelocity;

			base.OnSave( block );
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
		protected override void OnPostCreate( bool loaded )
		{
			base.OnPostCreate( loaded );

			SetTurnToPosition( Position + Rotation.GetForward() * 100 );

			CreatePhysicsModel();

			Body body = PhysicsModel.CreateBody();
			mainBody = body;
			body.Name = "main";
			body.Position = Position;
			body.Rotation = Rotation;
			body.Sleepiness = 0;
			body.LinearVelocity = linearVelocityForSerialization;
			body.AngularDamping = 10;
			body.CenterOfMassPosition = new Vec3( 0, 0, -Type.Height / 4 );
			body.MassMethod = Body.MassMethods.Manually;
			body.Mass = Type.Mass;
			body.InertiaTensorFactor = new Vec3( 100, 100, 100 );
			body.PhysX_SolverPositionIterations = 1;

			float length = Type.Height - Type.Radius * 2 - Type.WalkUpHeight;
			if( length < 0 )
			{
				Log.Error( "Character: OnPostCreate: Type.Height - Type.Radius * 2 - Type.WalkUpHeight < 0." );
				return;
			}

			//create main capsule
			{
				CapsuleShape shape = body.CreateCapsuleShape();
				shape.Length = length;
				shape.Radius = Type.Radius;
				shape.ContactGroup = (int)ContactGroup.Dynamic;
				shape.StaticFriction = 0;
				shape.DynamicFriction = 0;
				shape.Restitution = 0;
				shape.Hardness = 0;
				shape.SpecialLiquidDensity = 1500;
			}

			//create bottom capsule
			{
				CapsuleShape shape = body.CreateCapsuleShape();
				shape.Length = Type.Height - Type.BottomRadius * 2;
				shape.Radius = Type.BottomRadius;
				shape.Position = new Vec3( 0, 0,
					( Type.Height - Type.WalkUpHeight ) / 2 - Type.Height / 2 );
				shape.ContactGroup = (int)ContactGroup.Dynamic;
				shape.StaticFriction = 0;
				shape.DynamicFriction = 0;
				shape.Restitution = 0;
				shape.Hardness = 0;
				shape.SpecialLiquidDensity = 1500;
			}

			PhysicsModel.PushToWorld();

			PhysicsWorld.Instance.MainScene.PostStep += MainScene_PostStep;

			SubscribeToTickEvent();
		}

		protected override void OnDestroy()
		{
			if( PhysicsWorld.Instance != null )
				PhysicsWorld.Instance.MainScene.PostStep -= MainScene_PostStep;

			base.OnDestroy();
		}

        protected override void OnDamage(MapObject prejudicial, Vec3 pos, Engine.PhysicsSystem.Shape shape, float damage, bool allowMoveDamageToParent)
        {
            base.OnDamage(prejudicial, pos, shape, damage, allowMoveDamageToParent);
            // Dem Computer mitteilen, dass ein Alien einen Astronauten getroffen hat.
            Computer.AddExperiencePoints((int)damage);
        }

		protected override void OnSuspendPhysicsDuringMapLoading( bool suspend )
		{
			base.OnSuspendPhysicsDuringMapLoading( suspend );

			//After loading a map, the physics simulate 5 seconds, that bodies have fallen asleep.
			//During this time we will disable physics for this entity.
			foreach( Body body in PhysicsModel.Bodies )
				body.Static = suspend;
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
		protected override void OnTick()
		{
			base.OnTick();

			//clear groundBody when disposed
			if( groundBody != null && groundBody.IsDisposed )
				groundBody = null;

			TickMovement();

			if( Intellect != null )
				TickIntellect( Intellect );

			UpdateRotation( true );
			if( Type.JumpSupport )
				TickJump( false );

			if( IsOnGround() )
				onGroundTime += TickDelta;
			else
				onGroundTime = 0;
			if( !IsOnGround() )
				notOnGroundTime += TickDelta;
			else
				notOnGroundTime = 0;
			CalculateGroundRelativeVelocity();

			if( forceMoveVectorTimer != 0 )
				forceMoveVectorTimer--;

			if( Type.DamageFastChangeSpeedFactor != 0 )
				DamageFastChangeSpeedTick();

			lastTickPosition = Position;
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.Client_OnTick()"/>.</summary>
		protected override void Client_OnTick()
		{
			base.Client_OnTick();

			//clear groundBody when disposed
			if( groundBody != null && groundBody.IsDisposed )
				groundBody = null;

			Vec3 shouldAddForce;
			CalculateMainBodyGroundDistanceAndGroundBody( out shouldAddForce );

			if( IsOnGround() )
				onGroundTime += TickDelta;
			else
				onGroundTime = 0;
			if( !IsOnGround() )
				notOnGroundTime += TickDelta;
			else
				notOnGroundTime = 0;
			CalculateGroundRelativeVelocity();
			lastTickPosition = Position;
		}

		public bool IsNeedRun()
		{
			bool run = false;
			if( Intellect != null )
				run = Intellect.IsAlwaysRun();
			else
				run = false;

			if( Intellect != null && Intellect.IsControlKeyPressed( GameControlKeys.Run ) )
				run = !run;

			return run;
		}

		Vec2 GetMovementVectorByControlKeys()
		{
			//use specified force move vector
			if( forceMoveVectorTimer != 0 )
				return forceMoveVector;

			//TPS arcade specific
			//vector is depending on camera orientation
			if( GameMap.Instance != null && GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade &&
				PlayerIntellect.Instance == Intellect )
			{
				//this is not adapted for networking.
				//using RendererWorld.Instance.DefaultCamera is bad.

				Vec2 localVector = Vec2.Zero;
				localVector.X += Intellect.GetControlKeyStrength( GameControlKeys.Forward );
				localVector.X -= Intellect.GetControlKeyStrength( GameControlKeys.Backward );
				localVector.Y += Intellect.GetControlKeyStrength( GameControlKeys.Left );
				localVector.Y -= Intellect.GetControlKeyStrength( GameControlKeys.Right );

				if( localVector != Vec2.Zero )
				{
					Vec2 diff = Position.ToVec2() - RendererWorld.Instance.DefaultCamera.Position.ToVec2();
					Degree angle = new Radian( MathFunctions.ATan( diff.Y, diff.X ) );
					Degree vecAngle = new Radian( MathFunctions.ATan( -localVector.Y, localVector.X ) );
					Quat rot = new Angles( 0, 0, vecAngle - angle ).ToQuat();
					Vec2 vector = ( rot * new Vec3( 1, 0, 0 ) ).ToVec2();
					return vector;
				}
				else
					return Vec2.Zero;
			}

			//PlatformerDemo specific
			if( GameMap.Instance != null && GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo &&
				PlayerIntellect.Instance == Intellect )
			{
				Vec2 vector = Vec2.Zero;
				vector.X -= Intellect.GetControlKeyStrength( GameControlKeys.Left );
				vector.X += Intellect.GetControlKeyStrength( GameControlKeys.Right );
				return vector;
			}

			//default behaviour
			{
				Vec2 localVector = Vec2.Zero;
				localVector.X += Intellect.GetControlKeyStrength( GameControlKeys.Forward );
				localVector.X -= Intellect.GetControlKeyStrength( GameControlKeys.Backward );
				localVector.Y += Intellect.GetControlKeyStrength( GameControlKeys.Left );
				localVector.Y -= Intellect.GetControlKeyStrength( GameControlKeys.Right );

				Vec2 vector = ( new Vec3( localVector.X, localVector.Y, 0 ) * Rotation ).ToVec2();
				if( vector != Vec2.Zero )
				{
					float length = vector.Length();
					if( length > 1 )
						vector /= length;
				}
				return vector;
			}
		}

		void TickIntellect( Intellect intellect )
		{
			Vec2 forceVec = GetMovementVectorByControlKeys();
			if( forceVec != Vec2.Zero )
			{
				float speedCoefficient = 1;
				if( FastMoveInfluence != null )
					speedCoefficient = FastMoveInfluence.Type.Coefficient;

				float maxSpeed;
				float force;

				if( IsOnGround() )
				{
					//calcualate maxSpeed and force on ground.

					Vec2 localVec = ( new Vec3( forceVec.X, forceVec.Y, 0 ) * Rotation.GetInverse() ).ToVec2();

					float absSum = Math.Abs( localVec.X ) + Math.Abs( localVec.Y );
					if( absSum > 1 )
						localVec /= absSum;

					bool running = IsNeedRun();

					maxSpeed = 0;
					force = 0;

					if( Math.Abs( localVec.X ) >= .001f )
					{
						//forward and backward
						float speedX;
						if( localVec.X > 0 )
							speedX = running ? Type.RunForwardMaxSpeed : Type.WalkForwardMaxSpeed;
						else
							speedX = running ? Type.RunBackwardMaxSpeed : Type.WalkBackwardMaxSpeed;
						maxSpeed += speedX * Math.Abs( localVec.X );
						force += ( running ? Type.RunForce : Type.WalkForce ) * Math.Abs( localVec.X );
					}

					if( Math.Abs( localVec.Y ) >= .001f )
					{
						//left and right
						maxSpeed += ( running ? Type.RunSideMaxSpeed : Type.WalkSideMaxSpeed ) *
							Math.Abs( localVec.Y );
						force += ( running ? Type.RunForce : Type.WalkForce ) * Math.Abs( localVec.Y );
					}
				}
				else
				{
					//calcualate maxSpeed and force when flying.
					maxSpeed = Type.FlyControlMaxSpeed;
					force = Type.FlyControlForce;
				}

				//speedCoefficient
				maxSpeed *= speedCoefficient;
				force *= speedCoefficient;

				if( GetLinearVelocity().Length() < maxSpeed )
				{
					mainBody.AddForce( ForceType.Global, 0, new Vec3( forceVec.X, forceVec.Y, 0 ) *
						force * TickDelta, Vec3.Zero );
				}
			}

			lastTickForceVector = forceVec;
		}

		protected override void OnIntellectCommand( Intellect.Command command )
		{
			base.OnIntellectCommand( command );

			if( EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle() )
			{
				if( Type.JumpSupport && command.KeyPressed && command.Key == GameControlKeys.Jump )
					TryJump();
			}
		}

		void UpdateMainBodyDamping()
		{
			if( IsOnGround() && jumpInactiveTime == 0 )
			{
				//small distinction of different physics libraries.
				if( PhysicsWorld.Instance.IsPhysX() )
					mainBody.LinearDamping = 9.3f;
				else
					mainBody.LinearDamping = 10;
			}
			else
				mainBody.LinearDamping = .15f;
		}

		void TickMovement()
		{
			//wake up when ground is moving
			if( mainBody.Sleeping && groundBody != null && !groundBody.Sleeping &&
				( groundBody.LinearVelocity.LengthSqr() > .3f ||
				groundBody.AngularVelocity.LengthSqr() > .3f ) )
			{
				mainBody.Sleeping = false;
			}

			Vec3 shouldAddForce;
			CalculateMainBodyGroundDistanceAndGroundBody( out shouldAddForce );

			if( !mainBody.Sleeping || !IsOnGround() )
			{
				UpdateMainBodyDamping();

				if( IsOnGround() )
				{
					//reset angular velocity
					mainBody.AngularVelocity = Vec3.Zero;

					//move the object when it underground
					if( mainBodyGroundDistance + .01f < Type.DistanceFromPositionToFloor && jumpInactiveTime == 0 )
						Position = Position + new Vec3( 0, 0, Type.DistanceFromPositionToFloor - mainBodyGroundDistance );
				}

				//add force to body if need
				if( shouldAddForce != Vec3.Zero )
				{
					mainBody.AddForce( ForceType.GlobalAtLocalPos, TickDelta, shouldAddForce,
						Vec3.Zero );
				}

				//on dynamic ground velocity
				if( IsOnGround() && groundBody != null )
				{
					if( !groundBody.Static && !groundBody.Sleeping )
					{
						Vec3 groundVel = groundBody.LinearVelocity;

						Vec3 vel = mainBody.LinearVelocity;

						if( groundVel.X > 0 && vel.X >= 0 && vel.X < groundVel.X )
							vel.X = groundVel.X;
						else if( groundVel.X < 0 && vel.X <= 0 && vel.X > groundVel.X )
							vel.X = groundVel.X;

						if( groundVel.Y > 0 && vel.Y >= 0 && vel.Y < groundVel.Y )
							vel.Y = groundVel.Y;
						else if( groundVel.Y < 0 && vel.Y <= 0 && vel.Y > groundVel.Y )
							vel.Y = groundVel.Y;

						if( groundVel.Z > 0 && vel.Z >= 0 && vel.Z < groundVel.Z )
							vel.Z = groundVel.Z;
						else if( groundVel.Z < 0 && vel.Z <= 0 && vel.Z > groundVel.Z )
							vel.Z = groundVel.Z;

						mainBody.LinearVelocity = vel;

						//stupid anti damping
						mainBody.LinearVelocity += groundVel * .25f;
					}
				}

				//sleep if on ground and zero velocity

				bool needSleep = false;
				if( IsOnGround() )
				{
					bool groundStopped = groundBody.Sleeping ||
						( groundBody.LinearVelocity.LengthSqr() < .3f && groundBody.AngularVelocity.LengthSqr() < .3f );
					if( groundStopped && GetLinearVelocity().Length() < Type.MinSpeedToSleepBody )
						needSleep = true;
				}

				//strange fix for PhysX. The character can frezee in fly with zero linear velocity.
				if( PhysicsWorld.Instance.IsPhysX() )
				{
					if( !needSleep && mainBody.LinearVelocity == Vec3.Zero && lastTickPosition == Position )
					{
						mainBody.Sleeping = true;
						needSleep = false;
					}
				}

				if( needSleep )
					allowToSleepTime += TickDelta;
				else
					allowToSleepTime = 0;

				mainBody.Sleeping = allowToSleepTime > TickDelta * 2.5f;
				//mainBody.Sleeping = needSleep;
			}
		}

		void CalculateMainBodyGroundDistanceAndGroundBody( out Vec3 shouldAddForce )
		{
			shouldAddForce = Vec3.Zero;

			mainBodyGroundDistance = 1000;
			groundBody = null;

			for( int n = 0; n < 5; n++ )
			{
				Vec3 offset = Vec3.Zero;

				float step = Type.BottomRadius;

				switch( n )
				{
				case 0: offset = new Vec3( 0, 0, 0 ); break;
				case 1: offset = new Vec3( -step, -step, step/* * .1f*/ ); break;
				case 2: offset = new Vec3( step, -step, step/* * .1f*/ ); break;
				case 3: offset = new Vec3( step, step, step/* * .1f*/ ); break;
				case 4: offset = new Vec3( -step, step, step/* * .1f*/ ); break;
				}

				Vec3 pos = Position - new Vec3( 0, 0, Type.DistanceFromPositionToFloor -
					Type.WalkUpHeight + .01f ) + offset;
				RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
					new Ray( pos, new Vec3( 0, 0, -Type.Height * 1.5f ) ),
					(int)mainBody.Shapes[ 0 ].ContactGroup );

				if( piercingResult.Length == 0 )
					continue;

				foreach( RayCastResult result in piercingResult )
				{
					if( result.Shape.Body == mainBody )
						continue;

					float dist = Position.Z - result.Position.Z;
					if( dist < mainBodyGroundDistance )
					{
						bool bigSlope = false;

						//max slope check
						const float maxSlopeCoef = .7f;// MathFunctions.Sin( new Degree( 60.0f ).InRadians() );
						if( result.Normal.Z < maxSlopeCoef )
						{
							Vec3 vector = new Vec3( result.Normal.X, result.Normal.Y, 0 );
							if( vector != Vec3.Zero )
							{
								bigSlope = true;

								//add force
								vector.Normalize();
								vector *= mainBody.Mass * 2;
								shouldAddForce += vector;
							}
						}

						if( !bigSlope )
						{
							mainBodyGroundDistance = dist;
							groundBody = result.Shape.Body;
						}
					}
				}
			}
		}

		protected virtual void OnJump()
		{
			SoundPlay3D( Type.SoundJump, .5f, true );
		}

		void TickJump( bool ignoreTicks )
		{
			if( !ignoreTicks )
			{
				if( shouldJumpTime != 0 )
				{
					shouldJumpTime -= TickDelta;
					if( shouldJumpTime < 0 )
						shouldJumpTime = 0;
				}

				if( jumpInactiveTime != 0 )
				{
					jumpInactiveTime -= TickDelta;
					if( jumpInactiveTime < 0 )
						jumpInactiveTime = 0;
				}
			}

			if( IsOnGround() && onGroundTime > TickDelta && jumpInactiveTime == 0 && shouldJumpTime != 0 )
			{
				Vec3 vel = mainBody.LinearVelocity;
				vel.Z = Type.JumpSpeed;
				mainBody.LinearVelocity = vel;
				Position += new Vec3( 0, 0, .05f );

				jumpInactiveTime = .2f;
				shouldJumpTime = 0;

				UpdateMainBodyDamping();

				OnJump();

				if( EntitySystemWorld.Instance.IsServer() )
					Server_SendJumpEventToAllClients();
			}
		}

		public void TryJump()
		{
			if( !Type.JumpSupport )
				return;

			//cannot called on client.
			if( EntitySystemWorld.Instance.IsClientOnly() )
				Log.Fatal( "Character: TryJump: EntitySystemWorld.Instance.IsClientOnly()." );

			shouldJumpTime = .4f;
			TickJump( true );
		}

		[Browsable( false )]
		public Vec2 LastTickForceVector
		{
			get { return lastTickForceVector; }
		}

		protected override void OnSetTransform( ref Vec3 pos, ref Quat rot, ref Vec3 scl )
		{
			base.OnSetTransform( ref pos, ref rot, ref scl );

			if( PhysicsModel != null )
			{
				foreach( Body body in PhysicsModel.Bodies )
					body.Sleeping = false;
			}
		}

		void CalculateGroundRelativeVelocity()
		{
			if( EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle() )
			{
				//server or single mode

				if( mainBody != null )
				{
					groundRelativeVelocity = GetLinearVelocity();
					if( groundBody != null && groundBody.AngularVelocity.LengthSqr() < .3f )
						groundRelativeVelocity -= groundBody.LinearVelocity;
				}
				else
					groundRelativeVelocity = Vec3.Zero;

				if( EntitySystemWorld.Instance.IsServer() )
				{
					if( !groundRelativeVelocity.Equals( server_sentGroundRelativeVelocity, .1f ) )
					{
						Server_SendGroundRelativeVelocityToClients(
							EntitySystemWorld.Instance.RemoteEntityWorlds, groundRelativeVelocity );
						server_sentGroundRelativeVelocity = groundRelativeVelocity;
					}
				}
			}
			else
			{
				//client

				//groundRelativeVelocity is updated from server, 
				//because body velocities are not synchronized via network.
			}

			//groundRelativeVelocityToSmooth
			if( groundRelativeVelocitySmoothArray == null )
			{
				float seconds = .2f;
				float count = ( seconds / TickDelta ) + .999f;
				groundRelativeVelocitySmoothArray = new Vec3[ (int)count ];
			}
			for( int n = 0; n < groundRelativeVelocitySmoothArray.Length - 1; n++ )
				groundRelativeVelocitySmoothArray[ n ] = groundRelativeVelocitySmoothArray[ n + 1 ];
			groundRelativeVelocitySmoothArray[ groundRelativeVelocitySmoothArray.Length - 1 ] = groundRelativeVelocity;
			groundRelativeVelocitySmooth = Vec3.Zero;
			for( int n = 0; n < groundRelativeVelocitySmoothArray.Length; n++ )
				groundRelativeVelocitySmooth += groundRelativeVelocitySmoothArray[ n ];
			groundRelativeVelocitySmooth /= (float)groundRelativeVelocitySmoothArray.Length;
		}

		[Browsable( false )]
		public Vec3 GroundRelativeVelocity
		{
			get { return groundRelativeVelocity; }
		}

		[Browsable( false )]
		public Vec3 GroundRelativeVelocitySmooth
		{
			get { return groundRelativeVelocitySmooth; }
		}

		public Vec3 GetLinearVelocity()
		{
			if( EntitySystemWorld.Instance.Simulation )
				return ( Position - lastTickPosition ) / TickDelta;
			return Vec3.Zero;
		}

		void Server_SendJumpEventToAllClients()
		{
			SendDataWriter writer = BeginNetworkMessage( typeof( Character ),
				(ushort)NetworkMessages.JumpEventToClient );
			EndNetworkMessage();
		}

		[NetworkReceive( NetworkDirections.ToClient, (ushort)NetworkMessages.JumpEventToClient )]
		void Client_ReceiveJumpEvent( RemoteEntityWorld sender, ReceiveDataReader reader )
		{
			if( !reader.Complete() )
				return;
			OnJump();
		}

		protected override void Server_OnClientConnectedAfterPostCreate(
			RemoteEntityWorld remoteEntityWorld )
		{
			base.Server_OnClientConnectedAfterPostCreate( remoteEntityWorld );

			IList<RemoteEntityWorld> worlds = new RemoteEntityWorld[] { remoteEntityWorld };
			Server_SendGroundRelativeVelocityToClients( worlds, server_sentGroundRelativeVelocity );
		}

		void Server_SendGroundRelativeVelocityToClients( IList<RemoteEntityWorld> remoteEntityWorlds,
			Vec3 value )
		{
			SendDataWriter writer = BeginNetworkMessage( remoteEntityWorlds, typeof( Character ),
				(ushort)NetworkMessages.GroundRelativeVelocityToClient );
			writer.Write( value );
			EndNetworkMessage();
		}

		[NetworkReceive( NetworkDirections.ToClient, (ushort)NetworkMessages.GroundRelativeVelocityToClient )]
		void Client_ReceiveGroundRelativeVelocity( RemoteEntityWorld sender, ReceiveDataReader reader )
		{
			Vec3 value = reader.ReadVec3();
			if( !reader.Complete() )
				return;
			groundRelativeVelocity = value;
		}

		public override bool IsAllowToChangeScale( out string reason )
		{
			reason = ToolsLocalization.Translate( "Various", "Characters do not support scaling." );
			return false;
		}

		void MainScene_PostStep( PhysicsScene scene )
		{
			if( mainBody != null && !mainBody.Sleeping )
				UpdateRotation( false );
		}

		public void DamageFastChangeSpeedResetLastVelocity()
		{
			damageFastChangeSpeedLastVelocity = new Vec3( float.NaN, float.NaN, float.NaN );
		}

		void DamageFastChangeSpeedTick()
		{
			if( MainBody == null )
				return;
			Vec3 velocity = MainBody.LinearVelocity;

			if( float.IsNaN( damageFastChangeSpeedLastVelocity.X ) )
				damageFastChangeSpeedLastVelocity = velocity;

			Vec3 diff = velocity - damageFastChangeSpeedLastVelocity;
			if( diff.Z > 0 )
			{
				float v = diff.Z - Type.DamageFastChangeSpeedMinimalSpeed;
				if( v > 0 )
				{
					float damage = v * Type.DamageFastChangeSpeedFactor;
					if( damage > 0 )
						DoDamage( null, Position, null, damage, true );
				}
			}

			damageFastChangeSpeedLastVelocity = velocity;
		}
	}
}
