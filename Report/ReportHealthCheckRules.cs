﻿using PingCastle.Healthcheck;
using PingCastle.Rules;
using PingCastle.template;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace PingCastle.Report
{
    public class ReportHealthCheckRules : ReportBase
    {

        public string GenerateRawContent()
        {
            sb.Length = 0;
            GenerateContent();
            return sb.ToString();
        }

        protected override void GenerateFooterInformation()
        {
        }

		protected override void GenerateTitleInformation()
		{
			Add("RisX Health Check rules - ");
			Add(DateTime.Now.ToString("yyyy-MM-dd"));
		}

        protected override void ReferenceJSAndCSS()
        {
            AddStyle(TemplateManager.LoadReportBaseCss());
            AddStyle(TemplateManager.LoadReportHealthCheckRulesCss());
            AddScript(TemplateManager.LoadJqueryDatatableJs());
            AddScript(TemplateManager.LoadDatatableJs());
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
	$('table').not('.model_table').DataTable(
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


});");
        }

        protected override void GenerateBodyInformation()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = version.ToString(4);
#if DEBUG
            versionString += " Beta";
#endif
			GenerateNavigation("Healthcheck Rules", null, DateTime.Now);
			GenerateAbout(@"<p><strong>Generated with <a class=""hyperlink"" href=""https://10root.com"">10Root RisX</a> powered by <a class=""hyperlink"" href=""https://www.pingcastle.com"">Ping Castle</a> all rights reserved</strong></p>
<p>Open source components:</p>
<ul>
<li><a class=""hyperlink"" href=""https://getbootstrap.com/"">Bootstrap</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a class=""hyperlink"" href=""https://datatables.net/"">DataTables</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a class=""hyperlink"" href=""https://popper.js.org/"">Popper.js</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a class=""hyperlink"" href=""https://jquery.org"">JQuery</a> licensed under the <a class=""hyperlink"" href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
</ul>");
            Add(@"
<div id=""wrapper"" class=""container well"">
	<noscript>
		<div class=""alert alert-warning"">
			<p>RisX reports work best with Javascript enabled.</p>
		</div>
	</noscript>
<div class=""row""><div class=""col-lg-12""><h1>Rules evaluated during RisX Healthcheck</h1>
			<h3>Date: " + DateTime.Now.ToString("yyyy-MM-dd") + @" - Engine version: " + versionString + @"</h3>
</div></div>
");
            GenerateContent();
            Add(@"
</div>
");
        }

        private void GenerateContent()
        {
            GenerateRiskModelPanel();
            GenerateSubSection("Stale Objects");
            GenerateRuleAccordeon(RiskRuleCategory.StaleObjects);
            GenerateSubSection("Privileged Accounts");
            GenerateRuleAccordeon(RiskRuleCategory.PrivilegedAccounts);
            GenerateSubSection("Trusts");
            GenerateRuleAccordeon(RiskRuleCategory.Trusts);
            GenerateSubSection("Anomalies");
            GenerateRuleAccordeon(RiskRuleCategory.Anomalies);
            GenerateSubSection("ANSSI Rules mapping");
            GenerateANSSISection();
        }

        ResourceManager _resourceManager = new ResourceManager("PingCastle.Healthcheck.Rules.RuleDescription", typeof(RuleBase<>).Assembly);

        private void GenerateRuleAccordeon(RiskRuleCategory category)
        {
            var rules = RuleSet<HealthcheckData>.Rules;
            rules.Sort((RuleBase<HealthcheckData> a, RuleBase<HealthcheckData> b)
                =>
                {
                    int c = ReportHelper.GetEnumDescription(a.Model).CompareTo(ReportHelper.GetEnumDescription(b.Model));
                    if (c == 0)
                        c = a.Title.CompareTo(b.Title);
                    return c;
                }
            );
            var m = RiskModelCategory.Unknown;
            var data = new List<KeyValuePair<RiskModelCategory, List<RuleBase<HealthcheckData>>>>();
            foreach (var rule in rules)
            {
                if (rule.Category == category)
                {
                    if (rule.Model != m)
                    {
                        m = rule.Model;
                        data.Add(new KeyValuePair<RiskModelCategory, List<RuleBase<HealthcheckData>>>(rule.Model, new List<RuleBase<HealthcheckData>>()));
                    }
                    data[data.Count - 1].Value.Add(rule);
                }
            }
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
		<p>Each line represents a rule. Click on a rule to expand it and show the details of it.
		</p>
");
            foreach (var d in data)
            {
                Add(@"
		<div class=""row""><div class=""col-lg-12 mt-3"">
		<h3>");
                Add(ReportHelper.GetEnumDescription(d.Key));
                Add(@"
		</h3>
");
                string description = _resourceManager.GetString(d.Key.ToString() + "_Detail");
                if (!string.IsNullOrEmpty(description))
                {
                    Add(@"
		<div class=""row""><div class=""col-lg-12"">
		<p>");
                    Add(description);
                    Add(@"
		</p>
");
                }
                GenerateAccordion("rules" + d.Key.ToString(), () =>
                {
                    foreach (var rule in d.Value)
                    {
                        GenerateIndicatorPanelDetail(d.Key, rule);
                    }
                });
            }
        }

        protected void GenerateRiskModelPanel()
        {
            Add(@"
		<div class=""row""><div class=""col-lg-12"">
			<a data-toggle=""collapse"" data-target=""#riskModel"">
				<h2>Risk model</h2>
			</a>
		</div></div>
		<div class=""row""><div class=""col-lg-12"">
		<p>This model regroup all rules per category. It summarize what checks are performed. Click on a cell to show all rules associated to a category.
		</p>
		</div></div>
		<div class=""row collapse show"" id=""riskModel"">
			<div class=""col-md-12 table-responsive"">
				<table class=""model_table"">
					<thead><tr><th>Stale Objects</th><th>Privileged accounts</th><th>Trusts</th><th>Anomalies</th></tr></thead>
					<tbody>
");
            var riskmodel = new Dictionary<RiskRuleCategory, List<RiskModelCategory>>();
            foreach (RiskRuleCategory category in Enum.GetValues(typeof(RiskRuleCategory)))
            {
                riskmodel[category] = new List<RiskModelCategory>();
            }
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; ; i++)
                {
                    int id = (1000 * j + 1000 + i);
                    if (Enum.IsDefined(typeof(RiskModelCategory), id))
                    {
                        riskmodel[(RiskRuleCategory)j].Add((RiskModelCategory)id);
                    }
                    else
                        break;
                }
            }
            foreach (RiskRuleCategory category in Enum.GetValues(typeof(RiskRuleCategory)))
            {
                riskmodel[category].Sort(
                                        (RiskModelCategory a, RiskModelCategory b) =>
                                        {
                                            return string.Compare(ReportHelper.GetEnumDescription(a), ReportHelper.GetEnumDescription(b));
                                        });
            }
            for (int i = 0; ; i++)
            {
                string line = "<tr>";
                bool HasValue = false;
                foreach (RiskRuleCategory category in Enum.GetValues(typeof(RiskRuleCategory)))
                {
                    if (i < riskmodel[category].Count)
                    {
                        HasValue = true;
                        RiskModelCategory model = riskmodel[category][i];
                        int score = 0;
                        int numrules = 0;
                        var rulematched = new List<RuleBase<HealthcheckData>>();
                        foreach (var rule in RuleSet<HealthcheckData>.Rules)
                        {
                            if (rule.Model == model)
                            {
                                numrules++;
                                score += rule.Points;
                                rulematched.Add(rule);
                            }
                        }
                        string tdclass = "";
                        tdclass = "model_good";
                        string modelstring = ReportHelper.GetEnumDescription(model);
                        string tooltip = modelstring + " [Rules: " + numrules + "]";
                        string tooltipdetail = null;
                        rulematched.Sort((RuleBase<HealthcheckData> a, RuleBase<HealthcheckData> b)
                            =>
                        {
                            return a.Points.CompareTo(b.Points);
                        });
                        foreach (var rule in rulematched)
                        {
                            tooltipdetail += "<li>" + ReportHelper.Encode(rule.Title) + "</li><br>";
                        }
                        line += "<td class=\"model_cell " + tdclass + "\"><div class=\"div_model\" placement=\"auto\" data-toggle=\"popover\" title=\"" +
                            tooltip + "\" data-html=\"true\" data-content=\"" +
                            "<p>" + _resourceManager.GetString(model.ToString() + "_Detail") + "</p>" + (String.IsNullOrEmpty(tooltipdetail) ? "No rule matched" : "<p><ul>" + tooltipdetail + "</ul></p>") + "\"><span class=\"small\">" + modelstring + "</span></div></td>";
                    }
                    else
                        line += "<td class=\"model_empty_cell\"></td>";
                }
                line += "</tr>";
                if (HasValue)
                    Add(line);
                else
                    break;
            }
            Add(@"
					</tbody>
				</table>
			</div>
		</div>");
        }

        private void GenerateIndicatorPanelDetail(RiskModelCategory category, RuleBase<HealthcheckData> hcrule)
        {
            string safeRuleId = hcrule.RiskId.Replace("$", "dollar");
            GenerateAccordionDetail("rules" + safeRuleId, "rules" + category.ToString(), hcrule.Title + " (" + hcrule.RiskId + ")", null, true,
                () =>
                {
                    Add("<h3>");
                    Add(hcrule.Title);
                    Add("</h3>\r\n<strong>Rule ID:</strong><p class=\"text-justify\">");
                    Add(hcrule.RiskId);
                    Add("</p>\r\n<strong>Description:</strong><p class=\"text-justify\">");
                    Add(NewLineToBR(hcrule.Description));
                    Add("</p>\r\n<strong>Technical explanation:</strong><p class=\"text-justify\">");
                    Add(NewLineToBR(hcrule.TechnicalExplanation));
                    Add("</p>\r\n<strong>Advised solution:</strong><p class=\"text-justify\">");
                    Add(NewLineToBR(hcrule.Solution));
                    Add(@"</p>");
                    object[] models = hcrule.GetType().GetCustomAttributes(typeof(RuleIntroducedInAttribute), true);
                    if (models != null && models.Length != 0)
                    {
                        var model = (PingCastle.Rules.RuleIntroducedInAttribute)models[0];
                        Add("<strong>Introduced in:</strong>");
                        Add("<p class=\"text-justify\">");
                        Add(model.Version.ToString());
                        Add(@"</p>");
                    }
                    Add("<strong>Points:</strong><p>");
                    Add(NewLineToBR(hcrule.GetComputationModelString()));
                    Add("</p>\r\n");
                    if (!String.IsNullOrEmpty(hcrule.Documentation))
                    {
                        Add("<strong>Documentation:</strong><p>");
                        Add(hcrule.Documentation);
                        Add("</p>");
                    }
                });
        }

        private class DurANSSIRegroup
        {
            public string ID { get; set; }
            public List<int> Levels { get; set; }
            public int MinLevel { get; set; }
            public string Label { get; set; }
            public List<RuleBase<HealthcheckData>> PCRules { get; set; }

            public DurANSSIRegroup(RuleDurANSSIAttribute rule, List<RuleBase<HealthcheckData>> data)
            {
                PCRules = new List<RuleBase<HealthcheckData>>();
                ID = rule.ID;
                Label = rule.ANSSILabel;
                Levels = new List<int>();
                Levels.Add(rule.Level);
                MinLevel = rule.Level;
                PCRules = new List<RuleBase<HealthcheckData>>();
                PCRules.AddRange(data);
            }

            public void Add(RuleDurANSSIAttribute rule, List<RuleBase<HealthcheckData>> data)
            {
                if (!Levels.Contains(rule.Level))
                    Levels.Add(rule.Level);
                PCRules.AddRange(data);
                if (MinLevel > rule.Level)
                    MinLevel = rule.Level;
            }
        }

        private void GenerateANSSISection()
        {
            AddParagraph("This is the mapping of the ANSSI rules with RisX rules.");
            var reference = new Dictionary<RuleDurANSSIAttribute, List<RuleBase<HealthcheckData>>>();
            foreach (var rule in RuleSet<HealthcheckData>.Rules)
            {
                object[] frameworks = rule.GetType().GetCustomAttributes(typeof(RuleDurANSSIAttribute), true);
                foreach (RuleDurANSSIAttribute f in frameworks)
                {
                    if (!reference.ContainsKey(f))
                    {
                        reference[f] = new List<RuleBase<HealthcheckData>>();
                    }
                    reference[f].Add(rule);
                }
            }

            var perRuleId = new Dictionary<string, DurANSSIRegroup>();
            foreach (var rule in reference.Keys)
            {
                if (!perRuleId.ContainsKey(rule.ID))
                {
                    perRuleId[rule.ID] = new DurANSSIRegroup(rule, reference[rule]);
                }
                else
                {
                    perRuleId[rule.ID].Add(rule, reference[rule]);
                }
            }

            var regroup = new List<DurANSSIRegroup>(perRuleId.Values);
            regroup.Sort(
                (DurANSSIRegroup a, DurANSSIRegroup b)
                    =>
                {
                    int c = a.MinLevel.CompareTo(b.MinLevel);
                    if (c != 0)
                        return c;
                    return string.CompareOrdinal(a.Label, b.Label);
                });

            Add("<p>Number of ANSSI rules matched: ");
            Add(regroup.Count);
            Add("</p>");
            var count = new Dictionary<int, int>();
            foreach (var r in regroup)
            {
                if (!count.ContainsKey(r.MinLevel))
                    count[r.MinLevel] = 1;
                else
                    count[r.MinLevel]++;
            }
            Add("<div class='row'>");
            Add("<div class='col-lg-12'>");

            foreach (var l in count.Keys)
            {
                Add("<p>");
                Add("<span class=\"badge grade-");
                Add(l);
                Add("\">");
                Add(l);
                Add("</span>: ");
                Add(count[l]);
                Add("</p>");
            }
            Add("</div>");
            Add("</div>");
            foreach (var r in regroup)
            {
                Add("<div class='row'>");
                Add("<div class='col-lg-12'>");
                Add("<p>");
                r.Levels.Sort();
                foreach (var l in r.Levels)
                {
                    Add("<span class=\"badge grade-");
                    Add(l);
                    Add("\">");
                    Add(l);
                    Add("</span>");
                }
                Add(r.Label);
                Add(" (<a class=\"hyperlink\" href=\"https://www.cert.ssi.gouv.fr/uploads/guide-ad.html#");
                Add(r.ID);
                Add("\">link</a>)");
                Add("</p>");
                Add("</div>");
                Add("</div>");
                Add("<div class='row'>");
                Add("<div class='col-lg-12'>");
                Add("<p>ANSSI ID&nbsp;:");
                foreach (var l in r.Levels)
                {
                    Add(" <span class=\"text-monospace\">vuln");
                    Add(l);
                    Add("_");
                    Add(r.ID);
                    Add("</span>");
                }
                Add("</p>");
                Add("</div>");
                Add("</div>");
                Add("<div class='row'>");
                Add("<div class='col-lg-12'>");
                Add("<p>PingCastle ID&nbsp;:");
                r.PCRules.Sort((RuleBase<HealthcheckData> a, RuleBase<HealthcheckData> b) => { return string.CompareOrdinal(a.RiskId, b.RiskId); });
                foreach (var l in r.PCRules)
                {
                    Add(" <span class=\"text-monospace\">");
                    Add(l.RiskId);
                    Add("</span>");
                }
                Add("</p>");
                Add("</div>");
                Add("</div>");
            }
        }
    }
}
