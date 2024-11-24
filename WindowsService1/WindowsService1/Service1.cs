using System;
using System.Diagnostics;
using System.Net.Http;
using System.ServiceProcess;
using System.Timers;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        Timer ServiceTimer = new Timer();

        public Service1()
        {
            InitializeComponent();
        }

        private void LogEvent(string msg, EventLogEntryType eventLogType) {
            using (EventLog eventLog = new EventLog("Application")) {
                eventLog.Source = "MyTestGoogle";
                eventLog.WriteEntry(msg, eventLogType, 101, 1);
            }
        }

        private void TestConnection(object sender, ElapsedEventArgs e)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var result = client.GetAsync("https://google.com").Result.StatusCode;

                    if (result == System.Net.HttpStatusCode.OK)
                    {
                        LogEvent($"google.com works. Status: {result.ToString()}", EventLogEntryType.SuccessAudit);
                    }
                    else
                    {
                        LogEvent($"During connection to google.com something went wrong. " +
                            $"Status: {result.ToString()}", EventLogEntryType.Warning);
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"During connection to google.com occured an exception. " +
                            $"Status: {ex.ToString()}", EventLogEntryType.Warning);
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            ServiceTimer.Enabled = true;
            ServiceTimer.Interval = 30 * 1000;
            ServiceTimer.Elapsed += new ElapsedEventHandler(TestConnection);
            ServiceTimer.Start();
            LogEvent("Service started!", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            ServiceTimer.Stop();
            LogEvent("Service stopped!", EventLogEntryType.Information);
        }
    }
}