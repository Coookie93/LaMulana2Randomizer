using System;
using System.Windows.Forms;
using LM2Randomiser.Logging;
using LM2Randomiser.Utils;

namespace LM2Randomiser
{
    public partial class LM2Randomiser : Form
    {
        private Settings settings;

        public LM2Randomiser()
        {
            InitializeComponent();

            settings = new Settings();
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            bool canBeatGame = false;
            int attemptCount = 0;
            
            Randomiser randomiser = new Randomiser(settings, SeedInput.Text);

            OutputText.AppendText("Starting seed generation.");
            OutputText.AppendText(Environment.NewLine);

            OutputText.AppendText("Setting up world.");
            OutputText.AppendText(Environment.NewLine);

            if (!randomiser.SetupWorld())
            {
                OutputText.AppendText("Failed initial setup for generation.");
                OutputText.AppendText(Environment.NewLine);
                return;
            }
            
            do
            {
                randomiser.ClearItemsAndState();
                
                OutputText.AppendText("Placing random items.");
                OutputText.AppendText(Environment.NewLine);
                if(!randomiser.PlaceRandomItems())
                {
                    OutputText.AppendText("Failed to read data for random item placement.");
                    OutputText.AppendText(Environment.NewLine);
                    return;
                }

                attemptCount++;
                canBeatGame = randomiser.CanBeatGame();
                if(!canBeatGame)
                {
                    Logger.GetLogger.Log("Failed to generate beatable seed, generating new seed.");
                    OutputText.AppendText("Failed to generate seed, retrying.");
                    OutputText.AppendText(Environment.NewLine);
                }

            } while (!canBeatGame && attemptCount < 10);

            if (!FileUtils.WriteSpoilers(randomiser))
            {
                OutputText.AppendText("Failed to write spoiler log.");
                OutputText.AppendText(Environment.NewLine);
            }
            
            if (!FileUtils.WriteSeedFile(randomiser))
            {
                OutputText.AppendText("Failed to write seed.");
                OutputText.AppendText(Environment.NewLine);
            }


            Logger.GetLogger.Log("Successfully generated seed {0}", randomiser.Seed);
            OutputText.AppendText("Successfully generated seed");
            OutputText.AppendText(Environment.NewLine);
        }

        private void GrailCheck_CheckedChanged(object sender, EventArgs e)
        {
            settings.randomiseGrail = GrailCheck.Checked;
        }

        private void ScannerCheck_CheckedChanged(object sender, EventArgs e)
        {
            settings.randomiseScanner = ScannerCheck.Checked;
        }

        private void MiraiCheck_CheckedChanged(object sender, EventArgs e)
        {
            settings.requireMirai = MiraiCheck.Checked;
        }
    }
}
