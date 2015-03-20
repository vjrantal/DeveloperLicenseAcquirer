using System;
using System.Windows;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Threading;
using System.Windows.Automation;

namespace DeveloperLicenseAcquirer
{
    public partial class App : Application
    {
        private String tailoredDeployPath = null;
        private String username = null;
        private String password = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0 && e.Args.Length < 4)
            {
                tailoredDeployPath = e.Args[0];
                username = e.Args[1];
                password = e.Args[2];

                var processStartInfo = new ProcessStartInfo(tailoredDeployPath, "AcquireDeveloperLicense");

                var process = Process.Start(processStartInfo);
                process.WaitForInputIdle();

                Thread.Sleep(1000);

                var agreementWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id));

                var iAgreeButton = agreementWindow.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[0]
                                                    .FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[2];

                var buttonPattern = iAgreeButton.GetCurrentPattern(AutomationPattern.LookupById(InvokePattern.Pattern.Id)) as InvokePattern;
                buttonPattern.Invoke();

                Thread.Sleep(10000);

                var ongoingProcesses = Process.GetProcessesByName("dllhost");
                AutomationElement credentialsWindow = null;
                foreach (Process ongoingProcess in ongoingProcesses)
                {
                    AutomationElementCollection credentialsWindows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ProcessIdProperty, ongoingProcess.Id));
                    if (credentialsWindows.Count > 0 && credentialsWindows[0].Current.Name == "Developer License")
                    {
                        credentialsWindow = credentialsWindows[0];
                    }
                }

                if (credentialsWindow != null)
                {
                    var credentialsPane = credentialsWindow.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[0]
                                                            .FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[0]
                                                            .FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[0]
                                                            .FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[0]
                                                            .FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);

                    var usernameTextBox = credentialsPane[2].GetCurrentPattern(AutomationPattern.LookupById(ValuePattern.Pattern.Id)) as ValuePattern;
                    usernameTextBox.SetValue(username);

                    var passwordTextBox = credentialsPane[4].GetCurrentPattern(AutomationPattern.LookupById(ValuePattern.Pattern.Id)) as ValuePattern;
                    passwordTextBox.SetValue(password);

                    var loginButton = credentialsPane[5].GetCurrentPattern(AutomationPattern.LookupById(InvokePattern.Pattern.Id)) as InvokePattern;
                    loginButton.Invoke();

                    Thread.Sleep(10000);

                    Process finishUpProcess = Process.GetProcessesByName("TailoredDeploy")[0];

                    var finishWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ProcessIdProperty, finishUpProcess.Id));

                    try
                    {
                        var lastItems = finishWindow.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[0]
                                                    .FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
                        var closeButton = lastItems[3].GetCurrentPattern(AutomationPattern.LookupById(InvokePattern.Pattern.Id)) as InvokePattern;
                        closeButton.Invoke();
                    }
                    catch (System.NullReferenceException)
                    {
                        // We end up here in case signing in fails
                        this.Shutdown(1);
                    }
                    this.Shutdown(0);
                }
                else
                {
                    this.Shutdown(1);
                }
            }
            else
            {
                this.Shutdown(1);
            }
        }
    }
}
