﻿using AsyncAwaitBestPractices;
using DLSS_Swapper.Data.TechPowerUp;
using DLSS_Swapper.Data;
using DLSS_Swapper.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DLSS_Swapper
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static NavigationView NavigationView;
        public MainWindow()
        {
            Title = "DLSS Swapper [ beta ]";
            this.InitializeComponent();
            NavigationView = MainNavigationView;

            MainNavigationView.RequestedTheme = (ElementTheme)Settings.AppTheme;
        }

        void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            //FrameNavigationOptions navOptions = new FrameNavigationOptions();
            //navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

            if (args.InvokedItem is String invokedItem)
            {
                GoToPage(invokedItem);
            }
        }

        void GoToPage(string page)
        {
            Type pageType = null;

            if (page == "Games")
            {
                pageType = typeof(GameGridPage);
            }
            else if (page == "Download")
            {
                pageType = typeof(DownloadPage);
            }
            else if (page == "Settings")
            {
                pageType = typeof(SettingsPage);
            }
            else if (page == "InitialLoading")
            {
                pageType = typeof(InitialLoadingPage);
            }

            foreach (NavigationViewItem navigationViewItem in MainNavigationView.MenuItems)
            {
                if (navigationViewItem.Tag.ToString() == page)
                {
                    MainNavigationView.SelectedItem = navigationViewItem;
                    break;
                }
            }

            if (pageType != null)
            {
                ContentFrame.Navigate(pageType);
            }
        }

        async void MainNavigationView_Loaded(object sender, RoutedEventArgs e)
        {

            if (Settings.HasShownWorkInProgress == false)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Work in Progress - Please Read",
                    CloseButtonText = "Okay",
                    Content = @"DLSS Swapper not complete. This is an early beta, as such it may be somewhat confusing and not user friendly in its operation. 

For more details on how to use the tool please see the 'Usage' section of our site.",
                    PrimaryButtonText = "View Usage",
                    XamlRoot = MainNavigationView.XamlRoot,
                };
                var didClick = await dialog.ShowAsync();

                Settings.HasShownWorkInProgress = true;

                if (didClick == ContentDialogResult.Primary)
                {
                    await Launcher.LaunchUriAsync(new Uri("https://beeradmoore.github.io/dlss-swapper/usage/"));
                }
            }

            if (Settings.HasShownWarning == false)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Warning",
                    CloseButtonText = "Okay",
                    Content = @"Replacing dlls on your computer can be dangerous.

Placing a malicious dll into a game is just as bad as running Linking_park_-_nUmB_mp3.exe that you just downloaded from LimeWire.

More protections and validations will come in a future update.",
                    XamlRoot = MainNavigationView.XamlRoot,
                };
                await dialog.ShowAsync();

                Settings.HasShownWarning = true;
            }

            if (ShouldMigrate())
            {
                var dialog = new ContentDialog()
                {
                    Title = "Migration Needed",
                    CloseButtonText = "Close",
                    PrimaryButtonText = "Migrate",
                    SecondaryButtonText = "Migrate and delete old folder",
                    Content = @"DLSS Swapper now stores files in its app directory. We can migrate your existing files over to avoid you having to download them again.

If you choose to delete the old folder then everything inside ""Documents/DLSS Swapper/"" will be deleted, not just the files we migrated. This is usually the preferred option.",
                    XamlRoot = MainNavigationView.XamlRoot,
                };
                var migrationDialogResult = await dialog.ShowAsync();
                if (migrationDialogResult == ContentDialogResult.None)
                {
                    // If the user opts to not migrate we will never prompt them again.
                    Settings.MigrationAttempted = true;
                }
                else
                {
                    bool shouldDelete = (migrationDialogResult == ContentDialogResult.Secondary);
                    bool didMigrate = await Migrate(shouldDelete);
                    if (didMigrate == false)
                    {
                        var didntDeleteMessage = (shouldDelete ? " We didn't attempt to delete files." : String.Empty); 
                        dialog = new ContentDialog()
                        {
                            Title = "Ohno",
                            CloseButtonText = "Close",
                            Content = $@"Something went wrong and we weren't able to migrate all of your DLSS dlls.{didntDeleteMessage}

Migration will not be attempted again on next launch.",
                            XamlRoot = MainNavigationView.XamlRoot,
                        };
                        await dialog.ShowAsync();
                    }
                }
            }

            // We are now ready to show the games list.
            LoadingStackPanel.Visibility = Visibility.Collapsed;
            GoToPage("Games");

        // TODO: Remove in a future release.
        /// <summary>
        /// Urgghhh.. Apparently Documents folder was not the best place to store files. This will detect if we should attempt migration.
        /// </summary>
        /// <returns>True if migration should happen</returns>
        bool ShouldMigrate()
        {
            // If migration has been attemtped we don't attempt it again.
            if (Settings.MigrationAttempted)
            {
                return false;
            }

            // If the old DLSS Swapper directory doens't exist then there is nothing to migrate.
            var oldDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DLSS Swapper");
            if (Directory.Exists(oldDirectory) == false)
            {
                return false;
            }

            // If there are any dll files in this folder we should migrate.
            var dllFilesFound = Directory.GetFiles(oldDirectory, "*.dll", SearchOption.AllDirectories).Length;

            if (dllFilesFound > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Urgghhh.. Apparently Documents folder was not the best place to store files. This will migrate the old files into the apps directory.
        /// </summary>
        /// <param name="shouldDelete">If we should delete the old DLSS Swapper folder in documents after migration.</param>
        /// <returns>True if migration was successful</returns>
        async Task<bool> Migrate(bool shouldDelete)
        {
            LoadingProgressText.Text = "Migrating";

            Settings.MigrationAttempted = true;

            var oldDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DLSS Swapper");
            if (Directory.Exists(oldDirectory) == false)
            {
                // If old directory doesn't exist we tell a white lie an say the migration was a success.
                return true;
            }

            // If there are any dll or zip files in this folder we should migrate.
            var dllFiles = Directory.GetFiles(oldDirectory, "*.dll", SearchOption.AllDirectories);

            // Show progress ring as we process files.
            LoadingProgressRing.IsIndeterminate = false;
            LoadingProgressRing.Value = 0;
            LoadingProgressRing.Maximum = dllFiles.Length + 1;

            // Prep the output folders.
            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var dllsFolder = await storageFolder.CreateFolderAsync("dlls", Windows.Storage.CreationCollisionOption.OpenIfExists);

            // We keep track if all dlls were migrated. If they weren't we just let the user know that all files may or may not be there.
            bool didMigrateAll = true;
            foreach (var dllFile in dllFiles)
            {
                ++LoadingProgressRing.Value;

                var versionInfo = FileVersionInfo.GetVersionInfo(dllFile);


                var versionNumber = ((ulong)versionInfo.FileMajorPart << 48) +
                         ((ulong)versionInfo.FileMinorPart << 32) +
                         ((ulong)versionInfo.FileBuildPart << 16) +
                         ((ulong)versionInfo.FilePrivatePart);

                var knownDLSSVersions = _dlssRecords.Stable.Where(x => x.VersionNumber == versionNumber).ToList();
                
                // If we don't have a matching DLSS Version then who knows what this is.
                if (knownDLSSVersions.Count == 0)
                {
                    didMigrateAll = false;
                    continue;
                }

                var md5Hash = String.Empty;
                try
                {
                    using (var stream = File.OpenRead(dllFile))
                    {
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            var hash = md5.ComputeHash(stream);
                            md5Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                        }
                    }
                }
                catch (Exception )
                {
                    // Error checking MD5 of file, skip over it.
                    didMigrateAll = false;
                    continue;
                }

                // Unless we are able to narrow this down to a single DLSS records we will skip over it.
                var knownDLSSVersion = knownDLSSVersions.Where(x => x.MD5Hash == md5Hash).FirstOrDefault();
                if (knownDLSSVersion == null)
                {
                    didMigrateAll = false;
                    continue;
                }
                var dlssVersion = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";

                // Files are stored in a folder like 
                // 2.2.18.0_77A75B96DD2D36A4A291F3939D59C221
                // because there are instances where certain DLSS versions (eg. 2.2.18.0) have multiple versions such as 
                // 2.2.18.0_B2B6FAE8936719CF81D6B5577F257C40
                // 2.2.18.0_CD71EE48B994AC254DFF5DEC20828BE7
                var didWriteSuccess = false;
                //Windows.Storage.StorageFile dlssFile = null;
                Windows.Storage.StorageFolder dlssFolder = null;
                try
                {
                    dlssFolder = await dllsFolder.CreateFolderAsync($"{dlssVersion}_{md5Hash}", Windows.Storage.CreationCollisionOption.OpenIfExists);
                    var dlssFile = await dlssFolder.CreateFileAsync("nvngx_dlss.dll", Windows.Storage.CreationCollisionOption.FailIfExists);

                    // Copy data across.
                    using (var outputStream = await dlssFile.OpenStreamForWriteAsync())
                    {
                        using (var inputStream = File.OpenRead(dllFile))
                        {
                            await inputStream.CopyToAsync(outputStream);
                        }

                        outputStream.Position = 0;

                        // Double check MD5 hash that the file was written correctly.
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            var hash = md5.ComputeHash(outputStream);
                            var newMD5Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();

                            if (md5Hash == newMD5Hash)
                            {
                                didWriteSuccess = true;
                            }
                        }
                    }
                }
                catch (Exception )
                {
                    // This can happen if the migration folder already exists, or we were unable to write the file.
                    // In those cases we want to just say 🤷‍ and move on.
                    didMigrateAll = false;
                    continue;
                }
                finally
                {
                    if (didWriteSuccess == false)
                    {
                        didMigrateAll = false;
                        try
                        {
                            await dlssFolder?.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
                        }
                        catch (Exception )
                        {
                            // Sometimes you wonder how you got here. We failed to migrate the file, but also failed to delete it... Yikes.
                        }
                    }
                }
            }

            if (shouldDelete && didMigrateAll)
            {
                try
                {
                    Directory.Delete(oldDirectory, true);
                }
                catch (Exception )
                {
                    // NOOP
                }
            }

            return didMigrateAll;
        }
    }
}
