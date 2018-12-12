// Decompiled with JetBrains decompiler
// Type: Rank__.RankModule
// Assembly: Rank++, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2EC4B70E-FE85-4370-A8F3-0A96F8B6071C
// Assembly location: C:\Users\User\Desktop\Server Fougerite F1.67f RB1.87 + Plugins\Server\Modules\Rank++\Rank++.dll

using Fougerite;
using Fougerite.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Rank__
{
  public class RankModule : Module
  {
    private string n;
    private string l;
    private StreamWriter log;
    private DataStore ds;

    public virtual string Name
    {
      get
      {
        return "Rank++";
      }
    }

    public virtual string Author
    {
      get
      {
        return "Snake";
      }
    }

    public virtual string Description
    {
      get
      {
        return "Tops, rankings and all statistics of your players tracked!";
      }
    }

    public virtual Version Version
    {
      get
      {
        return new Version("2.2");
      }
    }

    public virtual void Initialize()
    {
      this.l = "[" + (object) DateTime.Now + "] ";
      bool flag = true;
      if (!Directory.Exists(this.get_ModuleFolder()))
      {
        flag = false;
        Directory.CreateDirectory(this.get_ModuleFolder());
      }
      if (File.Exists(this.get_ModuleFolder() + "\\log.txt"))
      {
        this.log = File.AppendText(this.get_ModuleFolder() + "\\log.txt");
        this.log.WriteLine(this.l + "---- RANK++ INITIALIZATION ----");
        if (!flag)
          this.log.WriteLine(this.l + "- WARNING : FOLDER WAS MISSING / NEW ONE CREATED");
        this.log.WriteLine(this.l + "LOG LOADED");
      }
      else
      {
        this.log = new StreamWriter((Stream) File.Create(this.get_ModuleFolder() + "\\log.txt"));
        this.log.WriteLine(this.l + "---- RANK++ INITIALIZATION ----");
        if (!flag)
          this.log.WriteLine(this.l + "WARNING : FOLDER WAS MISSING / NEW ONE CREATED");
        this.log.WriteLine(this.l + "WARNING : LOG WAS MISSING / NEW ONE CREATED");
      }
      this.log.AutoFlush = true;
      // ISSUE: method pointer
      Hooks.add_OnCommand(new Hooks.CommandHandlerDelegate((object) this, __methodptr(HandleCommand)));
      // ISSUE: method pointer
      Hooks.add_OnPlayerConnected(new Hooks.ConnectionHandlerDelegate((object) this, __methodptr(PlayerConnected)));
      // ISSUE: method pointer
      Hooks.add_OnPlayerDisconnected(new Hooks.DisconnectionHandlerDelegate((object) this, __methodptr(PlayerDisconnected)));
      // ISSUE: method pointer
      Hooks.add_OnPlayerKilled(new Hooks.KillHandlerDelegate((object) this, __methodptr(PlayerKilled)));
      // ISSUE: method pointer
      Hooks.add_OnPlayerGathering(new Hooks.PlayerGatheringHandlerDelegate((object) this, __methodptr(PlayerGathering)));
      // ISSUE: method pointer
      Hooks.add_OnNPCKilled(new Hooks.KillHandlerDelegate((object) this, __methodptr(NPCKilled)));
      this.log.WriteLine(this.l + "HOOKS LOADED");
    }

    public virtual void DeInitialize()
    {
      this.l = "[" + (object) DateTime.Now + "]";
      this.log.WriteLine(this.l + "------ PLUGIN DEINITIALIZE ------");
      // ISSUE: method pointer
      Hooks.remove_OnCommand(new Hooks.CommandHandlerDelegate((object) this, __methodptr(HandleCommand)));
      // ISSUE: method pointer
      Hooks.remove_OnPlayerConnected(new Hooks.ConnectionHandlerDelegate((object) this, __methodptr(PlayerConnected)));
      // ISSUE: method pointer
      Hooks.remove_OnPlayerDisconnected(new Hooks.DisconnectionHandlerDelegate((object) this, __methodptr(PlayerDisconnected)));
      // ISSUE: method pointer
      Hooks.remove_OnPlayerKilled(new Hooks.KillHandlerDelegate((object) this, __methodptr(PlayerKilled)));
      // ISSUE: method pointer
      Hooks.remove_OnPlayerGathering(new Hooks.PlayerGatheringHandlerDelegate((object) this, __methodptr(PlayerGathering)));
      // ISSUE: method pointer
      Hooks.remove_OnNPCKilled(new Hooks.KillHandlerDelegate((object) this, __methodptr(NPCKilled)));
      this.log.WriteLine(this.l + "HOOKS UNLOADED");
      this.log.Close();
    }

    public void HandleCommand(Player pl, string cmd, string[] args)
    {
      switch (cmd)
      {
        case "rank":
          switch (args.Length)
          {
            case 0:
              pl.MessageFrom(this.n, "[COLOR#00EB7E]Rank++ 2.2 [COLOR#FFFFFF]by Snake");
              pl.MessageFrom(this.n, "----------------------------");
              pl.MessageFrom(this.n, "Use '/rank' to see this info");
              pl.MessageFrom(this.n, "Use '/stats' to check your own stats");
              pl.MessageFrom(this.n, "Use '/stats playername' to check a player's stats");
              pl.MessageFrom(this.n, "Use '/top category' to see the top 5 most valuable players");
              pl.MessageFrom(this.n, "Valid Top Categories : kills, deaths, headshots, time, pve, gathering");
              if (!pl.get_Admin())
                return;
              pl.MessageFrom(this.n, "[COLOR#FB9A50]---------- Admin Only Commands ----------");
              pl.MessageFrom(this.n, "Use '/rank resetall' to reset all stats");
              pl.MessageFrom(this.n, "Use '/rank backup' to back up all the stats");
              pl.MessageFrom(this.n, "Use '/rank restore' to back up all the stats");
              return;
            case 1:
              if (!pl.get_Admin())
                return;
              switch (args[0])
              {
                case "resetall":
                  string str1 = this.backupStats();
                  this.deleteStats();
                  pl.MessageFrom(this.n, "[COLOR#00EB7E]Rank++ Stats deleted except Time");
                  pl.MessageFrom(this.n, "[COLOR#00EB7E]Emergency Rank++ Back-up saved as : [COLOR#FFA500]" + str1);
                  break;
                case "backup":
                  string str2 = this.backupStats();
                  pl.MessageFrom(this.n, "[COLOR#00EB7E]Rank++ Back-up saved as : [COLOR#FFA500]" + str2);
                  break;
              }
              return;
            case 2:
              switch (args[0])
              {
                case "restore":
                  if (pl.get_Admin())
                  {
                    string path = this.get_ModuleFolder() + "\\" + args[1];
                    if (!path.EndsWith(".ini"))
                      path += ".ini";
                    if (File.Exists(path))
                    {
                      IniParser iniParser = new IniParser(path);
                      foreach (string section in iniParser.get_Sections())
                      {
                        foreach (string str3 in iniParser.EnumSection(section))
                          this.ds.Add("Rank++" + section, (object) str3, (object) iniParser.GetSetting(section, str3));
                      }
                      pl.MessageFrom(this.n, "[COLOR#00EB7E]Rank++ Stats restored from [COLOR#FFA500]" + path);
                      return;
                    }
                    pl.MessageFrom(this.n, "[COLOR#00EB7E]The file [COLOR#FFA500]" + path + "[COLOR#00EB7E] was not found");
                    return;
                  }
                  pl.MessageFrom(this.n, "[COLOR#00EB7E]Only admins can acces this command");
                  return;
                case null:
                  return;
                default:
                  return;
              }
            default:
              return;
          }
        case "stats":
          switch (args.Length)
          {
            case 0:
              string steamId = pl.get_SteamID();
              double num1 = double.Parse(this.ds.Get("Rank++Kills", (object) steamId).ToString());
              double num2 = double.Parse(this.ds.Get("Rank++Deaths", (object) steamId).ToString());
              double num3 = double.Parse(this.ds.Get("Rank++Headshots", (object) steamId).ToString());
              double ms1 = double.Parse(this.ds.Get("Rank++Time", (object) steamId).ToString());
              double num4 = double.Parse(this.ds.Get("Rank++PVE", (object) steamId).ToString());
              double num5 = double.Parse(this.ds.Get("Rank++Gathering", (object) steamId).ToString());
              double num6 = Math.Round(num3 / num1 * 100.0, 1);
              double num7 = num2 != 0.0 ? Math.Round(num1 / num2, 2) : num1;
              pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]" + pl.get_Name() + " Rank++ Stats [COLOR#FFFFFF]-----");
              pl.MessageFrom(this.n, "[COLOR#FFFFFF]K : [COLOR#00EB7E]" + (object) num1 + "[COLOR#FFFFFF] | D : [COLOR#00EB7E]" + (object) num2 + "[COLOR#FFFFFF] | Headshots : [COLOR#00EB7E]" + (object) num3 + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) num6 + "[COLOR#FFFFFF]%) | K/D Ratio : [COLOR#00EB7E]" + (object) num7);
              pl.MessageFrom(this.n, "[COLOR#FFFFFF]Time Played : [COLOR#00EB7E]" + this.msToTime(ms1) + "[COLOR#FFFFFF] | PVE Kills : [COLOR#00EB7E]" + (object) num4 + "[COLOR#FFFFFF] | Resources Gathered : [COLOR#00EB7E]" + (object) num5);
              return;
            case 1:
              bool flag = false;
              foreach (object key in this.ds.Keys("LastName"))
              {
                string str3 = this.ds.Get("LastName", key).ToString();
                if (str3.Equals(args[0], StringComparison.OrdinalIgnoreCase))
                {
                  flag = true;
                  double num8 = double.Parse(this.ds.Get("Rank++Kills", key).ToString());
                  double num9 = double.Parse(this.ds.Get("Rank++Deaths", key).ToString());
                  double num10 = double.Parse(this.ds.Get("Rank++Headshots", key).ToString());
                  double ms2 = double.Parse(this.ds.Get("Rank++Time", key).ToString());
                  double num11 = double.Parse(this.ds.Get("Rank++PVE", key).ToString());
                  double num12 = double.Parse(this.ds.Get("Rank++Gathering", key).ToString());
                  double num13 = Math.Round(num10 / num8 * 100.0, 1);
                  double num14 = num9 != 0.0 ? Math.Round(num8 / num9, 2) : num8;
                  pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]" + str3 + " Rank++ Stats [COLOR#FFFFFF]-----");
                  pl.MessageFrom(this.n, "[COLOR#FFFFFF]K : [COLOR#00EB7E]" + (object) num8 + "[COLOR#FFFFFF] | D : [COLOR#00EB7E]" + (object) num9 + "[COLOR#FFFFFF] | Headshots : [COLOR#00EB7E]" + (object) num10 + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) num13 + "[COLOR#FFFFFF]%) | K/D Ratio : [COLOR#00EB7E]" + (object) num14);
                  pl.MessageFrom(this.n, "[COLOR#FFFFFF]Time Played : [COLOR#00EB7E]" + this.msToTime(ms2) + "[COLOR#FFFFFF] | PVE Kills : [COLOR#00EB7E]" + (object) num11 + "[COLOR#FFFFFF] | Resources Gathered : [COLOR#00EB7E]" + (object) num12);
                }
              }
              if (flag)
                return;
              pl.MessageFrom(this.n, "[COLOR#FFFFFF]The player [COLOR#00EB7E]" + args[1] + "[COLOR#FFFFFF] was not found");
              return;
            default:
              pl.MessageFrom(this.n, "[COLOR#FFFFFF]The player [COLOR#00EB7E]" + args[1] + "[COLOR#FFFFFF] was not found");
              return;
          }
        case "top":
          if (args.Length == 1)
          {
            switch (args[0])
            {
              case "kills":
                Dictionary<int, RankModule.TopPlayer> dictionary1 = this.calcTop("Kills");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]TOP 5 Most Kills [COLOR#FFFFFF]------");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]1. [COLOR#00EB7E]" + dictionary1[0].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary1[0].count + "[COLOR#FFFFFF] kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]2. [COLOR#00EB7E]" + dictionary1[1].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary1[1].count + "[COLOR#FFFFFF] kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]3. [COLOR#00EB7E]" + dictionary1[2].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary1[2].count + "[COLOR#FFFFFF] kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]4. [COLOR#00EB7E]" + dictionary1[3].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary1[3].count + "[COLOR#FFFFFF] kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]5. [COLOR#00EB7E]" + dictionary1[4].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary1[4].count + "[COLOR#FFFFFF] kills)");
                return;
              case "deaths":
                Dictionary<int, RankModule.TopPlayer> dictionary2 = this.calcTop("Deaths");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]TOP 5 Most Deaths [COLOR#FFFFFF]------");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]1. [COLOR#00EB7E]" + dictionary2[0].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary2[0].count + "[COLOR#FFFFFF] deaths)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]2. [COLOR#00EB7E]" + dictionary2[1].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary2[1].count + "[COLOR#FFFFFF] deaths)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]3. [COLOR#00EB7E]" + dictionary2[2].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary2[2].count + "[COLOR#FFFFFF] deaths)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]4. [COLOR#00EB7E]" + dictionary2[3].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary2[3].count + "[COLOR#FFFFFF] deaths)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]5. [COLOR#00EB7E]" + dictionary2[4].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary2[4].count + "[COLOR#FFFFFF] deaths)");
                return;
              case "headshots":
                Dictionary<int, RankModule.TopPlayer> dictionary3 = this.calcTop("Headshots");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]TOP 5 Most Headshots [COLOR#FFFFFF]------");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]1. [COLOR#00EB7E]" + dictionary3[0].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary3[0].count + "[COLOR#FFFFFF] headshots)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]2. [COLOR#00EB7E]" + dictionary3[1].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary3[1].count + "[COLOR#FFFFFF] headshots)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]3. [COLOR#00EB7E]" + dictionary3[2].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary3[2].count + "[COLOR#FFFFFF] headshots)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]4. [COLOR#00EB7E]" + dictionary3[3].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary3[3].count + "[COLOR#FFFFFF] headshots)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]5. [COLOR#00EB7E]" + dictionary3[4].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary3[4].count + "[COLOR#FFFFFF] headshots)");
                return;
              case "time":
                Dictionary<int, RankModule.TopPlayer> dictionary4 = this.calcTop("Time");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]TOP 5 Most Time Played [COLOR#FFFFFF]------");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]1. [COLOR#00EB7E]" + dictionary4[0].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + this.msToTime((double) dictionary4[0].count) + "[COLOR#FFFFFF])");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]2. [COLOR#00EB7E]" + dictionary4[1].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + this.msToTime((double) dictionary4[1].count) + "[COLOR#FFFFFF])");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]3. [COLOR#00EB7E]" + dictionary4[2].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + this.msToTime((double) dictionary4[2].count) + "[COLOR#FFFFFF])");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]4. [COLOR#00EB7E]" + dictionary4[3].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + this.msToTime((double) dictionary4[3].count) + "[COLOR#FFFFFF])");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]5. [COLOR#00EB7E]" + dictionary4[4].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + this.msToTime((double) dictionary4[4].count) + "[COLOR#FFFFFF])");
                return;
              case "pve":
                Dictionary<int, RankModule.TopPlayer> dictionary5 = this.calcTop("PVE");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]TOP 5 Most PVE Kills [COLOR#FFFFFF]------");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]1. [COLOR#00EB7E]" + dictionary5[0].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary5[0].count + "[COLOR#FFFFFF] PVE Kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]2. [COLOR#00EB7E]" + dictionary5[1].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary5[1].count + "[COLOR#FFFFFF] PVE Kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]3. [COLOR#00EB7E]" + dictionary5[2].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary5[2].count + "[COLOR#FFFFFF] PVE Kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]4. [COLOR#00EB7E]" + dictionary5[3].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary5[3].count + "[COLOR#FFFFFF] PVE Kills)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]5. [COLOR#00EB7E]" + dictionary5[4].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary5[4].count + "[COLOR#FFFFFF] PVE Kills)");
                return;
              case "gathering":
                Dictionary<int, RankModule.TopPlayer> dictionary6 = this.calcTop("Gathering");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]------ [COLOR#00EB7E]TOP 5 Most Resources Gathered [COLOR#FFFFFF]------");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]1. [COLOR#00EB7E]" + dictionary6[0].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary6[0].count + "[COLOR#FFFFFF] resources)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]2. [COLOR#00EB7E]" + dictionary6[1].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary6[1].count + "[COLOR#FFFFFF] resources)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]3. [COLOR#00EB7E]" + dictionary6[2].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary6[2].count + "[COLOR#FFFFFF] resources)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]4. [COLOR#00EB7E]" + dictionary6[3].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary6[3].count + "[COLOR#FFFFFF] resources)");
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]5. [COLOR#00EB7E]" + dictionary6[4].name + "[COLOR#FFFFFF] ([COLOR#00EB7E]" + (object) dictionary6[4].count + "[COLOR#FFFFFF] resources)");
                return;
              default:
                pl.MessageFrom(this.n, "[COLOR#FFFFFF]Valid TOPs : [COLOR#00EB7E]kills , deaths , headshots , time , pve , gathering");
                return;
            }
          }
          else
          {
            pl.MessageFrom(this.n, "[COLOR#FFFFFF]Valid TOPs : [COLOR#00EB7E]kills , deaths , headshots , time , pve , gathering");
            break;
          }
      }
    }

    public void PlayerConnected(Player pl)
    {
      string[] playerProperties = RankModule.TryToGetPlayerProperties(pl);
      if (playerProperties == null)
        return;
      string str1 = playerProperties[0];
      string str2 = playerProperties[1];
      this.ds.Add("LastplayerName", (object) str2, (object) str1);
      object obj1 = this.ds.Get("Rank++Kills", (object) str2);
      object obj2 = this.ds.Get("Rank++Deaths", (object) str2);
      object obj3 = this.ds.Get("Rank++Headshots", (object) str2);
      object obj4 = this.ds.Get("Rank++Time", (object) str2);
      object obj5 = this.ds.Get("Rank++PVE", (object) str2);
      object obj6 = this.ds.Get("Rank++Gathering", (object) str2);
      if (obj1 == null)
        this.ds.Add("Rank++Kills", (object) str2, (object) "0");
      if (obj2 == null)
        this.ds.Add("Rank++Deaths", (object) str2, (object) "0");
      if (obj3 == null)
        this.ds.Add("Rank++Headshots", (object) str2, (object) "0");
      if (obj4 == null)
        this.ds.Add("Rank++Time", (object) str2, (object) "1");
      if (obj5 == null)
        this.ds.Add("Rank++PVE", (object) str2, (object) "0");
      if (obj6 != null)
        return;
      this.ds.Add("Rank++Gathering", (object) str2, (object) "0");
    }

    public void PlayerDisconnected(Player player)
    {
      string[] playerProperties = RankModule.TryToGetPlayerProperties(player);
      if (playerProperties == null)
        return;
      string str1 = playerProperties[0];
      string str2 = playerProperties[1];
      this.ds.Add("LastName", (object) str2, (object) str1);
      double num = (double) player.get_TimeOnline() + double.Parse(this.ds.Get("Rank++Time", (object) str2).ToString());
      this.ds.Add("Rank++Time", (object) str2, (object) num.ToString((IFormatProvider) CultureInfo.InvariantCulture));
    }

    public void PlayerKilled(DeathEvent de)
    {
      if (((HurtEvent) de).get_Victim() == null || ((HurtEvent) de).get_Attacker() == null)
        return;
      Player victim = (Player) ((HurtEvent) de).get_Victim();
      string[] playerProperties1 = RankModule.TryToGetPlayerProperties(victim);
      if (playerProperties1 == null)
        return;
      string str1 = playerProperties1[1];
      double num1 = double.Parse(this.ds.Get("Rank++Deaths", (object) str1).ToString());
      this.ds.Add("Rank++Deaths", (object) str1, (object) (num1 + 1.0));
      if (!(((HurtEvent) de).get_Attacker() is Player))
        return;
      Player attacker = (Player) ((HurtEvent) de).get_Attacker();
      if (attacker != victim)
      {
        string[] playerProperties2 = RankModule.TryToGetPlayerProperties(attacker);
        if (playerProperties2 == null)
          return;
        string str2 = playerProperties2[1];
        double num2 = double.Parse(this.ds.Get("Rank++Kills", (object) str2).ToString());
        this.ds.Add("Rank++Kills", (object) str2, (object) (num2 + 1.0));
        DamageEvent damageEvent = ((HurtEvent) de).get_DamageEvent();
        if (this.isHead(((object) ((DamageEvent) ref damageEvent).get_bodyPart()).ToString()))
        {
          double num3 = double.Parse(this.ds.Get("Rank++Headshots", (object) str2).ToString());
          this.ds.Add("Rank++Headshots", (object) str2, (object) (num3 + 1.0));
        }
      }
    }

    public void PlayerGathering(Player player, GatherEvent ge)
    {
      string[] playerProperties = RankModule.TryToGetPlayerProperties(player);
      if (playerProperties == null)
        return;
      string str = playerProperties[1];
      double num = double.Parse(this.ds.Get("Rank++Gathering", (object) str).ToString());
      this.ds.Add("Rank++Gathering", (object) str, (object) (num + 1.0));
    }

    public void NPCKilled(DeathEvent de)
    {
      if (!(((HurtEvent) de).get_Attacker() is Player) || ((HurtEvent) de).get_Attacker() == null)
        return;
      string[] playerProperties = RankModule.TryToGetPlayerProperties((Player) ((HurtEvent) de).get_Attacker());
      if (playerProperties == null)
        return;
      string str = playerProperties[1];
      double num = double.Parse(this.ds.Get("Rank++PVE", (object) str).ToString());
      this.ds.Add("Rank++PVE", (object) str, (object) (num + 1.0));
    }

    public static string[] TryToGetPlayerProperties(Player player)
    {
      try
      {
        return new string[3]
        {
          player.get_Name(),
          player.get_SteamID(),
          player.get_IP()
        };
      }
      catch
      {
        return (string[]) null;
      }
    }

    private bool isHead(string bodypart)
    {
      switch (bodypart)
      {
        case "Head":
        case "Scalp":
        case "Nostrils":
        case "Jaw":
        case "TongueRear":
        case "TongueFront":
        case "L_Eye":
        case "R_Eye":
        case "L_EyeLidLower":
        case "L_EyeLidUpper":
        case "R_EyeLidLower":
        case "R_EyeLidUpper":
        case "L_BrowInner":
        case "L_BrowOuter":
        case "R_BrowInner":
        case "R_BrowOuter":
        case "L_Cheek":
        case "R_Cheek":
        case "L_LipUpper":
        case "L_LipLower":
        case "R_LipUpper":
        case "R_LipLower":
        case "L_LipCorner":
        case "R_LipCorner":
          return true;
        default:
          return false;
      }
    }

    public string backupStats()
    {
      string str = "backup_" + (object) DateTime.Now.Hour + "h-" + (object) DateTime.Now.Minute + "m_" + (object) DateTime.Now.Day + "-" + (object) DateTime.Now.Month + "-" + (object) DateTime.Now.Year + ".ini";
      File.Create(this.get_ModuleFolder() + "\\" + str).Close();
      IniParser iniParser = new IniParser(this.get_ModuleFolder() + "\\" + str);
      foreach (object key in this.ds.Keys("Rank++Time"))
      {
        iniParser.AddSetting("Kills", key.ToString(), this.ds.Get("Rank++Kills", key).ToString());
        iniParser.AddSetting("Deaths", key.ToString(), this.ds.Get("Rank++Deaths", key).ToString());
        iniParser.AddSetting("Headshots", key.ToString(), this.ds.Get("Rank++Headshots", key).ToString());
        iniParser.AddSetting("Time", key.ToString(), this.ds.Get("Rank++Time", key).ToString());
        iniParser.AddSetting("PVE", key.ToString(), this.ds.Get("Rank++PVE", key).ToString());
        iniParser.AddSetting("Gathering", key.ToString(), this.ds.Get("Rank++Gathering", key).ToString());
      }
      iniParser.Save();
      return str;
    }

    public void deleteStats()
    {
      foreach (object key in this.ds.Keys("Rank++Time"))
      {
        this.ds.Add("Rank++Kills", key, (object) "0");
        this.ds.Add("Rank++Deaths", key, (object) "0");
        this.ds.Add("Rank++Headshots", key, (object) "0");
        this.ds.Add("Rank++PVE", key, (object) "0");
        this.ds.Add("Rank++Gathering", key, (object) "0");
      }
    }

    public Dictionary<int, RankModule.TopPlayer> calcTop(string top)
    {
      Dictionary<int, RankModule.TopPlayer> dictionary = new Dictionary<int, RankModule.TopPlayer>(5);
      dictionary[0] = new RankModule.TopPlayer(" ", 0);
      dictionary[1] = new RankModule.TopPlayer(" ", 0);
      dictionary[2] = new RankModule.TopPlayer(" ", 0);
      dictionary[3] = new RankModule.TopPlayer(" ", 0);
      dictionary[4] = new RankModule.TopPlayer(" ", 0);
      foreach (object key in this.ds.Keys("Rank++" + top))
      {
        int count = int.Parse(this.ds.Get("Rank++" + top, key).ToString());
        if (count > dictionary[0].count)
        {
          dictionary[4] = dictionary[3];
          dictionary[3] = dictionary[2];
          dictionary[2] = dictionary[1];
          dictionary[1] = dictionary[0];
          dictionary[0] = new RankModule.TopPlayer(this.ds.Get("LastName", key).ToString(), count);
        }
        else if (count > dictionary[1].count)
        {
          dictionary[4] = dictionary[3];
          dictionary[3] = dictionary[2];
          dictionary[2] = dictionary[1];
          dictionary[1] = new RankModule.TopPlayer(this.ds.Get("LastName", key).ToString(), count);
        }
        else if (count > dictionary[2].count)
        {
          dictionary[4] = dictionary[3];
          dictionary[3] = dictionary[2];
          dictionary[2] = new RankModule.TopPlayer(this.ds.Get("LastName", key).ToString(), count);
        }
        else if (count > dictionary[3].count)
        {
          dictionary[4] = dictionary[3];
          dictionary[3] = new RankModule.TopPlayer(this.ds.Get("LastName", key).ToString(), count);
        }
        else if (count > dictionary[4].count)
          dictionary[4] = new RankModule.TopPlayer(this.ds.Get("LastName", key).ToString(), count);
      }
      return dictionary;
    }

    public string msToTime(double ms)
    {
      double num1 = Math.Round(ms / 1000.0);
      if (num1 <= 60.0)
        return num1.ToString() + "s";
      double num2 = (num1 - num1 % 60.0) / 60.0;
      double num3 = num1 % 60.0;
      if (num2 > 60.0)
      {
        double num4 = (num2 - num2 % 60.0) / 60.0;
        double num5 = num2 % 60.0;
        if (num4 > 24.0)
          return ((num4 - num4 % 24.0) / 24.0).ToString() + "days " + (object) (num4 % 60.0) + "h " + (object) num5 + "m " + (object) num3 + "s";
        return num4.ToString() + "h " + (object) num5 + "m " + (object) num3 + "s";
      }
      return num2.ToString() + "m " + (object) num3 + "s";
    }

    public RankModule()
    {
      base.\u002Ector();
    }

    public class TopPlayer
    {
      public string name;
      public int count;

      public TopPlayer(string name, int count)
      {
        this.name = name;
        this.count = count;
      }
    }
  }
}
