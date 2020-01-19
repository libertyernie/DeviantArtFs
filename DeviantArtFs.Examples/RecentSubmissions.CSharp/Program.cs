﻿using System;
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

            bool valid = Requests.Util.Placebo.IsValidAsync(new Token(token_string)).GetAwaiter().GetResult();
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

                using (var form = new WinForms.DeviantArtImplicitGrantForm(client_id, new Uri(url2), new[] { "browse", "user", "stash", "publish", "user.manage", "message" }))
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

        static IDeviantArtPagingParams Page(int offset, int limit) => new DeviantArtPagingParams { Offset = offset, Limit = limit };

        static async Task Sandbox(string token_string)
        {
            var token = new Token(token_string);

            Console.Write("Enter a username (leave blank to see your own submissions): ");
            string read = Console.ReadLine();
            Console.WriteLine();

            var me = await Requests.User.Whoami.ExecuteAsync(token);

            string username = read == ""
                ? me.username
                : read;

            var profile = await Requests.User.ProfileByName.ExecuteAsync(
                token,
                new Requests.User.ProfileByNameRequest(username));
            Console.WriteLine(profile.real_name);
            if (!string.IsNullOrEmpty(profile.tagline))
            {
                Console.WriteLine(profile.tagline);
            }
            Console.WriteLine($"{profile.stats.user_deviations} deviations");
            Console.WriteLine();

            var deviations = await Requests.Gallery.GalleryAllView.ExecuteAsync(
                token,
                Page(0, 1),
                new Requests.Gallery.GalleryAllViewRequest { Username = username });
            var deviation = deviations.results.FirstOrDefault()?.ToExistingDeviations()?.FirstOrDefault();
            if (deviation != null)
            {
                Console.WriteLine($"Most recent deviation: {deviation.title} ({deviation.published_time})");

                var metadata = await Requests.Deviation.MetadataById.ExecuteAsync(
                    token,
                    new Requests.Deviation.MetadataRequest(new[] { deviation.deviationid }));
                foreach (var m in metadata)
                {
                    Console.WriteLine(string.Join(" ", m.tags.Select(t => $"#{t.tag_name}")));
                }

                var favorites = await Requests.Deviation.WhoFaved.ToArrayAsync(
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

                var comments_req = new Requests.Comments.DeviationCommentsRequest(deviation.deviationid) { Maxdepth = 5 };
                var comments = await Requests.Comments.DeviationComments.ToArrayAsync(
                    token,
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

            var journals = await Requests.Browse.UserJournals.ExecuteAsync(
                token,
                Page(0, 1),
                new Requests.Browse.UserJournalsRequest(username) { Featured = false });
            var journal = journals.results.FirstOrDefault()?.ToExistingDeviations()?.FirstOrDefault();
            if (journal != null)
            {
                Console.WriteLine($"Most recent journal: {journal.title} ({journal.published_time})");

                var metadata = await Requests.Deviation.MetadataById.ExecuteAsync(
                    token,
                    new Requests.Deviation.MetadataRequest(new[] { journal.deviationid }));
                foreach (var m in metadata)
                {
                    Console.WriteLine(string.Join(" ", m.tags.Select(t => $"#{t.tag_name}")));
                }

                var favorites = await Requests.Deviation.WhoFaved.ToArrayAsync(
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

                var comments_req = new Requests.Comments.DeviationCommentsRequest(journal.deviationid) { Maxdepth = 5 };
                var comments = await Requests.Comments.DeviationComments.ToArrayAsync(
                    token,
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

            var statuses = await Requests.User.StatusesList.ExecuteAsync(
                token,
                Page(0, 1),
                username);
            var status = statuses.results.FirstOrDefault()?.ToExistingStatuses()?.FirstOrDefault();
            if (status != null)
            {
                Console.WriteLine($"Most recent status: {status.body} ({status.ts})");

                var comments_req = new Requests.Comments.StatusCommentsRequest(status.statusid) { Maxdepth = 5 };
                var comments = await Requests.Comments.StatusComments.ToArrayAsync(
                    token,
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

            var messages = await Requests.Messages.MessagesFeed.ToArrayAsync(
                token,
                new Requests.Messages.MessagesFeedRequest(),
                null,
                5);
            foreach (var m in messages) {
                string originator = m.GetOriginators()
                    .Select(u => u.username)
                    .DefaultIfEmpty("???")
                    .Single();
                object subject = m.GetSubjects()
                    .DefaultIfEmpty(null)
                    .Single();
                if (subject == null) {
                    Console.WriteLine($"New message, originator {originator}, no subject");
                } else if (subject is DeviantArtUser u) {
                    Console.WriteLine($"New message, originator {originator}, subject is user with ID {u.userid} and name {u.username}");
                } else {
                    Console.WriteLine($"New message, originator {originator}, subject = {subject}");
                }
            }
        }

        [STAThread]
        static int Main(string[] args)
        {
            string token_string = GetToken();
            Sandbox(token_string).GetAwaiter().GetResult();
            return 0;
        }
    }
}
