// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.Networking;
using ProjectCommon;
using Engine.Utils;


namespace ProjectEntities
{
	/// <summary>
	/// Defines the <see cref="GameWorld"/> entity type.
	/// </summary>
	public class GameWorldType : WorldType
	{
	}

	public class GameWorld : World
	{
		static GameWorld instance;

		//for moving player character between maps
		string needChangeMapName;
		string needChangeMapSpawnPointName;
		PlayerCharacter.ChangeMapInformation needChangeMapPlayerCharacterInformation;
		string needChangeMapPreviousMapName;

		bool needWorldDestroy;
        Vec3 pos;
        Quat rot;
        public static float revival;
        public static float timer;
        float buildTime = 20;
        float respawnbuildtime = 40;
        float objDistance=0;
        Vec3 diff;
        float radius = 10;
        Vec3 actObjPos;
        PlayerCharacter playercharacter;
        Boolean bothdied = false;
        Boolean oncedied = false;
        public static Boolean showtimer = false; 

		//

        enum NetworkMessages
        {
            respawntimer,
        }

		GameWorldType _type = null;
        
        public new GameWorldType Type { get { return _type; } }

		public GameWorld()
		{
			instance = this;
		}

		public static new GameWorld Instance
		{
			get { return instance; }
		}
        public Vec3 getpos()
        {

            return pos;
        }
        public void setpos(Vec3 pos) 
        {
            this.pos = pos;
        }

        public float timeelapsed
        {
            get { return timer; }
        }

        public float timerevival
        {
            get { return revival; }
        }

        
        public float BuildTime
        {
            get { return buildTime; }
            set { buildTime = value; }
        }

        public float RespawnBuildtime
        {
            get { return respawnbuildtime; }
            set { respawnbuildtime = value; }
        }
		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
		protected override void OnPostCreate( bool loaded )
		{
			base.OnPostCreate( loaded );

			SubscribeToTickEvent();

            //create PlayerManager, create Computer
			if( EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle() )
			{
				if( PlayerManager.Instance == null )
				{
					PlayerManager manager = (PlayerManager)Entities.Instance.Create(
						"PlayerManager", this );
					manager.PostCreate();
                }

                if (Computer.Instance == null)
                {
                    Computer computer = (Computer) Entities.Instance.Create("Computer", this);
                    computer.PostCreate();
                }

                if (AlienSound.Instance == null)
                {
                    AlienSound alienSound = (AlienSound)Entities.Instance.Create("AlienSound", this);
                    alienSound.PostCreate();
                }
            }
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();

			instance = null;
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            //single mode. recreate player units if need
            if (EntitySystemWorld.Instance.IsSingle())
            {
                if (GameMap.Instance.GameType == GameMap.GameTypes.Action ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TurretDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.VillageDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo)
                {
                    if (PlayerManager.Instance != null)
                    {
                        foreach (PlayerManager.ServerOrSingle_Player player in
                            PlayerManager.Instance.ServerOrSingle_Players)
                        {
                            if (player.Intellect == null || player.Intellect.ControlledObject == null)
                            {
                                ServerOrSingle_CreatePlayerUnit(player);
                            }
                        }
                    }
                }
            }



            //networking mode
            if (EntitySystemWorld.Instance.IsServer())
            {
                if (GameMap.Instance.GameType == GameMap.GameTypes.AVA)
                {
                    if (PlayerManager.Instance != null)
                    {
                        UserManagementServerNetworkService userManagementService =
                            GameNetworkServer.Instance.UserManagementService;

                        //remove users
                    again:
                        foreach (PlayerManager.ServerOrSingle_Player player in
                            PlayerManager.Instance.ServerOrSingle_Players)
                        {


                            if (player.Intellect.ControlledObject != null)
                            {

                                //aktuelle position 

                                player.pos = player.Intellect.ControlledObject.Position;
                                //  ((Dynamic)player.Intellect.ControlledObject).Die();
                            }

                            if (player.User != null && player.User != userManagementService.ServerUser)
                            {
                                NetworkNode.ConnectedNode connectedNode = player.User.ConnectedNode;
                                if (connectedNode == null ||
                                    connectedNode.Status != NetworkConnectionStatuses.Connected)
                                {
                                    if (player.Intellect != null)
                                    {
                                        PlayerIntellect playerIntellect = player.Intellect as PlayerIntellect;
                                        if (playerIntellect != null)
                                            playerIntellect.TryToRestoreMainControlledUnit();

                                        if (player.Intellect.ControlledObject != null)
                                            player.Intellect.ControlledObject.Die();

                                        player.Intellect.SetForDeletion(true);
                                        player.Intellect = null;
                                    }

                                    PlayerManager.Instance.ServerOrSingle_RemovePlayer(player);

                                    goto again;
                                }
                            }
                        }

                        //add users
                        foreach (UserManagementServerNetworkService.UserInfo user in
                            userManagementService.Users)
                        {
                            //check whether "EntitySystem" service on the client
                            if (user.ConnectedNode != null)
                            {
                                if (!user.ConnectedNode.RemoteServices.Contains("EntitySystem"))
                                    continue;
                            }

                            PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
                                ServerOrSingle_GetPlayer(user);

                            if (player == null)
                            {
                                player = PlayerManager.Instance.Server_AddClientPlayer(user);

                                PlayerIntellect intellect = (PlayerIntellect)Entities.Instance.
                                    Create("PlayerIntellect", World.Instance);
                                intellect.PostCreate();

                                player.Intellect = intellect;

                                if (GameNetworkServer.Instance.UserManagementService.ServerUser != user)
                                {
                                    //player on client
                                    RemoteEntityWorld remoteEntityWorld = GameNetworkServer.Instance.
                                        EntitySystemService.GetRemoteEntityWorld(user);
                                    intellect.Server_SendSetInstanceToClient(remoteEntityWorld);
                                }
                                else
                                {
                                    //player on this server
                                    PlayerIntellect.SetInstance(intellect);
                                }

                            }
                        }

                        //create units

                        // radius definieren 

                        Boolean creatAstronaut = true;
                        foreach (PlayerManager.ServerOrSingle_Player actplayer in
                            PlayerManager.Instance.ServerOrSingle_Players)
                        {



                            if (actplayer.Intellect.ControlledObject != null && actplayer.started)
                            {
                                actObjPos = actplayer.Intellect.ControlledObject.Position;



                                foreach (PlayerManager.ServerOrSingle_Player nonactplayer in PlayerManager.Instance.ServerOrSingle_Players)
                                {

                                    if (nonactplayer.Intellect.ControlledObject != null && nonactplayer != actplayer)
                                    {

                                        Vec3 nonactObjPos = nonactplayer.Intellect.ControlledObject.Position;
                                        //EngineConsole.Instance.Print("Die position von den 2 Astronaut ist   " + nonactObjPos + ", " + actObjPos);



                                        Map.Instance.GetObjects(new Sphere(actObjPos, radius), MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                                        {

                                            //Unit obj = (Unit)mapObject;

                                            diff = actObjPos - nonactObjPos;
                                            objDistance = diff.Length();
                                            //EngineConsole.Instance.Print("Der abstand zwichen den 2 Astronaut ist   " + objDistance);
                                            //EngineConsole.Instance.Print("Die zeit ist   " + revival);

                                            creatAstronaut = false;
                                            revival = 0f;
                                            timer = 0f;
                                            showtimer = false;
                                            Server_Sendrespawntime();
                                        });

                                    }
                                }
                            }



                            // wenn ein Astronaut stirbt

                            
                            
                                if (actplayer.Intellect != null && actplayer.Intellect.ControlledObject == null && actplayer.User.ConnectedNode != null && actplayer.started)
                                {
                                    //  Computer.Instance.IncrementKilledAstronouts();



                                    foreach (PlayerManager.ServerOrSingle_Player nonactplayer in PlayerManager.Instance.ServerOrSingle_Players)
                                    {

                                        if (nonactplayer.Intellect.ControlledObject != null && nonactplayer != actplayer)
                                        {

                                            Vec3 nonactObjPos = nonactplayer.Intellect.ControlledObject.Position;
                                            diff = actplayer.pos - nonactObjPos;
                                            objDistance = diff.Length();
                                            EngineConsole.Instance.Print("Der abstand zwichen den 2 Astronaut ist   " + objDistance);
                                            EngineConsole.Instance.Print("Die zeit ist   " + revival);



                                            //StatusMessageHandler.sendMessage(" zeit " + revival);




                                            if (objDistance > radius)
                                            {

                                                EngineConsole.Instance.Print("Der abstand zwichen den 2 Astronaut ist   " + objDistance);
                                                EngineConsole.Instance.Print("Die zeit ist   " + revival);
                                                EngineConsole.Instance.Print("Die zeit für timer   " + timer);
                                                creatAstronaut = false;
                                                showtimer = true;
                                                revival = 0f;
                                                timer += TickDelta / RespawnBuildtime;
                                                Server_Sendrespawntime();

                                            }
                                            if (oncedied) 
                                            {
                                                showtimer = false;
                                                Server_Sendrespawntime();
                                            }
                                        }
                                        // wenn der 2 spieler stirbt
                                        if (nonactplayer.Intellect.ControlledObject == null && nonactplayer.Intellect != null && nonactplayer.User.ConnectedNode != null
                                            && actplayer.Intellect != null && actplayer.Intellect.ControlledObject == null && actplayer.User.ConnectedNode != null && nonactplayer != actplayer && !bothdied)
                                        {
                                            bothdied = true;
                                            Computer.Instance.IncrementKilledAstronouts();
                                            Computer.Instance.SetWinner(false);
                                        }
                                    }
                                }
                            



                            if (creatAstronaut && timeelapsed <= 1 && !bothdied)
                            {
                                foreach (PlayerManager.ServerOrSingle_Player player in
                                    PlayerManager.Instance.ServerOrSingle_Players)
                                {

                                    if (player.Intellect != null && player.Intellect.ControlledObject == null && player.User.ConnectedNode != null)
                                    {
                                        //spawner Position ändern 
                                        //Unit type erstellen 
                                        if (player.started) //prüft ob der Spieler schonmal gesponed wurde.  
                                        {

                                            showtimer = true;
                                            revival += TickDelta / BuildTime;
                                            Server_Sendrespawntime();

                                            if (timerevival >= 1)
                                            {
                                                EngineConsole.Instance.Print("Die zeit ist   " + revival);



                                                //playercharacter.SetForDeletion(true);


                                                Dynamic.corpse.Die();
                                                ServerOrSingle_CreatePlayerUnit(player);
                                                Computer.Instance.IncrementReanimations();
                                                
                                                revival = 0f;
                                                timer = 0f;





                                            }
                                        }
                                        else
                                            ServerOrSingle_CreatePlayerUnit(player);
                                    }
                                }

                            }

                            if (creatAstronaut && timeelapsed > 1 && !bothdied && !oncedied)
                            {
                                showtimer = false;
                                Computer.Instance.IncrementKilledAstronouts();
                                oncedied = true;
                                Server_Sendrespawntime();
                                
                            }




                        }

                    }
                }
            }
        }


        
        void AddTextWithShadow(GuiRenderer renderer, string text, Vec2 position, HorizontalAlign horizontalAlign,
            VerticalAlign verticalAlign, ColorValue color)
        {
            Vec2 shadowOffset = 2.0f / RendererWorld.Instance.DefaultViewport.DimensionsInPixels.Size.ToVec2();

            renderer.AddText(text, position + shadowOffset, horizontalAlign, verticalAlign,
                new ColorValue(0, 0, 0, color.Alpha / 2));
            renderer.AddText(text, position, horizontalAlign, verticalAlign, color);
        }



        private void Server_Sendrespawntime()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(GameWorld),
                      (ushort)NetworkMessages.respawntimer);

            writer.Write(revival);
            writer.Write(timer);
            writer.Write(showtimer);

            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.respawntimer)]
        private void Client_Receiverespawntime(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            revival = reader.ReadSingle();
            timer = reader.ReadSingle();
            showtimer = reader.ReadBoolean();
            

            
        }


        public static float showcountdown()
        {
            
            if (revival>0.1)
                return revival;
            else return 0f;
        }



		internal void DoActionsAfterMapCreated()
		{
			if( EntitySystemWorld.Instance.IsSingle() )
			{
				if( GameMap.Instance.GameType == GameMap.GameTypes.Action ||
					GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
					GameMap.Instance.GameType == GameMap.GameTypes.TurretDemo ||
					GameMap.Instance.GameType == GameMap.GameTypes.VillageDemo ||
					GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.AVA)
				{
					string playerName = "__SinglePlayer__";

					//create Player
					PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
						ServerOrSingle_GetPlayer( playerName );
					if( player == null )
						player = PlayerManager.Instance.Single_AddSinglePlayer( playerName );

					//create PlayerIntellect
					PlayerIntellect intellect = null;
					{
						//find already created PlayerIntellect
						foreach( Entity entity in World.Instance.Children )
						{
							intellect = entity as PlayerIntellect;
							if( intellect != null )
								break;
						}

						if( intellect == null )
						{
							intellect = (PlayerIntellect)Entities.Instance.Create( "PlayerIntellect",
								World.Instance );
							intellect.PostCreate();

							player.Intellect = intellect;
						}

						//set instance
						if( PlayerIntellect.Instance == null )
							PlayerIntellect.SetInstance( intellect );
					}

					//create unit
					if( intellect.ControlledObject == null )
					{
						MapObject spawnPoint = null;
						if( !string.IsNullOrEmpty( needChangeMapSpawnPointName ) )
						{
							spawnPoint = Entities.Instance.GetByName( needChangeMapSpawnPointName ) as MapObject;
							if( spawnPoint == null )
							{
								if( GameMap.Instance.GameType != GameMap.GameTypes.TPSArcade )
									Log.Warning( "GameWorld: Object with name \"{0}\" is not exists.", needChangeMapSpawnPointName );
							}
						}

						Unit unit;
						if( spawnPoint != null )
							unit = ServerOrSingle_CreatePlayerUnit( player, spawnPoint );
						else
							unit = ServerOrSingle_CreatePlayerUnit( player );
                       
						if( needChangeMapPlayerCharacterInformation != null )
						{
							PlayerCharacter playerCharacter = (PlayerCharacter)unit;
							playerCharacter.ApplyChangeMapInformation(
								needChangeMapPlayerCharacterInformation, spawnPoint );
						}
						else
						{
							if( unit != null )
							{
								intellect.LookDirection = SphereDir.FromVector(
									unit.Rotation.GetForward() );
							}
						}
					}
				}
			}

			needChangeMapName = null;
			needChangeMapSpawnPointName = null;
			needChangeMapPlayerCharacterInformation = null;
		}

       
		Unit ServerOrSingle_CreatePlayerUnit( PlayerManager.ServerOrSingle_Player player, MapObject spawnPoint )
		{
            //ServerOrSingle_CreatePlayerUnit(player, spawnPoint.Position, spawnPoint.Rotation);
            
            string unitTypeName;
            if (!player.Bot)
            {

                if (GameMap.Instance.PlayerUnitType != null)
                    unitTypeName = GameMap.Instance.PlayerUnitType.Name;
                else
                    unitTypeName = "Astronaut";//"Rabbit";
            }
            else
                unitTypeName = player.Name;

            Unit unit = (Unit)Entities.Instance.Create(unitTypeName, Map.Instance);
            
            
            //  prüft 
            //  prüft ob ein spieler schonmal erstellt/gestarted wurde   
            //  falls nicht
            if (!player.started)
            {
                Vec3 posOffset = new Vec3(0, 0, 1.5f);
                unit.Position = spawnPoint.Position + posOffset;
                unit.Rotation = spawnPoint.Rotation;
            }
            
                
            //  falls Ja
            else 
            {
                
                Vec3 posOffset = new Vec3(0, 0, 1.5f);
                unit.Position = player.pos;
             
            }

            if (unit is PlayerCharacter)
            {
                foreach (var item in EntitySystemWorld.Instance.RemoteEntityWorlds)
                {
                    

                    string name = item.Description.Split(':')[1].Replace("\"", "").Split('(')[0].Trim();


                    if (name == player.Name)
                    {
                        ((PlayerCharacter)unit).Owner = item;
                        break;
                    }
                }
            }

            unit.PostCreate();


            if (player.Intellect != null)
            {
                player.Intellect.ControlledObject = unit;
                unit.SetIntellect(player.Intellect, false);
            }

			Teleporter teleporter = spawnPoint as Teleporter;
			if( teleporter != null )
				teleporter.ReceiveObject( unit, null );


            player.started = true;
			return unit;
		}

		Unit ServerOrSingle_CreatePlayerUnit( PlayerManager.ServerOrSingle_Player player )
		{
            SpawnPoint spawnPoint = null; // SpawnPoint.GetDefaultSpawnPoint();
            


			if( spawnPoint == null )
				spawnPoint = SpawnPoint.GetFreeRandomSpawnPoint();

			if( spawnPoint == null )
				return null;
			return ServerOrSingle_CreatePlayerUnit( player, spawnPoint );
		}

		public string NeedChangeMapName
		{
			get { return needChangeMapName; }
		}

		public string NeedChangeMapSpawnPointName
		{
			get { return needChangeMapSpawnPointName; }
		}

		public string NeedChangeMapPreviousMapName
		{
			get { return needChangeMapPreviousMapName; }
		}

		public void NeedChangeMap( string mapName, string spawnPointName,
			PlayerCharacter.ChangeMapInformation playerCharacterInformation )
		{
			if( needChangeMapName != null )
				return;
			needChangeMapName = mapName;
			needChangeMapSpawnPointName = spawnPointName;
			needChangeMapPlayerCharacterInformation = playerCharacterInformation;
			needChangeMapPreviousMapName = Map.Instance.VirtualFileName;
		}

		[Browsable( false )]
		public bool NeedWorldDestroy
		{
			get { return needWorldDestroy; }
			set { needWorldDestroy = value; }
		}
	}
}
