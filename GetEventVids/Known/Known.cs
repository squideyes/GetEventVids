using System.Collections.Immutable;

namespace GetEventVids
{
    internal static class Known
    {
        static Known()
        {
            var dict = new Dictionary<Event, Uri>
            {
                { 
                    Event.DotNetConf2021, new Uri("https://s.ch9.ms/Events/dotnetConf/2021/rss") 
                },
                {
                    Event.VisualStudio2022, new Uri(
                        "https://s.ch9.ms/Events/Visual-Studio/Visual-Studio-2022-Launch-Event/RSS/mp4high")
                }
            };

            Uris = dict.ToImmutableDictionary();
        }

        public static ImmutableDictionary<Event, Uri> Uris { get; }
    }
}
