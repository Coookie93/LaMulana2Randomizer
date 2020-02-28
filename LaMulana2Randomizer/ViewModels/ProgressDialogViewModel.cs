using System;
using System.Windows;
using System.Windows.Input;

namespace LaMulana2Randomizer.ViewModels
{
    public struct ProgressInfo
    {
        public string Label;
        public bool IsIndeterminate;
        public int ProgressValue;
    }

    public class ProgressDialogViewModel : BindableBase
    {
        private string _label;
        public string Label {
            get => _label;
            set => Set(ref _label, value);
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate {
            get => _isIndeterminate;
            set => Set(ref _isIndeterminate, value);
        }

        private int _progressValue;
        public int ProgressValue {
            get => _progressValue;
            set => Set(ref _progressValue, value);
        }

        private bool _taskComplete;
        public bool TaskComplete {
            get => _taskComplete;
            set => Set(ref _taskComplete, value);
        }

        public IProgress<ProgressInfo> progress;

        public ProgressDialogViewModel()
        {
            TaskComplete = false;
            progress = new Progress<ProgressInfo>(Update);
        }

        private void Update(ProgressInfo info)
        {
            Label = info.Label;
            IsIndeterminate = info.IsIndeterminate;
            ProgressValue = info.ProgressValue;
        }

        private ICommand _closeCommand;
        public ICommand CloseCommand {
            get {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand((x) => true, (x) => CloseWindow((Window)x));
                }
                return _closeCommand;
            }
        }

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }
    }
}
