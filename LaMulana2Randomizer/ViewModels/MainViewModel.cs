using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LaMulana2Randomizer.UI;
using LaMulana2Randomizer.Utils;
using Version = LaMulana2RandomizerShared.Version;

namespace LaMulana2Randomizer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private string _title;
        public string Title {
            get => _title;
            set => Set(ref _title, value);
        }

        private Settings _settings;
        public Settings Settings 
        {
            get => _settings;
            set => Set(ref _settings, value);
        }

        public MainViewModel()
        {
            Title = Version.version;
            Settings = FileUtils.LoadSettings();
            Reroll();
        }

        private ICommand _rerollCommand;
        public ICommand RerollCommand {
            get {
                if (_rerollCommand == null)
                {
                    _rerollCommand = new RelayCommand((x) => true, (x) => Reroll());
                }
                return _rerollCommand;
            }
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

        public void Reroll()
        {
            Settings.Seed = new Random().Next(int.MinValue, int.MaxValue);
        }

        public void Generate()
        {
            FileUtils.SaveSettings(Settings);
            ProgressDialogViewModel dialogViewModel = new ProgressDialogViewModel();
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
            const int MaxAttempts = 250;

            Randomiser randomiser = new Randomiser(Settings);

            progress.Report(new ProgressInfo 
            { 
                Label = $"Generating Seed ", 
                ProgressValue = 0, 
                IsIndeterminate = true 
            });

            int attemptCount = 0;
            try
            {
                randomiser.Setup();
                randomiser.ChooseStartingWeapon();
                bool entranceCheck;
                do
                {
                    attemptCount++;
                    randomiser.PlaceEntrances();
                    entranceCheck = randomiser.EntranceCheck();
                    if (!entranceCheck)
                    {
                        randomiser.ClearEntrances();
                        Logger.Log($"Failed to generate beatable entrance configuartion, retrying.");
                        progress.Report(new ProgressInfo
                        {
                            Label = $"Failed to generate beatable entrance configuartion, retrying attempt {attemptCount}.",
                            ProgressValue = 0,
                            IsIndeterminate = true
                        });
                    }

                } while (!entranceCheck);

                randomiser.FixAnkhLogic();
                randomiser.FixFDCLogic();

                attemptCount = 0;
                bool canBeatGame;
                do
                {
                    attemptCount++;
                    randomiser.PlaceItems();
                    randomiser.AdjustShopPrices();
                    canBeatGame = randomiser.CanBeatGame();
                    if (!canBeatGame)
                    {
                        randomiser.ClearPlacedItems();
                        Logger.Log($"Failed to generate beatable item placement, retrying.");
                        progress.Report(new ProgressInfo
                        {
                            Label = $"Failed to generate beatable item placement, retrying attempt {attemptCount}.",
                            ProgressValue = 0,
                            IsIndeterminate = true
                        });
                    }
                } while (!canBeatGame && attemptCount < MaxAttempts);

                if (attemptCount == MaxAttempts && !canBeatGame)
                {
                    FileUtils.WriteSpoilerLog(randomiser);
                    Logger.LogAndFlush($"Failed to generate beatable configuration for seed {randomiser.Settings.Seed}");
                    progress.Report(new ProgressInfo
                    {
                        Label = $"Failed to generate beatable configuration stopping after {MaxAttempts} attempts.",
                        ProgressValue = 100,
                        IsIndeterminate = false
                    });
                    return;
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

            if (!FileUtils.WriteSpoilerLog(randomiser))
            {
                progress.Report(new ProgressInfo
                {
                    Label = "Failed to write spoiler log.",
                    ProgressValue = 100,
                    IsIndeterminate = false
                });
                return;
            }

            if (!FileUtils.WriteSeedFile(randomiser))
            {
                progress.Report(new ProgressInfo
                {
                    Label = "Failed to write seed file.",
                    ProgressValue = 100,
                    IsIndeterminate = false
                });
                return;
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
