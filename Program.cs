﻿//
// Copyright (c) Ping Castle. All rights reserved.
// https://www.pingcastle.com
//
// Licensed under the Non-Profit OSL. See LICENSE file in the project root for full license information.
//
using PingCastle.ADWS;
using PingCastle.Data;
using PingCastle.Graph.Reporting;
using PingCastle.Healthcheck;
using PingCastle.Report;
using PingCastle.Scanners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PingCastle
{
	[LicenseProvider(typeof(PingCastle.ADHealthCheckingLicenseProvider))]
	public class Program : IPingCastleLicenseInfo
	{
		bool PerformHealthCheckReport = false;
		bool PerformHealthCheckConsolidation = false;
		private bool PerformAdvancedConsolidation = false;
		bool PerformGenerateKey = false;
		bool PerformCarto = false;
		bool PerformUploadAllReport;
		bool PerformHCRules = false;
		private bool PerformRegenerateReport;
		private bool PerformAdvancedRegenerateReport;
		private bool PerformHealthCheckReloadReport;
		bool PerformHealthCheckGenerateDemoReports;
		bool PerformScanner = false;
		bool PerformGenerateFakeReport = false;
		bool PerformBot = false;
		Tasks tasks = new Tasks();


        public static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                Trace.WriteLine("Running on dotnet:" + Environment.Version);
                Program program = new Program();
                program.Run(args);
                if (program.tasks.InteractiveMode)
                {
                    Console.WriteLine("=============================================================================");
                    Console.WriteLine("Program launched in interactive mode - press any key to terminate the program");
                    Console.WriteLine("=============================================================================");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Tasks.DisplayException("main program", ex);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Tasks.DisplayException("application domain", e.ExceptionObject as Exception);
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // hook required for "System.Runtime.Serialization.ContractNamespaceAttribute"
            var name = new AssemblyName(args.Name);    
            Trace.WriteLine("Needing assembly " + name + " unknown (" + args.Name + ")");
            return null;
        }

		private void Run(string[] args)
		{
			ADHealthCheckingLicense license = null;
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Trace.WriteLine("10Root RisX powered by PingCastle version " + version.ToString(4));
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Equals("--debug-license", StringComparison.InvariantCultureIgnoreCase))
				{
					EnableLogConsole();
				}
				else if (args[i].Equals("--license", StringComparison.InvariantCultureIgnoreCase) && i + 1 < args.Length)
				{
					_serialNumber = args[++i];
				}
			}
			Trace.WriteLine("Starting the license checking");
			try
			{
				license = LicenseManager.Validate(typeof(Program), this) as ADHealthCheckingLicense;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("the license check failed - please check that the .config file is in the same directory");
				WriteInRed(ex.Message);
				if (args.Length == 0)
				{
					Console.WriteLine("=============================================================================");
					Console.WriteLine("Program launched in interactive mode - press any key to terminate the program");
					Console.WriteLine("=============================================================================");
					Console.ReadKey();
				}
				return;
			}
			Trace.WriteLine("License checked");
			if (license.EndTime < DateTime.Now)
			{
				WriteInRed("The program is unsupported since: " + license.EndTime.ToString("u") + ")");
				if (args.Length == 0)
				{
					Console.WriteLine("=============================================================================");
					Console.WriteLine("Program launched in interactive mode - press any key to terminate the program");
					Console.WriteLine("=============================================================================");
					Console.ReadKey();
				}
				return;
			}
			if (license.EndTime < DateTime.MaxValue)
			{
				Console.WriteLine();
			}
			tasks.License = license;
			ConsoleMenu.Header = @"|     10Root RisX powered by PingCastle (Version " + version.ToString(4) + @"     " + ConsoleMenu.GetBuildDateTime(Assembly.GetExecutingAssembly()) + @")
|     Get Active Directory Security at 80% in 20% of the time
|     " + (license.EndTime < DateTime.MaxValue ? "End of support: " + license.EndTime.ToShortDateString() : "") + @"     |     https://10root.com
";
			if (!ParseCommandLine(args))
				return;
			// Trace to file or console may be enabled here
			Trace.WriteLine("[New run]" + DateTime.Now.ToString("u"));
			Trace.WriteLine("PingCastle version " + version.ToString(4));
			Trace.WriteLine("Running on dotnet:" + Environment.Version);
			if (!String.IsNullOrEmpty(license.DomainLimitation) && !Tasks.compareStringWithWildcard(license.DomainLimitation, tasks.Server))
			{
				WriteInRed("Limitations applies to the --server argument (" + license.DomainLimitation + ")");
				return;
			}
			if (!String.IsNullOrEmpty(license.CustomerNotice))
			{
				Console.WriteLine(license.CustomerNotice);
			}
			if (PerformGenerateKey)
			{
				if (!tasks.GenerateKeyTask()) return;
			}
			if (PerformScanner)
			{
				if (!tasks.ScannerTask()) return;
			}
			if (PerformCarto)
			{
				if (!tasks.CartoTask(PerformHealthCheckGenerateDemoReports)) return;
			}
			if (PerformBot)
			{
				if (!tasks.BotTask()) return;
			}
			if (PerformHealthCheckReport)
			{
				if (!tasks.AnalysisTask<HealthcheckData>()) return;
                // Addition
                if (tasks.xmlreports.Count > 0 && !string.IsNullOrEmpty(tasks.Server) && !string.IsNullOrEmpty(tasks.CustomConfigFileOrDirectory))
                {
					//get only one -- need to implement for * [ can get names from tasks.xmlreports]
					tasks.FileOrDirectory = "ad_hc_" + tasks.Server + ".xml";
					if (!tasks.AdvancedRegenerateHtmlTask()) return;
				}
            }
			if (PerformHealthCheckConsolidation || PerformAdvancedConsolidation || (PerformHealthCheckReport && tasks.Server == "*" && tasks.InteractiveMode))
			{
				if (!tasks.ConsolidationTask<HealthcheckData>()) return;
			}
			if (PerformHCRules)
			{
				if (!tasks.HealthCheckRulesTask()) return;
			}
			if (PerformRegenerateReport)
			{
				if (!tasks.RegenerateHtmlTask()) return;
			}
			if (PerformAdvancedRegenerateReport) // maybe add here Or To Run On Results
			{
				if (!tasks.AdvancedRegenerateHtmlTask()) return;
			}
			if (PerformHealthCheckReloadReport)
			{
				if (!tasks.ReloadXmlReport()) return;
			}
			if (PerformHealthCheckGenerateDemoReports && !PerformCarto)
			{
				if (!tasks.GenerateDemoReportTask()) return;
			}
			if (PerformUploadAllReport)
			{
				if (!tasks.UploadAllReportInCurrentDirectory()) return;
			}
			if (PerformGenerateFakeReport)
			{
				if (!tasks.GenerateFakeReport()) return;
			}
			tasks.CompleteTasks();
		}

        const string basicEditionLicense = "PC2H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/In7NX+PX+DV+A/r/r/F7/j7/6l/zO/3bv+avTb/W9P9n9G9O/6W/xumvMfs1il+jpf9Xv8aS/q5+jXP69yX9vfw1Ln6Nk18j+zUa+rbktnu/xvjXeEj/36Pft+n/L6h1Sz/P6WdNP6f0c0H/5fTXlCBk9F76a6wJQv5rAIk/6Nf4NX6NP/gv+0cfpv/Jn/+//Od/6V/4h//TyWd/4y8+/2XlX/qP/5GPz//Xs9/oN/off4c3/+Wf8Lf+js/u/YPL/d/61/x3/9Qfe/d//H5/0b/6B/4n/87//L/973/gv/Hb/ZKTX/Ef/zWfP/zr/pqf/O4//eWv9fm/+snf+eB7v80v+R3+kP9m8sf+H//KX/nuN/2N/7Xf7lt/2F/6f/8Of9d/++f+xD968fLHfqO/9J/8g//4P378a/9Wv99f9r/9Jr/8d5v9T9Vn//T/A/h89xASAQAA";
        string _serialNumber;
        public string GetSerialNumber()
        {
            if (String.IsNullOrEmpty(_serialNumber))
            {
                // try to load it from the configuration file
                try
                {
                    _serialNumber = ADHealthCheckingLicenseSettings.Settings.License;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception when getting the license string");
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        Trace.WriteLine(ex.InnerException.Message);
                        Trace.WriteLine(ex.InnerException.StackTrace);
                    }

                }
                if (!String.IsNullOrEmpty(_serialNumber))
                {
                    try
                    {
                        var license = new ADHealthCheckingLicense(_serialNumber);
                        return _serialNumber;
                    }
                    catch (Exception ex)
                    {
                        _serialNumber = null;
                        Trace.WriteLine("Exception when verifying the external license");
                        Trace.WriteLine(ex.Message);
                        Trace.WriteLine(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            Trace.WriteLine(ex.InnerException.Message);
                            Trace.WriteLine(ex.InnerException.StackTrace);
                        }
                    }

                }
            }
            // fault back to the default license:
            _serialNumber = basicEditionLicense;
            try
            {
                var license = new ADHealthCheckingLicense(_serialNumber);
            }
            catch (Exception)
            {
                throw new PingCastleException("Unable to load the license from the .config file and the license embedded in PingCastle is not valid. Check that all files have been copied in the same directory and that you have a valid license");
            }
            return _serialNumber;
        }

        private void WriteInRed(string data)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(data);
            Trace.WriteLine("[Red]" + data);
            Console.ResetColor();
        }

        private string GetCurrentDomain()
        {
            return IPGlobalProperties.GetIPGlobalProperties().DomainName;
        }

		// parse command line arguments
		private bool ParseCommandLine(string[] args)
		{
			string user = null;
			string userdomain = null;
			string password = null;
			bool delayedInteractiveMode = false;
			if (args.Length == 0)
			{
				if (!RunInteractiveMode())
					return false;
			}
			else
			{
				Trace.WriteLine("Before parsing arguments");
				for (int i = 0; i < args.Length; i++)
				{
					switch (args[i])
					{
						case "--add-data":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --add-data is mandatory");
								return false;
							}
							tasks.CustomConfigFileOrDirectory = args[++i];
							if (!File.Exists(tasks.CustomConfigFileOrDirectory))
							{
								WriteInRed("The file " + tasks.CustomConfigFileOrDirectory + " doesn't exist");
								return false;
							}
							break;
						case "--api-endpoint":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --api-endpoint is mandatory");
								return false;
							}
							tasks.apiEndpoint = args[++i];
							{
								Uri res;
								if (!Uri.TryCreate(tasks.apiEndpoint, UriKind.Absolute, out res))
								{
									WriteInRed("unable to convert api-endpoint into an URI");
									return false;
								}
							}
							break;
						case "--api-key":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --api-key is mandatory");
								return false;
							}
							tasks.apiKey = args[++i];
							break;
						case "--bot":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --bot is mandatory");
								return false;
							}
							tasks.botPipe = args[++i];
							PerformBot = true;
							break;
						case "--carto":
							PerformCarto = true;
							break;
						case "--center-on":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --center-on is mandatory");
								return false;
							}
							tasks.CenterDomainForSimpliedGraph = args[++i];
							break;
						case "--debug-license":
							break;
						case "--demo-reports":
							PerformHealthCheckGenerateDemoReports = true;
							break;
						case "--encrypt":
							tasks.EncryptReport = true;
							break;
						case "--foreigndomain":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --foreigndomain is mandatory");
								return false;
							}
							ForeignUsersScanner.EnumInboundSid = args[++i];
							break;
						case "--explore-trust":
							tasks.ExploreTerminalDomains = true;
							break;
						case "--explore-forest-trust":
							tasks.ExploreForestTrust = true;
							break;
						case "--explore-exception":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --explore-exception is mandatory");
								return false;
							}
							tasks.DomainToNotExplore = new List<string>(args[++i].Split(','));
							break;
						case "--filter-date":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --filter-date is mandatory");
								return false;
							}
							if (!DateTime.TryParse(args[++i], out tasks.FilterReportDate))
							{
								WriteInRed("Unable to parse the date \"" + args[i] + "\" - try entering 2016-01-01");
								return false;
							}
							break;
						case "--regen-report":
							PerformRegenerateReport = true;
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --regen-report is mandatory");
								return false;
							}
							tasks.FileOrDirectory = args[++i];
							break;
						case "--advanced-regen-report":
							PerformAdvancedRegenerateReport = true;
							if (i + 2 >= args.Length)
							{
								WriteInRed("arguments for --advanced-regen-report is mandatory");
								return false;
							}
							tasks.FileOrDirectory = args[++i];
							tasks.CustomConfigFileOrDirectory = args[++i];
							break;
						case "--generate-fake-reports":
							PerformGenerateFakeReport = true;
							break;
						case "--generate-key":
							PerformGenerateKey = true;
							break;
						case "--healthcheck":
							PerformHealthCheckReport = true;
							break;
						case "--hc-conso":
							PerformHealthCheckConsolidation = true;
							break;
						case "--advanced-hc-conso":
							PerformAdvancedConsolidation = true;
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --advanced-hc-conso is mandatory");
								return false;
							}
							tasks.AdvancedConsoDirectory = args[++i];
							break;
						case "--help":
							DisplayHelp();
							return false;
						case "--I-swear-I-paid-win7-support":
							Healthcheck.Rules.HeatlcheckRuleStaledObsoleteWin7.IPaidSupport = true;
							break;
						case "--interactive":
							delayedInteractiveMode = true;
							break;
						case "--level":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --level is mandatory");
								return false;
							}
							try
							{
								tasks.ExportLevel = (PingCastleReportDataExportLevel)Enum.Parse(typeof(PingCastleReportDataExportLevel), args[++i]);
							}
							catch (Exception)
							{
								WriteInRed("Unable to parse the level [" + args[i] + "] to one of the predefined value (" + String.Join(",", Enum.GetNames(typeof(PingCastleReportDataExportLevel))) + ")");
								return false;
							}
							break;
						case "--license":
							i++;
							break;
						case "--log":
							EnableLogFile();
							break;
						case "--log-console":
							EnableLogConsole();
							break;
						case "--log-samba":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for -log-samba is mandatory");
								return false;
							}
							LinuxSidResolver.LogLevel = args[++i];
							break;
						case "--max-nodes":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --max-nodes is mandatory");
								return false;
							}
							{
								int maxNodes;
								if (!int.TryParse(args[++i], out maxNodes))
								{
									WriteInRed("argument for --max-nodes is not a valid value (typically: 1000)");
									return false;
								}
								ReportGenerator.MaxNodes = maxNodes;
							}
							break;
						case "--max-depth":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --max-depth is mandatory");
								return false;
							}
							{
								int maxDepth;
								if (!int.TryParse(args[++i], out maxDepth))
								{
									WriteInRed("argument for --max-depth is not a valid value (typically: 30)");
									return false;
								}
								ReportGenerator.MaxDepth = maxDepth;
							}
							break;
						case "--no-enum-limit":
							ReportHealthCheckSingle.MaxNumberUsersInHtmlReport = int.MaxValue;
							break;
						case "--node":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --node is mandatory");
								return false;
							}
							tasks.NodesToInvestigate = new List<string>(Regex.Split(args[++i], @"(?<!(?<!\\)*\\)\,"));
							break;
						case "--nodes":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --nodes is mandatory");
								return false;
							}
							tasks.NodesToInvestigate = new List<string>(File.ReadAllLines(args[++i]));
							break;
						case "--notifyMail":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --notifyMail is mandatory");
								return false;
							}
							tasks.mailNotification = args[++i];
							break;
						case "--nslimit":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --nslimit is mandatory");
								return false;
							}
							if (!int.TryParse(args[++i], out NullSessionScanner.NullSessionEnumerationLimit))
							{
								WriteInRed("argument for --nslimit is not a valid value (typically: 5)");
								return false;
							}
							break;
						case "--password":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --password is mandatory");
								return false;
							}
							password = args[++i];
							break;
						case "--port":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --port is mandatory");
								return false;
							}
							if (!int.TryParse(args[++i], out tasks.Port))
							{
								WriteInRed("argument for --port is not a valid value (typically: 9389)");
								return false;
							}
							break;
						case "--protocol":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --protocol is mandatory");
								return false;
							}
							try
							{
								ADWebService.ConnectionType = (ADConnectionType)Enum.Parse(typeof(ADConnectionType), args[++i]);
							}
							catch (Exception ex)
							{
								Trace.WriteLine(ex.Message);
								WriteInRed("Unable to parse the protocol [" + args[i] + "] to one of the predefined value (" + String.Join(",", Enum.GetNames(typeof(ADConnectionType))) + ")");
								return false;
							}
							break;
						case "--reachable":
							tasks.AnalyzeReachableDomains = true;
							break;
						case "--rules":
							PerformHCRules = true;
							break;
						case "--scanner":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --scanner is mandatory");
								return false;
							}
							{
								var scanners = PingCastleFactory.GetAllScanners();
								string scannername = args[++i];
								if (!scanners.ContainsKey(scannername))
								{
									string list = null;
									var allscanners = new List<string>(scanners.Keys);
									allscanners.Sort();
									foreach (string name in allscanners)
									{
										if (list != null)
											list += ",";
										list += name;
									}
									WriteInRed("Unsupported scannername - available scanners are:" + list);
								}
								tasks.Scanner = scanners[scannername];
								PerformScanner = true;
							}
							break;
						case "--scmode-single":
							ScannerBase.ScanningMode = 2;
							break;
						case "--sendxmlTo":
						case "--sendXmlTo":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --sendXmlTo is mandatory");
								return false;
							}
							tasks.sendXmlTo = args[++i];
							break;
						case "--sendhtmlto":
						case "--sendHtmlTo":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --sendHtmlTo is mandatory");
								return false;
							}
							tasks.sendHtmlTo = args[++i];
							break;
						case "--sendallto":
						case "--sendAllTo":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --sendAllTo is mandatory");
								return false;
							}
							tasks.sendAllTo = args[++i];
							break;
						case "--server":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --server is mandatory");
								return false;
							}
							tasks.Server = args[++i];
							break;
						case "--skip-null-session":
							HealthcheckAnalyzer.SkipNullSession = true;
							break;
						case "--reload-report":
						case "--slim-report":
							PerformHealthCheckReloadReport = true;
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --slim-report is mandatory");
								return false;
							}
							tasks.FileOrDirectory = args[++i];
							break;
						case "--smtplogin":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --smtplogin is mandatory");
								return false;
							}
							tasks.smtpLogin = args[++i];
							break;
						case "--smtppass":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --smtppass is mandatory");
								return false;
							}
							tasks.smtpPassword = args[++i];
							break;
						case "--smtptls":
							tasks.smtpTls = true;
							break;
						case "--upload-all-reports":
							PerformUploadAllReport = true;
							break;
						case "--user":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --user is mandatory");
								return false;
							}
							i++;
							if (args[i].Contains("\\"))
							{
								int pos = args[i].IndexOf('\\');
								userdomain = args[i].Substring(0, pos);
								user = args[i].Substring(pos + 1);
							}
							else
							{
								user = args[i];
							}
							break;
						case "--webdirectory":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --webdirectory is mandatory");
								return false;
							}
							tasks.sharepointdirectory = args[++i];
							break;
						case "--webuser":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --webuser is mandatory");
								return false;
							}
							tasks.sharepointuser = args[++i];
							break;
						case "--webpassword":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --webpassword is mandatory");
								return false;
							}
							tasks.sharepointpassword = args[++i];
							break;
						case "--xmls":
							if (i + 1 >= args.Length)
							{
								WriteInRed("argument for --xmls is mandatory");
								return false;
							}
							tasks.FileOrDirectory = args[++i];
							break;
						default:
							WriteInRed("unknow argument: " + args[i]);
							DisplayHelp();
							return false;
					}
				}
				Trace.WriteLine("After parsing arguments");
			}
			if (!PerformHealthCheckReport && !PerformHealthCheckConsolidation && !PerformAdvancedConsolidation
				&& !PerformRegenerateReport && !PerformAdvancedRegenerateReport && !PerformHealthCheckReloadReport && !delayedInteractiveMode
				&& !PerformScanner
				&& !PerformGenerateKey && !PerformHealthCheckGenerateDemoReports && !PerformCarto
				&& !PerformUploadAllReport
				&& !PerformHCRules
				&& !PerformGenerateFakeReport
				&& !PerformBot)
			{
				WriteInRed("You must choose at least one value among --healthcheck --hc-conso --advanced-hc-conso --advanced-export --advanced-report --nullsession --carto");
				DisplayHelp();
				return false;
			}
			Trace.WriteLine("Things to do OK");
			if (delayedInteractiveMode)
			{
				RunInteractiveMode();
			}
			if (PerformHealthCheckReport || PerformScanner)
			{
				if (String.IsNullOrEmpty(tasks.Server))
				{
					tasks.Server = GetCurrentDomain();
					if (String.IsNullOrEmpty(tasks.Server))
					{
						WriteInRed("This computer is not connected to a domain. The program couldn't guess the domain or server to connect.");
						WriteInRed("Please run again this program with the flag --server <my.domain.com> or --server <mydomaincontroller.my.domain.com>");
						DisplayHelp();
						return false;
					}
				}
				if (user != null)
				{
					if (password == null)
						password = AskCredential();
					if (String.IsNullOrEmpty(userdomain))
					{
						tasks.Credential = new NetworkCredential(user, password);
					}
					else
					{
						tasks.Credential = new NetworkCredential(user, password, userdomain);
					}
				}
			}
			if (PerformHealthCheckConsolidation)
			{
				if (String.IsNullOrEmpty(tasks.FileOrDirectory))
				{
					tasks.FileOrDirectory = Directory.GetCurrentDirectory();
				}
				else
				{
					if (!Directory.Exists(tasks.FileOrDirectory))
					{
						WriteInRed("The path specified by --xmls isn't a directory");
						DisplayHelp();
						return false;
					}
				}
			}
			return true;
		}
					{
						WriteInRed("The path specified by --xmls isn't a directory");
						DisplayHelp();
						return false;
					}
				}
			}
			return true;
		}

        private void EnableLogFile()
        {
            Trace.AutoFlush = true;
            TextWriterTraceListener listener = new TextWriterTraceListener("trace.log");
            Trace.Listeners.Add(listener);
        }

        private void EnableLogConsole()
        {
            Trace.AutoFlush = true;
            TextWriterTraceListener listener = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Add(listener);
        }

        private string AskCredential()
        {
            StringBuilder builder = new StringBuilder();
            Console.WriteLine("Enter password: ");
            ConsoleKeyInfo nextKey = Console.ReadKey(true);

            while (nextKey.Key != ConsoleKey.Enter)
            {
                if (nextKey.Key == ConsoleKey.Backspace)
                {
                    if (builder.Length > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        // erase the last * as well
                        Console.Write(nextKey.KeyChar);
                        Console.Write(" ");
                        Console.Write(nextKey.KeyChar);
                    }
                }
                else
                {
                    builder.Append(nextKey.KeyChar);
                    Console.Write("*");
                }
                nextKey = Console.ReadKey(true);
            }
            Console.WriteLine();
            return builder.ToString();
        }

		private enum DisplayState
		{
			Exit,
			MainMenu,
			ScannerMenu,
			AskForServer,
			Run,
			AvancedMenu,
			AskForScannerParameter,
			ProtocolMenu,
			AskForFile,
			AskForDir, // Addition
		}

        DisplayState DisplayMainMenu()
        {
            PerformHealthCheckReport = false;
            PerformCarto = false;
            PerformHealthCheckConsolidation = false;
            PerformScanner = false;

			List<ConsoleMenuItem> choices = new List<ConsoleMenuItem>() {
				new ConsoleMenuItem("healthcheck","Score the risk of a domain", "This is the main functionnality of RisX. In a matter of minutes, it produces a report which will give you an overview of your Active Directory security. This report can be generated on other domains by using the existing trust links."),
				new ConsoleMenuItem("conso","Aggregate multiple reports into a single one", "With many healthcheck reports, you can get a single report for a whole scope. Maps will be generated."),
				new ConsoleMenuItem("carto","Build a map of all interconnected domains", "It combines the healthcheck reports that would be run on all trusted domains and then the conso option. But lighter and then faster."),
				new ConsoleMenuItem("scanner","Perform specific security checks on workstations", "You can know your local admins, if Bitlocker is properly configured, discover unprotect shares, ... A menu will be shown to select the right scanner."),
				new ConsoleMenuItem("advanced","Open the advanced menu", "This is the place you want to configure RisX without playing with command line switches."),
			};

            ConsoleMenu.Title = "What do you want to do?";
            ConsoleMenu.Information = "Using interactive mode.\r\nDo not forget that there are other command line switches like --help that you can use";
            int choice = ConsoleMenu.SelectMenu(choices);
            if (choice == 0)
                return DisplayState.Exit;

            string whattodo = choices[choice - 1].Choice;
            switch (whattodo)
            {
                default:
                case "healthcheck":
                    PerformHealthCheckReport = true;
                    return DisplayState.AskForServer;
                case "carto":
                    PerformCarto = true;
                    return DisplayState.AskForServer;
                case "conso":
                    PerformHealthCheckConsolidation = true;
                    return DisplayState.Run;
                case "scanner":
                    PerformScanner = true;
                    return DisplayState.ScannerMenu;
                case "advanced":
                    return DisplayState.AvancedMenu;
            }
        }

        DisplayState DisplayScannerMenu()
        {
            var scanners = PingCastleFactory.GetAllScanners();

            var choices = new List<ConsoleMenuItem>();
            foreach (var scanner in scanners)
            {
                Type scannerType = scanner.Value;
                IScanner iscanner = PingCastleFactory.LoadScanner(scannerType);
                string description = iscanner.Description;
                choices.Add(new ConsoleMenuItem(scanner.Key, description));
            }
            choices.Sort((ConsoleMenuItem a, ConsoleMenuItem b)
                =>
                {
                    return String.Compare(a.Choice, b.Choice);
                }
            );
            ConsoleMenu.Notice = "WARNING: Checking a lot of workstations may raise security alerts.";
            ConsoleMenu.Title = "Select a scanner";
            ConsoleMenu.Information = "What scanner whould you like to run ?";
            int choice = ConsoleMenu.SelectMenuCompact(choices, 1);
            if (choice == 0)
                return DisplayState.Exit;
            tasks.Scanner = scanners[choices[choice - 1].Choice];
            return DisplayState.AskForScannerParameter;
        }

        DisplayState DisplayAskForScannerParameter()
        {
            IScanner iscannerAddParam = PingCastleFactory.LoadScanner(tasks.Scanner);
            if (!iscannerAddParam.QueryForAdditionalParameterInInteractiveMode())
                return DisplayState.Exit;
            return DisplayState.AskForServer;
        }

        DisplayState DisplayAskServer()
        {
            string defaultDomain = tasks.Server;
            if (String.IsNullOrEmpty(defaultDomain))
                defaultDomain = GetCurrentDomain();
            while (true)
            {
                if (!String.IsNullOrEmpty(defaultDomain) || string.Equals(defaultDomain, "(None)", StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleMenu.Information = "Please specify the domain or server to investigate (default:" + defaultDomain + ")";
                }
                else
                {
                    ConsoleMenu.Information = "Please specify the domain or server to investigate:";
                }
                ConsoleMenu.Title = "Select a domain or server";
                tasks.Server = ConsoleMenu.AskForString();
                if (String.IsNullOrEmpty(tasks.Server))
                {
                    tasks.Server = defaultDomain;
		DisplayState DisplayAdvancedMenu()
		{
			PerformGenerateKey = false;
			PerformHealthCheckReloadReport = false;
			PerformRegenerateReport = false;
			PerformAdvancedRegenerateReport = false;
			PerformAdvancedConsolidation = false;
			PerformHCRules = false;
            return DisplayState.Run;
			List<ConsoleMenuItem> choices = new List<ConsoleMenuItem>() {
				new ConsoleMenuItem("protocol","Change the protocol used to query the AD (LDAP, ADWS, ...)"),
				new ConsoleMenuItem("hcrules","Generate a report containing all rules applied by RisX"),
				new ConsoleMenuItem("generatekey","Generate RSA keys used to encrypt and decrypt reports"),
				new ConsoleMenuItem("noenumlimit","Remove the 100 items limitation in healthcheck reports"),
				new ConsoleMenuItem("decrypt","Decrypt a xml report"),
				new ConsoleMenuItem("regenerate","Regenerate the html report based on the xml report"),
				new ConsoleMenuItem("advanced regenerate","Advanced regenerate of the html report based on report and config xml files"),
				new ConsoleMenuItem("advanced conso","Aggregate multiple reports into a single one adding advance data to each one",
					"With many healthcheck reports, you can get a single report for a whole scope.Maps will be generated."),
				new ConsoleMenuItem("log","Enable logging (log is " + (Trace.Listeners.Count > 1 ? "enabled":"disabled") + ")"),
			};
			List<ConsoleMenuItem> choices = new List<ConsoleMenuItem>() {
				new ConsoleMenuItem("protocol","Change the protocol used to query the AD (LDAP, ADWS, ...)"),
				new ConsoleMenuItem("hcrules","Generate a report containing all rules applied by PingCastle"),
				new ConsoleMenuItem("generatekey","Generate RSA keys used to encrypt and decrypt reports"),
				new ConsoleMenuItem("noenumlimit","Remove the 100 items limitation in healthcheck reports"),
				new ConsoleMenuItem("decrypt","Decrypt a xml report"),
			string whattodo = choices[choice - 1].Choice;
			switch (whattodo)
			{
				default:
				case "protocol":
					return DisplayState.ProtocolMenu;
				case "hcrules":
					PerformHCRules = true;
					return DisplayState.Run;
				case "generatekey":
					PerformGenerateKey = true;
					return DisplayState.Run;
				case "decrypt":
					PerformHealthCheckReloadReport = true;
					return DisplayState.AskForFile;
				case "regenerate":
					PerformRegenerateReport = true;
					return DisplayState.AskForFile;
				case "advanced regenerate":
					PerformAdvancedRegenerateReport = true;
					return DisplayState.AskForFile;
				case "advanced conso":
					PerformAdvancedConsolidation = true;
					return DisplayState.AskForDir;
				case "log":
					if (Trace.Listeners.Count <= 1)
						EnableLogFile();
					return DisplayState.Exit;
				case "noenumlimit":
					ReportHealthCheckSingle.MaxNumberUsersInHtmlReport = int.MaxValue;
					ConsoleMenu.Notice = "Limitation removed";
					return DisplayState.Exit;
			}
		}
					if (Trace.Listeners.Count <= 1)
						EnableLogFile();
					return DisplayState.Exit;
				case "noenumlimit":
					ReportHealthCheckSingle.MaxNumberUsersInHtmlReport = int.MaxValue;
					ConsoleMenu.Notice = "Limitation removed";
					return DisplayState.Exit;
			}
		}

        DisplayState DisplayProtocolMenu()
        {
            List<ConsoleMenuItem> choices = new List<ConsoleMenuItem>() {
                new ConsoleMenuItem("ADWSThenLDAP","default: ADWS then if failed, LDAP"),
                new ConsoleMenuItem("ADWSOnly","use only ADWS"),
                new ConsoleMenuItem("LDAPOnly","use only LDAP"),
                new ConsoleMenuItem("LDAPThenADWS","LDAP then if failed, ADWS"),
            };

            ConsoleMenu.Title = "What protocol do you want to use?";
            ConsoleMenu.Information = "ADWS (Active Directory Web Service - tcp/9389) is the fastest protocol but is limited 5 sessions in parallele and a 30 minutes windows. LDAP is more stable but slower.\r\nCurrent protocol: [" + ADWebService.ConnectionType + "]";
            int defaultChoice = 1;
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].Choice == ADWebService.ConnectionType.ToString())
                    defaultChoice = 1 + i;
            }
		DisplayState DisplayAskForFile()
		{
			string file = null;
			while (String.IsNullOrEmpty(file) || !File.Exists(file))
			{
				ConsoleMenu.Title = "Select an existing report";
				ConsoleMenu.Information = "Please specify the report to open.";
				file = ConsoleMenu.AskForString();
				if(String.IsNullOrEmpty(file))
					ConsoleMenu.Notice = "The file " + file + " was not found";
			}
			tasks.FileOrDirectory = file;
			if (PerformAdvancedRegenerateReport)
            {
				file = null;
				while (String.IsNullOrEmpty(file) || !File.Exists(file))
				{
					ConsoleMenu.Title = "Select a config xml file";
					ConsoleMenu.Information = "Please specify the config to use.";
					file = ConsoleMenu.AskForString();
					ConsoleMenu.Notice = "The file " + file + " was not found";
				}
				tasks.CustomConfigFileOrDirectory = file;
			}
			tasks.EncryptReport = false;
			return DisplayState.Run;
		}
				ConsoleMenu.Title = "Select an existing report";
				ConsoleMenu.Information = "Please specify the report to open.";
				file = ConsoleMenu.AskForString();
				if(String.IsNullOrEmpty(file))
					ConsoleMenu.Notice = "The file " + file + " was not found";
			}
			tasks.FileOrDirectory = file;
			if (PerformAdvancedRegenerateReport)
            {
				file = null;
				while (String.IsNullOrEmpty(file) || !File.Exists(file))
				{
					ConsoleMenu.Title = "Select a config xml file";
					ConsoleMenu.Information = "Please specify the config to use.";
					file = ConsoleMenu.AskForString();
					ConsoleMenu.Notice = "The file " + file + " was not found";
				}
				tasks.CustomConfigFileOrDirectory = file;
			}
			tasks.EncryptReport = false;
			return DisplayState.Run;
		}

		DisplayState DisplayAskForDirectory()
		{
			string dir = null;
			if (PerformAdvancedConsolidation)
			{
				dir = null;
				while (String.IsNullOrEmpty(dir) || !Directory.Exists(dir))
				{
					ConsoleMenu.Title = "Select a config xml files directory";
					ConsoleMenu.Information = "Please specify the directory of the data xml files.";
					dir = ConsoleMenu.AskForString();
					ConsoleMenu.Notice = "The directory " + dir + " was not found";
				}
				tasks.AdvancedConsoDirectory = dir;
			}
			tasks.EncryptReport = false;
			return DisplayState.Run;
		}

		DisplayState DisplayAskForDirectory()
		{
			string dir = null;
			if (PerformAdvancedConsolidation)
			{
				dir = null;
				while (String.IsNullOrEmpty(dir) || !Directory.Exists(dir))
				{
					ConsoleMenu.Title = "Select a config xml files directory";
					ConsoleMenu.Information = "Please specify the directory of the data xml files.";
					dir = ConsoleMenu.AskForString();
					ConsoleMenu.Notice = "The directory " + dir + " was not found";
				}
				tasks.AdvancedConsoDirectory = dir;
			}
			tasks.EncryptReport = false;
			return DisplayState.Run;
		}

		// interactive interface
		private bool RunInteractiveMode()
		{
			tasks.InteractiveMode = true;
			Stack<DisplayState> states = new Stack<DisplayState>();
			var state = DisplayState.MainMenu;

			states.Push(state);
			while (states.Count > 0 && states.Peek() != DisplayState.Run)
			{
				switch (state)
				{
					case DisplayState.MainMenu:
						state = DisplayMainMenu();
						break;
					case DisplayState.ScannerMenu:
						state = DisplayScannerMenu();
						break;
					case DisplayState.AskForServer:
						state = DisplayAskServer();
						break;
					case DisplayState.AskForScannerParameter:
						state = DisplayAskForScannerParameter();
						break;
					case DisplayState.AvancedMenu:
						state = DisplayAdvancedMenu();
						break;
					case DisplayState.AskForFile:
						state = DisplayAskForFile();
						break;
					case DisplayState.AskForDir:
						state = DisplayAskForDirectory();
						break;
					case DisplayState.ProtocolMenu:
						state = DisplayProtocolMenu();
						break;
					default:
						// defensive programming
						if (state != DisplayState.Exit)
						{
							Console.WriteLine("No implementation of state " + state);
							state = DisplayState.Exit;
						}
						break;
				}
				if (state == DisplayState.Exit)
				{
					states.Pop();
					if (states.Count > 0)
						state = states.Peek();
				}
				else
				{
					states.Push(state);
				}
			}
			return (states.Count > 0);
		}

		private static void DisplayHelp()
		{
			Console.WriteLine("switch:");
			Console.WriteLine("  --help              : display this message");
			Console.WriteLine("  --interactive       : force the interactive mode");
			Console.WriteLine("  --log               : generate a log file");
			Console.WriteLine("  --log-console       : add log to the console");
			Console.WriteLine("  --log-samba <option>: enable samba login (example: 10)");
			Console.WriteLine("");
			Console.WriteLine("Common options when connecting to the AD");
			Console.WriteLine("  --server <server>   : use this server (default: current domain controller)");
			Console.WriteLine("                        the special value * or *.forest do the healthcheck for all domains");
			Console.WriteLine("  --port <port>       : the port to use for ADWS or LDPA (default: 9389 or 389)");
			Console.WriteLine("  --user <user>       : use this user (default: integrated authentication)");
			Console.WriteLine("  --password <pass>   : use this password (default: asked on a secure prompt)");
			Console.WriteLine("  --protocol <proto>  : selection the protocol to use among LDAP or ADWS (fastest)");
			Console.WriteLine("                      : ADWSThenLDAP (default), ADWSOnly, LDAPOnly, LDAPThenADWS");
			Console.WriteLine("");
			Console.WriteLine("  --carto             : perform a quick cartography with domains surrounding");
			Console.WriteLine("");
			Console.WriteLine("  --healthcheck       : perform the healthcheck (step1)");
			Console.WriteLine("    --add-data <xml> : add advanced data to scan result");
			Console.WriteLine("    --api-endpoint <> : upload report via api call eg: http://server");
			Console.WriteLine("    --api-key  <key>  : and using the api key as registered");
			Console.WriteLine("    --explore-trust   : on domains of a forest, after the healthcheck, do the hc on all trusted domains except domains of the forest and forest trusts");
			Console.WriteLine("    --explore-forest-trust : on root domain of a forest, after the healthcheck, do the hc on all forest trusts discovered");
			Console.WriteLine("    --explore-trust and --explore-forest-trust can be run together");
			Console.WriteLine("    --explore-exception <domains> : comma separated values of domains that will not be explored automatically");
			Console.WriteLine("");
			Console.WriteLine("    --encrypt         : use an RSA key stored in the .config file to crypt the content of the xml report");
			Console.WriteLine("    --level <level>   : specify the amount of data found in the xml file");
			Console.WriteLine("                      : level: Full, Normal, Light");
			Console.WriteLine("    --no-enum-limit   : remove the max 100 users limitation in html report");
			Console.WriteLine("    --reachable       : add reachable domains to the list of discovered domains");
			Console.WriteLine("    --sendXmlTo <emails>: send xml reports to a mailbox (comma separated email)");
			Console.WriteLine("    --sendHtmlTo <emails>: send html reports to a mailbox");
			Console.WriteLine("    --sendAllTo <emails>: send html reports to a mailbox");
			Console.WriteLine("    --notifyMail <emails>: add email notification when the mail is received");
			Console.WriteLine("    --smtplogin <user>: allow smtp credentials ...");
			Console.WriteLine("    --smtppass <pass> : ... to be entered on the command line");
			Console.WriteLine("    --smtptls         : enable TLS/SSL in SMTP if used on other port than 465 and 587");
			Console.WriteLine("    --skip-null-session: do not test for null session");
			Console.WriteLine("    --webdirectory <dir>: upload the xml report to a webdav server");
			Console.WriteLine("    --webuser <user>  : optional user and password");
			Console.WriteLine("    --webpassword <password>");
			Console.WriteLine("");
			Console.WriteLine("    --I-swear-I-paid-win7-support : meaningless");
			Console.WriteLine("");
			Console.WriteLine("--rules               : Generate an html containing all the rules used by RisX");
			Console.WriteLine("");
			Console.WriteLine("  --generate-key      : generate and display a new RSA key for encryption");
			Console.WriteLine("");
			Console.WriteLine("  --hc-conso          : consolidate multiple healthcheck xml reports (step2)");
			Console.WriteLine("  --advanced-hc-conso <data directory> : consolidate multiple healthcheck xml reports adding custom data by xml files");
			Console.WriteLine("    --center-on <domain> : center the simplified graph on this domain");
			Console.WriteLine("                         default is the domain with the most links");
			Console.WriteLine("    --xmls <path>     : specify the path containing xml (default: current directory)");
			Console.WriteLine("    --filter-date <date>: filter report generated after the date.");
			Console.WriteLine("");
			Console.WriteLine("  --regen-report <xml> : regenerate a html report based on a xml report");
			Console.WriteLine("  --advanced-regen-report <base:xml> <additions:xml> : regenerate a html report based on a xml report and a custom data xml file");
			Console.WriteLine("  --reload-report <xml> : regenerate a xml report based on a xml report");
			Console.WriteLine("                          any healthcheck switches (send email, ..) can be reused");
			Console.WriteLine("    --level <level>   : specify the amount of data found in the xml file");
			Console.WriteLine("                      : level: Full, Normal, Light (default: Normal)");
			Console.WriteLine("    --encrypt         : use an RSA key stored in the .config file to crypt the content of the xml report");
			Console.WriteLine("                        the absence of this switch on an encrypted report will produce a decrypted report");
			Console.WriteLine("");
			Console.WriteLine("  --graph             : perform the light compromise graph computation directly to the AD");
			Console.WriteLine("    --encrypt         : use an RSA key stored in the .config file to crypt the content of the xml report");
			Console.WriteLine("    --max-depth       : maximum number of relation to explore (default:30)");
			Console.WriteLine("    --max-nodes       : maximum number of node to include (default:1000)");
			Console.WriteLine("    --node <node>     : create a report based on a object");
			Console.WriteLine("                      : example: \"cn=name\" or \"name\"");
			Console.WriteLine("    --nodes <file>    : create x report based on the nodes listed on a file");
			Console.WriteLine("");
			Console.WriteLine("  --scanner <type>    : perform a scan on one of all computers of the domain (using --server)");
			var scanner = PingCastleFactory.GetAllScanners();
			var scannerNames = new List<string>(scanner.Keys);
			scannerNames.Sort();
			foreach (var scannerName in scannerNames)
			{
				Type scannerType = scanner[scannerName];
				IScanner iscanner = PingCastleFactory.LoadScanner(scannerType);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(iscanner.Name);
				Console.ResetColor();
				Console.WriteLine(iscanner.Description);
			}
			Console.WriteLine("  options for scanners:");
			Console.WriteLine("    --scmode-single   : force scanner to check one single computer");
            Console.WriteLine("    --scmode-workstation : force scanner to check workstations");
            Console.WriteLine("    --scmode-server   : force scanner to check servers");
            Console.WriteLine("    --scmode-dc       : force scanner to check dc");
            Console.WriteLine("    --nslimit <number>: Limit the number of users to enumerate (default: unlimited)");
            Console.WriteLine("    --foreigndomain <sid> : foreign domain targeted using its FQDN or sids");
            Console.WriteLine("                        Example of SID: S-1-5-21-4005144719-3948538632-2546531719");
            Console.WriteLine("");
            Console.WriteLine("  --upload-all-reports: use the API to upload all reports in the current directory");
            Console.WriteLine("    --api-endpoint <> : upload report via api call eg: http://server");
            Console.WriteLine("    --api-key  <key>  : and using the api key as registered");
            Console.WriteLine("                        Note: do not forget to set --level Full to send all the information available");
            Console.WriteLine("");

        }
    }


}


