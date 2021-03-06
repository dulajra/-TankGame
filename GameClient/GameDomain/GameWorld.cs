﻿using System;
using System.Collections.Generic;
using System.Text;
using GameClient.Network.Messages;

namespace GameClient.GameDomain
{
    /*
    The class combining all the components of the Game. Consists of Map, Players. Handles dynamics related to time-elapse.
    */
    class GameWorld
    {
        /*
        Has the GameWorld been started?
        */
        public bool InputAllowed { get; set; }

        public int MyPlayerNumber { get; set; } // Starting from zero

        public GameWorldState State { get { return state; }
            set {
                state = value;
                if (state == GameWorldState.Finished)
                {
                    EventHandler handler = GameWorld.Instance.GameFinished;
                    if (handler != null)
                    {
                        EventArgs args = new EventArgs();
                        
                        handler(GameWorld.Instance, args);
                    }
                }
                else if (state == GameWorldState.Running)
                {
                    EventHandler handler = GameWorld.Instance.GameStarted;
                    if (handler != null)
                    {
                        EventArgs args = new EventArgs();

                        handler(GameWorld.Instance, args);
                    }
                }
            }
        }
        
        /*
        Contains locations of non-movable objects of map
        */
        public MapDetails Map { get; set; }

        /*
        The players in the GameWorlds
        */
        public PlayerDetails[] Players { get; set; }
        /*
        The updated states of bricks. 
        */
        public Brick[] BrickState { get; set; }

        /*
        The coins that are added to the world
        */
        private List<Coin> coins = new List<Coin>();

        /*
        The life packs that are added to the world
        */
        private List<LifePack> lifePacks = new List<LifePack>();

        //Singleton Instance
        private static GameWorld instance = null;

        private GameWorldState state = GameWorldState.NotStarted;


        public event EventHandler FrameAdvanced;


        private List<String> DeadPlayerNames = new List<String>();

        public event PlayerDiedEventHandler PlayerDied;
        public delegate void PlayerDiedEventHandler(object sender, PlayerDetails player);


        void NotifyPlayerDied(PlayerDetails p)
        {
            PlayerDiedEventHandler handler = PlayerDied;
            if (handler != null)
            {
                handler(this, p);
            }
        }

        /*
            Advance the gameworld to next frame
        */
        public void AdvanceFrame()
        {
            foreach(LifePack lifePack in lifePacks)
            {
                lifePack.AdvanceFrame();
            }
            foreach (Coin coin in coins)
            {
                coin.AdvanceFrame();
            }
            InputAllowed = true;

            EventHandler handler = FrameAdvanced;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }


            foreach (PlayerDetails p in Players)
            {
                if (p.Health <= 0)
                {
                    if (!DeadPlayerNames.Contains(p.Name))
                    {
                        DeadPlayerNames.Add(p.Name);

                        Coin coin = new Coin();
                        coin.Position = p.Position;
                        coin.TimeLimit = Int32.MaxValue;
                        coin.Value = p.Coins;

                        GameWorld.Instance.Coins.Add(coin);
                        //GameWorld.Instance.NotifyCoinPackAdded(coin);

                        GameWorld.Instance.NotifyPlayerDied(p);

                    }
                }
            }

        }

        /*
        The coins that are added to the world
        */
        public List<Coin> Coins
        {
            get
            {
                return coins;
            }
            set { coins = value; }
        }

        /*
        The life packs that are added to the world
        */
        public List<LifePack> LifePacks
        {
            get { return lifePacks;  }
            set { lifePacks = value; }
        }

        //events raised from GameWorld
        public event EventHandler GameFinished;
        public event EventHandler GameStarted;

        public event NegativeHonourEventHandler NegativeHonour;
        public delegate void NegativeHonourEventHandler(object Sender, NegativeHonourMessage.NegativeHonourReason reason);

        private GameWorld()
        {
            
        }

        public static GameWorld Instance
        {
            get
            {
                if(instance== null)
                    instance = new GameWorld();
                return instance;
            }
        }
        /*
        Notifies the gameworld that a negative honour has occured
        */
        public void NotifyNegativeHonour(NegativeHonourMessage.NegativeHonourReason reason)
        {
            NegativeHonourEventHandler handler = NegativeHonour;
            if (handler != null)
            {
                handler(this, reason);
            }
        }

        /*
         A textual description of entire GameWorld
        */
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("\nGame World Details ---------------------------------\n");
            builder.AppendLine("State: " + State.ToString());
            builder.AppendLine(" ");

            if (Map != null)
                builder.AppendLine("Map: " + Map.ToString());
            if (Players != null)
            {
                builder.AppendLine("Players: ");
                foreach (PlayerDetails player in Players)
                    builder.AppendLine(player.ToString());
            }
            if (BrickState != null && BrickState.Length>0)
            {
                builder.AppendLine("Bricks:");
                foreach (Brick brick in BrickState)
                    builder.Append(brick.ToString());
            }
            builder.AppendLine(" ");
            if (coins != null && coins.Count>0)
            {
                builder.AppendLine("Coins:");
                foreach (Coin coin in coins)
                    builder.Append(coin.ToString());
            }
            builder.AppendLine(" ");
            if (lifePacks != null && lifePacks.Count>0)
            {
                builder.AppendLine("Life pack:");
                foreach (LifePack lifePack in lifePacks)
                    builder.Append(lifePack.ToString());
            }
            builder.AppendLine(" ");
 
            return builder.ToString();
        }

        public enum GameWorldState
        {
            //Ready = Player has joined. But waiting for game to start
            // Running = Sever has sent the first global update. Hence, game has begun
            NotStarted, Ready ,Running, Finished
        }

    }
}
