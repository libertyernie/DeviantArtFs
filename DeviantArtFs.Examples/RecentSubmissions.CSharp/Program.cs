﻿using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviantArtFs.Examples.RecentSubmissions.CSharp
{
    class Token : IDeviantArtAccessToken
    {
        public string AccessToken { get; private set; }

        public Token(string accessToken)
        {
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }
    }

    class Program
    {
        static string GetToken()
        {
            string token_file = "token.txt";
            string token_string = File.Exists(token_file)
                ? File.ReadAllText(token_file)
                : "";

            bool valid = Api.Util.Placebo.IsValidAsync(new Token(token_string)).GetAwaiter().GetResult();
            if (valid)
            {
                return token_string;
            }
            else
            {
                Console.Write("Please enter the client ID (e.g. 1234): ");
                int client_id = int.Parse(Console.ReadLine());

                Console.Write("Please enter the redirect URL (default: https://www.example.com): ");
                string url1 = Console.ReadLine();
                string url2 = url1 == ""
                    ? "https://www.example.com"
                    : url1;

                using (var form = new WinForms.DeviantArtImplicitGrantForm(client_id, new Uri(url2), new[] { "browse", "user", "stash", "publish", "user.manage" }))
                {
                    if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        throw new Exception("Login cancelled");
                    }
                    else
                    {
                        File.WriteAllText(token_file, form.AccessToken);
                        return form.AccessToken;
                    }
                }
            }
        }

        static DeviantArtPagingParams Page(int offset, int limit) => new DeviantArtPagingParams(offset, limit);

        static async Task Sandbox(string token_string)
        {
            var token = new Token(token_string);

            var topics = await Api.Browse.Topics.ToArrayAsync(token, DeviantArtCommonParams.Default, 0, 5, new Api.Browse.TopicsRequest());
            Console.WriteLine("First 5 topics:");
            foreach (var t in topics) {
                Console.WriteLine("* " + t.name);
                var dd = await Api.Browse.Topic.ToArrayAsync(token, DeviantArtCommonParams.Default, 0, 3, t.canonical_name);
                foreach (var d in dd) {
                    Console.WriteLine("  * " + d.title);
                }
            }

            var topTopics = await Api.Browse.TopTopics.ExecuteAsync(token, DeviantArtCommonParams.Default);
            Console.WriteLine("Top topics:");
            foreach (var t in topTopics) {
                Console.WriteLine("* " + t.name);
                foreach (var d in t.ExampleDeviations) {
                    Console.WriteLine("  * Example: " + d.title);
                }
            }

            var watched_art = await Api.Browse.DeviantsYouWatch.ToArrayAsync(token, DeviantArtCommonParams.Default, 0, 50);
            Console.WriteLine("First 50 deviations by deviants you watch:");
            foreach (var d in watched_art) {
                Console.WriteLine($"* {d.title.OrNull()} ({d.published_time.OrNull()})");
            }
            Console.WriteLine();

            var watched_posts = await Api.Browse.PostsByDeviantsYouWatch.ToArrayAsync(token, DeviantArtCommonParams.Default, 0, 50);
            Console.WriteLine("First 50 status or journal posts by deviants you watch:");

            var watched_statuses = watched_posts.SelectMany(x => x.GetStatusObjects());
            Console.WriteLine($"{watched_statuses.Count()} statuses");
            var watched_journals = watched_posts.SelectMany(x => x.GetJournalObjects());
            Console.WriteLine($"{watched_journals.Count()} journals");

            Console.WriteLine();

            Console.Write("Enter a username (leave blank to see your own submissions): ");
            string read = Console.ReadLine();
            Console.WriteLine();

            var me = await Api.User.Whoami.ExecuteAsync(token);

            string username = read == ""
                ? me.username
                : read;

            var profile = await Api.User.ProfileByName.ExecuteAsync(
                token,
                new Api.User.ProfileByNameRequest(username));
            Console.WriteLine(profile.real_name);
            if (!string.IsNullOrEmpty(profile.tagline))
            {
                Console.WriteLine(profile.tagline);
            }
            Console.WriteLine($"{profile.stats.user_deviations} deviations");
            Console.WriteLine();

            var deviations = await Api.Gallery.GalleryAllView.ExecuteAsync(
                token,
                Page(0, 1),
                new Api.Gallery.GalleryAllViewRequest { Username = username });
            var deviation = deviations.results.Where(x => !x.is_deleted).FirstOrDefault();
            if (deviation != null)
            {
                Console.WriteLine($"Most recent (non-deleted) deviation: {deviation.title.OrNull()} ({deviation.published_time.OrNull()})");
                if (deviation.is_downloadable.IsTrue()) {
                    Console.WriteLine($"Downloadable (size = {deviation.download_filesize ?? -1}");
                } else {
                    Console.WriteLine("Not downloadable");
                }

                var metadata = await Api.Deviation.MetadataById.ExecuteAsync(
                    token,
                    new Api.Deviation.MetadataRequest(new[] { deviation.deviationid }));
                foreach (var m in metadata)
                {
                    Console.WriteLine(string.Join(" ", m.tags.Select(t => $"#{t.tag_name}")));
                }

                var favorites = await Api.Deviation.WhoFaved.ToArrayAsync(
                    token,
                    0,
                    int.MaxValue,
                    deviation.deviationid);
                if (favorites.Any())
                {
                    Console.WriteLine("Favorited by:");
                    foreach (var f in favorites)
                    {
                        Console.WriteLine($"    {f.user.username} {f.time}");
                    }
                }

                var comments_req = new Api.Comments.DeviationCommentsRequest(deviation.deviationid) { Maxdepth = 5 };
                var comments = await Api.Comments.DeviationComments.ToArrayAsync(
                    token,
                    DeviantArtCommonParams.Default,
                    0,
                    int.MaxValue,
                    comments_req);
                if (comments.Any())
                {
                    Console.WriteLine("Comments by:");
                    foreach (var c in comments)
                    {
                        Console.WriteLine($"    {c.user.username} {c.body}");
                    }
                }

                Console.WriteLine();
            }

            var journals = await Api.Browse.UserJournals.ExecuteAsync(
                token,
                DeviantArtCommonParams.Default,
                Page(0, 1),
                new Api.Browse.UserJournalsRequest(username) { Featured = false });
            var journal = journals.results.Where(x => !x.is_deleted).FirstOrDefault();
            if (journal != null)
            {
                Console.WriteLine($"Most recent (non-deleted) journal: {journal.title.OrNull()} ({journal.published_time.OrNull()})");

                var metadata = await Api.Deviation.MetadataById.ExecuteAsync(
                    token,
                    new Api.Deviation.MetadataRequest(new[] { journal.deviationid }));
                foreach (var m in metadata)
                {
                    Console.WriteLine(string.Join(" ", m.tags.Select(t => $"#{t.tag_name}")));
                }

                var favorites = await Api.Deviation.WhoFaved.ToArrayAsync(
                    token,
                    0,
                    int.MaxValue,
                    journal.deviationid);
                if (favorites.Any())
                {
                    Console.WriteLine("Favorited by:");
                    foreach (var f in favorites)
                    {
                        Console.WriteLine($"    {f.user.username} {f.time}");
                    }
                }

                var comments_req = new Api.Comments.DeviationCommentsRequest(journal.deviationid) { Maxdepth = 5 };
                var comments = await Api.Comments.DeviationComments.ToArrayAsync(
                    token,
                    DeviantArtCommonParams.Default,
                    0,
                    int.MaxValue,
                    comments_req);
                if (comments.Any())
                {
                    Console.WriteLine("Comments by:");
                    foreach (var c in comments)
                    {
                        Console.WriteLine($"    {c.user.username} {c.body}");
                    }
                }

                Console.WriteLine();
            }

            var statuses = await Api.User.StatusesList.ExecuteAsync(
                token,
                Page(0, 1),
                username);
            var status = statuses.results.Where(x => !x.is_deleted).FirstOrDefault();
            if (status != null)
            {
                Console.WriteLine($"Most recent (non-deleted) status: {status.body.OrNull()} ({status.ts.OrNull()})");

                var comments_req = new Api.Comments.StatusCommentsRequest(status.statusid.Value) { Maxdepth = 5 };
                var comments = await Api.Comments.StatusComments.ToArrayAsync(
                    token,
                    DeviantArtCommonParams.Default,
                    0,
                    int.MaxValue,
                    comments_req);
                if (comments.Any())
                {
                    Console.WriteLine("Comments:");
                }
                foreach (var c in comments)
                {
                    Console.WriteLine($"    {c.user.username}: {c.body}");
                }

                Console.WriteLine();
            }
        }

        [STAThread]
        static async Task<int> Main(string[] args)
        {
            string token_string = GetToken();
            await Sandbox(token_string);
            return 0;
        }
    }
}
