﻿using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;

namespace SteamDiscordBot.Jobs
{
	public class AlliedModdersThreadJob : Job
	{
        List<ThreadInfo> history;
		string url;
		string target;

		public AlliedModdersThreadJob(string url, string target)
		{
            this.history = new List<ThreadInfo>();
			this.target = target;
			this.url = url;
		}

		public async override void OnRun()
		{
            try
            {
                string xml = new WebClient().DownloadString(this.url);
                XDocument doc = XDocument.Parse(xml);
                XNamespace ns = "http://www.w3.org/2005/Atom/";

                var mainfeed = doc.Descendants().ElementAt(0).Descendants().Elements();

                var feed = mainfeed.Where(x => x.Name.ToString().Contains("item")).First().Elements();

                ThreadInfo current = new ThreadInfo(feed);

                if (history.Count == 0) // first fetch
                {
                    history.Add(current);
                }
                else
                {
                    if (!history.Exists(x => x.Equals(current)))
                    {
                        var emb = new EmbedBuilder();
                        emb.Title = "New SourceMod plugin";
                        emb.AddField(current.title, "URL: " + current.link);
                        emb.Color = Color.DarkBlue;

                        await Task.Run(() => MessageTools.SendMessageAllToTarget(target, "", emb));
                        history.Add(current);
                    }
                }
            }
            catch (Exception ex)
            {
                await Program.Instance.Log(new LogMessage(LogSeverity.Error, "AMThreadJob", ex.Message));
            }
		}
	}

    class ThreadInfo
    {
        public string title;
        public string link;
        public string description;

        public ThreadInfo(IEnumerable<XElement> feed)
        {
            this.title = feed.Where(x => x.Name.ToString().Contains("title")).First().Value.Trim();
            this.link = feed.Where(x => x.Name.ToString().Contains("link")).First().Value.Trim();
            this.description = feed.Where(x => x.Name.ToString().Contains("description")).First().Value.Trim();
        }

        public override string ToString()
        {
            return string.Format("{0} by {1}", this.title, this.description);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ThreadInfo compObj = obj as ThreadInfo;
            if (compObj == null) return false;
            else return Equals(compObj);
        }

        public bool Equals(ThreadInfo other)
        {
            return other.link.Equals(this.link);
        }

        public override int GetHashCode()
        {
            return link.GetHashCode();
        }
    }
}
