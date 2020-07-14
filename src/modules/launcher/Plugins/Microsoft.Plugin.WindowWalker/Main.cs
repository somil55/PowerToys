﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Plugin.WindowWalker.Components;
using Wox.Plugin;

namespace Microsoft.Plugin.WindowWalker
{
    public class Main : IPlugin, IPluginI18n
    {
        private static List<SearchResult> _results = new List<SearchResult>();
        private string IconPath { get; set; }
        private PluginInitContext Context { get; set; }

        static Main()
        {
            SearchController.Instance.OnSearchResultUpdate += SearchResultUpdated;
            OpenWindows.Instance.UpdateOpenWindowsList();
        }

        public List<Result> Query(Query query)
        {
            SearchController.Instance.UpdateSearchText(query.RawQuery).Wait();
            OpenWindows.Instance.UpdateOpenWindowsList();
            return _results.Select(x => new Result()
            {
                Title = x.Result.Title,
                IcoPath = IconPath,
                SubTitle = "Running: " + x.Result.ProcessName,
                Action = c =>
                {
                    x.Result.SwitchToWindow();
                    return true;
                }
            }
            ).ToList();
        }

        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        // Todo : Update with theme based IconPath
        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "Images/windowwalker.light.png";
            }
            else
            {
                IconPath = "Images/windowwalker.dark.png";
            }
        }

        private void OnThemeChanged(Theme _, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("wox_plugin_windowwalker_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("wox_plugin_windowwalker_plugin_description");
        }

        private static void SearchResultUpdated(object sender, SearchController.SearchResultUpdateEventArgs e)
        {
            _results = SearchController.Instance.SearchMatches;
        }
    }
}
