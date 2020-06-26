﻿using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Mages.Core;
using Wox.Plugin;

namespace Microsoft.Plugin.Calculator
{
    public class Main : IPlugin, IPluginI18n
    {
        private static readonly Regex RegValidExpressChar = new Regex(
                        @"^(" +
                        @"ceil|floor|exp|pi|e|max|min|det|abs|log|ln|sqrt|" +
                        @"sin|cos|tan|arcsin|arccos|arctan|" +
                        @"eigval|eigvec|eig|sum|polar|plot|round|sort|real|zeta|" +
                        @"bin2dec|hex2dec|oct2dec|" +
                        @"==|~=|&&|\|\||" +
                        @"[ei]|[0-9]|[\+\-\*\/\^\., ""]|[\(\)\|\!\[\]]" +
                        @")+$", RegexOptions.Compiled);
        private static readonly Regex RegBrackets = new Regex(@"[\(\)\[\]]", RegexOptions.Compiled);
        private static readonly Engine MagesEngine;
        private PluginInitContext Context { get; set; }
        private string IconPath { get; set; }

        static Main()
        {
            MagesEngine = new Engine();
        }

        public List<Result> Query(Query query)
        {
            if (query.Search.Length <= 2          // don't affect when user only input "e" or "i" keyword
                || !RegValidExpressChar.IsMatch(query.Search)
                || !IsBracketComplete(query.Search)) return new List<Result>();

            try
            {
                var result = MagesEngine.Interpret(query.Search);

                // This could happen for some incorrect queries, like pi(2) 
                if(result == null)
                {
                    return new List<Result>();
                }

                if (result.ToString() == "NaN")
                    result = Context.API.GetTranslation("wox_plugin_calculator_not_a_number");

                if (result is Function)
                    result = Context.API.GetTranslation("wox_plugin_calculator_expression_not_complete");


                if (!string.IsNullOrEmpty(result?.ToString()))
                {
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = result.ToString(),
                            IcoPath = IconPath,
                            Score = 300,
                            SubTitle = Context.API.GetTranslation("wox_plugin_calculator_copy_number_to_clipboard"),
                            Action = c =>
                            {
                                var ret = false;
                                var thread = new Thread(() =>
                                {
                                    try
                                    {
                                        Clipboard.SetText(result.ToString());
                                        ret = true;
                                    }
                                    catch (ExternalException)
                                    {
                                        MessageBox.Show("Copy failed, please try later");
                                    }
                                });
                                thread.SetApartmentState(ApartmentState.STA);
                                thread.Start();
                                thread.Join();
                                return ret;
                            }
                        } 
                    };
                }
            }
            catch
            {
                // ignored
            }

            return new List<Result>();
        }

        private bool IsBracketComplete(string query)
        {
            var matchs = RegBrackets.Matches(query);
            var leftBracketCount = 0;
            foreach (Match match in matchs)
            {
                if (match.Value == "(" || match.Value == "[")
                {
                    leftBracketCount++;
                }
                else
                {
                    leftBracketCount--;
                }
            }

            return leftBracketCount == 0;
        }

        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            ResetCalculatorIconPath(Context.API.GetCurrentTheme());
        }

        private void ResetCalculatorIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "Images/calculator_light.png";
            }
            else
            {
                IconPath = "Images/calculator_dark.png";
            }
        }

        private void OnThemeChanged(Theme _, Theme newTheme)
        {
            ResetCalculatorIconPath(newTheme);
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("wox_plugin_calculator_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("wox_plugin_calculator_plugin_description");
        }
    }
}
