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
            if (e.Args.Length == 3)
            {
                tailoredDeployPath = e.Args[0];
                username = e.Args[1];
                password = e.Args[2];

                Console.WriteLine("Got the right amount of arguments");
                Console.WriteLine("The given TailoredDeploy executable path was: " + tailoredDeployPath);

                ProcessStartInfo processStartInfo = new ProcessStartInfo(tailoredDeployPath, "AcquireDeveloperLicense");

                Process process = Process.Start(processStartInfo);
                bool reachedIdleState = process.WaitForInputIdle();
                Console.WriteLine("TailoredDeploy process reached idle state: " + reachedIdleState);

                Thread.Sleep(1000);

                AutomationElementCollection topLevelWindows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id));
                AutomationElement agreementWindow = null;
                foreach (AutomationElement topLevelWindow in topLevelWindows)
                {
                    Console.WriteLine("Found top level window with title: " + topLevelWindow.Current.Name);
                    if (topLevelWindow.Current.Name == "Developer License")
                    {
                        agreementWindow = topLevelWindow;
                    }
                }

                var iAgreeButton = agreementWindow.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[0]
                                                  .FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition)[2];
                Console.WriteLine("The name of the I agree button: " + iAgreeButton.Current.Name);
                var buttonPattern = iAgreeButton.GetCurrentPattern(AutomationPattern.LookupById(InvokePattern.Pattern.Id)) as InvokePattern;
                buttonPattern.Invoke();

                Thread.Sleep(5000);

                var ongoingProcesses = Process.GetProcessesByName("dllhost");
                AutomationElement credentialsWindow = null;
                foreach (Process ongoingProcess in ongoingProcesses)
                {
                    Console.WriteLine("Investigating windows from process: " + ongoingProcess.ProcessName);
                    AutomationElementCollection processWindows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ProcessIdProperty, ongoingProcess.Id));
                    if (processWindows.Count > 0)
                    {
                        Console.WriteLine("Found window with title: " + processWindows[0].Current.Name);
                        if (processWindows[0].Current.Name == "Developer License" && ongoingProcess.Id != process.Id)
                        {
                            credentialsWindow = processWindows[0];
                        }
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
                        // We might end up here in case signing in fails.
                        Console.WriteLine("Was not able to close the final window");
                    }
                    Console.WriteLine("License acquisition completed");
                    this.Shutdown(0);
                }
                else
                {
                    Console.WriteLine("Did not find a windows titled \"Developer License\"");
                    this.Shutdown(1);
                }
            }
            else
            {
                Console.WriteLine("Wrong amount of arguments!");
                Console.WriteLine("Usage:");
                Console.WriteLine("DeveloperLicenseAcquirer.exe \"<path-to-tailored-deploy-executable\" \"<username>\" \"<password>\"");
                this.Shutdown(1);
            }
        }
    }
}
