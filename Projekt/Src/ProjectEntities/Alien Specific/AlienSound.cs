using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.SoundSystem;
using Engine;
using Engine.EntitySystem;
using System.Timers;

namespace ProjectEntities
{
    public class AlienSoundType : EntityType
    {
    }

    class AlienSound: Entity
    {
        static AlienSound instance;
        AlienSoundType _type = null; public new AlienSoundType Type { get { return _type; } }

        // Channel zum abspielen des Default-Sounds für die kleinen Aliens
        Sound alienSound;
        VirtualChannel alienChannel;
        bool playingAllowed = true;
        String currentSoundName;
        Timer timer;

        public AlienSound()
		{
			if( instance != null )
				Log.Fatal( "AlienSound already created" );
			instance = this;
            timer = new Timer(3000);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(ResetTimer);
		}

        public static AlienSound Instance
        {
            get { return instance; }
        }

        void ResetTimer(object sender, ElapsedEventArgs e)
        {
            playingAllowed = true;
        }

        /// <summary>
        /// Spielt den Sound mit entsprechendem Alias ab
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="sound"></param>
        public void PlaySound(String soundName, Sound sound)
        {
            if (playingAllowed || currentSoundName != soundName)
            {
                if (alienChannel != null)
                {
                    alienChannel.Stop();
                }

                // Play sound
                alienSound = sound;
                currentSoundName = soundName;
                if (alienSound != null)
                {
                    this.alienChannel = SoundWorld.Instance.SoundPlay(alienSound, EngineApp.Instance.DefaultSoundChannelGroup, 1f, false);
                }

                // Timer Reseten
                playingAllowed = false;
                timer.Start();
            }
        }
    }
}
