﻿using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace DLSS_Swapper
{
    public static class Settings
    {
        static bool _hasShownWarning = false;
        public static bool HasShownWarning
        {
            get { return _hasShownWarning; }
            set
            {
                if (_hasShownWarning != value)
                {
                    _hasShownWarning = value;
                    ApplicationData.Current.LocalSettings.Values["HasShownWarning"] = value;
                }
            }
        }


        static bool _hideNonDLSSGames = true;
        public static bool HideNonDLSSGames
        {
            get { return _hideNonDLSSGames; }
            set
            {
                if (_hideNonDLSSGames != value)
                {
                    _hideNonDLSSGames = value;
                    ApplicationData.Current.LocalSettings.Values["HideNonDLSSGames"] = value;
                }
            }
        }


        static bool _groupGameLibrariesTogether = false;
        public static bool GroupGameLibrariesTogether
        {
            get { return _groupGameLibrariesTogether; }
            set
            {
                if (_groupGameLibrariesTogether != value)
                {
                    _groupGameLibrariesTogether = value;
                    ApplicationData.Current.LocalSettings.Values["GroupGameLibrariesTogether"] = value;
                }
            }
        }

        static ElementTheme _appTheme = ElementTheme.Default;
        public static ElementTheme AppTheme
        {
            get { return _appTheme; }
            set
            {
                if (_appTheme != value)
                {
                    _appTheme = value;
                    ApplicationData.Current.LocalSettings.Values["AppTheme"] = (int)value;
                }
            }
        }

        static bool _allowExperimental = false;
        public static bool AllowExperimental
        {
            get { return _allowExperimental; }
            set
            {
                if (_allowExperimental != value)
                {
                    _allowExperimental = value;
                    ApplicationData.Current.LocalSettings.Values["AllowExperimental"] = value;
                }
            }
        }


        static bool _allowUntrusted = false;
        public static bool AllowUntrusted
        {
            get { return _allowUntrusted; }
            set
            {
                if (_allowUntrusted != value)
                {
                    _allowUntrusted = value;
                    ApplicationData.Current.LocalSettings.Values["AllowUntrusted"] = value;
                }
            }
        }

        static DateTimeOffset _lastRecordsRefresh = DateTimeOffset.MinValue;
        public static DateTimeOffset LastRecordsRefresh
        {
            get { return _lastRecordsRefresh; }
            set
            {
                if (_lastRecordsRefresh != value)
                {
                    _lastRecordsRefresh = value;
                    ApplicationData.Current.LocalSettings.Values["LastRecordsRefresh"] = Windows.Foundation.PropertyValue.CreateDateTime(value);
                }
            }
        }


        static ulong _lastPromptWasForVersion = 0L;
        public static ulong LastPromptWasForVersion
        {
            get { return _lastPromptWasForVersion; }
            set
            {
                if (_lastPromptWasForVersion != value)
                {
                    _lastPromptWasForVersion = value;
                    ApplicationData.Current.LocalSettings.Values["LastPromptWasForVersion"] = _lastPromptWasForVersion;
                }
            }
        }

        // TODO: Remove after 0.9.9 release.
#if !RELEASE_WINDOWSSTORE
        static bool _hasShownWindowsStoreUpdateMessage = false;
        public static bool HasShownWindowsStoreUpdateMessage
        {
            get { return _hasShownWindowsStoreUpdateMessage; }
            set
            {
                if (_hasShownWindowsStoreUpdateMessage != value)
                {
                    _hasShownWindowsStoreUpdateMessage = value;
                    ApplicationData.Current.LocalSettings.Values["HasShownWindowsStoreUpdateMessage"] = value;
                }
            }
        }
#endif

        // Don't forget to change this back to off.
        static LoggingLevel _loggingLevel = LoggingLevel.Verbose;
        public static LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }
            set
            {
                if (_loggingLevel != value)
                {
                    _loggingLevel = value;
                    ApplicationData.Current.LocalSettings.Values["LoggingLevel"] = (int)_loggingLevel;
                }
            }
        }

        static Settings()
        {

            // Load BaseDirectory from settings.
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.TryGetValue("HasShownWarning", out object tempHasShownWarning))
            {
                if (tempHasShownWarning is bool hasShownWarning)
                {
                    _hasShownWarning = hasShownWarning;
                }
            }

            if (localSettings.Values.TryGetValue("HideNonDLSSGames", out object tempHideNonDLSSGames))
            {
                if (tempHideNonDLSSGames is bool hideNonDLSSGames)
                {
                    _hideNonDLSSGames = hideNonDLSSGames;
                }
            }

            if (localSettings.Values.TryGetValue("GroupGameLibrariesTogether", out object tempGroupGameLibrariesTogether))
            {
                if (tempGroupGameLibrariesTogether is bool groupGameLibrariesTogether)
                {
                    _groupGameLibrariesTogether = groupGameLibrariesTogether;
                }
            }

            if (localSettings.Values.TryGetValue("AppTheme", out object tempAppTheme))
            {
                if (tempAppTheme is int appTheme)
                {
                    _appTheme = (ElementTheme)appTheme;
                }
            }

            if (localSettings.Values.TryGetValue("AllowExperimental", out object tempAllowExperimental))
            {
                if (tempAllowExperimental is bool allowExperimental)
                {
                    _allowExperimental = allowExperimental;
                }
            }

            if (localSettings.Values.TryGetValue("AllowUntrusted", out object tempAllowUntrusted))
            {
                if (tempAllowUntrusted is bool allowUntrusted)
                {
                    _allowUntrusted = allowUntrusted;
                }
            }

            if (localSettings.Values.TryGetValue("LastRecordsRefresh", out object tempLastRecordsRefresh))
            {
                if (tempLastRecordsRefresh is DateTimeOffset lastRecordsRefresh)
                {
                    _lastRecordsRefresh = lastRecordsRefresh;
                }
            }

            if (localSettings.Values.TryGetValue("LastPromptWasForVersion", out object tempLastPromptWasForVersion))
            {
                if (tempLastPromptWasForVersion is ulong lastPromptWasForVersion)
                {
                    _lastPromptWasForVersion = lastPromptWasForVersion;
                }
            }

            // TODO: Remove after 0.9.9 release.
#if !RELEASE_WINDOWSSTORE
            if (localSettings.Values.TryGetValue("HasShownWindowsStoreUpdateMessage", out object tempHasShownWindowsStoreUpdateMessage))
            {
                if (tempHasShownWindowsStoreUpdateMessage is bool hasShownWindowsStoreUpdateMessage)
                {
                    _hasShownWindowsStoreUpdateMessage = hasShownWindowsStoreUpdateMessage;
                }
            }
#endif


            if (localSettings.Values.TryGetValue("LoggingLevel", out object tempLoggingLevel))
            {
                if (tempLoggingLevel is int loggingLevel)
                {
                    _loggingLevel = (LoggingLevel)loggingLevel;
                }
            }
        }
    }
}
