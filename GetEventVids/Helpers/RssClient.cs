using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using CodeHollow.FeedReader.Feeds.MediaRSS;
using HtmlAgilityPack;

namespace GetEventVids;

public static class RssClient
{
    public static async Task<List<Session>> GetSessionsAsync(Event @event)
    {
        var feed = await FeedReader.ReadAsync(Known.Uris[@event].AbsoluteUri);

        var sessions = new List<Session>();

        foreach (MediaRssFeedItem i in feed.SpecificFeed.Items)
        {
            var doc = new HtmlDocument();

            doc.LoadHtml(i.Description);

            var speakers = (i.Author ?? i.DC.Creator).Split(',')
                .Select(t => new Speaker() { DisplayName = t }).ToList();

            Media? media;

            if (i.Media.Count >= 1)
            {
                media = i.Media.First();
            }
            else if (i.MediaGroups.Count > 0)
            {
                media = i.MediaGroups.First().Media.Where(m => m.Medium ==
                    Medium.Video).OrderByDescending(m => m.FileSize).First();
            }
            else
            {
                continue;
            }

            sessions.Add(new Session()
            {
                Event = @event,
                SessionId = Guid.NewGuid(),
                Code = media.Url.ToMd5Hash(),
                Title = i.Title,
                Synopsis = doc?.DocumentNode?.SelectSingleNode("p")?.InnerText ?? i.Title,
                Speakers = speakers,
                SessionUri = new Uri(i.Link),
                StreamUri = new Uri(i.Link + "/player"),
                VideoUri = new Uri(media.Url),
                Duration = TimeSpan.FromSeconds(media!.Duration!.Value),
                PubDate = i.PublishingDate!.Value
            });
        }

        return sessions;
    }
}
