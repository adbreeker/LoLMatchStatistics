using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoLScraper
{
    internal class MatchStatistics
    {
        Match matchInfo;
        string summonerName;
        Participant player;
        BasePlayerStatistics basePlayerStatistics;

        public MatchStatistics(Match matchInfo, string summonerName) 
        {
            this.matchInfo = matchInfo;
            this.summonerName = summonerName;
            this.player = GetMyParticipantInfo();
            this.basePlayerStatistics = new BasePlayerStatistics(player, this);
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
            return matchInfo.Info.Participants[0];
        }

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

        public void WriteHistoryEntry()
        {
            Console.WriteLine(player.ChampionName + " ("+ player.TeamPosition + ") " + basePlayerStatistics.kills + "/" + basePlayerStatistics.deaths + "/" + basePlayerStatistics.assists);
        }

        public void WriteBaseStatistic()
        {
            Console.WriteLine("Game on: " + player.ChampionName + " (" + player.TeamPosition + ")\n-----------------------------------\n");

            Console.WriteLine("Kills: " + basePlayerStatistics.kills);
            Console.WriteLine("Deaths: " + basePlayerStatistics.deaths);
            Console.WriteLine("Assists: " + basePlayerStatistics.assists);
            Console.WriteLine("KDA: " + basePlayerStatistics.kda);
            Console.WriteLine("DMG/M: " + basePlayerStatistics.dmgM);
            Console.WriteLine("Vision score: " + basePlayerStatistics.vs);
            Console.WriteLine("Kill participation: " + basePlayerStatistics.kp + "%");
            if(player.TeamPosition == "UTILITY")
            {
                Console.WriteLine("CC: " + basePlayerStatistics.cc);
            }
            else
            {
                Console.WriteLine("CS/M: " + basePlayerStatistics.csM);
            }
            

            Console.WriteLine("\n\n\n");
        }

        public void CopyBaseStats()
        {
            string baseStatsClipboard = "";
            baseStatsClipboard += basePlayerStatistics.kills + "\t";
            baseStatsClipboard += basePlayerStatistics.deaths + "\t";
            baseStatsClipboard += basePlayerStatistics.assists + "\t";
            baseStatsClipboard += basePlayerStatistics.kda + "\t";
            baseStatsClipboard += basePlayerStatistics.dmgM + "\t";
            baseStatsClipboard += basePlayerStatistics.vs + "\t";
            baseStatsClipboard += basePlayerStatistics.kp + "%\t";
            if (player.TeamPosition == "UTILITY")
            {
                baseStatsClipboard += basePlayerStatistics.cc;
            }
            else
            {
                baseStatsClipboard += basePlayerStatistics.csM;
            }
            Thread thread = new Thread(() => Clipboard.SetText(baseStatsClipboard));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }

    class BasePlayerStatistics
    {
        public int kills;
        public int deaths;
        public int assists;
        public float kda;
        public int dmgM;
        public int vs;
        public int kp;
        public float csM;
        public int cc;
        public BasePlayerStatistics(Participant player, MatchStatistics ms) 
        {
            this.kills = player.Kills;
            this.deaths = player.Deaths;
            this.assists = player.Assists;
            this.kda = ms.CalculateKDA();
            this.dmgM = (int)(player.TotalDamageDealtToChampions / ms.CalculateGameMinutes());
            this.vs = player.VisionScore;
            this.kp = ms.CalculateKillParticipation();
            this.csM = ms.CalculateCSM();
            this.cc = player.TimeCCingOthers;
        }
    }
}
