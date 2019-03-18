using System;
using System.Windows.Forms;
using LM2Randomiser.Logging;
using LM2Randomiser.Utils;

namespace LM2Randomiser
{
    public partial class LM2Randomiser : Form
    {
        Settings settings;

        public LM2Randomiser()
        {
            InitializeComponent();

            settings = new Settings();
        }

        private void ModButton_MouseHover(object sender, EventArgs e)
        {
            this.ToolTip.Show("This only needs to be ran once, creates a modded version of the DLL, also creates a backup of the original. This takes a few seconds too complete.", ModButton);
        }

        private void ModButton_Click(object sender, EventArgs e)
        {
            OutputText.AppendText("Starting modding DLL.");
            OutputText.AppendText(Environment.NewLine);
            if (Modding.Modding.ModDLL())
            {
                OutputText.AppendText("Successfully modded the DLL.");
            }
            else
            {
                OutputText.AppendText("Failed modded the DLL.");
            }
            OutputText.AppendText(Environment.NewLine);
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            bool canBeatGame = false;
            Randomiser randomiser = new Randomiser(settings);

            OutputText.AppendText("Starting seed generation.");
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
                
                OutputText.AppendText("Placing non random items.");
                OutputText.AppendText(Environment.NewLine);

                if (!randomiser.PlaceNonRandomItems())
                {
                    OutputText.AppendText("Failed to read data for non random item placement.");
                    OutputText.AppendText(Environment.NewLine);
                    return;
                }
                
                OutputText.AppendText("Placing random items.");
                OutputText.AppendText(Environment.NewLine);
                if(!randomiser.PlaceRandomItems())
                {
                    OutputText.AppendText("Failed to read data for random item placement.");
                    OutputText.AppendText(Environment.NewLine);
                    return;
                }


                canBeatGame = randomiser.CanBeatGame();
                if(!canBeatGame)
                {
                    Logger.GetLogger.Log("Failed to generate beatable seed, generating new seed.");
                    OutputText.AppendText("Failed to generate seed, retrying.");
                    OutputText.AppendText(Environment.NewLine);
                }

            } while (!canBeatGame);

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


            Logger.GetLogger.Log("Succesfully generated seed {0}", randomiser.Seed);
            OutputText.AppendText("Succesfully generated seed: " + randomiser.Seed);
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

        private void LM2Randomiser_Load(object sender, EventArgs e)
        {

        }
        
    }
}
