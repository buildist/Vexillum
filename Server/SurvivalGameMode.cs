using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Game;
using Vexillum.Entities;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework;
using Vexillum.util;
using Vexillum;
using System.Threading;

namespace Server
{
    public class SurvivalGameMode : SurvivalGameModeShared
    {
        private Server server;
        private bool inProgress = false;
        private int greenFlagDropTime;
        private int blueFlagDropTime;
        private bool antiStalemate = false;
        public override void UpdatePlayerList()
        {
            numPlayers = server.players.Count;
            numGreen = 0;
            numBlue = 0;
            foreach(ServerPlayer p in server.players)
            {
                if(p.PlayerClass == PlayerClass.Green)
                    numGreen++;
                else if(p.PlayerClass == PlayerClass.Blue)
                    numBlue++;
            }
            server.ResetStrategy();
        }
        public void SetServer(Server s)
        {
            server = s;
            inProgress = true;
        }
        public void Command(ServerPlayer p, int c, object value)
        {
            if (value is byte)
                p.SendGameModeByte(c, (byte)value);
            else if (value is string)
                p.SendGameModeString(c, (string)value);
            else if (value is short)
                p.SendGameModeShort(c, (short)value);
            else if(value is Player)
                p.SendGameModeShort(c, (short) ((Player)value).GetID());
            else if (value == null)
                p.SendGameModeShort(c, -1);
            else
                Server.Debug("Tried to send invalid command type " + value); 
        }
        public void Command(int c, object value)
        {
            if (value is byte)
                server.SendGameModeByte(c, (byte)value);
            else if (value is string)
                server.SendGameModeString(c, (string)value);
            else if (value is short)
                server.SendGameModeShort(c, (short)value);
            else if (value is Player)
                server.SendGameModeShort(c, (short)((Player)value).GetID());
            else if(value == null)
                server.SendGameModeShort(c, -1);
            else
                Server.Debug("Tried to send invalid command type " + value);
        }
        public override void SetClass(Player p, PlayerClass pClass)
        {
            base.SetClass(p, pClass);
            p.Inventory = server.gameMode.GetPlayerInventory(p);
            p.SelectWeapon(0);
            if (((ServerPlayer)p).ready)
            {
                server.SendClassChange(p, pClass);
                p.Entity.Weapon.MouseUpServer(level, 0, 0, 0);
            }
        }
        public override void PlayerRemoved(Player p)
        {
            base.PlayerRemoved(p);
            DropFlag(p, true);
            server.SendChat(TextUtil.COLOR_ORANGE + p.name + " left the game");
        }
        public override void PlayerAdded(Player p)
        {
            base.PlayerAdded(p);
            if(blueFlagCarrier != null)
                Command((ServerPlayer)p, GameModeCommand.BLUEFLAG_CARRIER, blueFlagCarrier);
            if (greenFlagCarrier != null)
                Command((ServerPlayer)p, GameModeCommand.GREENFLAG_CARRIER, greenFlagCarrier);
            Command((ServerPlayer)p, GameModeCommand.BLUE_SCORE, (byte)blueCaptures);
            Command((ServerPlayer)p, GameModeCommand.GREEN_SCORE, (byte)greenCaptures);
            Command(GameModeCommand.MAX_CAPTURES, (byte) maxCaptures);
            server.SendChat(TextUtil.COLOR_ORANGE + p.name + " joined the game");

        }
        public override void PlayerHit(Player attacker, Player defender, Weapon weapon, Vec2 hitPos)
        {
            float d = (defender.Position - attacker.Position).Length();
            float damage = ((ReloadableWeapon)weapon).GetDamage(d);
            defender.Entity.Health -= damage;
            PlayerHealthChanged(defender, weapon);
        }

        public override void PlayerHealthChanged(Player p, Weapon weapon)
        {
            server.SendPlayerHealth(p);
            if (p.Entity.Health == 0)
            {
                DropFlag(p, true);
                SetClass(p, PlayerClass.Spectator);
                Kill(weapon == null ? (ServerPlayer) p : (ServerPlayer)weapon.entity.player, (ServerPlayer)p, weapon);
                new Thread(this.RespawnPlayer).Start(p);
            }
        }
        public void SetSpectator(Player p)
        {
            DropFlag(p, true);
            SetClass(p, PlayerClass.Spectator);
        }
        public void ResetPlayer(Player p)
        {
            p.Entity.Health = 0;
            PlayerHealthChanged(p, null);
        }
        private void RespawnPlayer(object p)
        {
            Thread.Sleep(respawnTime);
            ServerPlayer player = (ServerPlayer)p;
            if (server.players.Contains(player))
            {
                server.AddTask(delegate()
                {
                    player.Teleport(GetSpawnPosition(player.PlayerClass, player.Entity));
                    player.Entity.Health = player.Entity.MaxHealth;
                    PlayerHealthChanged(player, null);
                    SetClass(player, player.PlayerClass);
                });
            }
        }
        private void CheckForStalemate()
        {
            antiStalemate = greenFlagCarrier != null && blueFlagCarrier != null;
        }
        public override void OnFlagCollide(Player p, Entity flag)
        {
            if (p is AIPlayer)
                return;
            if (p.PlayerClass == PlayerClass.Green && greenFlagDropTime == 0)
            {
                if (flag is GreenFlagEntity && p == greenFlagCarrier)
                {
                    CaptureFlag(p);
                    server.ResetStrategy();
                }
                else if (flag is BlueFlagEntity && greenFlagCarrier == null && blueFlagDropTime == 0)
                {
                    TakeFlag(p);
                    server.ResetStrategy();
                }
            }
            else if (p.PlayerClass == PlayerClass.Blue)
            {
                if (flag is BlueFlagEntity && p == blueFlagCarrier)
                {
                    CaptureFlag(p);
                    server.ResetStrategy();
                }
                else if (flag is GreenFlagEntity )
                {
                    TakeFlag(p);
                    server.ResetStrategy();
                }
            }
        }
        private void TakeFlag(Player p)
        {
            if (p.CurrentClass != PlayerClass.Spectator)
                BroadcastFlagTakenMessage(p);
            if (p.PlayerClass == PlayerClass.Blue)
            {
                blueFlagCarrier = p;
                greenFlagDropTime = 0;
                ((ServerLevel)level).RemoveFlag(PlayerClass.Green);
                Command(GameModeCommand.BLUEFLAG_CARRIER, p);
            }
            else
            {
                greenFlagCarrier = p;
                blueFlagDropTime = 0;
                ((ServerLevel)level).RemoveFlag(PlayerClass.Blue);
                Command(GameModeCommand.GREENFLAG_CARRIER, p);
            }
            server.level.TakeFlag(GetEnemyClass(p.PlayerClass));
        }
        private void DropFlag(Player p, bool message)
        {
            if (p == greenFlagCarrier)
            {
                greenFlagCarrier = null;
                server.level.PlaceFlag(PlayerClass.Blue, p.Position);
                blueFlagDropTime = level.GetTime();
                Command(GameModeCommand.GREENFLAG_CARRIER, null);
                if (message)
                    BroadcastFlagDroppedMessage(p);
                server.ResetStrategy();
            }
            else if (p == blueFlagCarrier)
            {
                blueFlagCarrier = null;
                server.level.PlaceFlag(PlayerClass.Green, p.Position);
                greenFlagDropTime = level.GetTime();
                Command(GameModeCommand.BLUEFLAG_CARRIER, null);
                if (message)
                    BroadcastFlagDroppedMessage(p);
                server.ResetStrategy();
            }
        }
        private void CaptureFlag(Player pl)
        {
            if (pl == greenFlagCarrier)
            {
                greenFlagCarrier = null;
                server.level.PlaceFlag(PlayerClass.Blue);
                Command(GameModeCommand.GREENFLAG_CARRIER, null);
                greenCaptures++;
                Command(GameModeCommand.GREEN_SCORE, (byte) greenCaptures);

                pl.Score += 3;
                server.SendScore(pl);
                if (greenCaptures >= maxCaptures)
                    Win(PlayerClass.Green);
            }
            else if (pl == blueFlagCarrier)
            {
                blueFlagCarrier = null;
                server.level.PlaceFlag(PlayerClass.Green);
                Command(GameModeCommand.BLUEFLAG_CARRIER, null);
                blueCaptures++;
                Command(GameModeCommand.BLUE_SCORE, (byte) blueCaptures);

                pl.Score += 3;
                server.SendScore(pl);
                if (blueCaptures >= maxCaptures)
                    Win(PlayerClass.Blue);
            }

            PlayerClass cl = pl.PlayerClass;
            string[] name = new string[] { GetDisplayName(pl) };
            foreach (ServerPlayer p in server.players)
            {
                if (p.PlayerClass != PlayerClass.Spectator)
                {
                    if(p == pl)
                        p.SendMessage(Messages.OURFLAG_CAPTURED_1, null, 2000);
                    else if (p.PlayerClass != cl)
                        p.SendMessage(Messages.OURFLAG_CAPTURED, null, 2000);
                    else
                        p.SendMessage(Messages.THEIRFLAG_CAPTURED, name, 2000);
                }
            }
        }
        public void Kill(ServerPlayer attacker, ServerPlayer defender, Weapon w)
        {
            attacker.Score++;
            server.SendScore(attacker);
            if (attacker != defender)
            {
                defender.SendMessage(Messages.KILLED_BY, new string[] { GetDisplayName(attacker) }, 2000);
                attacker.SendMessage(Messages.YOU_KILLED, new string[] { GetDisplayName(defender) }, 2000);
            }
            if (defender.Entity.hook != null)
            {
                level.RemoveEntity(defender.Entity.hook);
                defender.Entity.hook = null;
            }
            if (defender is AIPlayer)
                ((AIPlayer)defender).ClearTarget();
        }
        private void ResetFlag(PlayerClass c)
        {
        }
        private void Win(PlayerClass c)
        {
            if (!inProgress)
                return;
            inProgress = false;
            new Thread(new ThreadStart(delegate()
            {
                if (c == PlayerClass.Green)
                    server.SendMessage(Messages.GREEN_WIN, null, 5000);
                else
                    server.SendMessage(Messages.BLUE_WIN, null, 5000);
                Thread.Sleep(5000);
                if (c == PlayerClass.Green)
                    Command(GameModeCommand.GREEN_WIN, null);
                else
                    Command(GameModeCommand.BLUE_WIN, null);
                Thread.Sleep(8000);
                server.AddTask(delegate()
                {
                    server.SetRandomLevel();
                });
            })).Start();
        }
        private void BroadcastFlagTakenMessage(Player pl)
        {
            PlayerClass cl = pl.PlayerClass;
            string[] name = new string[] {GetDisplayName(pl)};
            foreach (ServerPlayer p in server.players)
            {
                if (p.PlayerClass != PlayerClass.Spectator && p != pl)
                {
                    if (p.PlayerClass != cl)
                        p.SendMessage(Messages.OURFLAG_TAKEN, null, 2000);
                    else
                        p.SendMessage(Messages.THEIRFLAG_TAKEN, name, 2000);
                }
                else if (p == pl)
                    p.SendMessage(Messages.NOOB_INSTRUCTIONS, null, 2000);
            }
        }
        private void BroadcastFlagDroppedMessage(Player pl)
        {
            PlayerClass cl = pl.PlayerClass;
            string[] name = new string[] { GetDisplayName(pl) };
            foreach (ServerPlayer p in server.players)
            {
                if (p.PlayerClass != PlayerClass.Spectator && p != pl)
                {
                    if (p.PlayerClass != cl)
                        p.SendMessage(Messages.OURFLAG_DROPPED, null, 2000);
                    else
                        p.SendMessage(Messages.THEIRFLAG_DROPPED, name, 2000);
                }
            }
        }
        private void BroadcastFlagReturnedMessage(PlayerClass cl)
        {
            foreach (ServerPlayer p in server.players)
            {
                if (p.PlayerClass != PlayerClass.Spectator)
                {
                    if (p.PlayerClass == cl)
                        p.SendMessage(Messages.OURFLAG_RETURNED, null, 2000);
                    else
                        p.SendMessage(Messages.THEIRFLAG_RETURNED, null, 2000);
                }
            }
        }
        public void Step(int gameTime)
        {
            if (greenFlagDropTime != 0 && gameTime - greenFlagDropTime > 10000)
            {
                if (greenFlagDropTime != 0 && gameTime - greenFlagDropTime > 10000)
                {
                    ((ServerLevel)level).RemoveFlag(PlayerClass.Green);
                    ((ServerLevel)level).PlaceFlag(PlayerClass.Green);
                    greenFlagDropTime = 0;
                    BroadcastFlagReturnedMessage(PlayerClass.Green);
                }
            }
            if (blueFlagDropTime != 0 && gameTime - blueFlagDropTime > 10000)
            {
                ((ServerLevel)level).RemoveFlag(PlayerClass.Blue);
                ((ServerLevel)level).PlaceFlag(PlayerClass.Blue);
                blueFlagDropTime = 0;
                BroadcastFlagReturnedMessage(PlayerClass.Blue);
            }
        }
    }
}
