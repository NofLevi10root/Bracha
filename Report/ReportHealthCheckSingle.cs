﻿//
// Copyright (c) Ping Castle. All rights reserved.
// https://www.pingcastle.com
//
// Licensed under the Non-Profit OSL. See LICENSE file in the project root for full license information.
//
using PingCastle.Data;
using PingCastle.Graph.Database;
using PingCastle.Healthcheck;
using PingCastle.Rules;
using PingCastle.template;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PingCastle.Addition;

namespace PingCastle.Report
{
    public class ReportHealthCheckSingle : ReportRiskControls<HealthcheckData>, IPingCastleReportUser<HealthcheckData>
    {

        protected HealthcheckData Report;
        public static int MaxNumberUsersInHtmlReport = 100;
        protected ADHealthCheckingLicense _license;

		public string GenerateReportFile(HealthcheckData report, ADHealthCheckingLicense license, string filename)
		{
			Report = report;
			CustomData = null;
			_license = license;
			report.InitializeReportingData();
            ReportID = GenerateUniqueID(report);
            Brand(license);
            return GenerateReportFile(filename);
        }

		public string GenerateReportFile(HealthcheckData report, ADHealthCheckingLicense license, string filename, CustomHealthCheckData customData)
		{
			Report = report;
			CustomData = customData;
			_license = license;
			report.InitializeReportingData();
			ReportID = GenerateUniqueID(report);
			Brand(license);
			return GenerateReportFile(filename);
		}

		private string GenerateUniqueID(IPingCastleReport report)
        {
            var s = report.Domain.DomainSID.Split('-');
            return GenerateUniqueID(report.Domain.DomainName, long.Parse(s[s.Length - 1]));
        }

		public string GenerateRawContent(HealthcheckData report, ADHealthCheckingLicense aDHealthCheckingLicense)
		{
			Report = report;
			CustomData = null;
			_license = aDHealthCheckingLicense;
			report.InitializeReportingData();
			sb.Length = 0;
			GenerateContent();
			return sb.ToString();
		}

        public string GenerateRawContent(HealthcheckData report)
        {
            return GenerateRawContent(report, null);
        }

		protected override void GenerateTitleInformation()
		{
			AddEncoded(Report.DomainFQDN);
			Add(" RisX ");
			Add(Report.GenerationDate.ToString("yyyy-MM-dd"));
		}


        protected override void ReferenceJSAndCSS()
        {
            AddStyle(TemplateManager.LoadDatatableCss());
            AddStyle(TemplateManager.LoadReportBaseCss());
            AddStyle(TemplateManager.LoadReportRiskControlsCss());
            AddScript(TemplateManager.LoadJqueryDatatableJs());
            AddScript(TemplateManager.LoadDatatableJs());
            AddStyle(TemplateManager.LoadVisCss());
            AddStyle(TemplateManager.LoadReportCompromiseGraphCss());
            AddScript(TemplateManager.LoadVisJs());
            AddScript(TemplateManager.LoadReportCompromiseGraphJs());
            AddScript(@"

$(function() {
      $(window).scroll(function() {
         if($(window).scrollTop() >= 70) {  
            $('.information-bar').removeClass('hidden');
            $('.information-bar').fadeIn('fast');
         }else{
            $('.information-bar').fadeOut('fast');
         }
      });
   });
$(document).ready(function(){
	$('table').not('.model_table').not('.nopaging').DataTable(
		{
			'paging': true,
			'searching': true,
			'lengthMenu': [[10, 25, 50, 100, 500, 1000, -1], [10, 25, 50, 100, 500, 1000, 'All']],
		}
	);
	$('table').not('.model_table').filter('.nopaging').DataTable(
		{
			'paging': false,
			'searching': false
		}
	);
	$('[data-toggle=""tooltip""]').tooltip({html: true, container: 'body'});
	$('[data-toggle=""popover""]').popover();

	$('.div_model').on('click', function (e) {
		$('.div_model').not(this).popover('hide');
	});


});
");
        }

		protected override void GenerateBodyInformation()
		{
			GenerateNavigation("HealthCheck report", Report.DomainFQDN, Report.GenerationDate);
			GenerateAbout(@"<p><strong>Generated with <a class=""hyperlink"" href=""https://10root.com"">10Root RisX</a> powered by <a class=""hyperlink"" href=""https://www.pingcastle.com"">Ping Castle</a> all rights reserved</strong></p>
<p>Open source components:</p>
<ul>
<li><a class=""hyperlink"" href=""https://getbootstrap.com/"">Bootstrap</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a class=""hyperlink"" href=""https://datatables.net/"">DataTables</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a class=""hyperlink"" href=""https://popper.js.org/"">Popper.js</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a class=""hyperlink"" href=""https://jquery.org"">JQuery</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a class=""hyperlink"" href=""http://visjs.org/"">vis.js</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
</ul>");

            Add(@"
<div id=""wrapper"" class=""container well"">
	<noscript>
		<div class=""alert alert-warning"">
			<p>RisX reports work best with Javascript enabled.</p>
		</div>
	</noscript>
<div class=""row""><div class=""col-lg-12""><h1>");
            Add(Report.DomainFQDN);
            Add(@" - Healthcheck analysis</h1>
			<h3>Date: ");
            Add(Report.GenerationDate.ToString("yyyy-MM-dd"));
            Add(@" - Engine version: ");
            Add(Report.EngineVersion);
            Add(@"</h3>
");
			Add(@"<div class=""alert alert-info"">
This report has been generated with 10Root RisX powered by the ");
			Add(String.IsNullOrEmpty(_license.Edition) ? "Basic" : _license.Edition);
			Add(@" Edition of PingCastle");
			if (!string.IsNullOrEmpty(_license.CustomerNotice))
			{
				Add(@"&nbsp;<i class=""info-mark d-print-none"" data-placement=""bottom"" data-toggle=""tooltip""");
				Add(@" title="""" data-original-title=""");
				AddEncoded(_license.CustomerNotice);
				Add(@""">?</i>.");
			}
			if (String.IsNullOrEmpty(_license.Edition))
			{
				Add(@"
<br><strong class='auditor'>Being part of a commercial package is forbidden</strong> (selling the information contained in the report).<br>
If you are an auditor, you MUST purchase an Auditor license to share the development effort.");
            }
            Add(@"</div>
");
            Add(@"</div></div>
");
            GenerateContent();
            Add(@"
</div>
");
        }

		protected void GenerateContent()
		{
			GenerateSection("Active Directory Indicators", () =>
			{
				AddParagraph("This section focuses on the core security indicators.<br>Locate the sub-process determining the score and fix some rules in that area to get a score improvement.");
				GenerateIndicators(Report, Report.AllRiskRules);
				if (CustomData != null)
					GenerateAdvancedRiskModelPanel(Report.RiskRules);
				else
					GenerateRiskModelPanel(Report.RiskRules);
			});
            GenerateSection("Maturity Level", GenerateMaturityInformation);
			GenerateSection("Stale Objects", () =>
			{
				GenerateSubIndicator("Stale Objects", Report.GlobalScore, Report.StaleObjectsScore, "It is about operations related to user or computer objects");
				GenerateIndicatorPanel("DetailStale", "Stale Objects rule details", RiskRuleCategory.StaleObjects, Report.RiskRules, Report.applicableRules);
			});
			GenerateSection("Privileged Accounts", () =>
			{
				GenerateSubIndicator("Privileged Accounts", Report.GlobalScore, Report.PrivilegiedGroupScore, "It is about administrators of the Active Directory");
				GenerateIndicatorPanel("DetailPrivileged", "Privileged Accounts rule details", RiskRuleCategory.PrivilegedAccounts, Report.RiskRules, Report.applicableRules);
			});
			GenerateSection("Trusts", () =>
			{
				GenerateSubIndicator("Trusts", Report.GlobalScore, Report.TrustScore, "It is about operations related to user or computer objects");
				GenerateIndicatorPanel("DetailTrusts", "Trusts rule details", RiskRuleCategory.Trusts, Report.RiskRules, Report.applicableRules);
			});
			GenerateSection("Anomalies analysis", () =>
			{
				GenerateSubIndicator("Anomalies", Report.GlobalScore, Report.AnomalyScore, "It is about specific security control points");
				GenerateIndicatorPanel("DetailAnomalies", "Anomalies rule details", RiskRuleCategory.Anomalies, Report.RiskRules, Report.applicableRules);
			});
			if(CustomData != null)
            {
				foreach (var category in CustomData.Categories)
				{
					GenerateSection(category.Name, () =>
					{
						GenerateSubIndicator(category.Name, Report.GlobalScore, category.Score, category.Explanation);
						GenerateAdvancedIndicatorPanel("Detail" + category.Id, category.Name + "rule details", category.Id);
					});
				}
			}
			GenerateSection("Domain Information", GenerateDomainInformation);
			GenerateSection("User Information", GenerateUserInformation);
			GenerateSection("Computer Information", GenerateComputerInformation);
			GenerateSection("Admin Groups", GenerateAdminGroupsInformation);
			GenerateSection("Control Paths Analysis", GenerateCompromissionGraphInformation);
			GenerateSection("Trusts details", GenerateTrustInformation);
			GenerateSection("Anomalies", GenerateAnomalyDetail);
			GenerateSection("Password Policies", GeneratePasswordPoliciesDetail);
			GenerateSection("GPO", GenerateGPODetail);
			if(CustomData != null)
            {
				foreach(var section in CustomData.InformationSections)
                {
                    if(section.Show)
					    GenerateSection(section.Id, section.Name, () =>  GenerateAdvancedCustomSection(section));
                }
            }
		}

        protected override void GenerateFooterInformation()
        {
        }



        #region maturity

        protected void GenerateMaturityInformation()
        {
            Add(@"<p>This section represents the maturity score (inspired from <a class=""hyperlink"" href='https://www.cert.ssi.gouv.fr/dur/CERTFR-2020-DUR-001/'>ANSSI</a>).</p>");
            if (string.IsNullOrEmpty(_license.Edition))
            {
                AddParagraph("This feature is reserved for customers who have <a class=\"hyperlink\" href='https://www.pingcastle.com/services/'>purchased a license</a>");
                return;
            }
            var data = GetCurrentMaturityLevel();
            Add("<div class='row'><div class='col-sm-6 my-auto'><p class='display-4'>Maturity Level:</span></div><div class='col-sm-6'>");
            for (int i = 1; i <= 5; i++)
            {
                if (Report.MaturityLevel == i)
                {
                    Add("<span class=\"display-1\">");
                }
                Add("<span class=\"badge grade-");
                Add(i);
                Add("\">");
                Add(i);
                Add("</span>");
                if (Report.MaturityLevel == i)
                {
                    Add("</span>");
                }
            }
            Add("</div></div>");

            Add(@"<p>Maturity levels:<p>
<ul>
    <li><span class='badge grade-1'>1</span> Critical weaknesses and misconfigurations pose an immediate threat to all hosted resources. Corrective actions should be taken as soon as possible;</li>
	<li><span class='badge grade-2'>2</span> Configuration and management weaknesses put all hosted resources at risk of a short-term compromise. Corrective actions should be carefully planned and implemented shortly;</li>
    <li><span class='badge grade-3'>3</span> The Active Directory infrastructure does not appear to have been weakened from what default installation settings provide;</li>
    <li><span class='badge grade-4'>4</span> The Active Directory infrastructure exhibits an enhanced level of security and management;</li>
    <li><span class='badge grade-5'>5</span> The Active Directory infrastructure correctly implements the latest state-of-the-art administrative model and security features.</li>
</ul>");

            Add("<div class='row'><div class='col-lg-12'>");
            Add("<div class='card-deck'>");
            for (int i = 1; i <= 5; i++)
            {
                Add("<div class='card'>");
                Add("<div class='card-body'>");
                Add("<h5 class='card-title'>");
                Add("<span class=\"badge grade-");
                Add(i);
                Add("\">");
                Add("Level ");
                Add(i);
                Add("</span>");
                Add("</h5>");
                if (data.ContainsKey(i))
                {
                    Add("<p class='card-text'>");
                    if (Report.MaturityLevel == i)
                        Add("<strong>");
                    Add(data[i].Count);
                    Add(" rule(s) matched");
                    if (Report.MaturityLevel == i)
                        Add("</strong>");
                    Add("</p>");
                }
                else
                {
                    Add("<p class='card-text'>No rule matched</p>");
                }
                Add("</div>");
                Add("</div>");
            }
            Add("</div>");
            Add("</div></div>");

            List<string> l = null;
            int nextLevel = 0;
            for (int i = Report.MaturityLevel + 1; i < 6; i++)
            {
                if (data.ContainsKey(i) && data[i].Count > 0)
                {
                    l = data[Report.MaturityLevel];
                    nextLevel = i;
                    break;
                }
            }
            if (nextLevel != 0)
            {
                Add("<p class='mt-2'>To reach ");
                Add("<span class=\"badge grade-");
                Add(nextLevel);
                Add("\">");
                Add("Level ");
                Add(nextLevel);
                Add("</span> you need to fix the following rules:</p>");
                GenerateAccordion("rulesmaturity", () =>
                {
                    SortedDictionary<int, List<object>> levelRules = new SortedDictionary<int, List<object>>(); // need to oppose the sorting
					foreach(var rule in Report.RiskRules)
                    {
                        if (l.Contains(rule.RiskId))
                        {
                            if (!levelRules.ContainsKey(rule.Points))
                                levelRules.Add(rule.Points, new List<object>());
                            levelRules[rule.Points].Add(rule);
                        }
                    }
					if(CustomData != null)
                    {
						foreach(var rule in CustomData.HealthRules)
                        {
							if (l.Contains(rule.RiskId))
                            {
                                if (!levelRules.ContainsKey(rule.Points))
                                    levelRules.Add(rule.Points, new List<object>());
                                levelRules[rule.Points].Add(rule);
                            }
                        }
                    }
                    //levelRules.Sort((HealthcheckRiskRule a, HealthcheckRiskRule b)
                    //    =>
                    //{
                    //    return -a.Points.CompareTo(b.Points);
                    //});

                    foreach (var listRule in levelRules)
                    {
                        foreach(var rule in listRule.Value)
                        {
                            if (rule is HealthcheckRiskRule)
                                GenerateIndicatorPanelDetail("maturity", rule as HealthcheckRiskRule);
                            else if (rule is CustomHealthCheckRiskRule)
                                GenerateAdvancedIndicatorPanelDetail("maturity", rule as CustomHealthCheckRiskRule);

                        }

                    }
						
                });
            }

        }

        private Dictionary<int, List<string>> GetCurrentMaturityLevel()
        {
            var output = new Dictionary<int, List<string>>();
            foreach (var rule in Report.RiskRules)
            {
                var hcrule = RuleSet<HealthcheckData>.GetRuleFromID(rule.RiskId);
                if (hcrule == null)
                {
                    continue;
                }
                int level = hcrule.MaturityLevel;
                if (!output.ContainsKey(level))
                    output[level] = new List<string>();
                output[level].Add(hcrule.RiskId);
            }
			if(CustomData != null)
            {
				foreach(var rule in CustomData.HealthRules)
                {
                    if(!CustomData.GetRiskRule(rule.RiskId, out var hcrule))
						continue;
					int level = hcrule.Maturity;
					if(!output.ContainsKey(level))
						output[level] = new List<string>();
					output[level].Add(hcrule.Id);
				}
            }
            return output;
        }
        #endregion



        #region domain info
        protected void GenerateDomainInformation()
        {
            bool checkRecycleBin = Report.version >= new Version(2, 7, 0, 0);
            AddAnchor("domaininformation");

            AddParagraph("This section shows the main technical characteristics of the domain.");

            AddBeginTable("Domain information", true);
            AddHeaderText("Domain");
            AddHeaderText("Netbios Name");
            AddHeaderText("Domain Functional Level");
            AddHeaderText("Forest Functional Level");
            AddHeaderText("Creation date");
            AddHeaderText("DC count");
            AddHeaderText("Schema version");
            if (checkRecycleBin)
            {
                AddHeaderText(@"Recycle Bin enabled");
            }
            CustomTable custTable = null;
            if (CustomData != null)
            {
                if(CustomData.GetTable("Domain information", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();
            AddBeginRow();
            if(custTable != null && custTable.GetKeyLinkedSection(Report.DomainFQDN, out var targetSection))
            {
                Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                Add(@""">");
                AddEncoded(Report.DomainFQDN);
                Add("</a></td>");

                AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                GenerateAdvancedCustomSection(targetSection);
                AddEndModal();
            }
            else
                AddCellText(Report.DomainFQDN);
            AddCellText(Report.NetBIOSName);
            AddCellText(ReportHelper.DecodeDomainFunctionalLevel(Report.DomainFunctionalLevel));
            AddCellText(ReportHelper.DecodeForestFunctionalLevel(Report.ForestFunctionalLevel));
            AddCellDate(Report.DomainCreation);
            AddCellNum(Report.NumberOfDC);
            AddCellText(ReportHelper.GetSchemaVersion(Report.SchemaVersion));
            if (checkRecycleBin)
            {
                if (Report.IsRecycleBinEnabled)
                {
                    AddCellText("TRUE");
                }
                else
                {
                    AddCellText("FALSE", true);
                }
            }
            if(custTable != null)
            {
                for (int i = 1; i < custTable.Columns.Count; i++)
                {
                    if (custTable.Columns[i].Values.ContainsKey(Report.DomainFQDN))
                        AddCellText(custTable.Columns[i].Values[Report.DomainFQDN]);
                    else
                        AddCellText("");
                }
            }
            AddEndRow();
            AddEndTable();
            if(CustomData != null)
            {
                if(CustomData.GetSection("DomainInformation", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }

        #endregion domain info

        #region user info

        void AddAccountCheckHeader(bool computerView)
        {
            AddHeaderText("Nb Enabled", "Indicates the number of accounts not set as disabled.");
            AddHeaderText("Nb Disabled", "Indicates the number of accounts set as disabled.");
            AddHeaderText("Nb Active", "Indicates the number of enabled accounts where at least one logon occured in the last 6 months.");
            AddHeaderText("Nb Inactive", "Indicates the number of enabled accounts without any logon during the last 6 months.");
            if (!computerView)
            {
                AddHeaderText("Nb Locked", "Indicates the number of enabled accounts set as locked.");
                AddHeaderText("Nb pwd never Expire", "Indicates the number of enabled accounts which have a password which never expires.");
            }
            AddHeaderText("Nb SidHistory", "Indicates the number of enabled accounts having the attribute SIDHistory set. This attributes indicates a foreign origin.");
            AddHeaderText("Nb Bad PrimaryGroup", "Indicates the number of enabled account whose the group set as primary is not the default one.");
            if (!computerView)
            {
                AddHeaderText("Nb Password not Req.", "Indicates the number of enabled accounts which have a flag set in useraccountcontrol allowing empty passwords.");
                AddHeaderText("Nb Des enabled.", "Indicates the number of enabled accounts allowing the unsafe DES algorithm for authentication.");
            }
            AddHeaderText("Nb unconstrained delegations", "Indicates the number of enabled accounts having been granted the right to impersonate any users without any restrictions.");
            AddHeaderText("Nb Reversible password", "Indicates the number of enabled accounts whose password can be retrieved in clear text using hacking tools.");
        }

        protected void GenerateUserInformation()
        {
            AddParagraph("This section gives information about the user accounts stored in the Active Directory");
            if (Report.ListHoneyPot != null && Report.ListHoneyPot.Count > 0)
            {
                GenerateSubSection("Honey Pot", "honeypot");
                AddParagraph("A honey pot has been configured. It is used to generate fake security issues that are heavily monitored and that a hacker will spot using security tools like PingCastle. By enabling this feature, all the accounts listed below will not be evaluated with PingCastle rules.");
                GenerateAccordion("honeypotaccordion", () => GenerateListAccountDetail("honeypotaccordion", "honeypotid", "Honey pot accounts", Report.ListHoneyPot));
            }
            GenerateSubSection("Account analysis", "useraccountanalysis");
            AddBeginTable("Account analysis list", true);
            AddHeaderText("Nb User Accounts");
            AddAccountCheckHeader(false);

            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Account analysis list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();
            AddBeginRow();
            if (custTable != null && custTable.GetKeyLinkedSection(Report.UserAccountData.Number.ToString(), out var targetSection))
            {
                Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                Add(@""">");
                AddEncoded(Report.UserAccountData.Number.ToString());
                Add("</a></td>");

                AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                GenerateAdvancedCustomSection(targetSection);
                AddEndModal();
            }
            else
                AddCellNum(Report.UserAccountData.Number);
            AddCellNum(Report.UserAccountData.NumberEnabled);
            AddCellNum(Report.UserAccountData.NumberDisabled);
            AddCellNum(Report.UserAccountData.NumberActive);
            SectionList("usersaccordion", "sectioninactiveuser", Report.UserAccountData.NumberInactive, Report.UserAccountData.ListInactive);
            SectionList("usersaccordion", "sectionlockeduser", Report.UserAccountData.NumberLocked, Report.UserAccountData.ListLocked);
            SectionList("usersaccordion", "sectionneverexpiresuser", Report.UserAccountData.NumberPwdNeverExpires, Report.UserAccountData.ListPwdNeverExpires);
            SectionList("usersaccordion", "sectionsidhistoryuser", Report.UserAccountData.NumberSidHistory, Report.UserAccountData.ListSidHistory);
            SectionList("usersaccordion", "sectionbadprimarygroupuser", Report.UserAccountData.NumberBadPrimaryGroup, Report.UserAccountData.ListBadPrimaryGroup);
            SectionList("usersaccordion", "sectionpwdnotrequireduser", Report.UserAccountData.NumberPwdNotRequired, Report.UserAccountData.ListPwdNotRequired);
            SectionList("usersaccordion", "sectiondesenableduser", Report.UserAccountData.NumberDesEnabled, Report.UserAccountData.ListDesEnabled);
            SectionList("usersaccordion", "sectiontrusteddelegationuser", Report.UserAccountData.NumberTrustedToAuthenticateForDelegation, Report.UserAccountData.ListTrustedToAuthenticateForDelegation);
            SectionList("usersaccordion", "sectionreversiblenuser", Report.UserAccountData.NumberReversibleEncryption, Report.UserAccountData.ListReversibleEncryption);
            if (custTable != null)
            {
                for (int i = 1; i < custTable.Columns.Count; i++)
                {
                    if (custTable.Columns[i].Values.ContainsKey(Report.UserAccountData.Number.ToString()))
                        AddCellText(custTable.Columns[i].Values[Report.UserAccountData.Number.ToString()]);
                    else
                        AddCellText("");
                }
            }
            AddEndRow();
            AddEndTable();
            GenerateListAccount(Report.UserAccountData, "user", "usersaccordion");
            if (Report.PasswordDistribution != null && Report.PasswordDistribution.Count > 0)
            {
                GenerateSubSection("Password Age Distribution", "passworddistribution");
                if (string.IsNullOrEmpty(_license.Edition))
                {
                    AddParagraph("This feature is reserved for customers who have <a class=\"hyperlink\" href='https://www.pingcastle.com/services/'>purchased a license</a>");
                }
                else
                {
                    AddParagraph("Here is the distribution where the password has been changed for the last time. Only enabled user accounts are analyzed (no guest account for example).");
                    AddPasswordDistributionChart(Report.PasswordDistribution, "general");
                }
            }
            GenerateDomainSIDHistoryList(Report.UserAccountData);
            if (CustomData != null)
            {
                if (CustomData.GetSection("UserInformation", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }

        private void AddPasswordDistributionChart(List<HealthcheckPwdDistributionData> input, string id, Dictionary<int, string> tooltips = null)
        
		{
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            var data = new SortedDictionary<int, int>();
            int highest = 0;
            int max = 0;
            const int division = 36;
            const double horizontalStep = 25;
            foreach (var entry in input)
            {
                data.Add(entry.HigherBound, entry.Value);
                if (highest < entry.HigherBound)
                    highest = entry.HigherBound;
                if (max < entry.Value)
                    max = entry.Value;
            }
            // add missing data
            if (max > 10000)
                max = 10000;
            else if (max >= 5000)
                max = 10000;
            else if (max >= 1000)
                max = 5000;
            else if (max >= 500)
                max = 1000;
            else if (max >= 100)
                max = 500;
            else if (max >= 50)
                max = 100;
            else if (max >= 10)
                max = 50;
            else
                max = 10;

            int other = 0;
            for (int i = 0; i < division; i++)
            {
                if (!data.ContainsKey(i))
                    data[i] = 0;
            }
            for (int i = division; i <= highest; i++)
            {
                if (data.ContainsKey(i))
                    other += data[i];
            }
            Add(@"<div id='pdwdistchart");
            Add(id);
            Add(@"'><svg viewBox='0 0 1000 400'>");
            Add(@"<g transform=""translate(40,20)"">");
            // horizontal scale
            Add(@"<g transform=""translate(0,290)"" fill=""none"" font-size=""10"" font-family=""sans-serif"" text-anchor=""middle"">");
            Add(@"<path class=""domain"" stroke=""#000"" d=""M0.5,0V0.5H950V0""></path>");
            for (int i = 0; i < division; i++)
            {
                double v = 13.06 + (i) * horizontalStep;
                Add(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">" +
                    (i * 30) + "-" + ((i + 1) * 30) + @" days</text></g>");
            }
            {
                double v = 13.06 + (division) * horizontalStep;
                Add(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">Other</text></g>");
            }
            Add(@"</g>");
            // vertical scale
            Add(@"<g fill=""none"" font-size=""10"" font-family=""sans-serif"" text-anchor=""end"">");
            Add(@"<path class=""domain"" stroke=""#000"" d=""M-6,290.5H0.5V0.5H-6""></path>");
            for (int i = 0; i < 6; i++)
            {
                double v = 290 - i * 55;
                Add(@"<g class=""tick"" opacity=""1"" transform=""translate(0," + v.ToString(nfi) + @")""><line stroke=""#000"" x2=""-6""></line><text fill=""#000"" x=""-9"" dy=""0.32em"">" +
                    (max / 5 * i) + @"</text></g>");
            }
            Add(@"</g>");
            // bars
            for (int i = 0; i < division; i++)
            {
                double v = 3.28 + horizontalStep * (i);
                int value = 0;
                if (data.ContainsKey(i))
                    value = data[i];
                double size = 290 * value / max;
                if (size > 290) size = 290;
                double w = horizontalStep - 3;
                string tooltip = value.ToString();
                if (tooltips != null && tooltips.ContainsKey(i))
                    tooltip = tooltips[i];
                Add(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                AddEncoded(tooltip);
                Add(@"""></rect>");
            }
            {
                double v = 3.28 + horizontalStep * (division);
                int value = 0;
                value = other;
                double size = 290 * value / max;
                if (size > 290) size = 290;
                double w = horizontalStep - 3;
                string tooltip = string.Empty;
                if (tooltips != null)
                {
                    foreach (var t in tooltips)
                    {
                        if (t.Key > division)
                            tooltip += t.Value + "\r\n";
                    }
                }
                if (string.IsNullOrEmpty(tooltip))
                    tooltip = value.ToString();
                Add(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                AddEncoded(tooltip);
                Add(@"""></rect>");
            }
            Add(@"</g></svg></div>");
        }

        private void GenerateListAccount(HealthcheckAccountData data, string root, string accordion)
        {
            GenerateAccordion(accordion,
                () =>
                {
                    if (data.ListInactive != null && data.ListInactive.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectioninactive" + root, "Inactive objects (Last usage > 6 months) ", data.ListInactive);
                    }
                    if (data.ListLocked != null && data.ListLocked.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionlocked" + root, "Locked objects ", data.ListLocked);
                    }
                    if (data.ListPwdNeverExpires != null && data.ListPwdNeverExpires.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionneverexpires" + root, "Objects with a password which never expires ", data.ListPwdNeverExpires);
                    }
                    if (data.ListSidHistory != null && data.ListSidHistory.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionsidhistory" + root, "Objects having the SIDHistory populated ", data.ListSidHistory);
                    }
                    if (data.ListBadPrimaryGroup != null && data.ListBadPrimaryGroup.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionbadprimarygroup" + root, "Objects having the primary group attribute changed ", data.ListBadPrimaryGroup);
                    }
                    if (data.ListPwdNotRequired != null && data.ListPwdNotRequired.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionpwdnotrequired" + root, "Objects which can have an empty password ", data.ListPwdNotRequired);
                    }
                    if (data.ListDesEnabled != null && data.ListDesEnabled.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectiondesenabled" + root, "Objects which can use DES in kerberos authentication ", data.ListDesEnabled);
                    }
                    if (data.ListTrustedToAuthenticateForDelegation != null && data.ListTrustedToAuthenticateForDelegation.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectiontrusteddelegation" + root, "Objects trusted to authenticate for delegation ", data.ListTrustedToAuthenticateForDelegation);
                    }
                    if (data.ListReversibleEncryption != null && data.ListReversibleEncryption.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionreversible" + root, "Objects having a reversible password ", data.ListReversibleEncryption);
                    }
                    if (data.ListDuplicate != null && data.ListDuplicate.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionduplicate" + root, "Objects being duplicates ", data.ListDuplicate);
                    }
                    if (data.ListNoPreAuth != null && data.ListNoPreAuth.Count > 0)
                    {
                        GenerateListAccountDetail(accordion, "sectionnopreauth" + root, "Objects without kerberos preauthentication ", data.ListNoPreAuth);
                    }
                });
        }

        void SectionList(string accordion, string section, int value, List<HealthcheckAccountDetailData> list)
        {
            if (value > 0 && list != null && list.Count > 0)
            {
                Add(@"<td class='num'><a data-toggle=""collapse"" href=""#");
                Add(section);
                Add(@""" data-parent=""#");
                Add(accordion);
                Add(@""">");
                Add(value);
                Add(@"</a></td>");
            }
            else
            {
                AddCellNum(value);
            }
        }

        void GenerateListAccountDetail(string accordion, string id, string title, List<HealthcheckAccountDetailData> list)
        {
            if (list == null)
            {
                return;
            }
            bool eventDate = false;
            foreach (var u in list)
            {
                if (u.Event != DateTime.MinValue)
                {
                    eventDate = true;
                    break;
                }
            }
            GenerateAccordionDetail(id, accordion, title, list.Count, false, () =>
                {
                    AddBeginTable("Account list");
                    AddHeaderText("Name");
                    AddHeaderText("Creation");
                    AddHeaderText("Last logon");
                    if (eventDate)
                    {
                        AddHeaderText("Event date");
                    }
                    AddHeaderText("Distinguished name");
                    AddBeginTableData();

                    int number = 0;
                    list.Sort((HealthcheckAccountDetailData a, HealthcheckAccountDetailData b)
                        =>
                        {
                            return String.Compare(a.Name, b.Name);
                        }
                        );
                    foreach (HealthcheckAccountDetailData detail in list)
                    {
                        AddBeginRow();
                        AddCellText(detail.Name);
                        AddCellText((detail.CreationDate > DateTime.MinValue ? detail.CreationDate.ToString("u") : "Access Denied"));
                        AddCellText((detail.LastLogonDate > DateTime.MinValue ? detail.LastLogonDate.ToString("u") : "Never"));
                        if (eventDate)
                        {
                            if (detail.Event == DateTime.MinValue)
                            {
                                AddCellText("Unknown");
                            }
                            else
                            {
                                AddCellText(detail.Event.ToString("u"));
                            }
                        }
                        AddCellText(detail.DistinguishedName);
                        AddEndRow();
                        number++;
                        if (number >= MaxNumberUsersInHtmlReport)
                        {
                            break;
                        }
                    }
                    AddEndTable(() =>
                    {
                        if (number >= MaxNumberUsersInHtmlReport)
                        {
                            Add("<td colspan='");
                            Add(eventDate ? 5 : 4);
                            Add("' class='text'>Output limited to ");
                            Add(MaxNumberUsersInHtmlReport);
                            Add(" items - go to the advanced menu before running the report or add \"--no-enum-limit\" to remove that limit</td>");
                        }
                    });
                });
        }

        private void GenerateDomainSIDHistoryList(HealthcheckAccountData data)
        {
            if (data.ListDomainSidHistory == null || data.ListDomainSidHistory.Count == 0)
                return;

            GenerateSubSection("SID History", "sidhistory");
            AddBeginTable("SID History list");
            AddHeaderText("SID History from domain");
            AddHeaderText("First date seen", "This is the oldest creation date of an object having SIDHistory related to this domain");
            AddHeaderText("Last date seen", "This is the youngest creation date of an object having SIDHistory related to this domain");
            AddHeaderText("Count");
            if (Report.version >= new Version(2, 9))
            {
                AddHeaderText("Dangerous SID Found");
            }
            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("SID History list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();

            data.ListDomainSidHistory.Sort(
                (HealthcheckSIDHistoryData x, HealthcheckSIDHistoryData y) =>
                {
                    return String.Compare(x.FriendlyName, y.FriendlyName);
                }
                );
            foreach (HealthcheckSIDHistoryData domainSidHistory in data.ListDomainSidHistory)
            {
                AddBeginRow();
                if (custTable != null && custTable.GetKeyLinkedSection(domainSidHistory.FriendlyName, out var targetSection))
                {
                    Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                    Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                    Add(@""">");
                    AddEncoded(domainSidHistory.FriendlyName);
                    Add("</a></td>");

                    AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                    GenerateAdvancedCustomSection(targetSection);
                    AddEndModal();
                }
                else
                    AddCellText(domainSidHistory.FriendlyName);
                AddCellDate(domainSidHistory.FirstDate);
                AddCellDate(domainSidHistory.LastDate);
                AddCellNum(domainSidHistory.Count);
                if (Report.version >= new Version(2, 9))
                {
                    AddCellText(domainSidHistory.DangerousSID.ToString());
                }
                if (custTable != null)
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (custTable.Columns[i].Values.ContainsKey(domainSidHistory.FriendlyName))
                            AddCellText(custTable.Columns[i].Values[domainSidHistory.FriendlyName]);
                        else
                            AddCellText("");
                    }
                    
                }
                AddEndRow();
            }
            AddEndTable();
        }

        #endregion user info
        #region computer info
        protected void GenerateComputerInformation()
        {
            GenerateSubSection("Account analysis", "computeraccountanalysis");
            AddParagraph("This section gives information about the computer accounts stored in the Active Directory");
            AddBeginTable("Computer information list", true);
            AddHeaderText("Nb Computer Accounts");
            AddAccountCheckHeader(true);

            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Computer information list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();

            AddBeginRow();
            if (custTable != null && custTable.GetKeyLinkedSection(Report.ComputerAccountData.Number.ToString(), out var targetSection))
            {
                Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                Add(@""">");
                AddEncoded(Report.ComputerAccountData.Number.ToString());
                Add("</a></td>");

                AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                GenerateAdvancedCustomSection(targetSection);
                AddEndModal();
            }
            else
                AddCellNum(Report.ComputerAccountData.Number);
            AddCellNum(Report.ComputerAccountData.NumberEnabled);
            AddCellNum(Report.ComputerAccountData.NumberDisabled);
            AddCellNum(Report.ComputerAccountData.NumberActive);
            SectionList("computersaccordion", "sectioninactivecomputer", Report.ComputerAccountData.NumberInactive, Report.ComputerAccountData.ListInactive);
            SectionList("computersaccordion", "sectionsidhistorycomputer", Report.ComputerAccountData.NumberSidHistory, Report.ComputerAccountData.ListSidHistory);
            SectionList("computersaccordion", "sectionbadprimarygroupcomputer", Report.ComputerAccountData.NumberBadPrimaryGroup, Report.ComputerAccountData.ListBadPrimaryGroup);
            SectionList("computersaccordion", "sectiontrusteddelegationcomputer", Report.ComputerAccountData.NumberTrustedToAuthenticateForDelegation, Report.ComputerAccountData.ListTrustedToAuthenticateForDelegation);
            SectionList("computersaccordion", "sectionreversiblencomputer", Report.ComputerAccountData.NumberReversibleEncryption, Report.ComputerAccountData.ListReversibleEncryption);
            if (custTable != null)
            {
                for (int i = 1; i < custTable.Columns.Count; i++)
                {
                    if (custTable.Columns[i].Values.ContainsKey(Report.ComputerAccountData.Number.ToString()))
                        AddCellText(custTable.Columns[i].Values[Report.ComputerAccountData.Number.ToString()]);
                    else
                        AddCellText("");
                }
            }
            AddEndRow();
            AddEndTable();

            GenerateListAccount(Report.ComputerAccountData, "computer", "computersaccordion");
            GenerateOperatingSystemList();
            GenerateDomainSIDHistoryList(Report.ComputerAccountData);
            GenerateDCInformation();
            if (CustomData != null)
            {
                if (CustomData.GetSection("ComputerInformation", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }

		private void GenerateOperatingSystemList()
		{
			GenerateSubSection("Operating Systems", "operatingsystems");
			bool oldOS = Report.version <= new Version(2, 5, 0, 0);
			if (oldOS)
			{
				AddBeginTable("Operating System list");
				AddHeaderText("Operating System");
				AddHeaderText("Count");
                CustomTable custTable = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Operating System list", out custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();
				Report.OperatingSystem.Sort(
					(HealthcheckOSData x, HealthcheckOSData y) =>
					{
						return OrderOS(x.OperatingSystem, y.OperatingSystem);
					}
					);
				{
					foreach (HealthcheckOSData os in Report.OperatingSystem)
					{
						AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(os.OperatingSystem, out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(os.OperatingSystem);
                            Add("</a></td>");

                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddCellText(os.OperatingSystem);
						AddCellNum(os.NumberOfOccurence);
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(os.OperatingSystem))
                                    AddCellText(custTable.Columns[i].Values[os.OperatingSystem]);
                                else
                                    AddCellText("");
                            }
                            
                        }
                        AddEndRow();
					}
				}
				AddEndTable();
			}
			else
			{
				AddBeginTable("Operating System list");
				AddHeaderText("Operating System");
				AddHeaderText("Nb OS");
				AddAccountCheckHeader(true);

				AddBeginTableData();
                CustomTable custTable = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Operating System list", out custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                Report.OperatingSystem.Sort(
					(HealthcheckOSData x, HealthcheckOSData y) =>
					{
						return OrderOS(x.OperatingSystem, y.OperatingSystem);
					}
					);
				{
					foreach (HealthcheckOSData os in Report.OperatingSystem)
					{
						AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(os.OperatingSystem, out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(os.OperatingSystem);
                            Add("</a></td>");

                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddCellText(os.OperatingSystem);
						AddCellNum(os.data.Number);
						AddCellNum(os.data.NumberEnabled);
						AddCellNum(os.data.NumberDisabled);
						AddCellNum(os.data.NumberActive);
						AddCellNum(os.data.NumberInactive);
						AddCellNum(os.data.NumberSidHistory);
						AddCellNum(os.data.NumberBadPrimaryGroup);
						AddCellNum(os.data.NumberTrustedToAuthenticateForDelegation);
						AddCellNum(os.data.NumberReversibleEncryption);
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(os.OperatingSystem))
                                    AddCellText(custTable.Columns[i].Values[os.OperatingSystem]);
                                else
                                    AddCellText("");
                            }
                            
                        }
                        AddEndRow();
					}
				}
				AddEndTable();
			}
		}
		private void GenerateAdvancedCustomSection(CustomInformationSection section)
        {
			foreach(var child in section.Children)
            {
                switch(child.Type)
                {
                    case CustomSectionChildType.Table:
                        if(CustomData.GetTable(child.Id, out var table))
                        {
                            AddBeginTable(table.Id);

                            foreach (var col in table.Columns)
                            {
                                if(!string.IsNullOrEmpty(col.Tooltip))
                                    AddHeaderText(col.Header, col.Tooltip);
                                else
                                AddHeaderText(col.Header);
                            }
                            AddBeginTableData();
                            foreach(var key in table.Keys)
                            {
                                AddBeginRow();
                                for(int i = 0; i < table.Columns.Count; i++)
                                {
                                    if(i == 0 && table.GetKeyLinkedSection(key, out var targetSection))
                                    {
                                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                                        Add(@""">");
                                        AddEncoded(key);
                                        Add("</a></td>");

                                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                                        GenerateAdvancedCustomSection(targetSection);
                                        AddEndModal();
                                    }
                                    else
                                    {
                                        if (table.Columns[i].Values.ContainsKey(key))
                                            AddCellText(table.Columns[i].Values[key]);
                                        else
                                            AddCellText("");
                                    }
                                }
                                AddEndRow();
                            }
                            AddEndTable();
                        }
                        break;
                    case CustomSectionChildType.Chart:
                        if (CustomData.GetChart(child.Id, out var chart))
                            Add(chart.GetChartString());
                        break;
                    case CustomSectionChildType.Paragraph:
                        if(!string.IsNullOrEmpty(child.Value))
                            AddParagraph(child.Value);
                        break;
                    case CustomSectionChildType.SubSectionTitle:
                        if (!string.IsNullOrEmpty(child.Value))
                        {
                            GenerateSubSection(child.Value);
                        }
                        break;
                    case CustomSectionChildType.Modal:
                        if (!string.IsNullOrEmpty(child.Id) && !string.IsNullOrEmpty(child.Value) && child.Id != section.Id)
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(child.Id));
                            Add(@""">");
                            AddEncoded(child.Value);
                            Add("</a></td>");

                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(child.Id), child.Value, ShowModalType.XL);
                            if(CustomData.GetSection(child.Id, out var modalSection))
                            {
                                GenerateAdvancedCustomSection(modalSection);
                            }
                            AddEndModal();
                        }
                        break;
                }
            }
		}

        private void GenerateDCInformation()
        {
            if (Report.DomainControllers == null || Report.DomainControllers.Count == 0)
                return;

            GenerateSubSection("Domain controllers", "domaincontrollersection");
            AddParagraph("Here is a specific zoom related to the Active Directory servers: the domain controllers.");
            GenerateAccordion("domaincontrollers", ()
                =>
                {
                    GenerateAccordionDetail("domaincontrollersdetail", "domaincontrollers", "Domain controllers", Report.DomainControllers.Count, false,
                        () =>
                        {
                            AddBeginTable("Domain Controllers list");
                            AddHeaderText("Domain controller");
                            AddHeaderText("Operating System");
                            AddHeaderText("Creation Date", "Indicates the creation date of the underlying computer object.");
                            AddHeaderText("Startup Time");
                            AddHeaderText("Uptime");
                            AddHeaderText("Owner", "This is the owner of the underlying domain controller object stored in the active directory partition. The nTSecurityDescriptor attribute stores its value.");
                            AddHeaderText("Null sessions", "Indicates if an anonymous user can extract information from the domain controller");
                            AddHeaderText("SMB v1", "Indicates if the domain controller supports this unsafe SMB v1 network protocol.");
                            if (Report.version >= new Version(2, 5, 3))
                            {
                                AddHeaderText("Remote spooler", "Indicates if the spooler service is remotely accessible.");
                            }
                            if (Report.version >= new Version(2, 7))
                            {
                                AddHeaderText("FSMO role", "Flexible Single Master Operation. Indicates the server responsible for each role.");
                            }
                            CustomTable custTable = null;
                            if (CustomData != null)
                            {
                                if (CustomData.GetTable("Domain Controllers list", out custTable))
                                {
                                    for (int i = 1; i < custTable.Columns.Count; i++)
                                    {
                                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                                        else
                                            AddHeaderText(custTable.Columns[i].Header);
                                    }
                                }
                            }
                            AddBeginTableData();

                            int count = 0;
                            foreach (var dc in Report.DomainControllers)
                            {
                                count++;
                                AddBeginRow();
                                if (custTable != null && custTable.GetKeyLinkedSection(dc.DCName, out var targetSection))
                                {
                                    Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                                    Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                                    Add(@""">");
                                    AddEncoded(dc.DCName);
                                    Add("</a></td>");

                                    AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                                    GenerateAdvancedCustomSection(targetSection);
                                    AddEndModal();
                                }
                                else
                                    AddCellText(dc.DCName);
                                AddCellText(dc.OperatingSystem);
                                AddCellText((dc.CreationDate == DateTime.MinValue ? "Unknown" : dc.CreationDate.ToString("u")));
                                AddCellText((dc.StartupTime == DateTime.MinValue ? (dc.LastComputerLogonDate.AddDays(60) < DateTime.Now ? "Inactive?" : "Unknown") : (dc.StartupTime.AddMonths(6) < DateTime.Now ? /*"<span class='unticked'>" + */dc.StartupTime.ToString("u")/* + "</span>"*/ : dc.StartupTime.ToString("u"))));
                                AddCellText((dc.StartupTime == DateTime.MinValue ? "" : (DateTime.Now.Subtract(dc.StartupTime)).Days + " days"));
                                AddCellText((String.IsNullOrEmpty(dc.OwnerName) ? dc.OwnerSID : dc.OwnerName));
                                AddCellText((dc.HasNullSession ? "YES" : "NO"), true, !dc.HasNullSession);
                                AddCellText((dc.SupportSMB1 ? "YES" : "NO"), true, !dc.SupportSMB1);

                                if (Report.version >= new Version(2, 5, 3))
                                {
                                    AddCellText((dc.RemoteSpoolerDetected ? "YES" : "NO"), true, !dc.RemoteSpoolerDetected);
                                }
                                if (Report.version >= new Version(2, 7))
                                {
                                    Add(@"<Td>");
                                    if (dc.FSMO != null)
                                    {
                                        Add(string.Join(",<br>", dc.FSMO.ConvertAll(x => ReportHelper.Encode(x)).ToArray()));
                                    }
                                    Add("</Td>");
                                }
                                if (custTable != null)
                                {
                                    for (int i = 1; i < custTable.Columns.Count; i++)
                                    {
                                        if (custTable.Columns[i].Values.ContainsKey(dc.DCName))
                                            AddCellText(custTable.Columns[i].Values[dc.DCName]);
                                        else
                                            AddCellText("");
                                    }
                                    
                                }
                                AddEndRow();
                            }
                            AddEndTable();
                        }
                    );
                }
            );

        }


        #endregion computer info

        #region admin groups
        protected void GenerateAdminGroupsInformation()
        {
            if (Report.PrivilegedGroups != null)
            {
                GenerateSubSection("Groups", "admingroups");
                AddParagraph("This section is focused on the groups which are critical for admin activities. If the report has been saved which the full details, each group can be zoomed with its members. If it is not the case, for privacy reasons, only general statictics are available.");
                AddBeginTable("Admin groups list");
                AddHeaderText("Group Name");
                AddHeaderText("Nb Admins", "This is the number of user accounts member of this group");
                AddHeaderText("Nb Enabled", "This is the number of user accounts not marked as disabled");
                AddHeaderText("Nb Disabled", "This is the number of user accounts marked as disabled");
                AddHeaderText("Nb Inactive", "This is the number of enabled user accounts without login activities far at least 6 months");
                AddHeaderText("Nb PWd never expire", "This is the number of enabled user accounts having a password marked as never expire");
                if (Report.version >= new Version(2, 5, 2))
                {
                    AddHeaderText("Nb Smart Card required", "This is the number of enabled user accounts required to have a smart card");
                }
                if (Report.version >= new Version(2, 5, 3))
                {
                    AddHeaderText("Nb Service accounts", "This is the number of enabled user accounts authorized to be a service. This is defined by setting the attribute servicePrincipalName.");
                }
                AddHeaderText("Nb can be delegated", "This is the number of enabled user accounts which doesn't have the flag 'this account is sensitive and cannot be delegated'. This is an effective mitigation against unconstrained delegation attacks.");
                AddHeaderText("Nb external users", "This is the number of item identified as coming from a foreign domain");
                if (Report.version >= new Version(2, 9))
                {
                    AddHeaderText("Nb protected users", "This is the number of users in the Protected Users group");
                }
                CustomTable custTable = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Admin groups list", out custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();

                Report.PrivilegedGroups.Sort((HealthCheckGroupData a, HealthCheckGroupData b)
                    =>
                    {
                        return String.Compare(a.GroupName, b.GroupName);
                    }
                );
                foreach (HealthCheckGroupData group in Report.PrivilegedGroups)
                {
                    AddBeginRow();
                    if (custTable != null && custTable.GetKeyLinkedSection(group.GroupName, out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(group.GroupName);
                        Add("</a></td>");

                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else if (group.Members != null && group.Members.Count > 0)
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(group.GroupName));
                        Add(@""">");
                        AddEncoded(group.GroupName);
                        Add("</a></td>");
                    }
                    else
                    {
                        AddCellText(group.GroupName);
                    }
                    AddCellNum(group.NumberOfMember);
                    AddCellNum(group.NumberOfMemberEnabled);
                    AddCellNum(group.NumberOfMemberDisabled);
                    AddCellNum(group.NumberOfMemberInactive);
                    AddCellNum(group.NumberOfMemberPwdNeverExpires);
                    if (Report.version >= new Version(2, 5, 2))
                    {
                        AddCellNum(group.NumberOfSmartCardRequired);
                    }
                    if (Report.version >= new Version(2, 5, 3))
                    {
                        AddCellNum(group.NumberOfServiceAccount);
                    }
                    AddCellNum(group.NumberOfMemberCanBeDelegated);
                    AddCellNum(group.NumberOfExternalMember);
                    if (Report.version >= new Version(2, 9))
                    {
                        AddCellNum(group.NumberOfMemberInProtectedUsers);
                    }
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(group.GroupName))
                                AddCellText(custTable.Columns[i].Values[group.GroupName]);
                            else
                                AddCellText("");
                        }
                        
                    }
                    AddEndRow();
                }
                AddEndTable();
                foreach (HealthCheckGroupData group in Report.PrivilegedGroups)
                {
                    if (group.Members != null && group.Members.Count > 0)
                    {
                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(group.GroupName), group.GroupName, ShowModalType.XL);
                        GenerateAdminGroupsDetail(group.Members);
                        AddEndModal();
                    }
                }
            }

            if (Report.AllPrivilegedMembers != null && Report.AllPrivilegedMembers.Count > 0)
            {
                Add(@"
		<div class=""row"">
			<div class=""col-md-12"">
");
                GenerateAccordion("admingroupsaccordeon",
                    () =>
                    {
                        GenerateAccordionDetail("allprivileged", "admingroupsaccordeon", "All users in Admins groups", Report.AllPrivilegedMembers.Count, false, () => GenerateAdminGroupsDetail(Report.AllPrivilegedMembers));
                    });
                Add("</div></div>");
            }
            if (Report.ProtectedUsersNotPrivileged != null && Report.ProtectedUsersNotPrivileged.Members != null && Report.ProtectedUsersNotPrivileged.Members.Count > 0)
            {
                Add(@"
		<div class=""row"">
			<div class=""col-md-12"">
");
                GenerateAccordion("protectedusersaccordeon",
                    () =>
                    {
                        GenerateAccordionDetail("protectedusers", "protectedusersaccordeon", "Protected Users and not Admins", Report.ProtectedUsersNotPrivileged.Members.Count, false, () => GenerateAdminGroupsDetail(Report.ProtectedUsersNotPrivileged.Members));
                    });
                Add("</div></div>");
            }
            GenerateSubSection("Last Logon Distribution", "adminlastlogondistribution");
            if (string.IsNullOrEmpty(_license.Edition))
            {
                AddParagraph("This feature is reserved for customers who have <a href='https://www.pingcastle.com/services/'>purchased a license</a>");
            }
            else
            {
                List<HealthcheckPwdDistributionData> lastLogon = new List<HealthcheckPwdDistributionData>();
                Dictionary<int, string> tooltips = new Dictionary<int, string>();
                Dictionary<int, string> tooltips2 = new Dictionary<int, string>();
                List<HealthcheckPwdDistributionData> pwdLastSet = new List<HealthcheckPwdDistributionData>();

                ComputePrivilegedDistribution(lastLogon, tooltips, pwdLastSet, tooltips2);

                if (lastLogon.Count == 0)
                    lastLogon = Report.PrivilegedDistributionLastLogon;
                if (lastLogon != null && lastLogon.Count > 0)
                {
                    AddParagraph("Here is the distribution of the last logon of privileged users. Only enabled accounts are analyzed.");
                    AddPasswordDistributionChart(lastLogon, "logonadmin", tooltips);
                }

                GenerateSubSection("Password Age Distribution", "adminpwdagedistribution");
                if (pwdLastSet.Count == 0)
                    pwdLastSet = Report.PrivilegedDistributionPwdLastSet;
                if (pwdLastSet != null && pwdLastSet.Count > 0)
                {
                    AddParagraph("Here is the distribution of the password age for privileged users. Only enabled accounts are analyzed.");
                    AddPasswordDistributionChart(pwdLastSet, "pwdlastsetadmin", tooltips2);
                }
            }
            if (Report.Delegations != null && Report.Delegations.Count > 0)
            {
                Add(@"
		<div class=""row"">
			<div class=""col-md-12"">
");
                GenerateSubSection("Delegations", "admindelegation");
                AddParagraph("Each specific rights defined for Organizational Unit (OU) are listed below.");
                GenerateAccordion("delegationaccordeon",
                    () =>
                    {
                        GenerateAccordionDetail("alldelegation", "delegationaccordeon", "All delegations", Report.Delegations.Count, false, GenerateDelegationDetail);
                    });
                Add("</div></div>");
            }
            if (CustomData != null)
            {
                if (CustomData.GetSection("AdminGroups", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }

        private void ComputePrivilegedDistribution(List<HealthcheckPwdDistributionData> lastLogon, Dictionary<int, string> tooltips, List<HealthcheckPwdDistributionData> pwdLastSet, Dictionary<int, string> tooltips2)
        {
            if (Report.AllPrivilegedMembers != null && Report.AllPrivilegedMembers.Count > 0)
            {
                var pwdDistribution = new Dictionary<int, int>();
                var logonDistribution = new Dictionary<int, int>();
                foreach (var user in Report.AllPrivilegedMembers)
                {
                    if (user.IsEnabled)
                    {
                        int i;
                        if (user.LastLogonTimestamp != DateTime.MinValue)
                        {
                            i = HealthcheckAnalyzer.ConvertDateToKey(user.LastLogonTimestamp);
                        }
                        else
                        {
                            i = 10000;
                        }

                        if (logonDistribution.ContainsKey(i))
                            logonDistribution[i]++;
                        else
                            logonDistribution[i] = 1;
                        if (tooltips.ContainsKey(i))
                            tooltips[i] += "\r\n" + user.Name;
                        else
                            tooltips[i] = user.Name;

                        if (user.PwdLastSet != DateTime.MinValue)
                        {
                            i = HealthcheckAnalyzer.ConvertDateToKey(user.PwdLastSet);
                        }
                        else
                        {
                            i = HealthcheckAnalyzer.ConvertDateToKey(user.Created);
                        }
                        if (pwdDistribution.ContainsKey(i))
                            pwdDistribution[i]++;
                        else
                            pwdDistribution[i] = 1;
                        if (tooltips2.ContainsKey(i))
                            tooltips2[i] += "\r\n" + user.Name;
                        else
                            tooltips2[i] = user.Name;
                    }
                }
                foreach (var p in pwdDistribution)
                {
                    pwdLastSet.Add(new HealthcheckPwdDistributionData() { HigherBound = p.Key, Value = p.Value });
                }
                foreach (var p in logonDistribution)
                {
                    lastLogon.Add(new HealthcheckPwdDistributionData() { HigherBound = p.Key, Value = p.Value });
                }
            }
        }

        private string GenerateModalAdminGroupIdFromGroupName(string groupname)
        {
            return "modal" + groupname.Replace(" ", "-").Replace("<", "");
        }

        private void GenerateDelegationDetail()
        {
            AddBeginTable("Delegations list");
            AddHeaderText("DistinguishedName");
            AddHeaderText("Account");
            AddHeaderText("Right");
            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Delegations list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();

            Report.Delegations.Sort(OrderDelegationData);

            foreach (HealthcheckDelegationData delegation in Report.Delegations)
            {
                int dcPathPos = delegation.DistinguishedName.IndexOf(",DC=");
                string path = delegation.DistinguishedName;
                if (dcPathPos > 0)
                    path = delegation.DistinguishedName.Substring(0, dcPathPos);
                AddBeginRow();
                if (custTable != null && custTable.GetKeyLinkedSection(path, out var targetSection))
                {
                    Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                    Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                    Add(@""">");
                    AddEncoded(path);
                    Add("</a></td>");

                    AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), targetSection.Name, ShowModalType.XL);
                    GenerateAdvancedCustomSection(targetSection);
                    AddEndModal();
                }
                else
                    AddCellText(path);
                AddCellText(delegation.Account);
                AddCellText(delegation.Right);
                if (custTable != null)
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (custTable.Columns[i].Values.ContainsKey(path))
                            AddCellText(custTable.Columns[i].Values[path]);
                        else
                            AddCellText("");
                    }

                }
                AddEndRow();
            }
            AddEndTable();
        }

        private void GenerateAdminGroupsDetail(List<HealthCheckGroupMemberData> members)
        {
            if (members != null)
            {
                AddBeginTable("Admin groups detail");
                AddHeaderText("SamAccountName", "Indicates login name of the user account.");
                AddHeaderText("Enabled", "Indicates if the account is not marked as disabled.");
                AddHeaderText("Active", "Indicates if the user is not set as disabled and at least one login occured during the last 6 months.");
                AddHeaderText("Pwd never Expired", "Indicates for enabled accounts if the password is set to never expires.");
                AddHeaderText("Locked", "Indicates for enabled accounts if the account is locked");
                if (Report.version >= new Version(2, 5, 2))
                {
                    AddHeaderText("Smart Card required", "Indicates for enabled accounts if a smart card is required to login");
                }
                if (Report.version >= new Version(2, 5, 3))
                {
                    AddHeaderText("Service account", "Indicates for enabled accounts it has been marked as service. This is done by setting the servicePrincipalName attribute.");
                }
                AddHeaderText("Flag Cannot be delegated present", "Indicates for enabled accounts if the protection 'this is account is sensitive and cannot be delegated' is in place.");
                if (Report.version >= new Version(2, 8, 0))
                {
                    AddHeaderText("Creation date", "Indicates when the account has been created.");
                }
                AddHeaderText("Last login", "Indicates the last login date. Note: this value has a 14 days error margin.");
                AddHeaderText("Password last set", "Indicates when the password has been changed for the last time");
                if (Report.version >= new Version(2, 9, 0))
                {
                    AddHeaderText("In Protected Users", "Indicates if the account is a member of the special group Protected Users.");
                }
                AddHeaderText("Distinguished name", "Indicates the location of the object in the AD tree.");
                AddBeginTableData();
                members.Sort((HealthCheckGroupMemberData a, HealthCheckGroupMemberData b)
                    =>
                        {
                            return String.Compare(a.Name, b.Name);
                        }
                );
                foreach (HealthCheckGroupMemberData member in members)
                {
                    if (member.IsExternal)
                    {
                        AddBeginRow();
                        AddCellText(member.Name);
                        AddCellText("External");
                        AddCellText("External");
                        AddCellText("External");
                        AddCellText("External");
                        AddCellText("External");
                        if (Report.version >= new Version(2, 5, 2))
                        {
                            AddCellText("External");
                        }
                        if (Report.version >= new Version(2, 5, 3))
                        {
                            AddCellText("External");
                        }
                        if (Report.version >= new Version(2, 8, 0))
                        {
                            AddCellText("External");
                        }
                        AddCellText("External");
                        AddCellText("External");
                        if (Report.version >= new Version(2, 9, 0))
                        {
                            AddCellText("External");
                        }
                        AddCellText(member.DistinguishedName);
                        AddEndRow();
                    }
                    else
                    {
                        AddBeginRow();
                        AddCellText(member.Name);
                        AddCellText((member.IsEnabled ? "YES" : "NO"), true, member.IsEnabled);
                        AddCellText((member.IsActive ? "YES" : "NO"), true, member.IsActive);
                        AddCellText((member.DoesPwdNeverExpires ? "YES" : "NO"), true, !member.DoesPwdNeverExpires);
                        AddCellText((member.IsLocked ? "YES" : "NO"), true, !member.IsLocked);
                        if (Report.version >= new Version(2, 5, 2))
                        {
                            AddCellText((member.SmartCardRequired ? "YES" : "NO"), true, member.SmartCardRequired);
                        }
                        if (Report.version >= new Version(2, 5, 3))
                        {
                            AddCellText((member.IsService ? "YES" : "NO"), true, !member.IsService);
                        }
                        AddCellText((!member.CanBeDelegated ? "YES" : "NO"), true, !member.CanBeDelegated);
                        if (Report.version >= new Version(2, 8, 0))
                        {
                            AddCellDate(member.Created);
                        }
                        AddCellDate(member.LastLogonTimestamp);
                        AddCellDate(member.PwdLastSet);
                        if (Report.version >= new Version(2, 9, 0))
                        {
                            AddCellText((member.IsInProtectedUser ? "YES" : "NO"), true, member.IsInProtectedUser);
                        }
                        AddCellText(member.DistinguishedName);
                        AddEndRow();
                    }
                }
                AddEndTable();
            }
        }

        // revert an OU string order to get a string orderable
        // ex: OU=myOU,DC=DC   => DC=DC,OU=myOU
        private string GetDelegationSortKey(HealthcheckDelegationData a)
        {
            string[] apart = a.DistinguishedName.Split(',');
            string[] apart1 = new string[apart.Length];
            for (int i = 0; i < apart.Length; i++)
            {
                apart1[i] = apart[apart.Length - 1 - i];
            }
            return String.Join(",", apart1);
        }
        private int OrderDelegationData(HealthcheckDelegationData a, HealthcheckDelegationData b)
        {
            if (a.DistinguishedName == b.DistinguishedName)
                return String.Compare(a.Account, b.Account);
            return String.Compare(GetDelegationSortKey(a), GetDelegationSortKey(b));
        }

        #endregion admin groups

        #region compromission graph analysis
        protected void GenerateCompromissionGraphInformation()
        {
            if (Report.ControlPaths == null)
                return;
            AddAnchor("controlpath");
            GenerateCompromissionGraphDependanciesInformation();
            GenerateCompromissionGraphIndirectLinksInformation();
            GenerateCompromissionGraphDetailedAnalysis();
            GenerateCompromissionGraphJasonOutput();
            if (CustomData != null)
            {
                if (CustomData.GetSection("ControlPathsAnalysis", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }

        protected void GenerateCompromissionGraphDependanciesInformation()
        {
            AddParagraph("This section focuses on permissions issues that can be exploited to take control of the domain.<br>This is an advanced section that should be examined after having looked at the <a class=\"hyperlink\" href='#admingroups'>Admin Groups</a> section.");
            GenerateSubSection("Foreign domain involved", "cgtrust");
            AddParagraph("This analysis focuses on accounts found in control path and located in other domains.");
            if (Report.ControlPaths.Dependancies == null || Report.ControlPaths.Dependancies.Count == 0)
            {
                AddParagraph("No operative link with other domains has been found.");
                AddParagraph("No operative link with other domains has been found.");
                return;
            }

            AddParagraph("The following table lists all the foreign domains whose compromission can impact this domain. The impact is listed by typology of objects.");
            AddBeginTable("Compromission graph dependancies list");
            AddHeaderText("FQDN", rowspan: 2);
            AddHeaderText("NetBIOS", rowspan: 2);
            AddHeaderText("SID", rowspan: 2);

            int numTypology = 0;
            foreach (var typology in (CompromiseGraphDataTypology[])Enum.GetValues(typeof(CompromiseGraphDataTypology)))
            {
                AddHeaderText(ReportHelper.GetEnumDescription(typology), colspan: 3);
                numTypology++;
            }

            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Compromission graph dependancies list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip, rowspan: 2);
                        else
                            AddHeaderText(custTable.Columns[i].Header, rowspan: 2);
                    }
                }
            }
            AddEndRow();
            AddBeginRow();
            for (int i = 0; i < numTypology; i++)
            {
                AddHeaderText("Group", "Number of group impacted by this domain");
                AddHeaderText("Resolved", "Number of unique SID (account, group, computer, ...) resolved");
                AddHeaderText("Unresolved", "Number of unique SID (account, group, computer, ...) NOT resolved meaning that the underlying object may have been removed");
            }
            AddBeginTableData();
            foreach (var header in Report.ControlPaths.Dependancies)
            {
                AddBeginRow();
                //Add("<td class='text'>");
                if (GetUrlCallback == null)
                {
                    if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(header.FQDN), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(header.FQDN);
                        Add("</a></td>");

                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(header.FQDN), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                    {
                        Add("<td class='text'>");
                        AddEncoded(header.FQDN);
                        Add("</td>");
                    }
                }
                else
                {
                    var urlCallBack = GetUrlCallback(header.Domain, !string.IsNullOrEmpty(header.FQDN) ? header.FQDN : header.Netbios);
                    if (custTable != null && custTable.GetKeyLinkedSection(urlCallBack, out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(urlCallBack);
                        Add("</a></td>");

                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), urlCallBack, ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                    {
                        Add("<td class='text'>");
                        Add(GetUrlCallback(header.Domain, !string.IsNullOrEmpty(header.FQDN) ? header.FQDN : header.Netbios));
                        Add("</td>");
                    }
                }
                //Add("</td>");
                AddCellText(header.Netbios);
                AddCellText(header.Sid);
                foreach (var typology in (CompromiseGraphDataTypology[])Enum.GetValues(typeof(CompromiseGraphDataTypology)))
                {
                    bool found = false;
                    foreach (var item in header.Details)
                    {
                        if (item.Typology != typology)
                            continue;
                        found = true;
                        AddCellNum(item.NumberOfGroupImpacted);
                        AddCellNum(item.NumberOfResolvedItems);
                        AddCellNum(item.NumberOfUnresolvedItems);
                        break;
                    }
                    if (!found)
                    {
                        AddCellNum(0, true);
                        AddCellNum(0, true);
                        AddCellNum(0, true);
                    }
                }
                if (custTable != null)
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (custTable.Columns[i].Values.ContainsKey(header.FQDN))
                            AddCellText(custTable.Columns[i].Values[header.FQDN]);
                        else
                            AddCellText("");
                    }
                    
                }
                AddEndRow();
            }
            AddEndTable();
        }

        protected void GenerateCompromissionGraphIndirectLinksInformation()
        {
            GenerateSubSection("Indirect links", "cgindirectlinks");
            AddParagraph("This part tries to summarize in a single table if major issues have been found.<br>Focus on finding critical objects such as the Everyone group then try to decrease the number of objects having indirect access.<br>The detail is displayed below.");
            if (Report.ControlPaths.AnomalyAnalysis == null || Report.ControlPaths.AnomalyAnalysis.Count == 0)
            {
                AddParagraph("No data has been found.");
                return;
            }
            AddBeginTable("Compromission Grapth Indirect links list");
            AddHeaderText("Priority to remediate", "Indicates a set of objects considered as a priority when establishing a remediation plan.");
            AddHeaderText("Critical Object Found", "Indicates if critical objects such as everyone, authenticated users or domain users can take control, directly or not, of one of the objects.");
            AddHeaderText("Number of objects with Indirect", "Indicates the count of objects per category having at least one indirect user detected.");
            AddHeaderText("Max number of indirect numbers", "Indicates the maximum on all objects of the number of users having indirect access to the object.");
            AddHeaderText("Max ratio", "Indicates in percentage the value of (number of indirect users / number of direct users) if at least one direct users exists. Else the value is zero.");

            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Compromission Grapth Indirect links list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();
            foreach (var objectRisk in (CompromiseGraphDataObjectRisk[])Enum.GetValues(typeof(CompromiseGraphDataObjectRisk)))
            {
                AddBeginRow();
                if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.GetEnumDescription(objectRisk), out var targetSection))
                {
                    Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                    Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                    Add(@""">");
                    AddEncoded(ReportHelper.GetEnumDescription(objectRisk));
                    Add("</a></td>");

                    AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.GetEnumDescription(objectRisk), ShowModalType.XL);
                    GenerateAdvancedCustomSection(targetSection);
                    AddEndModal();
                }
                else
                {
                    AddHeaderText(ReportHelper.GetEnumDescription(objectRisk));
                }
                bool found = false;
                foreach (var analysis in Report.ControlPaths.AnomalyAnalysis)
                {
                    if (analysis.ObjectRisk != objectRisk)
                        continue;
                    found = true;
                    AddCellText(analysis.CriticalObjectFound ? "YES" : "NO", true, !analysis.CriticalObjectFound);
                    AddCellNum(analysis.NumberOfObjectsWithIndirect);
                    AddCellNum(analysis.MaximumIndirectNumber);
                    AddCellNum(analysis.MaximumDirectIndirectRatio);
                    break;
                }
                if (!found)
                {
                    AddCellNum(0, true);
                    AddCellNum(0, true);
                    AddCellNum(0, true);
                    AddCellNum(0, true);
                }
                if (custTable != null)
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (custTable.Columns[i].Values.ContainsKey(ReportHelper.GetEnumDescription(objectRisk).ToString()))
                            AddCellText(custTable.Columns[i].Values[ReportHelper.GetEnumDescription(objectRisk).ToString()]);
                        else
                            AddCellText("");
                    }
                }
                AddEndRow();
            }
            AddEndTable();
        }

        private void GenerateCompromissionGraphDetailedAnalysis()
        {
            if (Report.ControlPaths.Data == null || Report.ControlPaths.Data.Count == 0)
                return;

            foreach (var typology in (CompromiseGraphDataTypology[])Enum.GetValues(typeof(CompromiseGraphDataTypology)))
            {
                var line = new Dictionary<int, SingleCompromiseGraphData>();
                for (int i = 0; i < Report.ControlPaths.Data.Count; i++)
                {
                    var data = Report.ControlPaths.Data[i];
                    if (data.Typology != typology)
                        continue;
                    line.Add(i, data);
                }

                if (line.Count == 0)
                    continue;

                GenerateSubSection(ReportHelper.GetEnumDescription(typology));
                AddParagraph("If the report has been saved which the full details, each object can be zoomed with its full detail. If it is not the case, for privacy reasons, only general statictics are available.");
                AddBeginTable("Summary of group");
                AddHeaderText("Group or user account", "The graph represents the objects which can take control of this group or user account.");
                AddHeaderText("Priority", "Indicates relatively to other objects the importance of this object when establishing a remediation plan. This importance is computed based on the impact and the easiness to proceed.");
                AddHeaderText("Number of users member of the group", "Indicates the number of local user accounts. Foreign users or groups are excluded.");
                AddHeaderText("Number of computer member of the group", "Indicates the number of local user accounts. Foreign users or groups are excluded.");
                AddHeaderText("Number of object having indirect control", "Indicates the number of local user accounts. Foreign users or groups are excluded.");
                AddHeaderText("Number of unresolved members (removed?)", "Indicates the number of local user accounts. Foreign users or groups are excluded.");
                AddHeaderText("Link with other domains");
                AddHeaderText("Detail");
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Summary of group", out var custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();
                foreach (var i in line.Keys)
                {
                    GenerateSummary(i, line[i]);
                }
                AddEndTable();
            }

            for (int i = 0; i < Report.ControlPaths.Data.Count; i++)
            {
                GenerateModalGraph(i);
                GenerateUserModalMember(i);
                GenerateModalIndirectMember(i);
                GenerateModalDependancy(i);
                GenerateModalComputerMember(i);
                GenerateModalDeletedObjects(i);
            }
        }

        private void GenerateSummary(int index, SingleCompromiseGraphData data)
        {
            CustomData.GetTable("Summary of group", out var custTable);

            AddBeginRow();
            if (custTable != null && custTable.GetKeyLinkedSection(data.Description, out var targetSection))
            {
                Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                Add(@""">");
                AddEncoded(data.Description);
                Add("</a></td>");

                AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), data.Description, ShowModalType.XL);
                GenerateAdvancedCustomSection(targetSection);
                AddEndModal();
            }
            else
            {
                AddCellText(data.Description);
            }
            AddCellText(ReportHelper.GetEnumDescription(data.ObjectRisk));
            bool isAGroup = true;
            foreach (var node in data.Nodes)
            {
                if (node.Id == 0)
                {
                    if (node.Type != "group")
                        isAGroup = false;
                    break;
                }
            }
            if (isAGroup)
            {
                Add("<td class=\"num\">");
                Add(data.NumberOfDirectUserMembers);
                if (data.DirectUserMembers.Count > 0)
                {
                    Add(@" <a href=""#mod-member-");
                    Add(index);
                    Add(@""" data-toggle=""modal"">");
                    Add("(Details)");
                    Add(@"</a>");
                }
                Add("</td>");

                Add("<td class=\"num\">");
                Add(data.NumberOfDirectComputerMembers);
                if (data.DirectComputerMembers.Count > 0)
                {
                    Add(@" <a href=""#mod-cmember-");
                    Add(index);
                    Add(@""" data-toggle=""modal"">");
                    Add("(Details)");
                    Add(@"</a>");
                }
                Add("</td>");
            }
            else
            {
                AddCellNum(0, true);
                AddCellNum(0, true);
            }

            Add("<td class=\"num\">");
            Add(data.NumberOfIndirectMembers);
            if (data.CriticalObjectFound)
            {
                Add(" including <span class='unticked'>EVERYONE</span>");
            }
            if (data.IndirectMembers.Count > 0)
            {
                Add(@" <a href=""#mod-indirectmember-");
                Add(index);
                Add(@""" data-toggle=""modal"">");
                Add("(Details)");
                Add(@"</a>");
            }
            Add("</td>");

            Add("<td class=\"num\">");
            Add(data.NumberOfDeletedObjects);
            if (data.DeletedObjects.Count != 0)
            {
                Add(@" <a href=""#mod-deleted-");
                Add(index);
                Add(@""" data-toggle=""modal"">");
                Add("(Details)");
                Add(@"</a>");
            }
            Add("</td>");

            if (data.Dependancies.Count != 0)
            {
                Add("<td>");
                for (int i = 0; i < data.Dependancies.Count; i++)
                {
                    var d = data.Dependancies[i];
                    if (i > 0)
                        Add("<br>");
                    Add(@"<a href = ""#mod-dependancy-");
                    Add(index);
                    Add(@""" data-toggle=""modal"">");
                    if (!String.IsNullOrEmpty(d.Netbios))
                    {
                        AddEncoded(d.Netbios);
                    }
                    else
                    {
                        Add("Unknown&nbsp;Domain&nbsp;");
                        Add(i);
                    }
                    Add("[");
                    Add(d.NumberOfResolvedItems);
                    Add("+");
                    Add(d.NumberOfUnresolvedItems);
                    Add("]</a>");
                }
                Add("</td>");
            }
            else
            {
                AddCellText(@"None");
            }
            if (data.Nodes == null || data.Nodes.Count == 0)
            {
                AddCellText("Not available");
            }
            else
            {
                Add(@"<td><a href=""#mcg-");
                Add(GenerateModalId(data.Description));
                Add(@""" data-toggle=""modal"">Analysis");
                Add(@"</a></td>");
            }
            if (custTable != null)
            {
                for (int i = 1; i < custTable.Columns.Count; i++)
                {
                    if (custTable.Columns[i].Values.ContainsKey(data.Description))
                        AddCellText(custTable.Columns[i].Values[data.Description]);
                    else
                        AddCellText("");
                }
                
            }
            AddEndRow();
        }

        private void GenerateModalDependancy(int i)
        {
            AddBeginModal("mod-dependancy-" + i, Report.ControlPaths.Data[i].Description, ShowModalType.XL);
            foreach (var dependancy in Report.ControlPaths.Data[i].Dependancies)
            {
                Add(@"<h4>");
                if (!String.IsNullOrEmpty(dependancy.FQDN))
                {
                    AddEncoded(dependancy.FQDN);
                }
                else
                {
                    Add("Unknown&nbsp;Domain");
                }
                Add(@"</h4>");
                Add(@"<div class=""row""><div class=""col-lg-12""><dl class=""row"">
    <dt class=""col-sm-3"">NetBios</dt>
    <dd class=""col-sm-9"">");
                AddEncoded(dependancy.Netbios);
                Add(@"</dd>
    <dt class=""col-sm-3"">SID</dt>
    <dd class=""col-sm-9"">");
                AddEncoded(dependancy.Sid);
                Add(@"</dd>
  </dl></div></div>");
                if (dependancy.NumberOfResolvedItems > 0)
                {
                    Add(@"<h5>Resolved accounts (");
                    Add(dependancy.NumberOfResolvedItems);
                    Add(@")</h5>");
                    foreach (var account in dependancy.Items)
                    {
                        if (account.Sid != account.Name)
                        {
                            AddEncoded(account.Name);
                            Add(" (");
                            AddEncoded(account.Sid);
                            Add(")<br>");
                        }
                    }
                }
                if (dependancy.NumberOfUnresolvedItems > 0)
                {
                    Add(@"<h5>Unresolved accounts (");
                    Add(dependancy.NumberOfUnresolvedItems);
                    Add(@")</h5>");
                    foreach (var account in dependancy.Items)
                    {
                        if (account.Sid == account.Name)
                        {
                            AddEncoded(account.Sid);
                            Add("<br>");
                        }
                    }
                }
            }
            AddEndModal();
        }

        private void GenerateModalIndirectMember(int i)
        {
            AddBeginModal("mod-indirectmember-" + i, Report.ControlPaths.Data[i].Description, ShowModalType.XL);
            Add(@"<div class=""row""><div class=""col-lg-12""><h4>Indirect Members</h4></div></div>");
            AddBeginTable("Indirect member list");
            AddHeaderText("Name");
            AddHeaderText("Distance");
            AddHeaderText("Last authorized object");
            AddHeaderText("Path");
            AddBeginTableData();
            foreach (var member in Report.ControlPaths.Data[i].IndirectMembers)
            {
                DisplayIndirectMember(member);
            }
            AddEndTable();
            AddEndModal();
        }

        private void GenerateModalDeletedObjects(int i)
        {
            AddBeginModal("mod-deleted-" + i, Report.ControlPaths.Data[i].Description, ShowModalType.XL);
            Add(@"<div class=""row""><div class=""col-lg-12""><h4>Deleted objects</h4></div></div>");
            AddBeginTable("Deleted objects list");
            AddHeaderText("Security Identifier");
            AddBeginTableData();
            foreach (var member in Report.ControlPaths.Data[i].DeletedObjects)
            {
                AddBeginRow();
                AddCellText(member.Sid);
                AddEndRow();
            }
            AddEndTable();
            AddEndModal();
        }

        private void DisplayIndirectMember(SingleCompromiseGraphIndirectMemberData member)
        {
            AddBeginRow();
            if (!string.IsNullOrEmpty(member.Sid))
            {
                AddCellText(member.Name + @" (" + member.Sid + @")");
            }
            else
            {
                AddCellText(member.Name);
            }
            AddCellNum(member.Distance);
            AddCellText(member.AuthorizedObject);
            AddCellText(member.Path);
            AddEndRow();
        }

        private void GenerateUserModalMember(int i)
        {
            if (Report.ControlPaths.Data[i].DirectUserMembers == null || Report.ControlPaths.Data[i].DirectUserMembers.Count == 0)
                return;
            AddBeginModal("mod-member-" + i, Report.ControlPaths.Data[i].Description, ShowModalType.XL);
            Add(@"<div class=""row""><div class=""col-lg-12""><h4>Direct User Members</h4></div></div>");
            AddBeginTable("User list");
            AddHeaderText("SamAccountName");
            AddHeaderText("Enabled");
            AddHeaderText("Active");
            AddHeaderText("Pwd never Expired");
            AddHeaderText("Locked");
            AddHeaderText("Smart Card required");
            AddHeaderText("Service account");
            AddHeaderText("Flag Cannot be delegated present");
            AddHeaderText("Distinguished name");
            AddBeginTableData();
            foreach (var member in Report.ControlPaths.Data[i].DirectUserMembers)
            {
                DisplayUserMember(member);
            }
            AddEndTable();
            AddEndModal();
        }

        private void DisplayUserMember(SingleCompromiseGraphUserMemberData member)
        {
            AddBeginRow();
            AddCellText(member.Name);
            AddCellText(member.IsEnabled ? "YES" : "NO", true, member.IsEnabled);
            AddCellText(member.IsActive ? "YES" : "NO", true, member.IsActive);
            AddCellText(member.DoesPwdNeverExpires ? "YES" : "NO", true, !member.DoesPwdNeverExpires);
            AddCellText(member.IsLocked ? "YES" : "NO", true, !member.IsLocked);
            AddCellText(member.SmartCardRequired ? "YES" : "NO", member.SmartCardRequired, member.SmartCardRequired);
            AddCellText(member.IsService ? "YES" : "NO", member.IsService, !member.IsService);
            AddCellText(!member.CanBeDelegated ? "YES" : "NO", true, !member.CanBeDelegated);
            AddCellText(member.DistinguishedName);
            AddEndRow();
        }

        private void GenerateModalComputerMember(int i)
        {
            if (Report.ControlPaths.Data[i].DirectComputerMembers == null || Report.ControlPaths.Data[i].DirectComputerMembers.Count == 0)
                return;
            AddBeginModal("mod-cmember-" + i, Report.ControlPaths.Data[i].Description, ShowModalType.XL);
            Add(@"<div class=""row""><div class=""col-lg-12""><h4>Direct Computer Members</h4></div></div>");
            AddBeginTable("Computer list");
            AddHeaderText("SamAccountName");
            AddHeaderText("Enabled");
            AddHeaderText("Active");
            AddHeaderText("Locked");
            AddHeaderText("Flag Cannot be delegated present");
            AddHeaderText("Distinguished name");
            AddBeginTableData();
            foreach (var member in Report.ControlPaths.Data[i].DirectComputerMembers)
            {
                DisplayComputerMember(member);
            }
            AddEndTable();
            AddEndModal();
        }

        private void DisplayComputerMember(SingleCompromiseGraphComputerMemberData member)
        {
            AddBeginRow();
            AddCellText(member.Name);
            AddCellText(member.IsEnabled ? "YES" : "NO", true, member.IsEnabled);
            AddCellText(member.IsActive ? "YES" : "NO", true, member.IsActive);
            AddCellText(member.IsLocked ? "YES" : "NO", true, !member.IsLocked);
            AddCellText(!member.CanBeDelegated ? "YES" : "NO", true, !member.CanBeDelegated);
            AddCellText(member.DistinguishedName);
            AddEndRow();
        }

        private string GenerateModalId(string title)
        {
            return title.Replace(" ", "");
        }

        private void GenerateModalGraph(int i)
        {
            if (Report.ControlPaths.Data[i].Nodes == null || Report.ControlPaths.Data[i].Nodes.Count == 0)
                return;
            AddBeginModal("mcg-" + GenerateModalId(Report.ControlPaths.Data[i].Description), Report.ControlPaths.Data[i].Description, ShowModalType.FullScreen);
            Add(@"<div class=""progress mt-2 d-none"" id=""progress");
            Add(GenerateModalId(Report.ControlPaths.Data[i].Description));
            Add(@""">
					<div class=""progress-bar"" role=""progressbar"" aria-valuenow=""0"" aria-valuemin=""0"" aria-valuemax=""100"">
						0%
					</div>
				</div>
				<div id=""mynetwork");
            Add(GenerateModalId(Report.ControlPaths.Data[i].Description));
            Add(@""" class=""network-area""></div>

				<div class=""legend"">
					Legend: <br>
					<i class=""legend_user"">u</i> user<br>
					<i class=""legend_fsp"">w</i> external user or group<br>
					<i class=""legend_computer"">m</i> computer<br>
					<i class=""legend_group"">g</i> group<br>
					<i class=""legend_ou"">o</i> OU<br>
					<i class=""legend_gpo"">x</i> GPO<br>
					<i class=""legend_unknown"">?</i> Other<br>
					Settings: <br>
					<div class=""custom-control custom-switch"">
						<input type=""checkbox"" class=""custom-control-input"" checked id=""switch-1-");
            Add(GenerateModalId(Report.ControlPaths.Data[i].Description));
            Add(@""">
						<label class=""custom-control-label""  for=""switch-1-");
            Add(GenerateModalId(Report.ControlPaths.Data[i].Description));
            Add(@""">Compact display</label>
					</div>
					<div class=""custom-control custom-switch"">
						<input type=""checkbox"" class=""custom-control-input"" checked id=""switch-2-");
            Add(GenerateModalId(Report.ControlPaths.Data[i].Description));
            Add(@""">
						<label class=""custom-control-label""  for=""switch-2-");
            Add(GenerateModalId(Report.ControlPaths.Data[i].Description));
            Add(@""">Hierarchical view</label>
					</div>
				</div>
");
            AddEndModal(ShowModalType.FullScreen);
        }

        protected void GenerateCompromissionGraphJasonOutput()
        {
            for (int i = 0; i < Report.ControlPaths.Data.Count; i++)
            {
                AddLine(@"<script type=""application/json"" data-pingcastle-selector=""Data_" + GenerateModalId(Report.ControlPaths.Data[i].Description) + @""">");
                AddLine(BuildJasonFromSingleCompromiseGraph(Report.ControlPaths.Data[i]));
                AddLine("</script>");
            }
            AddLine(@"<script type=""application/json"" data-pingcastle-selector=""RelationTypeDescription"">");
            AddLine("{");
            bool first = true;
            foreach (var relationtype in (RelationType[])Enum.GetValues(typeof(RelationType)))
            {
                if (!first)
                    AddLine(",");
                else
                    first = false;
                var description = ReportHelper.GetEnumDescription(relationtype);
                Add("\"");
                AddJsonEncoded(relationtype.ToString());
                Add("\" : \"");
                AddJsonEncoded(description);
                Add("\"");
            }
            AddLine("}");
            AddLine("</script>");
        }


        string BuildJasonFromSingleCompromiseGraph(SingleCompromiseGraphData data)
        {
            StringBuilder output = new StringBuilder();
            Dictionary<int, int> idconversiontable = new Dictionary<int, int>();
            output.Append("{");
            // START OF NODES

            output.Append("  \"nodes\": [");
            // it is important to put the root node as the first node for correct display
            for (int i = 0; i < data.Nodes.Count; i++)
            {
                var node = data.Nodes[i];
                if (i != 0)
                    output.Append("    },");
                output.Append("    {");
                output.Append("      \"id\": " + node.Id + ",");
                output.Append("      \"name\": \"" + ReportHelper.EscapeJsonString(node.Name) + "\",");
                output.Append("      \"type\": \"" + node.Type + "\",");
                output.Append("      \"shortName\": \"" + ReportHelper.EscapeJsonString(node.ShortName) + "\",");
                if (node.Suspicious)
                {
                    output.Append("      \"suspicious\": 1,");
                }
                if (node.Critical)
                {
                    output.Append("      \"critical\": 1,");
                }
                if (node.Distance == 0)
                    output.Append("      \"dist\": null");
                else
                    output.Append("      \"dist\": \"" + node.Distance + "\"");
            }
            output.Append("    }");
            output.Append("  ],");
            // END OF NODES

            // START LINKS
            output.Append("  \"links\": [");
            // avoid a final ","
            for (int i = 0; i < data.Links.Count; i++)
            {
                var relation = data.Links[i];
                if (i != 0)
                    output.Append("    },");

                output.Append("    {");
                output.Append("      \"source\": " + relation.Source + ",");
                output.Append("      \"target\": " + relation.Target + ",");
                output.Append("      \"rels\": [");
                var hints = relation.Hints.Split(' ');
                for (int j = 0; j < hints.Length; j++)
                {
                    output.Append("         \"" + hints[j] + "\"" + (j == hints.Length - 1 ? String.Empty : ","));
                }

                output.Append("       ]");
            }
            if (data.Links.Count > 0)
            {
                output.Append("    }");
            }
            output.Append("  ]");
            // END OF LINKS
            output.Append("}");
            return output.ToString();
        }

        #endregion

        #region trust
        protected void GenerateTrustInformation()
        {
            List<string> knowndomains = new List<string>();
            AddParagraph("This section focuses on the relations that this domain has with other domains");
            GenerateSubSection("Discovered Domains", "discovereddomains");
            AddParagraph("This part displays the direct links that this domain has with other domains.");
            AddBeginTable("Trusts list");
            AddHeaderText("Trust Partner");
            AddHeaderText("Type");
            AddHeaderText("Attribut");
            AddHeaderText("Direction", @"<div class='text-left'><b>Bidirectional:</b> Each domain or forest has access to the resources of the other domain or forest. <br>
                <b>Inbound:</b> The other domain or forest has access to the resources of this domain or forest. This domain or forest does not have access to resources that belong to the other domain or forest. <br>
                <b>Outbound:</b> This domain or forest has access to resources of the other domain or forest. The other domain or forest does not have access to the resources of this domain or forest.</div>",
                true);
            AddHeaderText("SID Filtering active", @"<div class='text-left'>Indicates if the protection for the trust has been enabled or disabled.<br>
                A NO means that forged kerberos ticket with a security identifier from this domain will be accepted.<br>
                Please note that this check is being performed only at ONE direction of a BI-directional trust<br>
                Make sure you also run RisX in the Trust Partner domain for complete information</div>",
                true);
            AddHeaderText("TGT Delegation", @"<div class='text-left'>Indicates if the kerberos delegation works accross forest trusts<br>
                A YES means that TGTs are being sent over the trust<br>
                Please note that this check is being performed only at ONE direction of a BI-directional trust<br>Make sure you also run RisX in the Trust Partner domain for complete information</div>",
                true);
            AddHeaderText("Creation", "Indicates creation date of the underlying AD object");
            AddHeaderText("Is Active ?", "The account used to store the secret should be modified every 30 days if it is active. It indicates if a change occured during the last 40 days");
            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Trusts list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();

            foreach (HealthCheckTrustData trust in Report.Trusts)
            {
                string sid = (string.IsNullOrEmpty(trust.SID) ? "[Unknown]" : trust.SID);
                string netbios = (string.IsNullOrEmpty(trust.NetBiosName) ? "[Unknown]" : trust.NetBiosName);
                string sidfiltering = TrustAnalyzer.GetSIDFiltering(trust);
                if (sidfiltering == "Yes")
                {
                    sidfiltering = "<span class=\"ticked\">" + sidfiltering + "</span>";
                }
                else if (sidfiltering == "No")
                {
                    sidfiltering = "<span class=\"unticked\">" + sidfiltering + "</span>";
                }
                string tgtDelegation = TrustAnalyzer.GetTGTDelegation(trust);
                if (tgtDelegation == "Yes")
                {
                    tgtDelegation = "<span class=\"unticked\">" + tgtDelegation + "</span>";
                }
                else if (tgtDelegation == "No")
                {
                    tgtDelegation = "<span class=\"ticked\">" + tgtDelegation + "</span>";
                }
                AddBeginRow();
                //Add(@"<td class='text'>");
                if (GetUrlCallback == null)
                {
                    if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(trust.TrustPartner), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(trust.TrustPartner);
                        Add("</a>");
                        AddBeginTooltip();
                        Add("SID: ");
                        Add(sid);
                        Add("<br>Netbios: ");
                        Add(netbios);
                        AddEndTooltip();
                        Add("</td>");
                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(trust.TrustPartner), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                    {
                        Add(@"<td class='text'>");
                        AddEncoded(trust.TrustPartner);
                        AddBeginTooltip();
                        Add("SID: ");
                        Add(sid);
                        Add("<br>Netbios: ");
                        Add(netbios);
                        AddEndTooltip();
                        Add(@"</td>");

                    }
                    
                }
                else
                {
                    if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(GetUrlCallback(trust.Domain, trust.TrustPartner)), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(GetUrlCallback(trust.Domain, trust.TrustPartner));
                        Add("</a></td>");

                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(GetUrlCallback(trust.Domain, trust.TrustPartner)), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                    {
                        Add(@"<td class='text'>");
                        Add(GetUrlCallback(trust.Domain, trust.TrustPartner));
                        Add(@"</td>");
                    }
                    
                }
                //Add(@"</td>");
                AddCellText(TrustAnalyzer.GetTrustType(trust.TrustType));
                AddCellText(TrustAnalyzer.GetTrustAttribute(trust.TrustAttributes));
                AddCellText(TrustAnalyzer.GetTrustDirection(trust.TrustDirection));
                Add("<td class='text'>");
                Add(sidfiltering);
                Add("</td><td class='text'>");
                Add(tgtDelegation);
                Add("</td>");
                AddCellDate(trust.CreationDate);
                AddCellText((trust.IsActive ? "TRUE" : "FALSE"), true, trust.IsActive);
                if (custTable != null)
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (custTable.Columns[i].Values.ContainsKey(trust.TrustPartner))
                            AddCellText(custTable.Columns[i].Values[trust.TrustPartner]);
                        else
                            AddCellText("");
                    }
                }
                AddEndRow();
            }
            AddEndTable();

			GenerateSubSection("Reachable Domains");
			AddParagraph("These are the domains that RisX was able to detect but which is not releated to direct trusts. It may be children of a forest or bastions.");
			AddBeginTable("Reachable domains list");
			AddHeaderText("Reachable domain");
			AddHeaderText("Discovered using");
			AddHeaderText("Netbios");
			AddHeaderText("Creation date");
            custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Reachable domains list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();

            foreach (HealthCheckTrustData trust in Report.Trusts)
            {
                if (trust.KnownDomains == null)
                    continue;
                trust.KnownDomains.Sort((HealthCheckTrustDomainInfoData a, HealthCheckTrustDomainInfoData b)
                    =>
                {
                    return String.Compare(a.DnsName, b.DnsName);
                }
                );
                foreach (HealthCheckTrustDomainInfoData di in trust.KnownDomains)
                {
                    AddBeginRow();
                    //Add(@"<td class='text'>");
                    if (GetUrlCallback == null)
                    {
                        if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(di.DnsName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(di.DnsName);
                            Add("</a></td>");

                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(di.DnsName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                        {
                            Add(@"<td class='text'>");
                            AddEncoded(di.DnsName);
                            Add(@"</td>");
                        }

                    }
                    else
                    {
                        Add(@"<td class='text'>");

                        Add(GetUrlCallback(di.Domain, di.DnsName));
                        Add(@"</td>");

                    }
                    //Add(@"</td><td class='text'>");
                    Add(@"<td class='text'>");
                    if (GetUrlCallback == null)
                    {
                        AddEncoded(trust.TrustPartner);
                    }
                    else
                    {
                        Add(GetUrlCallback(trust.Domain, trust.TrustPartner));
                    }
                    Add(@"</td><td class='text'>");
                    AddEncoded(di.NetbiosName);
                    Add(@"</td><td class='text'>");
                    if (di.CreationDate == DateTime.MinValue)
                    {
                        Add("Unknown");
                    }
                    else
                    {
                        Add(di.CreationDate);
                    }
                    Add(@"</td>");
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(di.DnsName))
                                AddCellText(custTable.Columns[i].Values[di.DnsName]);
                            else
                                AddCellText("");
                        }
                    }
                    AddEndRow();
                }
            }
            if (Report.ReachableDomains != null)
            {
                foreach (HealthCheckTrustDomainInfoData di in Report.ReachableDomains)
                {
                    AddBeginRow();
                    //Add(@"<td class='text'>");
                    if (GetUrlCallback == null)
                    {
                        if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(di.DnsName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(di.DnsName);
                            Add("</a></td>");

                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(di.DnsName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                        {
                            Add(@"<td class='text'>");
                            AddEncoded(di.DnsName);
                            Add(@"</td>");
                        }
                    }
                    else
                    {
                        if (custTable != null && custTable.GetKeyLinkedSection(GetUrlCallback(di.Domain, di.DnsName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(GetUrlCallback(di.Domain, di.DnsName));
                            Add("</a></td>");

                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), GetUrlCallback(di.Domain, di.DnsName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        Add(@"<td class='text'>");
                        Add(GetUrlCallback(di.Domain, di.DnsName));
                        Add(@"</td>");
                    }
                    //Add(@"</td>");
                    AddCellText("Unknown");
                    AddCellText(di.NetbiosName);
                    AddCellText("Unknown");
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(di.DnsName))
                                AddCellText(custTable.Columns[i].Values[di.DnsName]);
                            else
                                AddCellText("");
                        }
                    }
                    AddEndRow();
                }
            }

            AddEndTable();

            if (Report.AzureADSSOLastPwdChange != DateTime.MinValue)
            {
                GenerateSubSection("Azure", "azure");
                AddParagraph("The account AZUREADSSOACC is used under the hood to provide SSO functionalities with AzureAD.");
                Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>The password of the AZUREADSSOACC account should be changed twice every 40 days using this <a class=""hyperlink"" href=""https://itpro-tips.com/wp-content/uploads/files/TechnetGallery/Azure-AD-SSO-Key-Rollover-d2f1604a.zip"">script</a></p>
<p>You can use the version gathered using replication metadata from two reports to guess the frequency of the password change or if the two consecutive resets has been done. Version starts at 1.</p>
<p><strong>AZUREADSSOACC password last changed: </strong> " + Report.AzureADSSOLastPwdChange.ToString("u") + @"
<strong>version: </strong> " + Report.AzureADSSOVersion + @"
</p>
		</div></div>
");
            }

            if (CustomData != null)
            {
                if (CustomData.GetSection("TrustsInformation", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }
        #endregion trust

        void AddGPOName(IGPOReference GPO)
        {
            Add(@"<td class='text'>");
            AddEncoded(GPO.GPOName);
            if (!string.IsNullOrEmpty(GPO.GPOId))
            {
                if (!Report.GPOInfoDic.ContainsKey(GPO.GPOId))
                {
                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                    return;
                }
                var refGPO = Report.GPOInfoDic[GPO.GPOId];
                if (refGPO.IsDisabled)
                {
                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                }
                if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                {
                    AddBeginTooltip(true);
                    Add("<div class='text-left'>Linked to:<br><ul>");
                    foreach (var i in refGPO.AppliedTo)
                    {
                        Add("<li>");
                        AddEncoded(i);
                        Add("</li>");
                    }
                    Add("</ul></div>");
                    Add("<div class='text-left'>Technical id:<br>");
                    AddEncoded(GPO.GPOId);
                    Add("</div>");
                    AddEndTooltip();
                }
                else
                {
                    Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                    AddBeginTooltip();
                    Add("<div class='text-left'>Technical id:<br>");
                    AddEncoded(GPO.GPOId);
                    Add("</div>");
                    AddEndTooltip();
                }
            }
            Add("</td>");
        }

        #region anomaly
        protected void GenerateAnomalyDetail()
        {
            AddParagraph("This section focuses on security checks specific to the Active Directory environment.");
            GenerateSubSection("Backup", "backup");
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>The program checks the last date of the AD backup. This date is computed using the replication metadata of the attribute dsaSignature (<a class=""hyperlink"" href=""https://technet.microsoft.com/en-us/library/jj130668(v=ws.10).aspx"">reference</a>).</p>
<p><strong>Last backup date: </strong> " + (Report.LastADBackup == DateTime.MaxValue ? "<span class=\"unticked\">Never</span>" : (Report.LastADBackup == DateTime.MinValue ? "<span class=\"unticked\">Not checked (older version of PingCastle)</span>" : Report.LastADBackup.ToString("u"))) + @"</p>
		</div></div>
");

            GenerateSubSection("LAPS", "laps");
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p><a class=""hyperlink"" href=""https://support.microsoft.com/en-us/kb/3062591"">LAPS</a> is used to have a unique local administrator password on all workstations / servers of the domain.
Then this password is changed at a fixed interval. The risk is when a local administrator hash is retrieved and used on other workstation in a pass-the-hash attack.</p>
<p>Mitigation: having a process when a new workstation is created or install LAPS and apply it through a GPO</p>
<p><strong>LAPS installation date: </strong> " + (Report.LAPSInstalled == DateTime.MaxValue ? "<span class=\"unticked\">Never</span>" : (Report.LAPSInstalled == DateTime.MinValue ? "<span class=\"unticked\">Not checked (older version of PingCastle)</span>" : Report.LAPSInstalled.ToString("u"))) + @"</p>
		</div></div>
");
            GenerateSubSection("Windows Event Forwarding (WEF)");
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>Windows Event Forwarding is a native mechanism used to collect logs on all workstations / servers of the domain.
Microsoft recommends to <a class=""hyperlink"" href=""https://docs.microsoft.com/en-us/windows/threat-protection/use-windows-event-forwarding-to-assist-in-instrusion-detection"">Use Windows Event Forwarding to help with intrusion detection</a>
Here is the list of servers configured for WEF found in GPO</p>
<p><strong>Number of WEF configuration found: </strong> " + (Report.GPOEventForwarding.Count) + @"</p>
		</div></div>
");
            // wef
            if (Report.GPOEventForwarding.Count > 0)
            {
                Add(@"
		<div class=""row"">
			<div class=""col-md-12"">");
                GenerateAccordion("wef", () =>
                    {
                        GenerateAccordionDetail("wefPanel", "wef", "Windows Event Forwarding servers", Report.GPOEventForwarding.Count, false, () =>
                            {
                                AddBeginTable("WEF list");
                                AddHeaderText("GPO Name");
                                AddHeaderText("Order");
                                AddHeaderText("Server");
                                CustomTable custTable = null;
                                if (CustomData != null)
                                {
                                    if (CustomData.GetTable("WEF list", out custTable))
                                    {
                                        for (int i = 1; i < custTable.Columns.Count; i++)
                                        {
                                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                                            else
                                                AddHeaderText(custTable.Columns[i].Header);
                                        }
                                    }
                                }
                                AddBeginTableData();

                                // descending sort
                                Report.GPOEventForwarding.Sort(
                                    (GPOEventForwardingInfo a, GPOEventForwardingInfo b)
                                        =>
                                    {
                                        int comp = String.Compare(a.GPOName, b.GPOName);
                                        if (comp == 0)
                                            comp = (a.Order > b.Order ? 1 : (a.Order == b.Order ? 0 : -1));
                                        return comp;
                                    }
                                    );

                                foreach (var info in Report.GPOEventForwarding)
                                {
                                    AddBeginRow();
                                    if (custTable != null && custTable.GetKeyLinkedSection(info.GPOName, out var targetSection))
                                    {
                                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                                        Add(@""">");
                                        AddEncoded(info.GPOName);
                                        Add("</a></td>");

                                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), info.GPOName, ShowModalType.XL);
                                        GenerateAdvancedCustomSection(targetSection);
                                        AddEndModal();
                                    }
                                    else
                                        AddCellText(info.GPOName);
                                    AddCellNum(info.Order);
                                    AddCellText(info.Server);
                                    if (custTable != null)
                                    {
                                        for (int i = 1; i < custTable.Columns.Count; i++)
                                        {
                                            if (custTable.Columns[i].Values.ContainsKey(info.GPOName))
                                                AddCellText(custTable.Columns[i].Values[info.GPOName]);
                                            else
                                                AddCellText("");
                                        }
                                        
                                    }
                                    AddEndRow();
                                }
                                AddEndTable();
                            });
                    });
                Add(@"
			</div>
		</div>
");
            }


            // krbtgt
            GenerateSubSection("krbtgt (Used for Golden ticket attacks)", "krbtgt");
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>The account password for the <em>krbtgt</em> account should be rotated twice yearly at a minimum. More frequent password rotations are recommended, with 40 days the current recommendation by ANSSI. Additional rotations based on external events, such as departure of an employee who had privileged network access, are also strongly recommended.</p>
<p>You can perform this action using this <a class=""hyperlink"" href=""https://github.com/microsoft/New-KrbtgtKeys.ps1"">script</a></p>
<p>You can use the version gathered using replication metadata from two reports to guess the frequency of the password change or if the two consecutive resets has been done. Version starts at 1.</p>
<p><strong>Kerberos password last changed: </strong> " + Report.KrbtgtLastChangeDate.ToString("u") + @"
<strong>version: </strong> " + Report.KrbtgtLastVersion + @"
</p>
		</div></div>
");
            // adminSDHolder
            GenerateSubSection("AdminSDHolder (detect temporary elevated accounts)", "admincountequalsone");
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>This control detects accounts which are former 'unofficial' admins.
Indeed when an account belongs to a privileged group, the attribute admincount is set. If the attribute is set without being an official member, this is suspicious. To suppress this warning, the attribute admincount of these accounts should be removed after review.</p>
<p><strong>Number of accounts to review:</strong> " +
        (Report.AdminSDHolderNotOKCount > 0 ? "<span class=\"unticked\">" + Report.AdminSDHolderNotOKCount + "</span>" : "0")
    + @"</p>
		</div></div>
");
            if (Report.AdminSDHolderNotOKCount > 0 && Report.AdminSDHolderNotOK != null && Report.AdminSDHolderNotOK.Count > 0)
            {
                GenerateAccordion("adminsdholder", () => GenerateListAccountDetail("adminsdholder", "adminsdholderpanel", "AdminSDHolder User List", Report.AdminSDHolderNotOK));
            }

            // unix user password
            GenerateSubSection("Unix Passwords", "unixpasswordsfound");
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>This control detects if one of the attributes userPassword or unixUserPassword has been set on accounts.
Indeed, these attributes are designed to store encrypted secrets for unix (or mainframe) interconnection. However in the large majority, interconnected systems are poorly designed and the user password is stored in these attributes in clear text or poorly encrypted.
The userPassword attribute is also used in classic LDAP systems to change the user password by setting its value. But, with Active Directory, it is considered by default as a normal attribute and doesn't trigger a password but shows instead the password in clear text.
</p>
<p><strong>Number of accounts to review:</strong> " +
        (Report.UnixPasswordUsersCount > 0 ? "<span class=\"unticked\">" + Report.UnixPasswordUsersCount + "</span>" : "0")
    + @"</p>
		</div></div>
");
            if (Report.UnixPasswordUsersCount > 0 && Report.UnixPasswordUsers != null && Report.UnixPasswordUsers.Count > 0)
            {
                GenerateAccordion("unixpasswords", () => GenerateListAccountDetail("unixpasswords", "unixpasswordspanel", "User List With Unix Passwords", Report.UnixPasswordUsers));
            }

            if (Report.DomainControllers != null)
            {
                int countnullsession = 0;
                foreach (var DC in Report.DomainControllers)
                {
                    if (DC.HasNullSession)
                    {
                        countnullsession++;
                    }
                }
                if (countnullsession > 0)
                {
                    GenerateSubSection("NULL SESSION (anonymous access)", "nullsession");
                    Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>This control detects domain controllers which can be accessed without authentication.
Hackers can then perform a reconnaissance of the environement with only a network connectivity and no account at all.</p>
			<p><strong>Domain controllers vulnerable:</strong> <span class=""unticked"">" + countnullsession + @"</span>
		</div></div>
		<div class=""row"">
			<div class=""col-md-12"">
");
                    GenerateAccordion("nullsessions", () =>
                        {
                            GenerateAccordionDetail("nullsessionPanel", "nullsessions", "Domain controllers with NULL SESSION Enabled", countnullsession, false, () =>
                                {
                                    AddBeginTable("Null session list");
                                    AddHeaderText("Domain Controller");
                                    CustomTable custTable2 = null;
                                    if (CustomData != null)
                                    {
                                        if (CustomData.GetTable("Null session list", out custTable2))
                                        {
                                            for (int i = 1; i < custTable2.Columns.Count; i++)
                                            {
                                                if (!string.IsNullOrEmpty(custTable2.Columns[i].Tooltip))
                                                    AddHeaderText(custTable2.Columns[i].Header, custTable2.Columns[i].Tooltip);
                                                else
                                                    AddHeaderText(custTable2.Columns[i].Header);
                                            }
                                        }
                                    }
                                    AddBeginTableData();
                                    foreach (var DC in Report.DomainControllers)
                                    {
                                        if (DC.HasNullSession)
                                        {
                                            AddBeginRow();
                                            if (custTable2 != null && custTable2.GetKeyLinkedSection(DC.DCName, out var targetSection))
                                            {
                                                Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                                                Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                                                Add(@""">");
                                                AddEncoded(DC.DCName);
                                                Add("</a></td>");

                                                AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), DC.DCName, ShowModalType.XL);
                                                GenerateAdvancedCustomSection(targetSection);
                                                AddEndModal();
                                            }
                                            else
                                                AddCellText(DC.DCName);
                                            if (custTable2 != null)
                                            {
                                                for (int i = 1; i < custTable2.Columns.Count; i++)
                                                {
                                                    if (custTable2.Columns[i].Values.ContainsKey(DC.DCName))
                                                        AddCellText(custTable2.Columns[i].Values[DC.DCName]);
                                                    else
                                                        AddCellText("");
                                                }
                                                
                                            }
                                            AddEndRow();
                                        }
                                    }
                                    AddEndTable();
                                }
                            );
                        }
                    );
                    Add(@"
			</div>
		</div>
");
                }

                if (Report.SmartCardNotOK != null && Report.SmartCardNotOK.Count > 0)
                {
                    // smart card
                    GenerateSubSection("Smart Card and Password", "smartcardmandatorywithnopasswordchange");
                    Add(@"
		<div class=""row""><div class=""col-lg-12"">
<p>This control detects users which use only smart card and whose password hash has not been changed for at least 40 days.
Indeed, once the smart card required check is activated in the user account properties, a random password hash is set.
But this hash is not changed anymore like for users having a password whose change is controlled by password policies.
As a consequence, a capture of the hash using a memory attack tool can lead to a compromission of this account unlimited in time.
The best practice is to reset these passwords on a regular basis or to uncheck and check again the &quot;require smart card&quot; property to force a hash change.</p>
			<p><strong>Users with smart card and having their password unchanged since at least 40 days:</strong> " +
        (Report.SmartCardNotOK == null ? 0 : Report.SmartCardNotOK.Count)
        + @"</p>
		</div></div>
");
                    GenerateAccordion("anomalysmartcard", () => GenerateListAccountDetail("anomalysmartcard", "smartcard", "Smart card and Password >40 days List", Report.SmartCardNotOK));
                }

                // logon script
                GenerateSubSection("Logon scripts", "logonscripts");
                AddParagraph("You can check here backdoors or typo error in the scriptPath attribute");
                AddBeginTable("Logon script list");
                AddHeaderText("Script Name");
                AddHeaderText("Count");
                CustomTable custTable = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Logon script list", out custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();
                // descending sort
                Report.LoginScript.Sort(
                    (HealthcheckLoginScriptData a, HealthcheckLoginScriptData b)
                        =>
                    {
                        return b.NumberOfOccurence.CompareTo(a.NumberOfOccurence);
                    }
                    );

                int number = 0;
                foreach (HealthcheckLoginScriptData script in Report.LoginScript)
                {
                    AddBeginRow();
                    if (custTable != null && custTable.GetKeyLinkedSection(script.LoginScript, out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(script.LoginScript);
                        Add("</a></td>");

                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), script.LoginScript, ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                        AddCellText(String.IsNullOrEmpty(script.LoginScript.Trim()) ? "<spaces>" : script.LoginScript);
                    AddCellNum(script.NumberOfOccurence);
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(script.LoginScript.Trim()))
                                AddCellText(custTable.Columns[i].Values[script.LoginScript.Trim()]);
                            else
                                AddCellText("");
                        }
                        
                    }
                    AddEndRow();
                    number++;
                    if (number >= MaxNumberUsersInHtmlReport)
                    {
                        break;
                    }
                }
                Add(@"
				</tbody>");
                if (number >= MaxNumberUsersInHtmlReport)
                {
                    Add("<tfoot><tr><td colspan='2' class='text'>Output limited to ");
                    Add(MaxNumberUsersInHtmlReport);
                    Add(" items - go to the advanced menu before running the report or add \"--no-enum-limit\" to remove that limit</td></tr></tfoot>");
                }
                Add(@"
			</table>
		</div>
	</div>
");
                // certificate
                GenerateSubSection("Certificates", "certificates");
                Add(@"
		<div class=""row"">
			<div class=""col-lg-12"">
				<p>This detects trusted certificate which can be used in man in the middle attacks or which can issue smart card logon certificates</p>
				<p><strong>Number of trusted certificates:</strong> " + Report.TrustedCertificates.Count + @" 
			</div>
		</div>
		<div class=""row"">
			<div class=""col-lg-12"">
");
                GenerateAccordion("trustedCertificates", () =>
                    {
                        GenerateAccordionDetail("trustedCertificatesPanel", "trustedCertificates", "Trusted certificates", Report.TrustedCertificates.Count, false, () =>
                            {
                                AddBeginTable("Certificates list");
                                AddHeaderText("Source");
                                AddHeaderText("Store");
                                AddHeaderText("Subject");
                                AddHeaderText("Issuer");
                                AddHeaderText("NotBefore");
                                AddHeaderText("NotAfter");
                                AddHeaderText("Module size");
                                AddHeaderText("Signature Alg");
                                AddHeaderText("SC Logon");
                                custTable = null;
                                if (CustomData != null)
                                {
                                    if (CustomData.GetTable("Certificates list", out custTable))
                                    {
                                        for (int i = 1; i < custTable.Columns.Count; i++)
                                        {
                                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                                            else
                                                AddHeaderText(custTable.Columns[i].Header);
                                        }
                                    }
                                }
                                AddBeginTableData();

                                foreach (HealthcheckCertificateData data in Report.TrustedCertificates)
                                {
                                    X509Certificate2 cert = new X509Certificate2(data.Certificate);
                                    bool SCLogonAllowed = false;
                                    foreach (X509Extension ext in cert.Extensions)
                                    {
                                        if (ext.Oid.Value == "1.3.6.1.4.1.311.20.2.2")
                                        {
                                            SCLogonAllowed = true;
                                            break;
                                        }
                                    }
                                    int modulesize = 0;
                                    RSA key = null;
                                    try
                                    {
                                        key = cert.PublicKey.Key as RSA;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    if (key != null)
                                    {
                                        RSAParameters rsaparams = key.ExportParameters(false);
                                        modulesize = rsaparams.Modulus.Length * 8;
                                    }
                                    AddBeginRow();
                                    if (data.Source == "NTLMStore")
                                    {
                                        if (custTable != null && custTable.GetKeyLinkedSection("Enterprise NTAuth", out var targetSection))
                                        {
                                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                                            Add(@""">");
                                            AddEncoded("Enterprise NTAuth");
                                            Add("</a>");
                                            AddBeginTooltip();
                                            Add("This store is used by the Windows PKI. You can view it with the command 'certutil -viewstore -enterprise NTAuth' or edit it with the command 'Manage AD Container' of the 'Enterprise PKI' snapin of mmc.");
                                            AddEndTooltip();
                                            Add("</td>");

                                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), "Enterprise NTAuth", ShowModalType.XL);
                                            GenerateAdvancedCustomSection(targetSection);
                                            AddEndModal();
                                        }
                                        else
                                        {
                                            Add("<td class='text'>");
                                            Add(@"Enterprise NTAuth");
                                            AddBeginTooltip();
                                            Add("This store is used by the Windows PKI. You can view it with the command 'certutil -viewstore -enterprise NTAuth' or edit it with the command 'Manage AD Container' of the 'Enterprise PKI' snapin of mmc.");
                                            AddEndTooltip();
                                            Add(@"</td>");
                                        }
                                    }
                                    else
                                    {
                                        if (custTable != null && custTable.GetKeyLinkedSection(data.Source, out var targetSection))
                                        {
                                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                                            Add(@""">");
                                            AddEncoded(data.Source);
                                            Add("</a>");
                                            Add("</td>");

                                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), data.Source, ShowModalType.XL);
                                            GenerateAdvancedCustomSection(targetSection);
                                            AddEndModal();
                                        }
                                        else
                                            AddCellText(data.Source);
                                    }
                                    AddCellText(data.Store);
                                    AddCellTextNoWrap(cert.Subject);
                                    AddCellTextNoWrap(cert.Issuer);
                                    AddCellDateNoWrap(cert.NotBefore);
                                    AddCellDateNoWrap(cert.NotAfter);
                                    AddCellNum(modulesize);
                                    AddCellText(cert.SignatureAlgorithm.FriendlyName);
                                    AddCellText(SCLogonAllowed.ToString());
                                    if (custTable != null)
                                    {
                                        for (int i = 1; i < custTable.Columns.Count; i++)
                                        {
                                            if (custTable.Columns[i].Values.ContainsKey(data.Source))
                                                AddCellText(custTable.Columns[i].Values[data.Source]);
                                            else
                                                AddCellText("");
                                        }
                                    }
                                    AddEndRow();
                                }
                                AddEndTable();
                            }
                        );
                    }
                    );
                Add(@"
			</div>
		</div>
");

                GenerateSubSection("Advanced");
                AddParagraph("This section display advanced information, if any has been found");
                if (Report.lDAPIPDenyList != null && Report.lDAPIPDenyList.Count > 0)
                {
                    Add(@"
		<div class=""row"">
			<div class=""col-lg-12"">
				<p><strong>IP denied for LDAP communication</strong>
			</div>
		</div>");
                    AddBeginTable("LDAP forbidden list");
                    AddHeaderText("Entry");
                    custTable = null;
                    if (CustomData != null)
                    {
                        if (CustomData.GetTable("LDAP forbidden list", out custTable))
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                    AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                                else
                                    AddHeaderText(custTable.Columns[i].Header);
                            }
                        }
                    }
                    AddBeginTableData();
                    foreach (var e in Report.lDAPIPDenyList)
                    {
                        AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(e, out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(e);
                            Add("</a>");
                            Add("</td>");

                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), e, ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddCellText(e);
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(e))
                                    AddCellText(custTable.Columns[i].Values[e]);
                                else
                                    AddCellText("");
                            }
                        }
                        AddEndRow();
                    }
                    AddEndTable();
                }

            }

            if (CustomData != null)
            {
                if (CustomData.GetSection("Anomalies", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }
        #endregion anomaly

        #region password policies

        protected void GeneratePasswordPoliciesDetail()
        {
            GenerateSubSection("Password policies", "passwordpolicies");
            Add(@"<p>Note: PSO (Password Settings Objects) will be visible only if the user which collected the information has the permission to view it.<br>PSO shown in the report will be prefixed by &quot;PSO:&quot;</p>");
            AddBeginTable("Password policies list");
            AddHeaderText("Policy Name");
            AddHeaderText("Complexity");
            AddHeaderText("Max Password Age");
            AddHeaderText("Min Password Age");
            AddHeaderText("Min Password Length");
            AddHeaderText("Password History");
            AddHeaderText("Reversible Encryption");
            AddHeaderText("Lockout Threshold");
            AddHeaderText("Lockout Duration");
            AddHeaderText("Reset account counter locker after");
            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Password policies list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();
            if (Report.GPPPasswordPolicy != null)
            {
                foreach (GPPSecurityPolicy policy in Report.GPPPasswordPolicy)
                {
                    AddBeginRow();
                    if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(policy.GPOName), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(policy.GPOName);
                        Add("</a>");
                        if (!string.IsNullOrEmpty(policy.GPOId))
                        {
                            if (!Report.GPOInfoDic.ContainsKey(policy.GPOId))
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                return;
                            }
                            var refGPO = Report.GPOInfoDic[policy.GPOId];
                            if (refGPO.IsDisabled)
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                            }
                            if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                            {
                                AddBeginTooltip(true);
                                Add("<div class='text-left'>Linked to:<br><ul>");
                                foreach (var i in refGPO.AppliedTo)
                                {
                                    Add("<li>");
                                    AddEncoded(i);
                                    Add("</li>");
                                }
                                Add("</ul></div>");
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(policy.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                            else
                            {
                                Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                AddBeginTooltip();
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(policy.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                        }
                        Add("</td>");
                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(policy.GPOName), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                        AddGPOName(policy);
                    AddPSOStringValue(policy, "PasswordComplexity");
                    AddPSOStringValue(policy, "MaximumPasswordAge");
                    AddPSOStringValue(policy, "MinimumPasswordAge");
                    AddPSOStringValue(policy, "MinimumPasswordLength");
                    AddPSOStringValue(policy, "PasswordHistorySize");
                    AddPSOStringValue(policy, "ClearTextPassword");
                    AddPSOStringValue(policy, "LockoutBadCount");
                    AddPSOStringValue(policy, "LockoutDuration");
                    AddPSOStringValue(policy, "ResetLockoutCount");
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(policy.GPOName))
                                AddCellText(custTable.Columns[i].Values[policy.GPOName]);
                            else
                                AddCellText("");
                        }
                    }
                    AddEndRow();
                }
            }
            AddEndTable();

            GenerateSubSection("Screensaver policies");
            AddParagraph("This is the settings related to screensavers stored in Group Policies. Each non compliant setting is written in red.");
            AddBeginTable("Screensaver policies list");
            AddHeaderText("Policy Name");
            AddHeaderText("Screensaver enforced");
            AddHeaderText("Password request");
            AddHeaderText("Start after (seconds)");
            AddHeaderText("Grace Period (seconds)");
            custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Screensaver policies list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();
            if (Report.GPOScreenSaverPolicy != null)
            {
                foreach (GPPSecurityPolicy policy in Report.GPOScreenSaverPolicy)
                {
                    AddBeginRow();
                    if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(policy.GPOName), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(policy.GPOName);
                        Add("</a>");
                        if (!string.IsNullOrEmpty(policy.GPOId))
                        {
                            if (!Report.GPOInfoDic.ContainsKey(policy.GPOId))
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                return;
                            }
                            var refGPO = Report.GPOInfoDic[policy.GPOId];
                            if (refGPO.IsDisabled)
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                            }
                            if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                            {
                                AddBeginTooltip(true);
                                Add("<div class='text-left'>Linked to:<br><ul>");
                                foreach (var i in refGPO.AppliedTo)
                                {
                                    Add("<li>");
                                    AddEncoded(i);
                                    Add("</li>");
                                }
                                Add("</ul></div>");
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(policy.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                            else
                            {
                                Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                AddBeginTooltip();
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(policy.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                        }
                        Add("</td>");
                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(policy.GPOName), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                        AddGPOName(policy);
                    AddPSOStringValue(policy, "ScreenSaveActive");
                    AddPSOStringValue(policy, "ScreenSaverIsSecure");
                    AddPSOStringValue(policy, "ScreenSaveTimeOut");
                    AddPSOStringValue(policy, "ScreenSaverGracePeriod");
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(policy.GPOName))
                                AddCellText(custTable.Columns[i].Values[policy.GPOName]);
                            else
                                AddCellText("");
                        }
                    }
                    AddEndRow();
                }
            }
            AddEndTable();
            if (CustomData != null)
            {
                if (CustomData.GetSection("PasswordPolicies", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }

        #endregion password policies

        #region GPO
        protected void GenerateGPODetail()
        {
            AddParagraph("This section focuses on security settings stored in the Active Directory technical security policies.");
            GenerateSubSection("Obfuscated Passwords", "gpoobfuscatedpassword");
            AddParagraph("The password in GPO are obfuscated, not encrypted. Consider any passwords listed here as compromised and change it immediatly.");
            if (Report.GPPPassword != null && Report.GPPPassword.Count > 0)
            {
                AddBeginTable("Obfuscated passwords list");
                AddHeaderText("GPO Name");
                AddHeaderText("Password origin");
                AddHeaderText("UserName");
                AddHeaderText("Password");
                AddHeaderText("Changed");
                AddHeaderText("Other");
                CustomTable custTable2 = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Obfuscated passwords list", out custTable2))
                    {
                        for (int i = 1; i < custTable2.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable2.Columns[i].Tooltip))
                                AddHeaderText(custTable2.Columns[i].Header, custTable2.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable2.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();
                foreach (GPPPassword password in Report.GPPPassword)
                {
                    AddBeginRow();
                    if (custTable2 != null && custTable2.GetKeyLinkedSection(ReportHelper.Encode(password.GPOName), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(password.GPOName);
                        Add("</a>");
                        if (!string.IsNullOrEmpty(password.GPOId))
                        {
                            if (!Report.GPOInfoDic.ContainsKey(password.GPOId))
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                return;
                            }
                            var refGPO = Report.GPOInfoDic[password.GPOId];
                            if (refGPO.IsDisabled)
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                            }
                            if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                            {
                                AddBeginTooltip(true);
                                Add("<div class='text-left'>Linked to:<br><ul>");
                                foreach (var i in refGPO.AppliedTo)
                                {
                                    Add("<li>");
                                    AddEncoded(i);
                                    Add("</li>");
                                }
                                Add("</ul></div>");
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(password.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                            else
                            {
                                Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                AddBeginTooltip();
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(password.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                        }
                        Add("</td>");
                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(password.GPOName), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                        AddGPOName(password);
                    AddCellText(password.Type);
                    AddCellText(password.UserName);
                    AddCellText(password.Password, true);
                    AddCellDate(password.Changed);
                    AddCellText(password.Other);
                    if (custTable2 != null)
                    {
                        for (int i = 1; i < custTable2.Columns.Count; i++)
                        {
                            if (custTable2.Columns[i].Values.ContainsKey(password.GPOName))
                                AddCellText(custTable2.Columns[i].Values[password.GPOName]);
                            else
                                AddCellText("");
                        }
                    }
                    AddEndRow();
                }
                AddEndTable();
            }

            GenerateSubSection("Restricted Groups");
            AddParagraph("Giving local group membership in a GPO is a way to become administrator.<br>The local admin of a domain controller can become domain administrator instantly.");
            if (Report.GPOLocalMembership != null && Report.GPOLocalMembership.Count > 0)
            {
                Report.GPOLocalMembership.Sort((GPOMembership a, GPOMembership b) =>
                {
                    int sort = String.Compare(a.GPOName, b.GPOName);
                    if (sort == 0)
                        sort = String.Compare(a.User, b.User);
                    if (sort == 0)
                        sort = String.Compare(a.MemberOf, b.MemberOf);
                    return sort;
                }
                );
                AddBeginTable("restricted groups list");
                AddHeaderText("GPO Name");
                AddHeaderText("User or group");
                AddHeaderText("Member of");
                CustomTable custTable2 = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Restricted groups list", out custTable2))
                    {
                        for (int i = 1; i < custTable2.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable2.Columns[i].Tooltip))
                                AddHeaderText(custTable2.Columns[i].Header, custTable2.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable2.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();

                foreach (GPOMembership membership in Report.GPOLocalMembership)
                {
                    AddBeginRow();
                    if (custTable2 != null && custTable2.GetKeyLinkedSection(membership.GPOName, out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(membership.GPOName);
                        Add("</a>");
                        Add("</td>");

                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), membership.GPOName, ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                        AddCellText(membership.GPOName);
                    AddCellText(membership.User);
                    AddCellText(membership.MemberOf);
                    if (custTable2 != null)
                    {
                        for (int i = 1; i < custTable2.Columns.Count; i++)
                        {
                            if (custTable2.Columns[i].Values.ContainsKey(membership.GPOName))
                                AddCellText(custTable2.Columns[i].Values[membership.GPOName]);
                            else
                                AddCellText("");
                        }
                        
                    }
                    AddEndRow();
                }
                AddEndTable();
            }

            GenerateSubSection("Security settings", "lsasettings");
            AddParagraph(@"A GPO can be used to deploy security settings to workstations.<br>The best practice out of the default security baseline is reported in <span class=""ticked"">green</span>.<br>The following settings in <span class=""unticked"">red</span> are unsual and may need to be reviewed.<br>Each setting is accompagnied which its value and a link to the GPO explanation.");
            AddBeginTable("Security settings list");
            AddHeaderText("Policy Name");
            AddHeaderText("Setting");
            AddHeaderText("Value");
            CustomTable custTable = null;
            if (CustomData != null)
            {
                if (CustomData.GetTable("Security settings list", out custTable))
                {
                    for (int i = 1; i < custTable.Columns.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                            AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                        else
                            AddHeaderText(custTable.Columns[i].Header);
                    }
                }
            }
            AddBeginTableData();
            if (Report.GPOLsaPolicy != null)
            {
                foreach (GPPSecurityPolicy policy in Report.GPOLsaPolicy)
                {
                    foreach (GPPSecurityPolicyProperty property in policy.Properties)
                    {
                        AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(policy.GPOName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(policy.GPOName);
                            Add("</a>");
                            if (!string.IsNullOrEmpty(policy.GPOId))
                            {
                                if (!Report.GPOInfoDic.ContainsKey(policy.GPOId))
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                    return;
                                }
                                var refGPO = Report.GPOInfoDic[policy.GPOId];
                                if (refGPO.IsDisabled)
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                }
                                if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                                {
                                    AddBeginTooltip(true);
                                    Add("<div class='text-left'>Linked to:<br><ul>");
                                    foreach (var i in refGPO.AppliedTo)
                                    {
                                        Add("<li>");
                                        AddEncoded(i);
                                        Add("</li>");
                                    }
                                    Add("</ul></div>");
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(policy.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                                else
                                {
                                    Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                    AddBeginTooltip();
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(policy.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                            }
                            Add("</td>");
                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(policy.GPOName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddGPOName(policy);
                        Add(@"<td class='text'>");
                        Add(GetLinkForLsaSetting(property.Property));
                        Add(@"</td>");
                        AddLsaSettingsValue(property.Property, property.Value);
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(policy.GPOName))
                                    AddCellText(custTable.Columns[i].Values[policy.GPOName]);
                                else
                                    AddCellText("");
                            }
                            
                        }
                        AddEndRow();
                    }
                }
            }
            AddEndTable();

            if (Report.version >= new Version(2, 8))
            {
                GenerateSubSection("Audit settings", "auditsettings");
                AddParagraph(@"Audit settings allow the system to generate logs which are useful to detect intrusions. Here are the settings found in GPO.");
                AddParagraph("Simple audit events are <a class=\"hyperlink\" href='https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-gpsb/01f8e057-f6a8-4d6e-8a00-99bcd241b403'>described here</a> and Advanced audit events are <a href='https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-gpac/77878370-0712-47cd-997d-b07053429f6d'>described here</a>");
                AddParagraph("You can get a list of all audit settings with the command line: <code>auditpol.exe /get /category:*</code> (<a class=\"hyperlink\" href='https://blogs.technet.microsoft.com/askds/2011/03/11/getting-the-effective-audit-policy-in-windows-7-and-2008-r2/'>source</a>)");
                AddParagraph("Simple audit settings are located in: Computer Configuration / Policies / Windows Settings / Security Settings / Local Policies / Audit Policy. Simple audit settings are named [Simple Audit].");
                AddParagraph("Advanced audit settings are located in: Computer Configuration / Policies / Windows Settings / Security Settings / Advanced Audit Policy Configuration. There category is displayed below.");
                AddBeginTable("Audit settings list");
                AddHeaderText("Policy Name");
                AddHeaderText("Category");
                AddHeaderText("Setting");
                AddHeaderText("Value");
                custTable = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Audit settings list", out custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();
                if (Report.GPOAuditSimple != null)
                {
                    foreach (var a in Report.GPOAuditSimple)
                    {
                        AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(a.GPOName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(a.GPOName);
                            Add("</a>");
                            if (!string.IsNullOrEmpty(a.GPOId))
                            {
                                if (!Report.GPOInfoDic.ContainsKey(a.GPOId))
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                    return;
                                }
                                var refGPO = Report.GPOInfoDic[a.GPOId];
                                if (refGPO.IsDisabled)
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                }
                                if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                                {
                                    AddBeginTooltip(true);
                                    Add("<div class='text-left'>Linked to:<br><ul>");
                                    foreach (var i in refGPO.AppliedTo)
                                    {
                                        Add("<li>");
                                        AddEncoded(i);
                                        Add("</li>");
                                    }
                                    Add("</ul></div>");
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(a.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                                else
                                {
                                    Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                    AddBeginTooltip();
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(a.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                            }
                            Add("</td>");
                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(a.GPOName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddGPOName(a);
                        AddCellText("[Simple Audit]");
                        AddCellText(GetAuditSimpleDescription(a.Category));
                        AddCellText(GetAuditSimpleValue(a.Value));
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(a.GPOName))
                                    AddCellText(custTable.Columns[i].Values[a.GPOName]);
                                else
                                    AddCellText("");
                            }
                            
                        }
                        AddEndRow();
                    }
                }
                if (Report.GPOAuditSimple != null)
                {
                    foreach (var a in Report.GPOAuditAdvanced)
                    {
                        AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(a.GPOName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(a.GPOName);
                            Add("</a>");
                            if (!string.IsNullOrEmpty(a.GPOId))
                            {
                                if (!Report.GPOInfoDic.ContainsKey(a.GPOId))
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                    return;
                                }
                                var refGPO = Report.GPOInfoDic[a.GPOId];
                                if (refGPO.IsDisabled)
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                }
                                if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                                {
                                    AddBeginTooltip(true);
                                    Add("<div class='text-left'>Linked to:<br><ul>");
                                    foreach (var i in refGPO.AppliedTo)
                                    {
                                        Add("<li>");
                                        AddEncoded(i);
                                        Add("</li>");
                                    }
                                    Add("</ul></div>");
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(a.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                                else
                                {
                                    Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                    AddBeginTooltip();
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(a.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                            }
                            Add("</td>");
                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(a.GPOName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddGPOName(a);
                        AddCellText(GetAuditAdvancedCategory(a.SubCategory));
                        AddCellText(GetAuditAdvancedDescription(a.SubCategory));
                        AddCellText(GetAuditSimpleValue(a.Value));
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(a.GPOName))
                                    AddCellText(custTable.Columns[i].Values[a.GPOName]);
                                else
                                    AddCellText("");
                            }
                            
                        }
                        AddEndRow();
                    }
                }
                AddEndTable();
            }

            GenerateSubSection("Privileges", "gpoprivileges");
            AddParagraph("Giving privileges in a GPO is a way to become administrator without being part of a group.<br>For example, SeTcbPriviledge give the right to act as SYSTEM, which has more privileges than the administrator account.");
            if (Report.GPPRightAssignment != null && Report.GPPRightAssignment.Count > 0)
            {
                AddBeginTable("Privileges list");
                AddHeaderText("GPO Name");
                AddHeaderText("Privilege");
                AddHeaderText("Members");
                custTable = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("Privileges list", out custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();

                foreach (GPPRightAssignment right in Report.GPPRightAssignment)
                {
                    AddBeginRow();
                    if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(right.GPOName), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(right.GPOName);
                        Add("</a>");
                        if (!string.IsNullOrEmpty(right.GPOId))
                        {
                            if (!Report.GPOInfoDic.ContainsKey(right.GPOId))
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                return;
                            }
                            var refGPO = Report.GPOInfoDic[right.GPOId];
                            if (refGPO.IsDisabled)
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                            }
                            if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                            {
                                AddBeginTooltip(true);
                                Add("<div class='text-left'>Linked to:<br><ul>");
                                foreach (var i in refGPO.AppliedTo)
                                {
                                    Add("<li>");
                                    AddEncoded(i);
                                    Add("</li>");
                                }
                                Add("</ul></div>");
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(right.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                            else
                            {
                                Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                AddBeginTooltip();
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(right.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                        }
                        Add("</td>");
                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(right.GPOName), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                        AddGPOName(right);
                    AddCellText(right.Privilege);
                    AddCellText(right.User);
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(right.GPOName))
                                AddCellText(custTable.Columns[i].Values[right.GPOName]);
                            else
                                AddCellText("");
                        }
                        
                    }
                    AddEndRow();
                }
                AddEndTable();
            }

            if (Report.version >= new Version(2, 8))
            {
                GenerateSubSection("Login", "gpologin");
                AddParagraph("Login authorization and restriction can be set by GPO. Indeed, by default, everyone is allowed to login on every computer except domain controllers. Defining login restriction is a way to have different isolated tiers. Here are the settings found in GPO.");
                if (Report.GPPLoginAllowedOrDeny != null && Report.GPPLoginAllowedOrDeny.Count > 0)
                {
                    AddBeginTable("Login list");
                    AddHeaderText("GPO Name");
                    AddHeaderText("Privilege");
                    AddHeaderText("Members");
                    custTable = null;
                    if (CustomData != null)
                    {
                        if (CustomData.GetTable("Login list", out custTable))
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                    AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                                else
                                    AddHeaderText(custTable.Columns[i].Header);
                            }
                        }
                    }
                    AddBeginTableData();

                    foreach (GPPRightAssignment right in Report.GPPLoginAllowedOrDeny)
                    {
                        AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(right.GPOName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(right.GPOName);
                            Add("</a>");
                            if (!string.IsNullOrEmpty(right.GPOId))
                            {
                                if (!Report.GPOInfoDic.ContainsKey(right.GPOId))
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                    return;
                                }
                                var refGPO = Report.GPOInfoDic[right.GPOId];
                                if (refGPO.IsDisabled)
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                }
                                if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                                {
                                    AddBeginTooltip(true);
                                    Add("<div class='text-left'>Linked to:<br><ul>");
                                    foreach (var i in refGPO.AppliedTo)
                                    {
                                        Add("<li>");
                                        AddEncoded(i);
                                        Add("</li>");
                                    }
                                    Add("</ul></div>");
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(right.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                                else
                                {
                                    Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                    AddBeginTooltip();
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(right.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                            }
                            Add("</td>");
                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(right.GPOName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddGPOName(right);
                        Add(@"<td class='text'>");
                        AddPrivilegeToGPO(right.Privilege);
                        Add(@"</td>");
                        AddCellText(right.User);
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(right.GPOName))
                                    AddCellText(custTable.Columns[i].Values[right.GPOName]);
                                else
                                    AddCellText("");
                            }
                            
                        }
                        AddEndRow();
                    }
                    AddEndTable();
                }
            }

            GenerateSubSection("GPO Login script", "gpologin");
            AddParagraph("A GPO login script is a way to force the execution of data on behalf of users. Only enabled users are analyzed.");
            if (Report.GPOLoginScript != null && Report.GPOLoginScript.Count > 0)
            {
                AddBeginTable("GPO login script list");
                AddHeaderText("GPO Name");
                AddHeaderText("Action");
                AddHeaderText("Source");
                AddHeaderText("Command line");
                AddHeaderText("Parameters");
                custTable = null;
                if (CustomData != null)
                {
                    if (CustomData.GetTable("GPO login script list", out custTable))
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                            else
                                AddHeaderText(custTable.Columns[i].Header);
                        }
                    }
                }
                AddBeginTableData();

                foreach (HealthcheckGPOLoginScriptData loginscript in Report.GPOLoginScript)
                {
                    AddBeginRow();
                    if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(loginscript.GPOName), out var targetSection))
                    {
                        Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                        Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                        Add(@""">");
                        AddEncoded(loginscript.GPOName);
                        Add("</a>");
                        if (!string.IsNullOrEmpty(loginscript.GPOId))
                        {
                            if (!Report.GPOInfoDic.ContainsKey(loginscript.GPOId))
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                return;
                            }
                            var refGPO = Report.GPOInfoDic[loginscript.GPOId];
                            if (refGPO.IsDisabled)
                            {
                                Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                            }
                            if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                            {
                                AddBeginTooltip(true);
                                Add("<div class='text-left'>Linked to:<br><ul>");
                                foreach (var i in refGPO.AppliedTo)
                                {
                                    Add("<li>");
                                    AddEncoded(i);
                                    Add("</li>");
                                }
                                Add("</ul></div>");
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(loginscript.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                            else
                            {
                                Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                AddBeginTooltip();
                                Add("<div class='text-left'>Technical id:<br>");
                                AddEncoded(loginscript.GPOId);
                                Add("</div>");
                                AddEndTooltip();
                            }
                        }
                        Add("</td>");
                        AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(loginscript.GPOName), ShowModalType.XL);
                        GenerateAdvancedCustomSection(targetSection);
                        AddEndModal();
                    }
                    else
                        AddGPOName(loginscript);
                    AddCellText(loginscript.Action);
                    AddCellText(loginscript.Source);
                    AddCellText(loginscript.CommandLine);
                    AddCellText(loginscript.Parameters);
                    if (custTable != null)
                    {
                        for (int i = 1; i < custTable.Columns.Count; i++)
                        {
                            if (custTable.Columns[i].Values.ContainsKey(loginscript.GPOName))
                                AddCellText(custTable.Columns[i].Values[loginscript.GPOName]);
                            else
                                AddCellText("");
                        }
                        
                    }
                    AddEndRow();
                }
                AddEndTable();
            }
            if (Report.version >= new Version(2, 7, 0, 0))
            {
                GenerateSubSection("GPO Deployed Files", "gpodeployedfiles");
                AddParagraph("A GPO can be used to deploy applications or copy files. These files may be controlled by a third party to control the execution of local programs.");
                if (Report.GPPFileDeployed != null && Report.GPPFileDeployed.Count > 0)
                {
                    AddBeginTable("GPO deployed files list");
                    AddHeaderText("GPO Name");
                    AddHeaderText("Type");
                    AddHeaderText("File");
                    custTable = null;
                    if (CustomData != null)
                    {
                        if (CustomData.GetTable("GPO deployed files list", out custTable))
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                                    AddHeaderText(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                                else
                                    AddHeaderText(custTable.Columns[i].Header);
                            }
                        }
                    }
                    AddBeginTableData();

                    foreach (var file in Report.GPPFileDeployed)
                    {
                        AddBeginRow();
                        if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(file.GPOName), out var targetSection))
                        {
                            Add(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            Add(GenerateModalAdminGroupIdFromGroupName(targetSection.Id));
                            Add(@""">");
                            AddEncoded(file.GPOName);
                            Add("</a>");
                            if (!string.IsNullOrEmpty(file.GPOId))
                            {
                                if (!Report.GPOInfoDic.ContainsKey(file.GPOId))
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                    return;
                                }
                                var refGPO = Report.GPOInfoDic[file.GPOId];
                                if (refGPO.IsDisabled)
                                {
                                    Add(@" <span class=""font-weight-light"">[Disabled]</span>");
                                }
                                if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                                {
                                    AddBeginTooltip(true);
                                    Add("<div class='text-left'>Linked to:<br><ul>");
                                    foreach (var i in refGPO.AppliedTo)
                                    {
                                        Add("<li>");
                                        AddEncoded(i);
                                        Add("</li>");
                                    }
                                    Add("</ul></div>");
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(file.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                                else
                                {
                                    Add(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                                    AddBeginTooltip();
                                    Add("<div class='text-left'>Technical id:<br>");
                                    AddEncoded(file.GPOId);
                                    Add("</div>");
                                    AddEndTooltip();
                                }
                            }
                            Add("</td>");
                            AddBeginModal(GenerateModalAdminGroupIdFromGroupName(targetSection.Id), ReportHelper.Encode(file.GPOName), ShowModalType.XL);
                            GenerateAdvancedCustomSection(targetSection);
                            AddEndModal();
                        }
                        else
                            AddGPOName(file);
                        AddCellText(file.Type);
                        AddCellText(file.FileName);
                        if (custTable != null)
                        {
                            for (int i = 1; i < custTable.Columns.Count; i++)
                            {
                                if (custTable.Columns[i].Values.ContainsKey(file.GPOName))
                                    AddCellText(custTable.Columns[i].Values[file.GPOName]);
                                else
                                    AddCellText("");
                            }
                            
                        }
                        AddEndRow();
                    }
                    AddEndTable();
                }
            }

            if (CustomData != null)
            {
                if (CustomData.GetSection("GPOInformation", out var section))
                {
                    GenerateAdvancedCustomSection(section);
                    CustomData.InformationSections.Remove(section);
                }
            }
        }

        private string GetAuditSimpleDescription(string category)
        {
            switch (category)
            {
                case "AuditSystemEvents":
                    return "Audit system events";
                case "AuditLogonEvents":
                    return "Audit logon events";
                case "AuditPrivilegeUse":
                    return "Audit privilege use";
                case "AuditPolicyChange":
                    return "Audit policy change";
                case "AuditAccountManage":
                    return "Audit account management";
                case "AuditProcessTracking":
                    return "Audit process tracking";
                case "AuditDSAccess":
                    return "Audit directory service access";
                case "AuditObjectAccess":
                    return "Audit object access";
                case "AuditAccountLogon":
                    return "Audit account logon events";
                default:
                    return category;
            }

        }

        private class AuditAdvancedDescription
        {
            public string target { get; set; }
            public string subcategory { get; set; }
            public AuditAdvancedDescription(string t, string s)
            {
                target = t;
                subcategory = s;
            }
        }
        static Dictionary<Guid, AuditAdvancedDescription> auditAdvancedDescription = new Dictionary<Guid, AuditAdvancedDescription>
        {
            {new Guid("{0CCE9213-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("System", "IPsec Driver")},
            {new Guid("{0CCE9212-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("System", "System Integrity")},
            {new Guid("{0CCE9211-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("System", "Security System Extension")},
            {new Guid("{0CCE9210-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("System", "Security State Change")},
            {new Guid("{0CCE9214-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("System", "Other System Events")},
            {new Guid("{0CCE9243-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "Network Policy Server")},
            {new Guid("{0CCE921C-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "Other Logon/Logoff")},
            {new Guid("{0CCE921B-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "Special Logon")},
            {new Guid("{0CCE921A-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "IPsec Extended Mode")},
            {new Guid("{0CCE9219-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "IPsec Quick Mode")},
            {new Guid("{0CCE9218-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "IPsec Main Mode")},
            {new Guid("{0CCE9217-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "Account Lockout")},
            {new Guid("{0CCE9216-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "Logoff")},
            {new Guid("{0CCE9215-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Logon/Logoff", "Logon")},
            {new Guid("{0CCE9223-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Handle Manipulation")},
            {new Guid("{0CCE9244-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Detailed File Share")},
            {new Guid("{0CCE9227-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Other Object Access")},
            {new Guid("{0CCE9226-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Filtering Platform Connection")},
            {new Guid("{0CCE9225-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Filtering Platform Packet Drop")},
            {new Guid("{0CCE9224-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "File Share")},
            {new Guid("{0CCE9222-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Application Generated")},
            {new Guid("{0CCE9221-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Certification Services")},
            {new Guid("{0CCE9220-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "SAM")},
            {new Guid("{0CCE921F-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Kernel Object")},
            {new Guid("{0CCE921E-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "Registry")},
            {new Guid("{0CCE921D-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Object Access", "File System")},
            {new Guid("{0CCE9229-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Privilege Use", "Non Sensitive Privilege Use")},
            {new Guid("{0CCE922A-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Privilege Use", "Other Privilege Use Events")},
            {new Guid("{0CCE9228-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Privilege Use", "Sensitive Privilege Use")},
            {new Guid("{0CCE922D-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Detailed Tracking", "DPAPI Activity")},
            {new Guid("{0CCE922C-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Detailed Tracking", "Process Termination")},
            {new Guid("{0CCE922B-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Detailed Tracking", "Process Creation")},
            {new Guid("{0CCE922E-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Detailed Tracking", "RPC Events")},
            {new Guid("{0CCE9232-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Policy Change", "MPSSVC Rule-Level Policy Change")},
            {new Guid("{0CCE9234-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Policy Change", "Other Policy Change Events")},
            {new Guid("{0CCE9233-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Policy Change", "Filtering Platform Policy Change")},
            {new Guid("{0CCE922F-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Policy Change", "Audit Policy Change")},
            {new Guid("{0CCE9231-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Policy Change", "Authorization Policy Change")},
            {new Guid("{0CCE9230-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Policy Change", "Authentication Policy Change")},
            {new Guid("{0CCE923A-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Management", "Other Account Management Events")},
            {new Guid("{0CCE9239-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Management", "Application Group Management")},
            {new Guid("{0CCE9238-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Management", "Distribution Group Management")},
            {new Guid("{0CCE9237-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Management", "Security Group Management")},
            {new Guid("{0CCE9236-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Management", "Computer Account Management")},
            {new Guid("{0CCE9235-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Management", "User Account Management")},
            {new Guid("{0CCE923E-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("DS Access", "Detailed Directory Service Replication")},
            {new Guid("{0CCE923B-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("DS Access", "Directory Service Access")},
            {new Guid("{0CCE923D-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("DS Access", "Directory Service Replication")},
            {new Guid("{0CCE923C-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("DS Access", "Directory Service Changes")},
            {new Guid("{0CCE9241-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Logon", "Other Account Logon Events")},
            {new Guid("{0CCE9240-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Logon", "Kerberos Service Ticket Operations")},
            {new Guid("{0CCE923F-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Logon", "Credential Validation")},
            {new Guid("{0CCE9242-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("Account Logon", "Kerberos Authentication Service")},
            {new Guid("{0CCE9245-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("System", "Removable Storage")},
            {new Guid("{0CCE9246-69AE-11D9-BED3-505054503030}"), new AuditAdvancedDescription("System", "Central Access Policy Staging")},
            {new Guid("{0cce9247-69ae-11d9-bed3-505054503030}"), new AuditAdvancedDescription("System", "User/Device Claims")},
            {new Guid("{0cce9248-69ae-11d9-bed3-505054503030}"), new AuditAdvancedDescription("System", "PNP Activity")},
            {new Guid("{0cce9249-69ae-11d9-bed3-505054503030}"), new AuditAdvancedDescription("System", "Group Membership")},

        };

        private string GetAuditAdvancedCategory(Guid guid)
        {
            if (auditAdvancedDescription.ContainsKey(guid))
            {
                return auditAdvancedDescription[guid].target;
            }
            else
            {
                return "Undocumented";
            }
        }

        private string GetAuditAdvancedDescription(Guid guid)
        {
            if (auditAdvancedDescription.ContainsKey(guid))
            {
                return auditAdvancedDescription[guid].subcategory;
            }
            else
            {
                return "Undocumented (" + guid + ")";
            }
        }

        private string GetAuditSimpleValue(int value)
        {
            switch (value)
            {
                case 0: return "Unchanged";
                default:
                    return "No Auditing";
                case 1:
                    return "Success";
                case 2:
                    return "Failure";
                case 3:
                    return "Success and Failure";
            }
        }

        private string GetAuditUserValue(int value)
        {
            if (value == 1)
                return "Success";
            if (value == 4)
                return "Failure";
            if (value == 5)
                return "Success and Failure";
            if (value == 0)
                return "None";
            var match = new List<string>();
            if ((value & 1) != 0)
            {
                match.Add("Success");
            }
            if ((value & 2) != 0)
            {
                match.Add("Exclude Success");
            }
            if ((value & 4) != 0)
            {
                match.Add("Failure");
            }
            if ((value & 8) != 0)
            {
                match.Add("Exclude Failure");
            }
            return string.Join(" and ", match.ToArray());
        }

        private void AddPrivilegeToGPO(string privilege)
        {
            string gpodescr = null;
            if (string.Equals(privilege, "SeInteractiveLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Allow log on locally";
            }
            else if (string.Equals(privilege, "SeRemoteInteractiveLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Allow logon through Remote Desktop Services";
            }
            else if (string.Equals(privilege, "SeNetworkLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Access this computer from the network";
            }
            else if (string.Equals(privilege, "SeServiceLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Log on as a service";
            }
            else if (string.Equals(privilege, "SeBatchLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Log on as a batch job";
            }
            else if (string.Equals(privilege, "SeDenyServiceLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Deny log on as a service";
            }
            else if (string.Equals(privilege, "SeDenyRemoteInteractiveLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Deny logon through Remote Desktop Services";
            }
            else if (string.Equals(privilege, "SeDenyNetworkLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Deny access to this computer from the network";
            }
            else if (string.Equals(privilege, "SeDenyInteractiveLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Deny log on locally";
            }
            else if (string.Equals(privilege, "SeDenyBatchLogonRight", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Deny log on as a batch job";
            }
            else if (string.Equals(privilege, "SeDebugPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Debug programs";
            }
            else if (string.Equals(privilege, "SeBackupPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Back up files and directories";
            }
            else if (string.Equals(privilege, "SeCreateTokenPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Create a token object";
            }
            else if (string.Equals(privilege, "SeEnableDelegationPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Enable computer and user accounts to be trusted for delegation";
            }
            else if (string.Equals(privilege, "SeSyncAgentPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Synchronize directory service data";
            }
            else if (string.Equals(privilege, "SeTakeOwnershipPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Take ownership of files or other objects";
            }
            else if (string.Equals(privilege, "SeTcbPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Act as part of the operating system";
            }
            else if (string.Equals(privilege, "SeTrustedCredManAccessPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Access Credential Manager as a trusted caller";
            }
            else if (string.Equals(privilege, "SeMachineAccountPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Add workstations to domain";
            }
            else if (string.Equals(privilege, "SeLoadDriverPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Load and unload device drivers";
            }
            else if (string.Equals(privilege, "SeRestorePrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Restore files and directories";
            }
            else if (string.Equals(privilege, "SeImpersonatePrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Impersonate a client after authentication";
            }
            else if (string.Equals(privilege, "SeAssignPrimaryTokenPrivilege", StringComparison.OrdinalIgnoreCase))
            {
                gpodescr = "Replace a process level token";
            }
            if (gpodescr == null)
            {
                AddEncoded(privilege);
            }
            else
            {
                Add(gpodescr);
                AddBeginTooltip();
                AddEncoded(privilege);
                AddEndTooltip();
            }

        }
        #endregion GPO
    }
}
