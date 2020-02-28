using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LaMulana2Randomizer.UI;
using LaMulana2Randomizer.Utils;

namespace LaMulana2Randomizer.ViewModels
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
            dialog.ShowDialog();
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
                    try
                    {
                        attemptCount++;
                        randomiser.Setup();
                        randomiser.PlaceItems();
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
                    }
                    catch (RandomiserException ex)
                    {
                        Logger.Flush();
                        progress.Report(new ProgressInfo
                        {
                            Label = ex.Message,
                            ProgressValue = 100,
                            IsIndeterminate = false
                        });
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogAndFlush(ex.Message);
                        progress.Report(new ProgressInfo
                        {
                            Label = "Something has gone very wrong!",
                            ProgressValue = 100,
                            IsIndeterminate = false
                        });
                        return;
                    }

                } while (!canBeatGame && attemptCount < 10);

                if (attemptCount == 10)
                {
                    Logger.LogAndFlush($"Failed to generate beatable configuration for seed {randomiser.Settings.Seed}"); 
                    progress.Report(new ProgressInfo
                    {
                        Label = "Failed to generate beatable configuration stopping after 10 attempts.",
                        ProgressValue = 100,
                        IsIndeterminate = false
                    });
                    return;
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
