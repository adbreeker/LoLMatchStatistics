using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace LoLMatchStatistics
{
    internal class MatchStatistics
    {
        RiotApi_Match matchInfo;
        string summonerName;
        Participant player;
        PlayerStatistics playerStatistics;

        public bool playerFoundInGame = false; //true if player identified by summoner name was in processed match

        public MatchStatistics(RiotApi_Match matchInfo, string summonerName) 
        {
            this.matchInfo = matchInfo;
            this.summonerName = summonerName;
            this.player = GetMyParticipantInfo();
            if(player != null ) 
            {
                this.playerStatistics = new PlayerStatistics(player, this);
                playerFoundInGame = true;
            }
        }

        public Participant GetMyParticipantInfo()
        {
            
            foreach (Participant participant in matchInfo.Info.Participants)
            {
                if (participant.SummonerName == summonerName)
                {
                    return participant;
                }
            }
            return null;
        }

        //calculate some game statistics ---------------------------------------------------------------------------------------------------- calculate some game statistics

        public float CalculateGameMinutes()
        {
            return matchInfo.Info.GameDuration / 60.0f;
        }

        public float CalculateKDA()
        {
            float kda;
            if(player.Deaths != 0)
            {
                kda = (player.Kills + player.Assists) / (float)player.Deaths;
            }
            else
            {
                kda = (player.Kills + player.Assists) / 1.0f;
            }
            
            return (float)Math.Round(kda, 2);
        }

        public int CalculateKillParticipation()
        {

            Team myTeam;
            if (matchInfo.Info.Teams[0].Win == player.Win)
            {
                myTeam = matchInfo.Info.Teams[0];
            }
            else
            {
                myTeam = matchInfo.Info.Teams[1];
            }

            float kp = (player.Kills + player.Assists) / (float)myTeam.Objectives.Champion.Kills;
            kp *= 100;
            return (int)Math.Round(kp, 0);
        }

        public float CalculateCSM()
        {
            float csm = (player.TotalMinionsKilled + player.NeutralMinionsKilled) / CalculateGameMinutes();
            return (float)Math.Round(csm, 1);
        }

        public float CalculateVSM()
        {
            float vsm = player.VisionScore / CalculateGameMinutes();
            return (float)Math.Round(vsm, 2);
        }

        float CalculateScore()
        {
            float score = 0;
            if (player.TeamPosition != "UTILITY")
            {
                score += 1.0f * playerStatistics.kills;
                score += -2.0f * playerStatistics.deaths;
                score += 1.0f * playerStatistics.assists;
            }
            else
            {
                score += 0.75f * playerStatistics.kills;
                score += -2.0f * playerStatistics.deaths;
                score += 1.25f * playerStatistics.assists;
            }

            if (score < 0)
            {
                score = 0;
            }

            score += playerStatistics.dmgM / 100.0f;
            score += playerStatistics.dmg_tM / 100.0f;
            score += playerStatistics.dmg_oM / 200.0f;

            score += playerStatistics.vsM * 3.5f;
            score += playerStatistics.kp / 10.0f;
            score += playerStatistics.csM;
            score += playerStatistics.cc / 10.0f;

            return (float)Math.Round(score, 2);
        }

        //print some data -------------------------------------------------------------------------------------------------------------------------------- print some data

        public void WriteHistoryEntry()
        {
            if(!playerFoundInGame)
            {
                return;
            }

            Console.WriteLine(player.ChampionName + " ("+ player.TeamPosition + ") " + playerStatistics.kills + "/" + playerStatistics.deaths + "/" + playerStatistics.assists);
        }

        public void WriteBaseStatistic()
        {
            if (!playerFoundInGame)
            {
                return;
            }

            Console.WriteLine($"{summonerName}: {player.ChampionName} ({player.TeamPosition})\n-----------------------------------");
            Console.WriteLine("Score: " + CalculateScore() + "\n");

            Console.WriteLine("Kills: " + playerStatistics.kills);
            Console.WriteLine("Deaths: " + playerStatistics.deaths);
            Console.WriteLine("Assists: " + playerStatistics.assists);
            Console.WriteLine("KDA: " + playerStatistics.kda);
            Console.WriteLine("DMG/M: " + playerStatistics.dmgM);
            Console.WriteLine("DMG-T/M: " + playerStatistics.dmg_tM);
            Console.WriteLine("DMG-O/M: " + playerStatistics.dmg_oM);
            Console.WriteLine("VS/M: " + playerStatistics.vsM);
            Console.WriteLine("Kill participation: " + playerStatistics.kp + "%");
            Console.WriteLine("CS/M: " + playerStatistics.csM);
            Console.WriteLine("CC: " + playerStatistics.cc);

            Console.WriteLine("\n\n");
        }

        public void CopyBaseStats() //copying stats to clipboard
        {
            if (!playerFoundInGame)
            {
                return;
            }

            string baseStatsClipboard = "";
            baseStatsClipboard += playerStatistics.kills + "\t";
            baseStatsClipboard += playerStatistics.deaths + "\t";
            baseStatsClipboard += playerStatistics.assists + "\t";
            baseStatsClipboard += playerStatistics.kda + "\t";
            baseStatsClipboard += playerStatistics.dmgM + "\t";
            baseStatsClipboard += playerStatistics.dmg_tM + "\t";
            baseStatsClipboard += playerStatistics.dmg_oM + "\t";
            baseStatsClipboard += playerStatistics.vsM + "\t";
            baseStatsClipboard += playerStatistics.kp + "%\t";
            baseStatsClipboard += playerStatistics.csM + "\t";
            baseStatsClipboard += playerStatistics.cc;
            Thread thread = new Thread(() => Clipboard.SetText(baseStatsClipboard));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        

        public List<Object> GetBaseStatisticsAsList() //get statistics as list ready to use in filling google spreadsheet
        {
            if (!playerFoundInGame)
            {
                return null;
            }

            List<Object> statistics = new List<Object>
            {
                CalculateScore(),
                playerStatistics.kills,
                playerStatistics.deaths,
                playerStatistics.assists,
                playerStatistics.kda,
                playerStatistics.dmgM,
                playerStatistics.dmg_tM,
                playerStatistics.dmg_oM,
                playerStatistics.vsM,
                (playerStatistics.kp.ToString() + "%"),
                playerStatistics.csM,
                playerStatistics.cc
            };
            return statistics;
        }
    }

    class PlayerStatistics
    {
        public int kills;
        public int deaths;
        public int assists;
        public float kda;
        public int dmgM;
        public int dmg_tM;
        public int dmg_oM;
        public float vsM;
        public int kp;
        public float csM;
        public int cc;
        public PlayerStatistics(Participant player, MatchStatistics ms) 
        {
            this.kills = player.Kills;
            this.deaths = player.Deaths;
            this.assists = player.Assists;
            this.kda = ms.CalculateKDA();
            this.dmgM = (int)(player.TotalDamageDealtToChampions / ms.CalculateGameMinutes());
            this.dmg_tM = (int)(player.DamageDealtToBuildings / ms.CalculateGameMinutes());
            this.dmg_oM = (int)(player.DamageDealtToObjectives / ms.CalculateGameMinutes());
            this.vsM = ms.CalculateVSM();
            this.kp = ms.CalculateKillParticipation();
            this.csM = ms.CalculateCSM();
            this.cc = player.TimeCCingOthers;
        }
    }
}
