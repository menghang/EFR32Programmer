using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static EFR32Programmer.ProgViewModel.ProgProcess.ProgStep;

namespace EFR32Programmer
{
    internal class ProgTask
    {
        public static string CommanderPath = string.Empty;
        public static string Rom1 = string.Empty;
        public static string Rom2 = string.Empty;
        private string ConsoleLog = string.Empty;
        //private string ConsoleOutputLog = string.Empty;
        //private string ConsoleErrorLog = string.Empty;
        private DateTime StartTime;
        private static readonly string RomImageFull = "fullimage.s37";
        private static readonly string RomImageFirmware = "firmware.s37";
        private static readonly string RomImageLockbits = "lockbits.s37";
        private static readonly string TargetDevice = "EFR32MG13P832F512IM48";
        private static readonly string DebugSpeed = "8000";
        private static readonly string DebugInterface = "SWD";
        private static readonly int OneStep = 1;
        private static readonly int TotalSteps = 10;
        public string MAC { private set; get; }
        public string InstallCode { private set; get; }
        public string JlinkSn { private set; get; }
        public string RomFolder { get; private set; }

        public delegate void TaskProgressUpdateHandler(object sender, ProgTaskProgressUpdatedEventArgs e);
        public event TaskProgressUpdateHandler TaskProgressUpdated;
        public delegate void StepUpdatedHandler(object sender, ProgStepUpdatedEventArgs e);
        public event StepUpdatedHandler StepUpdated;
        public delegate void TaskCompletedHandler(object sender, EventArgs e);
        public event TaskCompletedHandler TaskCompleted;

        public ProgTask(string _jlinkSn, bool randomMAC, bool RandomInstallCode)
        {
            this.JlinkSn = _jlinkSn;
            this.MAC = randomMAC ? GetRandomMAC() : GetMac();
            this.InstallCode = RandomInstallCode ? GetRandomInstallCode() : GetInstallCode();
        }

        public async Task<bool> Run()
        {
            if (!File.Exists(CommanderPath))
            {
                MessageBox.Show("Commander.exe does not exist. Please check first.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            this.StartTime = DateTime.Now;
            bool result = await Program();
            CleanRomFolder();
            TimeSpan tp = DateTime.Now - this.StartTime;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_TimeEscape, null, tp.TotalSeconds.ToString("0.0") + "s", null));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_JLinkSN, Status.Success, null, null));
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_TimeEscape, Status.Success, null, null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_JLinkSN, Status.Failure, null, null));
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_TimeEscape, Status.Failure, null, null));
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            TaskCompleted(this, new EventArgs());
            return result;
        }

        private bool CreateRomFolder()
        {
            this.RomFolder = Path.Combine(Environment.CurrentDirectory, "TMP_" + GetRandomHex(16));
            try
            {
                Directory.CreateDirectory(this.RomFolder);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        private void CleanRomFolder()
        {
            try
            {
                Directory.Delete(this.RomFolder, true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private async Task<bool> Program()
        {
            if (!await CreateImage())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_CreateImage));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await ResetDevice())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_ResetDevice1));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await UnlockDevice())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_UnlockDevice));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await MassErase())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_MassErase));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await FlashFirmware())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_FlashFirmware));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await FlashLockbits())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_FlashLockbits));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await VerifyImage())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_VerifyImage));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await DumpTokens())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_DumpTokens));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            if (!await ResetDevice2())
            {
                TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(TotalSteps - ProgViewModel.N_ResetDevice2));
                return false;
            }
            TaskProgressUpdated(this, new ProgTaskProgressUpdatedEventArgs(OneStep));
            return true;
        }

        private async Task<bool> CreateImage()
        {
            if (!CreateRomFolder())
            {
                string msg = "################" + Environment.NewLine;
                msg += "Fail to create random ROM folder.";
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, Status.Failure, "Fail", msg));
                return false;
            }
            if (!await CreateFlashImage())
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, Status.Failure, "Fail", null));
                return false;
            }
            if (!await ExtractFirmware())
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, Status.Failure, "Fail", null));
                return false;
            }
            if (!await ExtractLockbits())
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, Status.Failure, "Fail", null));
                return false;
            }
            string tmp = "Pass";
            tmp += Environment.NewLine + "MAC Address: " + this.MAC;
            tmp += Environment.NewLine + "Zigbee Install Code: " + this.InstallCode;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, Status.Success, tmp, null));
            return true;
        }

        private async Task<bool> CreateFlashImage()
        {
            string tmp1 = Rom1.Equals(string.Empty) ? string.Empty : ('\"' + Rom1 + '\"');
            string tmp2 = Rom2.Equals(string.Empty) ? string.Empty : ('\"' + Rom2 + '\"');
            string target = (tmp1 + " " + tmp2).Trim();
            if (target.Equals(string.Empty))
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, null, null, "No available ROM files."));
                return false;
            }
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--outfile", '\"' +Path.Combine(this.RomFolder, RomImageFull)+ '\"'),
                ("--tokengroup", "znet"),
                ("--token", "TOKEN_MFG_CUSTOM_EUI_64:"+this.MAC),
                ("--token", "\"Install Code\":"+this.InstallCode),
                ("--device", TargetDevice)};
            bool result = await RunCommander("convert", target, args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, null, null, msg));
            return result;
        }

        private async Task<bool> ExtractFirmware()
        {
            string target = '\"' + Path.Combine(this.RomFolder, RomImageFull) + '\"';
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--outfile", '\"' +Path.Combine(this.RomFolder, RomImageFirmware)+ '\"'),
                ("--range", "0x00000000:0x0FE04000"),
                ("--range", "0x0FE04800:0xFFFFFFFF"),
                ("--force", null)};
            bool result = await RunCommander("convert", target, args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, null, null, msg));
            return result;
        }

        private async Task<bool> ExtractLockbits()
        {
            string target = '\"' + Path.Combine(this.RomFolder, RomImageFull) + '\"';
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--outfile", '\"' +Path.Combine(this.RomFolder, RomImageLockbits)+ '\"'),
                ("--range", "0x0FE04000:0x0FE04800"),
                ("--force", null)};
            bool result = await RunCommander("convert", target, args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_CreateImage, null, null, msg));
            return result;
        }

        private async Task<bool> ResetDevice()
        {
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--serialno", this.JlinkSn)};
            bool result = await RunCommander("device", "reset", args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_ResetDevice1, null, null, msg));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_ResetDevice1, Status.Success, "Pass", null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_ResetDevice1, Status.Failure, "Fail", null));
            }
            return result;
        }

        private async Task<bool> UnlockDevice()
        {
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--debug", "disable"),
                ("--serialno", this.JlinkSn)};
            bool result = await RunCommander("device", "lock", args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_UnlockDevice, null, null, msg));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_UnlockDevice, Status.Success, "Pass", null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_UnlockDevice, Status.Failure, "Fail", null));
            }
            return result;
        }

        private async Task<bool> MassErase()
        {
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--serialno", this.JlinkSn),
                ("--tif", DebugInterface)};
            bool result = await RunCommander("device", "masserase", args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_MassErase, null, null, msg));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_MassErase, Status.Success, "Pass", null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_MassErase, Status.Failure, "Fail", null));
            }
            return result;
        }

        private async Task<bool> FlashFirmware()
        {
            string target = '\"' + Path.Combine(this.RomFolder, RomImageFirmware) + '\"';
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--serialno", this.JlinkSn),
                ("--speed", DebugSpeed),
                ("--tif", DebugInterface),
                ("--noverify", null),
                ("--halt", null)};
            bool result = await RunCommander("flash", target, args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_FlashFirmware, null, null, msg));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_FlashFirmware, Status.Success, "Pass", null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_FlashFirmware, Status.Failure, "Fail", null));
            }
            return result;
        }

        private async Task<bool> FlashLockbits()
        {
            string target = '\"' + Path.Combine(this.RomFolder, RomImageLockbits) + '\"';
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--serialno", this.JlinkSn),
                ("--speed", DebugSpeed),
                ("--tif", DebugInterface),
                ("--noverify", null),
                ("--halt", null)};
            bool result = await RunCommander("flash", target, args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_FlashLockbits, null, null, msg));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_FlashLockbits, Status.Success, "Pass", null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_FlashLockbits, Status.Failure, "Fail", null));
            }
            return result;
        }

        private async Task<bool> VerifyImage()
        {
            string target = '\"' + Path.Combine(this.RomFolder, RomImageFull) + '\"';
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--serialno", this.JlinkSn),
                ("--speed", DebugSpeed),
                ("--tif", DebugInterface)};
            bool result = await RunCommander("verify", target, args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_VerifyImage, null, null, msg));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_VerifyImage, Status.Success, "Pass", null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_VerifyImage, Status.Failure, "Fail", null));
            }
            return result;
        }

        private async Task<bool> DumpTokens()
        {
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--serialno", this.JlinkSn),
                ("--tokengroup", "znet"),
                ("--speed", DebugSpeed),
                ("--tif", DebugInterface)};
            bool result = await RunCommander("tokendump", null, args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_DumpTokens, null, null, msg));
            if (!result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_DumpTokens, Status.Failure, "Fail", null));
                return false;
            }
            string pattern1 = @"MFG_CUSTOM_EUI_64\s*:\s*([0-9A-F]{16})";
            string pattern2 = @"Install\s+Code\s*:\s*([0-9A-F]{32})";
            MatchCollection ms1 = Regex.Matches(this.ConsoleLog, pattern1);
            MatchCollection ms2 = Regex.Matches(this.ConsoleLog, pattern2);
            if (ms1.Count != 1 || ms2.Count != 1)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_DumpTokens, Status.Failure, "Fail", null));
                return false;
            }
            string tmpMAC = ms1[0].Groups[1].Value;
            string tmpInstallCode = ms2[0].Groups[1].Value;
            if (tmpMAC.Equals(this.MAC) && tmpInstallCode.Equals(this.InstallCode))
            {
                string tmp = "Pass";
                tmp += Environment.NewLine + "MAC Address: " + tmpMAC;
                tmp += Environment.NewLine + "Zigbee Install Code: " + tmpInstallCode;
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_DumpTokens, Status.Success, tmp, null));
                return true;
            }
            else
            {
                string tmp = "Fail";
                tmp += Environment.NewLine + "MAC Address: " + tmpMAC;
                tmp += Environment.NewLine + "Zigbee Install Code: " + tmpInstallCode;
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_DumpTokens, Status.Failure, tmp, null));
                return false;
            }
        }

        private async Task<bool> ResetDevice2()
        {
            List<(string option, string argument)> args = new List<(string option, string argument)>() {
                ("--device", TargetDevice),
                ("--serialno", this.JlinkSn)};
            bool result = await RunCommander("device", "reset", args);
            string msg = "################" + Environment.NewLine;
            msg += this.ConsoleLog;
            StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_ResetDevice2, null, null, msg));
            if (result)
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_ResetDevice2, Status.Success, "Pass", null));
            }
            else
            {
                StepUpdated(this, new ProgStepUpdatedEventArgs(ProgViewModel.N_ResetDevice2, Status.Failure, "Fail", null));
            }
            return result;
        }
        public async Task<(List<string> jlinkList, string commanderVersion)> GetJlinkList()
        {
            List<string> list = new List<string>();
            string version = string.Empty;
            if (!File.Exists(CommanderPath))
            {
                MessageBox.Show("Commander.exe does not exist. Please check first.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return (list, version);
            }
            List<(string option, string argument)> args = new List<(string option, string argument)>() { ("--version", null) };
            bool result = await RunCommander(null, null, args);
            if (!result)
            {
                MsgWindow msg = new MsgWindow();
                msg.SetTitle("Error");
                msg.SetStep("Get Connected J-Link List");
                msg.SetResult("Failure");
                msg.SetMsg(this.ConsoleLog);
                msg.Show();
                return (list, version);
            }
            string pattern = @"Emulator\s+found\s+with\s+SN=([0-9]{8})\s+USBAddr=([0-9]+)";
            foreach (Match m in Regex.Matches(this.ConsoleLog, pattern))
            {
                list.Add(m.Groups[1].Value);
            }
            string pattern2 = @"Simplicity\s+Commander\s+([0-9a-zA-Z]+)";
            foreach (Match m in Regex.Matches(this.ConsoleLog, pattern2))
            {
                version = m.Groups[1].Value;
            }
            return (list, version);
        }

        private string GetRandomHex(int length)
        {
            char[] hexCode = "0123456789ABCDEF".ToCharArray();
            string randomHex = string.Empty;
            if (length <= 0)
            {
                return randomHex;
            }
            for (int ii = 0; ii < length; ii++)
            {
                byte[] randomBytes = new byte[4];
                RNGCryptoServiceProvider rngCrypto = new RNGCryptoServiceProvider();
                rngCrypto.GetBytes(randomBytes);
                int rngNum = BitConverter.ToInt32(randomBytes, 0);
                Random random = new Random(rngNum);
                randomHex += hexCode[random.Next(0, 16)].ToString();
            }
            return randomHex;
        }

        private string GetRandomMAC()
        {
            return GetRandomHex(16);
        }

        private string GetRandomInstallCode()
        {
            return GetRandomHex(32);
        }

        private string GetMac()
        {
            throw new NotImplementedException();
        }

        private string GetInstallCode()
        {
            throw new NotImplementedException();
        }

        private async Task<int> RunConsole(string command)
        {
            using (Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = CommanderPath,
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            })
            {
                //this.ConsoleOutputLog = string.Empty;
                //this.ConsoleErrorLog = string.Empty;
                this.ConsoleLog = string.Empty;
                return await RunProcessAsync(p);
            }
        }

        private Task<int> RunProcessAsync(Process process)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);
            process.OutputDataReceived += (sender, e) => this.ConsoleLog += e.Data + Environment.NewLine;
            process.ErrorDataReceived += (sender, e) => this.ConsoleLog += e.Data + Environment.NewLine;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;
        }

        private async Task<bool> RunCommander(string command, string target, List<(string option, string argument)> args)
        {
            try
            {
                string cmd = (command + " " + target).Trim();
                foreach ((string option, string argument) in args)
                {
                    cmd += " " + option + " " + argument;
                    cmd = cmd.Trim();
                }
                return (await RunConsole(cmd)) == 0;
            }
            catch (Exception ex)
            {
                this.ConsoleLog += ex.ToString();
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        public class ProgTaskProgressUpdatedEventArgs : EventArgs
        {
            public ProgTaskProgressUpdatedEventArgs(int _inc)
            {
                this.Inc = _inc;
            }
            public int Inc { get; private set; }
        }

        public class ProgStepUpdatedEventArgs : EventArgs
        {
            public ProgStepUpdatedEventArgs(int _step, Status? _status, string _text, string _detailMsg)
            {
                this.Step = _step;
                this.Status = _status;
                this.Text = _text;
                this.DetailMsg = _detailMsg;
            }
            public int Step { get; private set; }
            public Status? Status { get; private set; }
            public string Text { get; private set; }
            public string DetailMsg { get; private set; }
        }
    }
}
