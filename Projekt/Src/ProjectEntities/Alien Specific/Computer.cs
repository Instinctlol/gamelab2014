using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
    public class ComputerType : EntityType
    {
    }

    /// <summary>
    /// Main computer that can be controlled by the boss alien and
    /// the astronauts
    /// </summary>
    public class Computer : Entity
    {
        static Computer instance;

        /*************/
        /* Attribute */
        /*************/
        ComputerType _type = null; public new ComputerType Type { get { return _type; } }
        const int maxAliens = 20;
        const int maxPowerCoupons = 10;
        const int maxRotationCoupons = 10;

        [FieldSerialize]
        int experiencePoints = 50;
        int rotationCoupons = 0;
        int powerCoupons = 0;
        int availableAliens = 0;
        int usedAliens = 0;
        bool noSpawnTime = false;

        // Für USB
        bool alienControlPaused = false;
        Timer alienControlTimer;

        //Schere Stein PapierWindow
        private Task csspwTask;
        //Ob Licht geändert werden darf oder nicht
        private bool allowedToChangeLight;


        // Bis zu welcher Gruppennummer dürfen Items gedroppt werden
        int maxItemDropGroupNr = 6;

        // Speichert die im Spiel durchgeführten Rotationen, damit das SectorStatusWindow diese beim initialize nachmachen kann
        // ringRotations[0] ist Ring F1, ringRotations[1] ist Ring F2 und ringRotations[2] ist Ring F3 (innerer Ring)
        // Bei Rechtsrotation wird 1 % 8 addiert, bei Linksrotation 1 % 8 subtrahiert.
        int[] ringRotations = new int[3];

        // Verwaltet alle Alienleichen in der Reihenfolge, wie sie gestorben sind
        Queue<Corpse> corpseList = new Queue<Corpse>();
        const int maxAlienCorpses = 10;

        // Verwaltet die Signale für das Radar
        public ThreadSafeList<Signal> signalList = new ThreadSafeList<Signal>();
        
        // Statistik
        Statistic statistic;

        // Anzeige der Statistik für Server und Clients
        bool winnerFound = false;
        bool astronautwin = false;
        bool ende = false;
        int diedAstronouts = 0;
        
        enum NetworkMessages
        {
            DamageAstronoutsToClients,
            KilledAliensToClients,
            KilledAstronoutsToClients,
            SpawnedAliensToClients,
            ReanimationsToClients,
            AstronoutWinToClients,
            EndGameToClients
        }
        public event StatisticEventDelegate endGame;
        public delegate void StatisticEventDelegate();
        

        /*******************/
        /* Getter / Setter */
        /*******************/
        public Computer()
		{
			if( instance != null )
				Log.Fatal( "Computer: Computer is already created." );
			instance = this;
		}

        public static new Computer Instance
        {
            get{ return instance; }
        }

        public bool Astronautwin
        {
            get { return astronautwin; }
            set { SetWinner(value); }
        }

        public bool WinnerFound
        {
            get { return winnerFound; }
            set { winnerFound = value; }
        }
        
        public int ExperiencePoints
        {
            get { return experiencePoints; }
            set { experiencePoints = value; }
        }

        public int RotationCoupons
        {
            get { return rotationCoupons; }
        }

        public int PowerCoupons
        {
            get { return powerCoupons; }
        }

        public int AvailableAliens
        {
            get { return availableAliens; }
        }

        public int UsedAliens
        {
            get { return usedAliens; }
        }

        public int[] RingRotations
        {
            get { return ringRotations; }
        }

        public Task CsspwTask
        {
            get { return csspwTask; }
            set { csspwTask = value; }
        }

        public bool AllowedToChangeLight
        {
            get { return allowedToChangeLight; }
            set { allowedToChangeLight = value; }
        }

        public int MaxItemDropGroupNr
        {
            get { return maxItemDropGroupNr; }
        }

        public bool AlienControlPaused
        {
            get { return alienControlPaused; }
        }

        public bool NoSpawnTime
        {
            get { return noSpawnTime; }
            set { noSpawnTime = value; }
        }

        public int DiedAstronouts
        {
            get { return diedAstronouts; }
        }

        public Statistic Statistic
        {
            get { return statistic; }
        }

        /**************/
        /* Funktionen */
        /**************/
        /// <summary>
        /// Anzahl erhöhen bis Gesamtanzahl an Gruppen der Drop-Items des Aliens erreicht wurde. Damit kann eingeschränkt werden, 
        /// bis zu welcher Gruppennummer die Items gedroppt werden dürfen.
        /// </summary>
        public void IncrementMaxItemDropGroupNr()
        {
            Alien a = Entities.Instance.Create("Alien", Map.Instance) as Alien;
            if (MaxItemDropGroupNr < a.Type.DieObjects.Count)
            {
                maxItemDropGroupNr++;
            }
        }

        /// <summary>Light
        /// ExperiencePoints hinzufügen
        /// </summary>
        /// <param name="value"></param>
        public void AddExperiencePoints(int value)
        {
            if (value >= 0)
            {
                experiencePoints += value;
            }
        }

        /// <summary>
        /// ExperiencePoints um Eins erniedrigen
        /// </summary>
        public void DecrementExperiencePoints()
        {
            if (ExperiencePoints > 0)
            {
                experiencePoints--;
            }
        }
        /// <summary>
        /// Rotation Coupons um Eins erhöhen
        /// </summary>
        public void IncrementRotationCoupons()
        {
            if (RotationCoupons < maxRotationCoupons)
            {
                rotationCoupons++;
            }
        }
        /// <summary>
        /// Rotation Coupons um Eins erniedrigen
        /// </summary>
        public void DecrementRotationCoupons()
        {
            if (RotationCoupons > 0)
            {
                rotationCoupons--;
            }
        }

        /// <summary>
        /// Power Coupons um Eins erhöhen
        /// </summary>
        public void IncrementPowerCoupons()
        {
            if (PowerCoupons < maxPowerCoupons)
            {
                powerCoupons++;
            }
        }
        /// <summary>
        /// Power Coupons um Eins erniedrigen
        /// </summary>
        public void DecrementPowerCoupons()
        {
            if (PowerCoupons > 0)
            {
                powerCoupons--;
            }
        }

        /// <summary>
        /// Ein Alien wurde gespawnt, d.h. Ein verfügbares Alien weniger und ein aktives Alien mehr
        /// </summary>
        public void AddUsedAlien()
        {
            if (AvailableAliens > 0)
            {
                DecrementAvailableAliens();
                IncrementUsedAliens();
            }
        }

        /// <summary>
        /// Verfügbare Aliens um Eins erhöhen
        /// </summary>
        public void IncrementAvailableAliens()
        {
            if ((AvailableAliens + UsedAliens) < maxAliens)
            {
                availableAliens++;
            }
        }
        /// <summary>
        /// Verfügbare Aliens um Eins erniedrigen
        /// </summary>
        private void DecrementAvailableAliens()
        {
            availableAliens--;
        }

        /// <summary>
        /// Aktive Aliens um Eins erhöhen
        /// </summary>
        private void IncrementUsedAliens()
        {
            if (EntitySystemWorld.Instance.IsServer()){
                usedAliens++;
                statistic.IncrementSpawnedAliens();
                Server_SpawnedAliensToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }

        }
        
        /// <summary>
        /// Aktive Aliens um Eins erniedrigen
        /// </summary>
        public void DecrementUsedAliens()
        {
            if (UsedAliens > 0)
            {
                if (EntitySystemWorld.Instance.IsServer())
                {
                    usedAliens--;
                    statistic.IncrementKilledAliens(); 
                    Server_KilledAliensToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
                }                
            }
        }

        public void IncrementDiedAstronouts()
        {
            diedAstronouts++;
            if (diedAstronouts >= 2)
            {
                SetWinner(false);
            }
        }

        /// <summary>
        /// Do one rotation of one ring to the left or to the right side.
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="left"></param>
        public void RotateRing(Ring ring, bool left)
        {
            if (AlienControlPaused)
            {
                StatusMessageHandler.sendMessage("Die Astronauten haben die Kontrolle");
            }
            else if (ring == null)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Ring ausgewählt");
            }
            else if (RotationCoupons <= 0)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Keine Rotationen möglich");
            }
            else if (ring.CanRotate() == false)
            {
                StatusMessageHandler.sendMessage("Dieser Ring ist zur Zeit nicht rotierbar");
            }
            else
            {
                DecrementRotationCoupons();
                int ringNumber = ring.GetRingNumber();
                if (left)
                {
                    //EngineConsole.Instance.Print("computer links drehen");
                    ringRotations[ringNumber - 1] = mod(RingRotations[ringNumber - 1] - 1, 8);
                    ring.RotateLeft();
                }
                else
                {
                    //EngineConsole.Instance.Print("computer rechts drehen");
                    ringRotations[ringNumber - 1] = mod(RingRotations[ringNumber - 1] + 1, 8);
                    ring.RotateRight();
                }
                ///ToDo: Gridaktualisierung für die Rotation in der Map
                GridBasedNavigationSystem.Instances[0].UpdateMotionMap();
            }
        }

        public void AstronautRotateRing(Ring ring, bool left)
        {
            if (ring == null)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Ring ausgewählt");
            }
            else
            {
                int ringNumber = ring.GetRingNumber();
                if (left)
                {
                    //EngineConsole.Instance.Print("computer links drehen");
                    ringRotations[ringNumber - 1] = mod(RingRotations[ringNumber - 1] - 1, 8);
                    ring.RotateLeft();
                }
                else
                {
                    //EngineConsole.Instance.Print("computer rechts drehen");
                    ringRotations[ringNumber - 1] = mod(RingRotations[ringNumber - 1] + 1, 8);
                    ring.RotateRight();
                }
                ///ToDo: Gridaktualisierung für die Rotation in der Map
                GridBasedNavigationSystem.Instances[0].UpdateMotionMap();
            }
        }

        /// <summary>
        /// Switches off or on the power of one sector (room)
        /// </summary>
        /// <param name="sector"></param>
        public void SetSectorPower(Sector sector)
        {
            if (AlienControlPaused)
            {
                StatusMessageHandler.sendMessage("Die Astronauten haben die Kontrolle");
            }
            else if (AllowedToChangeLight)
            {
                if (sector == null)
                {
                    // Nachricht ausgeben
                    StatusMessageHandler.sendMessage("Kein Sector ausgewählt");
                }
                else if (PowerCoupons <= 0)
                {
                    // Nachricht ausgeben
                    StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
                }
                else
                {
                    DecrementPowerCoupons();
                    sector.SwitchLights(false);
                }
            }
            else
            {
                StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
            }
            
        }

        /// <summary>
        /// Switches on the light of one sectorgroup
        /// </summary>
        /// <param name="sectorGroup"></param>
        /// <param name="b"</param>
        public void SetSectorGroupPowerAstronaut(SectorGroup sectorGroup, bool b)
        {
            if(sectorGroup != null)
                sectorGroup.LightStatus = b;
        }

        /// <summary>
        /// Switches off or on the power of one sectorgroup
        /// </summary>
        /// <param name="sectorGroup"></param>
        /// <param name="b"</param>
        public void SetSectorGroupPower(SectorGroup sectorGroup, bool b)
        {
            if (AlienControlPaused)
            {
                StatusMessageHandler.sendMessage("Die Astronauten haben die Kontrolle");
            }
            else if (AllowedToChangeLight)
            {
                if (sectorGroup == null)
                {
                    // Nachricht ausgeben
                    StatusMessageHandler.sendMessage("Kein Sector ausgewählt");
                }
                else if (PowerCoupons <= 0)
                {
                    // Nachricht ausgeben
                    StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
                }
                else
                {
                    DecrementPowerCoupons();
                    sectorGroup.LightStatus = b;
                }
            }
            else
            {
                StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
            }
            
        }

        /// <summary>
        /// Liefert die Anzahl Astronauten, die leben
        /// </summary>
        /// <returns></returns>
        public static int GetNumberOfActiveAstronauts()
        {
            IEnumerable<GameCharacter> player = Entities.Instance.EntitiesCollection.OfType<GameCharacter>();
            int counter = player.Count();
            // Falls tote Spieler trotzdem ausgelesen werden prüfen, ob diese noch Lebenspunkte haben oder schon tod sind
            foreach (GameCharacter p in player)
            {
                if (p.IsDestroyed || p.Died || p.Health <= 0)
                {
                    // Dann um Eins dekrementieren
                    counter--;
                }
            }
            // Anzahl zurückgeben
            return counter;
        }

        /// <summary>
        /// Alles wird auf das Maximum gesetzt
        /// </summary>
        public void SetToMaximum()
        {
            availableAliens = (maxAliens - UsedAliens);
            rotationCoupons = maxRotationCoupons;
            powerCoupons = maxPowerCoupons;
        }

        /*public static void doCsspwSet()
        {
            if (csspwSet != null)
                csspwSet();
        }*/

        private int mod(int x, int m)
        {
            return (x%m + m)%m;
        }

        /// <summary>
        /// Fügt eine neue Alienleiche der Liste hinzu
        /// </summary>
        /// <param name="corpse"></param>
        public void AddAlienCorpse(Corpse corpse)
        {
            corpseList.Enqueue(corpse);
            if (corpseList.Count > maxAlienCorpses)
            {
                DeleteFirstAlienCorpse();
            }
        }

        /// <summary>
        /// Wenn USB von Astronauten eingesetzt wurde, dann hat das Alien für 2 Min keine Kontrolle.
        /// Timmer erstellen, der nach 2 Min die Kontrolle wieder einstellt.
        /// </summary>
        public void SetAlienControlPaused()
        {
            alienControlPaused = true;
            StatusMessageHandler.sendControlMessage("Astronauten haben die Kontrolle!");
            alienControlTimer = new Timer(120000); // 2 Min
            alienControlTimer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            alienControlTimer.Enabled = true; // Enable it
        }

        /// <summary>
        /// Kontrolle für Alien wieder einstellen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            alienControlPaused = false;
            StatusMessageHandler.sendControlMessage("");
        }

        /// <summary>
        /// Löscht eine Alienleiche aus der Liste und lässt die Leiche im Spiel verschwinden
        /// </summary>
        private void DeleteFirstAlienCorpse()
        {
            Corpse corpse = corpseList.Dequeue();
            corpse.Die();
        }
        
        public void AddDamageAstronouts(float damage)
        {
            if(EntitySystemWorld.Instance.IsServer())
            {
                statistic.AddDamageAstronouts(damage);
                Server_DamageAstronoutsToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }
            
        }

        public void IncrementKilledAstronouts()
        {
            if (EntitySystemWorld.Instance.IsServer())
            {
                statistic.IncrementKilledAstronouts();
                Server_KilledAstronoutsToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }
        }

        /// <summary>
        /// Fügt ein Signal der Minimap für eine gewisse Zeit hinzu
        /// </summary>
        /// <param name="v"></param>
        public void AddRadarElement(Vec2 min, Vec2 max)
        {
            //Position an AlienGameWindow senden, um damit weiter zu arbeiten
            Signal s = new Signal(min, max);
            if (!signalList.Contains(s))
            {
                signalList.AddLast(s);

                Timer radarTimer = new Timer(30000);
                radarTimer.Elapsed += new ElapsedEventHandler(RemoveRadarElement);
                radarTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Löscht das erste Signalelement der Signalliste
        /// </summary>
        /// <param name="v"></param>
        void RemoveRadarElement(object source, ElapsedEventArgs e)
        {
            //Position an AlienGameWindow senden, um damit weiter zu arbeiten
            if (signalList.Count() > 0 && signalList != null) 
            {
                signalList.TryRemoveFirst();
            }
        }

        /// <summary>
        /// Reset aller Attribute, die mit der Zeit erhöht werden, damit bei einem Neustart (ohne Beenden der exe) die alten Werte nicht
        /// einfach weiter hochgezählt werden. Wird beim OnAttach im AlienGameWindow ausgeführt.
        /// </summary>
        public static void Reset()
        {
            Instance.ResetInstance();
        }

        private void ResetInstance()
        {
            statistic = new Statistic();
            astronautwin = false;
            experiencePoints = 50;
            rotationCoupons = 0;
            powerCoupons = 0;
            availableAliens = 0;
            usedAliens = 0;
            maxItemDropGroupNr = 6;
            diedAstronouts = 0;
        }

        public void SetWinner(bool astronout)
        {
            WinnerFound = true;
            astronautwin = astronout;

            // Clients bescheid geben
            if (EntitySystemWorld.Instance.IsServer())
            {
                Server_AstronountWinToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }

            ende = true;

            if (EntitySystemWorld.Instance.IsServer())
            {
                Server_EndGameToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }
            if (endGame != null)
            {
                endGame();
            }
        }

        ///////////////////////////////////////////
        // Server side
        ///////////////////////////////////////////
        void Server_DamageAstronoutsToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Computer), (ushort)NetworkMessages.DamageAstronoutsToClients);
            writer.Write(statistic.DamageAstronouts);
            EndNetworkMessage();
        }
        void Server_KilledAliensToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Computer), (ushort)NetworkMessages.KilledAliensToClients);
            writer.WriteVariableInt32(statistic.KilledAliens);
            EndNetworkMessage();
        }
        void Server_KilledAstronoutsToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Computer), (ushort)NetworkMessages.KilledAstronoutsToClients);
            writer.WriteVariableInt32(statistic.KilledAstronouts);
            EndNetworkMessage();
        }
        void Server_SpawnedAliensToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Computer), (ushort)NetworkMessages.SpawnedAliensToClients);
            writer.WriteVariableInt32(statistic.SpawnedAliens);
            EndNetworkMessage();
        }
        void Server_ReanimationsToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Computer), (ushort)NetworkMessages.ReanimationsToClients);
            writer.WriteVariableInt32(statistic.Reanimations);
            EndNetworkMessage();
        }      
        void Server_AstronountWinToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Computer), (ushort)NetworkMessages.AstronoutWinToClients);
            writer.Write(astronautwin);
            EndNetworkMessage();
        }
        void Server_EndGameToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Computer), (ushort)NetworkMessages.EndGameToClients);
            writer.Write(ende);
            EndNetworkMessage();
        }

        ///////////////////////////////////////////
        // Client side
        ///////////////////////////////////////////
        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.DamageAstronoutsToClients)]
        void Client_ReceiveDamageAstronounts(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            // Daten lesen
            float damageAstronouts = reader.ReadSingle();

            if (!reader.Complete())
            {
                return;
            }

            // Daten setzen 
            this.statistic.DamageAstronouts = damageAstronouts;
        }
        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.KilledAliensToClients)]
        void Client_ReceiveKilledAliens(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            // Daten lesen
            int killedAliens = reader.ReadVariableInt32();
        
            if (!reader.Complete())
            {
                return;
            }

            // Daten setzen 
            this.statistic.KilledAliens = killedAliens;
        }
        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.KilledAstronoutsToClients)]
        void Client_ReceiveKilledAstronounts(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            // Daten lesen
            int killedAstronouts = reader.ReadVariableInt32();
        
            if (!reader.Complete())
            {
                return;
            }

            // Daten setzen 
            this.statistic.KilledAstronouts = killedAstronouts;
        }
        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SpawnedAliensToClients)]
        void Client_ReceiveSpawnedAliens(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            // Daten lesen
            int spawnedAliens = reader.ReadVariableInt32();
        
            if (!reader.Complete())
            {
                return;
            }

            // Daten setzen 
            this.statistic.SpawnedAliens = spawnedAliens;
        }
        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.ReanimationsToClients)]
        void Client_ReceiveReanimations(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            // Daten lesen
            int reanimations = reader.ReadVariableInt32();
        
            if (!reader.Complete())
            {
                return;
            }

            // Daten setzen 
            this.statistic.Reanimations = reanimations;
        }
        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.AstronoutWinToClients)]
        void Client_ReceiveAstronoutWin(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            // Daten lesen
            bool astronoutwin = reader.ReadBoolean();
        
            if (!reader.Complete())
            {
                return;
            }

            // Daten setzen 
            this.winnerFound = true;
            this.astronautwin = astronoutwin;
        }
        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.EndGameToClients)]
        void Client_ReceiveEndGame(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            // Daten lesen
            bool endeGame = reader.ReadBoolean();

            if (!reader.Complete())
            {
                return;
            }

            // Daten setzen 
            this.ende = endeGame;
        }
    }
}