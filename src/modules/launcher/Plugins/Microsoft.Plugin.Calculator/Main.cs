﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Mages.Core;
using Wox.Infrastructure.Logger;
using Wox.Plugin;

namespace Microsoft.Plugin.Calculator
{
    public class Main : IPlugin, IPluginI18n, IDisposable
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
        private static readonly Engine MagesEngine = new Engine();
        private PluginInitContext Context { get; set; }
        private string IconPath { get; set; }
        private bool _disposed = false;

        public List<Result> Query(Query query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(paramName: nameof(query));
            }

            if (query.Search.Length <= 2          // don't affect when user only input "e" or "i" keyword
                || !RegValidExpressChar.IsMatch(query.Search)
                || !IsBracketComplete(query.Search)) return new List<Result>();

            try
            {
                var result = MagesEngine.Interpret(query.Search);

                // This could happen for some incorrect queries, like pi(2) 
                if (result == null)
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
            //We want to keep the process alive if any the mages library throws any exceptions.
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Exception($"|Microsoft.Plugin.Calculator.Main.Query|Exception when query for <{query}>", e);
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
            if (context == null)
            {
                throw new ArgumentNullException(paramName: nameof(context));
            }

            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        // Todo : Update with theme based IconPath
        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "Images/calculator.light.png";
            }
            else
            {
                IconPath = "Images/calculator.dark.png";
            }
        }

        private void OnThemeChanged(Theme _, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("wox_plugin_calculator_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("wox_plugin_calculator_plugin_description");
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Context.API.ThemeChanged -= OnThemeChanged;
                    _disposed = true;
                }
            }
        }
    }
}