using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace BilliardsApp
{
    public class ScoreEntry
    {
        public string Name;
        public int Score;
        public string Time; // ISO string
        public ScoreEntry(string name, int score)
        {
            Name = name;
            Score = score;
            Time = DateTime.UtcNow.ToString("o");
        }
    }

    public partial class api : Page
    {
        private const string APP_KEY = "BILLIARDS_SCORES";

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            string action = (Request["action"] ?? "list").ToLower();

            EnsureListExists();

            switch (action)
            {
                case "save":
                    SaveScore();
                    break;
                case "list":
                    ListScores();
                    break;
                case "clear":
                    ClearScores();
                    break;
                case "ping":
                default:
                    WriteJson("{\"ok\":true}");
                    break;
            }
        }

        private void EnsureListExists()
        {
            if (Application[APP_KEY] == null)
            {
                Application[APP_KEY] = new List<ScoreEntry>();
            }
        }

        private List<ScoreEntry> GetScores()
        {
            return Application[APP_KEY] as List<ScoreEntry>;
        }

        private void SaveScore()
        {
            string name = Request["name"] ?? "Anonymous";
            int score = 0;
            int.TryParse(Request["score"] ?? "0", out score);

            var entry = new ScoreEntry(name, score);

            Application.Lock();
            try
            {
                var list = GetScores();
                list.Add(entry);
                // keep only latest 100 entries
                if (list.Count > 100) list.RemoveRange(0, list.Count - 100);
            }
            finally
            {
                Application.UnLock();
            }

            WriteJson("{\"ok\":true}");
        }

        private void ListScores()
        {
            int limit = 50;
            int.TryParse(Request["n"] ?? "50", out limit);
            if (limit <= 0) limit = 50;

            var list = GetScores();
            // return newest first
            int count = list.Count;
            int start = Math.Max(0, count - limit);
            var slice = list.GetRange(start, count - start);

            // build json
            var sb = new StringBuilder();
            sb.Append("{\"ok\":true,\"scores\":[");
            for (int i = slice.Count - 1; i >= 0; i--)
            {
                var e = slice[i];
                if (sb[sb.Length - 1] != '[') sb.Append(',');
                sb.Append("{");
                sb.Append("\"name\":\"").Append(EscapeJson(e.Name)).Append("\",");
                sb.Append("\"score\":").Append(e.Score).Append(",");
                sb.Append("\"time\":\"").Append(EscapeJson(e.Time)).Append("\"");
                sb.Append("}");
            }
            sb.Append("]}");
            WriteJson(sb.ToString());
        }

        private void ClearScores()
        {
            Application.Lock();
            try
            {
                var list = GetScores();
                list.Clear();
            }
            finally
            {
                Application.UnLock();
            }
            WriteJson("{\"ok\":true}");
        }

        private void WriteJson(string json)
        {
            Response.Clear();
            Response.Write(json);
            Response.End();
        }

        private string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32 || c > 126)
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}