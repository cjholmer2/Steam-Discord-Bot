﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using SteamStoreQuery;

namespace SteamDiscordBot.Commands
{
    public class AppInfoCommand : ModuleBase
    {
        [Command("appinfo"), Summary("Fetches up application info from steam.")]
        public async Task Say(params string[] args)
        {
            string input = "";
            for (int i = 0; i < args.Length - 1; i++)
            {

                input += args[i].Trim() + " ";
            }
            input += args[args.Length - 1];
            int appid = InputToAppId(input);

            if (appid == -1)
            {
                var emb = new EmbedBuilder();
                emb.Title = "Error!";
                emb.WithDescription("No games found! :(");
                emb.Color = Color.Red;

                await Context.Channel.SendMessageAsync("", false, emb);
                return;
            }

            try
            {
                string json = new WebClient().DownloadString("http://store.steampowered.com/api/appdetails?appids=" + "" + appid);
                JObject obj = JObject.Parse(json);

                string name = (string)obj["" + appid]["data"]["name"];
                int age = (int)obj["" + appid]["data"]["required_age"];
                string website = (string)obj["" + appid]["data"]["website"];
                double price = (int)obj["" + appid]["data"]["price_overview"]["final"] / 100.0;

                var emb = new EmbedBuilder();
                emb.Title = "AppInfo Fetched";
                emb.AddField("Name", name);
                emb.AddField("Age Requirement", "" + age);
                emb.AddField("Price", string.Format("${0} (USD)", price));
                emb.AddField("Link", website);
                emb.Color = Color.Red;

                await Context.Channel.SendMessageAsync("", false, emb);
            }
            catch (Exception ex)
            {
                await Program.Instance.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "AppInfo", ex.Message));

                await Context.Channel.SendMessageAsync("Internal Error: " + ex.Message);
            }
        }

        public static int InputToAppId(string input)
        {
            if (int.TryParse(input, out int result))
            {
                return result;
            }

            /* Steam Store Query for AppID */
            List<Listing> results = Query.Search(input);

            if (results.Count == 0)
                return -1;

            return results.First().AppId;
        }
    }
}
