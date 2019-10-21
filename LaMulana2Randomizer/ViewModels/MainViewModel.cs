using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LM2Randomizer.Logging;
using LM2Randomizer.UI;
using LM2Randomizer.Utils;

namespace LM2Randomizer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private Settings _settings;
        public Settings Settings 
        {
            get => _settings;
            set => Set(ref _settings, value);
        }

        public MainViewModel()
        {
            Settings = new Settings();
        }

        private ICommand _generateCommand;
        public ICommand GenerateCommand {
            get {
                if (_generateCommand == null)
                {
                    _generateCommand = new RelayCommand((x) => true, (x) => Generate());
                }
                return _generateCommand;
            }
        }

        public void Generate()
        {
            ProgressDialogViewModel dialogViewModel = new ProgressDialogViewModel()
            {
                Label = "Hello World!",
                IsIndeterminate = true
            };
            ProgressDialog dialog = new ProgressDialog()
            {
                DataContext = dialogViewModel,
                Owner = Application.Current.MainWindow
            };

            var task = Task.Factory.StartNew(() => GenerateSeed(dialogViewModel.progress));
            task.ContinueWith(_ => dialogViewModel.TaskComplete = true);
            dialog.Show();
        }

        public void GenerateSeed(IProgress<ProgressInfo> progress)
        {
            for (int i = 0; i < 1; i++)
            {
                int attemptCount = 0;
                bool canBeatGame;
                Randomiser randomiser = new Randomiser(Settings);

                progress.Report(new ProgressInfo 
                { 
                    Label = $"Generating Seed, {i + 1}/1.", 
                    ProgressValue = 0, 
                    IsIndeterminate = true 
                });
                
                do
                {
                    if (!randomiser.Setup())
                    {
                        progress.Report(new ProgressInfo { 
                            Label = "Failed setup for generation.", 
                            ProgressValue = 100, 
                            IsIndeterminate = false 
                        });
                        return;
                    }

                    if (!randomiser.PlaceItems())
                    {
                        progress.Report(new ProgressInfo 
                        { 
                            Label = "Failed to read data for random item placement.", 
                            ProgressValue = 100, 
                            IsIndeterminate = false 
                        });
                        return;
                    }

                    attemptCount++;
                    canBeatGame = randomiser.CanBeatGame();
                    if (!canBeatGame)
                    {
                        Logger.Log($"Failed to generate beatable configuartion, retrying."); 
                        progress.Report(new ProgressInfo
                        {
                            Label = $"Failed to generate beatable configuartion, retrying attempt {attemptCount}.",
                            ProgressValue = 0,
                            IsIndeterminate = true
                        });
                    }

                } while (!canBeatGame && attemptCount < 10);

                if (attemptCount == 10)
                {
                    Logger.LogAndFlush($"Failed to generate beatable configuration for seed {randomiser.Settings.Seed}"); progress.Report(new ProgressInfo
                    {
                        Label = "Failed to generate beatable configuration stopping after 10 attempts.",
                        ProgressValue = 100,
                        IsIndeterminate = false
                    });
                    continue;
                }


                if (!FileUtils.WriteSpoilers(randomiser))
                {
                    progress.Report(new ProgressInfo
                    {
                        Label = "Failed to write spoiler log.",
                        ProgressValue = 100,
                        IsIndeterminate = false
                    });
                    continue;
                }

                if (!FileUtils.WriteSeedFile(randomiser))
                {
                    progress.Report(new ProgressInfo
                    {
                        Label = "Failed to write seed file.",
                        ProgressValue = 100,
                        IsIndeterminate = false
                    });
                    continue;
                }

                Logger.LogAndFlush($"Successfully generated for seed {randomiser.Settings.Seed}");
                progress.Report(new ProgressInfo
                {
                    Label = "Successfully generated seed.",
                    ProgressValue = 100,
                    IsIndeterminate = false
                });
            }
        }
    }
}
