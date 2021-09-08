using PingCastle.Addition;
using PingCastle.Addition.LogicEnteties;
using PingCastle.Addition.ReportEnteties;
using PingCastle.Addition.StructureEnteties;
using PingCastle.Healthcheck;
using PingCastle.Rules;
using System;
using System.Collections.Generic;

namespace PingCastle.Report
{
    public abstract class ReportRiskControls<T> : ReportBase where T : IRiskEvaluation
    {

        int GetRulesNumberForCategory(List<HealthcheckRiskRule> rules, RiskRuleCategory category)
        {
            int count = 0;
            foreach (var rule in rules)
            {
                if (rule.Category == category)
                    count++;
            }
            return count;
        }

        #region indicators

        protected void GenerateIndicators(IRiskEvaluation data, IList<IRuleScore> rules)
        {
            GenerateSubSection("Indicators");
            Add(@"
		<div class=""row"">
			<div class=""col-md-4"">
				<div class=""chart-gauge"">");
            GenerateGauge(data.GlobalScore);
            Add(@"</div>
			</div>
			<div class=""col-md-8"">
					<p class=""lead"">Domain Risk Level: ");
            Add(data.GlobalScore.ToString());
            Add(@" / 100</p>
					<p>It is the maximum score of the 4 indicators and one score cannot be higher than 100. The lower the better</p>
			</div>
		</div>
		<div class=""row indicators-border"">
");
			GenerateSubIndicator("Stale Object", data.GlobalScore, data.StaleObjectsScore, rules, RiskRuleCategory.StaleObjects, "It is about operations related to user or computer objects");
			GenerateSubIndicator("Trusts", data.GlobalScore, data.TrustScore, rules, RiskRuleCategory.Trusts, "It is about links between two Active Directories");
			GenerateSubIndicator("Privileged Accounts", data.GlobalScore, data.PrivilegiedGroupScore, rules, RiskRuleCategory.PrivilegedAccounts, "It is about administrators of the Active Directory");
			GenerateSubIndicator("Anomalies", data.GlobalScore, data.AnomalyScore, rules, RiskRuleCategory.Anomalies, "It is about specific security control points");

			CustomData.GenerateCustomCategoriesSubIndicators(data.GlobalScore);
			Add(@"
		</div>
");
        }

		void GenerateSubIndicator(string category, int globalScore, int score, IList<IRuleScore> rules, RiskRuleCategory RiskRuleCategory, string explanation)
		{
			int numrules = 0;
			if (rules != null)
			{
				foreach (var rule in rules)
				{
					if (rule.Category == RiskRuleCategory)
						numrules++;
				}
			}
			numrules += CustomData.CountCategoryHealthRules(RiskRuleCategory.ToString());
			GenerateSubIndicator(category, globalScore, score, numrules, explanation);
		}

        protected void GenerateSubIndicator(string category, int globalScore, int score, int numrules, string explanation)
        {

            Add(@"
			<div class=""col-xs-12 col-md-6 col-sm-6"">
				<div class=""row"">
					<div class=""col-md-4 col-xs-8 col-sm-9"">
						<div class=""chart-gauge"">");
            GenerateGauge(score);
            Add(@"</div>
					</div>
					<div class=""col-md-6 col-xs-8 col-sm-9"">
					");
            Add((score == globalScore ? "<strong>" : ""));
            Add(@"<p>");
            Add(category);
            Add(@" : ");
            Add(score.ToString());
            Add(@" /100</p>");
            Add((score == globalScore ? "</strong>" : ""));
            Add(@"
					<p class=""small"">");
            AddEncoded(explanation);
            Add(@"</p>
					</div>
					<div class=""col-md-2 col-xs-4 col-sm-3 collapse-group"">
						<p class=""small"">");
            Add(numrules.ToString());
            Add(@" rules matched</p>
					</div>
				</div>
			</div>
");
        }

        protected void GenerateRiskModelPanel(List<HealthcheckRiskRule> rules, int numberOfDomain = 1)
        {
			ICustomData customData;
			if(CustomConsoData != null)
            {
				customData = CustomConsoData;
			}
			else
            {
				customData = CustomData;
			}

			Add(@"
		<div class=""row d-print-none""><div class=""col-lg-12"">
			<a data-toggle=""collapse"" data-target=""#riskModel"">
				<h2>Risk model</h2>
			</a>
		</div></div>
		<div class=""row collapse show d-print-none"" id=""riskModel"">
			<div class=""col-md-12 table-responsive"">
				<table class=""model_table"">
					<thead><tr><th>Stale Objects</th><th>Privileged accounts</th><th>Trusts</th><th>Anomalies</th>"
					+ CustomRiskRuleCategory.ParseCategoriesToTableHeaders(customData.Categories)
					+ @"</tr></thead>
					<tbody>
");
			var riskmodel = new Dictionary<string, List<CustomRiskModelCategory>>();
			foreach (RiskRuleCategory category in Enum.GetValues(typeof(RiskRuleCategory)))
			{
				riskmodel[category.ToString()] = new List<CustomRiskModelCategory>();
			}
			CustomRiskRuleCategory.AddCategoriesToRiskModelDictionary(riskmodel, customData.Categories);

			for (int j = 0; j < 4; j++)
			{
				for (int i = 0; ; i++)
				{
					int id = (1000 * j + 1000 + i);
					if (Enum.IsDefined(typeof(RiskModelCategory), id))
					{
						riskmodel[((RiskRuleCategory)j).ToString()].Add(new CustomRiskModelCategory((RiskModelCategory)id));
					}
					else
						break;
				}
			}

			foreach (var model in customData.Models)
			{
				riskmodel[model.Category].Add(model);
			}

			foreach (var category in riskmodel.Keys)
			{
				riskmodel[category].Sort((CustomRiskModelCategory a, CustomRiskModelCategory b) =>
				{
					return string.Compare(a.Description, b.Description);
				});
			}

			for (int i = 0; ; i++)
			{
				string line = "<tr>";
				bool HasValue = false;
				foreach (var category in riskmodel.Keys)
				{
					if (i < riskmodel[category].Count)
					{
						HasValue = true;
						CustomRiskModelCategory model = riskmodel[category][i];
						int score = 0;
						int numrules = 0;
						List<HealthcheckRiskRule> rulematched = new List<HealthcheckRiskRule>();
						foreach (HealthcheckRiskRule rule in rules)
						{
							if (rule.Model.ToString() == model.Id)
							{
								numrules++;
								score += rule.Points;
								rulematched.Add(rule);
							}
						}

						foreach (CustomHealthCheckRiskRule rule in customData.HealthRules)
						{
							if (rule.CheckIsInModel(model.Id))
							{
								numrules++;
								score += rule.Points;
								rulematched.Add(CustomHealthCheckRiskRule.ParseToHealthcheckRiskRule(rule));
							}
						}
						string tdclass = "";
						if (numrules == 0)
						{
							tdclass = "model_good";
						}
						else if (score == 0)
						{
							tdclass = "model_info";
						}
						else if (score <= 10 * numberOfDomain)
						{
							tdclass = "model_toimprove";
						}
						else if (score <= 30 * numberOfDomain)
						{
							tdclass = "model_warning";
						}
						else
						{
							tdclass = "model_danger";
						}
						string tooltip = "Rules: " + numrules + " Score: " + (numberOfDomain == 0? 100 : score / numberOfDomain);
						string tooltipdetail = null;
						string modelstring = model.Description;
						rulematched.Sort((HealthcheckRiskRule a, HealthcheckRiskRule b)
							=>
						{
							return a.Points.CompareTo(b.Points);
						});
						foreach (var rule in rulematched)
						{
							tooltipdetail += ReportHelper.Encode(rule.Rationale) + "<br>";
							var hcrule = CustomRiskRule.GetFromRuleBase(RuleSet<T>.GetRuleFromID(rule.RiskId));
							if(hcrule == null)
                            {
								customData.GetRiskRule(rule.RiskId, out hcrule);
                            }
							if (hcrule != null && !string.IsNullOrEmpty(hcrule.ReportLocation))
							{
								tooltipdetail += "<small  class='text-muted'>" + ReportHelper.Encode(hcrule.ReportLocation) + "</small><br>";
							}
						}
						line += "<td class=\"model_cell " + tdclass + "\"><div class=\"div_model\" placement=\"auto right\" data-toggle=\"popover\" title=\"" +
							tooltip + "\" data-html=\"true\" data-content=\"" +
							(String.IsNullOrEmpty(tooltipdetail) ? "No rule matched" : "<p>" + tooltipdetail + "</p>") + "\"><span class=\"small\">" + modelstring + "</span></div></td>";
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
			<div class=""col-md-12"" id=""maturityModel"">
		Legend: <br>
			<i class=""risk_model_none"">&nbsp;</i> score is 0 - no risk identified but some improvements detected<br>
			<i class=""risk_model_low"">&nbsp;</i> score between 1 and 10  - a few actions have been identified<br>
			<i class=""risk_model_medium"">&nbsp;</i> score between 10 and 30 - rules should be looked with attention<br>
			<i class=""risk_model_high"">&nbsp;</i> score higher than 30 - major risks identified
			</div>
		</div>");
		}

        protected void GenerateIndicatorPanel(string id, string title, RiskRuleCategory category, List<HealthcheckRiskRule> rules, List<RuleBase<HealthcheckData>> applicableRules)
        {

			Add(@"
		<div class=""row""><div class=""col-lg-12 mt-2"">
			<a data-toggle=""collapse"" data-target=""#" + id + @""">
				<h2>");
			Add(title);
			Add(@" [");
			Add((GetRulesNumberForCategory(rules, category) + CustomData.CountCategoryHealthRules(category.ToString())).ToString());
			Add(@" rules matched on a total of ");
			Add((GetApplicableRulesNumberForCategory(applicableRules, category) + CustomData.CountCategoryRiskRules(category.ToString())).ToString());
			Add(@"]</h2>
			</a>
		</div></div>
		<div class=""row collapse show"" id=""");
            Add(id);
            Add(@"""><div class=""col-lg-12"">
");
			bool hasRule = false;
			foreach (HealthcheckRiskRule rule in rules)
			{
				if (rule.Category == category)
				{
					hasRule = true;
					break;
				}
			}
			if(hasRule == false)
            {
				CustomData.CheckHasRule(category.ToString());
			}
			if (hasRule)
			{
				GenerateAccordion("rules" + category.ToString(), () =>
					{
						rules.Sort((HealthcheckRiskRule a, HealthcheckRiskRule b)
							=>
						{
							return -a.Points.CompareTo(b.Points);
						}
						);
						foreach (HealthcheckRiskRule rule in rules)
						{
							if (rule.Category == category)
                                GenerateIndicatorPanelDetail(category.ToString(), rule);
						}
						foreach(CustomHealthCheckRiskRule rule in CustomData.HealthRules)
                        {
							if(rule.CheckIsInCategory(category.ToString()))
								GenerateAdvancedIndicatorPanelDetail(category.ToString(), rule);
                        }
					});
			}
			else
			{
				Add("<p>No rule matched</p>");
			}
			Add(@"
			</div>
		</div>");
		}

		protected void GenerateAdvancedIndicatorPanel(string id, string title, string category)
		{
			Add(@"
		<div class=""row""><div class=""col-lg-12 mt-2"">
			<a data-toggle=""collapse"" data-target=""#" + id + @""">
				<h2>");
			Add(title);
			Add(@" [");
			Add(CustomData.CountCategoryHealthRules(category));
			Add(@" rules matched on a total of ");
			Add(CustomData.CountCategoryRiskRules(category));
			Add(@"]</h2>
			</a>
		</div></div>
		<div class=""row collapse show"" id=""");
			Add(id);
			Add(@"""><div class=""col-lg-12"">
");
			if (CustomData.CheckHasRule(category))
			{
				GenerateAccordion("rules" + category.ToString(), () =>
				{
					CustomData.HealthRules.Sort((CustomHealthCheckRiskRule a, CustomHealthCheckRiskRule b)
						=>
					{
						return -a.Points.CompareTo(b.Points);
					}
					);
					foreach (CustomHealthCheckRiskRule rule in CustomData.HealthRules)
					{
						if(rule.CheckIsInCategory(category))
							GenerateAdvancedIndicatorPanelDetail(category, rule);
					}
				});
			}
			else
			{
				Add("<p>No rule matched</p>");
			}
			Add(@"
			</div>
		</div>");
        }

        private int GetApplicableRulesNumberForCategory(List<RuleBase<HealthcheckData>> applicableRules, RiskRuleCategory category)
        {
            int count = 0;
            foreach (var rule in applicableRules)
            {
                if (rule.Category == category)
                    count++;
            }
            return count;
        }

        protected void GenerateSubIndicator(string category, int globalScore, int score, string explanation)
        {

            Add(@"
			<div class=""col-lg-12"">
				<div class=""row"">
					<div class="" col-lg-3 col-md-4 col-xs-8 col-sm-9"">
						<div class=""chart-gauge"">");
            GenerateGauge(score);
            Add(@"</div>
					</div>
					<div class=""col-lg-9 col-md-8 col-xs-8 col-sm-9"">
					");
            Add((score == globalScore ? "<strong>" : ""));
            Add(@"<p>");
            Add(category);
            Add(@" : ");
            Add(score.ToString());
            Add(@" /100</p>");
            Add((score == globalScore ? "</strong>" : ""));
            Add(@"
					<p class=""small"">");
            AddEncoded(explanation);
            Add(@"</p>
					</div>
				</div>
			</div>
");
        }

		protected void GenerateIndicatorPanelDetail(string category, HealthcheckRiskRule rule, string optionalId = null)
		{
			string safeRuleId = rule.RiskId.Replace("$", "dollar");
			var hcrule = CustomRiskRule.GetFromRuleBase(RuleSet<T>.GetRuleFromID(rule.RiskId));
			if(hcrule == null)
            {
				CustomData.GetRiskRule(rule.RiskId, out hcrule);
			}
			GenerateAccordionDetail("rules" + optionalId + safeRuleId, "rules" + category, rule.Rationale, rule.Points, true,
				() =>
				{
					if (hcrule != null)
					{
						Add("<h3>");
						Add(hcrule.Title);
						Add("</h3>\r\n<strong>Rule ID:</strong><p class=\"text-justify\">");
						Add(hcrule.Id);
						Add("</p>\r\n<strong>Description:</strong><p class=\"text-justify\">");
						Add(NewLineToBR(hcrule.Description));
						Add("</p>\r\n<strong>Technical explanation:</strong><p class=\"text-justify\">");
						Add(NewLineToBR(hcrule.TechnicalExplanation));
						Add("</p>\r\n<strong>Advised solution:</strong><p class=\"text-justify\">");
						Add(NewLineToBR(hcrule.Solution));
						Add("</p>\r\n<strong>Points:</strong><p>");
						Add(NewLineToBR(hcrule.GetComputationModelString()));
						Add("</p>\r\n");
						if (!String.IsNullOrEmpty(hcrule.Documentation))
						{
							Add("<strong>Documentation:</strong><p>");
							Add(hcrule.Documentation);
							Add("</p>");
						}
					}
					
					if ((rule.Details != null && rule.Details.Count > 0) || (hcrule != null && !String.IsNullOrEmpty(hcrule.ReportLocation)))
					{
						Add("<strong>Details:</strong>");
						if (!String.IsNullOrEmpty(hcrule.ReportLocation))
						{
							Add("<p>");
							Add(hcrule.ReportLocation);
							Add("</p>");
						}
						if (rule.Details != null && rule.Details.Count > 0 && !string.IsNullOrEmpty(rule.Details[0]))
						{
							var test = rule.Details[0].Replace("Domain controller:","Domain_controller:").Split(' ');
							if (test.Length > 1 && test[0].EndsWith(":"))
							{
								var tokens = new List<string>();
								for (int i = 0; i < test.Length; i++)
								{
									if (!string.IsNullOrEmpty(test[i]) && test[i].EndsWith(":"))
									{
										tokens.Add(test[i]);
									}
								}
								Add(@"<div class=""row"">
			<div class=""col-md-12 table-responsive"">
				<table class=""table table-striped table-bordered"">
					<thead><tr>");
								foreach(var token in tokens)
								{
									Add("<th>");
									if(token == "Domain_controller:")
										AddEncoded(token.Replace("Domain_controller:", "Domain controller:").Substring(0, token.Length - 1));
									else
                                    {
										string parsedToken = token.Replace("#$%%$#", " ").Replace("#$%:%$#", ": ");
										AddEncoded(parsedToken.Substring(0, parsedToken.Length - 1));
									}
									Add("</th>");
								}
								Add("</tr></thead><tbody>");
								foreach (var d in rule.Details)
								{
									if (string.IsNullOrEmpty(d))
										continue;
									Add("<tr>");
									var t = d.Replace("Domain controller:", "Domain_controller:").Split(' ');
									for (int i = 0, j = 0; i < t.Length && j <= tokens.Count; i++)
									{
										if (j < tokens.Count && t[i] == tokens[j])
										{
											if (j != 0)
												Add("</td>");
                                            j++;
                                            Add("<td>");
                                        }
                                        else
                                        {
                                            Add(t[i].Replace("#$%:%$#", ": "));
                                            Add(" ");
                                        }
                                    }
                                    Add("</td>");
                                    Add("</tr>");
                                }
                                Add("</tbody></table></div></div>");

                            }
                            else
                            {
                                Add("<p>");
                                Add(String.Join("<br>\r\n", rule.Details.ToArray()));
                                Add("</p>");
                            }
                        }
                    }
                });
        }

		protected void GenerateAdvancedIndicatorPanelDetail(string category, CustomHealthCheckRiskRule rule, string optionalId = null)
		{
			string safeRuleId = rule.RiskId.Replace("$", "dollar");
			var hcrule = CustomRiskRule.GetFromRuleBase(RuleSet<T>.GetRuleFromID(rule.RiskId));
			if (hcrule == null)
			{
				CustomData.GetRiskRule(rule.RiskId, out hcrule);
			}
			GenerateAccordionDetail("rules" + optionalId + safeRuleId, "rules" + category, rule.Rationale, rule.Points, true,
				() =>
				{
					if (hcrule != null)
					{
						Add("<h3>");
						Add(hcrule.Title);
						Add("</h3>\r\n<strong>Rule ID:</strong><p class=\"text-justify\">");
						Add(hcrule.Id);
						Add("</p>\r\n<strong>Description:</strong><p class=\"text-justify\">");
						Add(NewLineToBR(hcrule.Description));
						Add("</p>\r\n<strong>Technical explanation:</strong><p class=\"text-justify\">");
						Add(NewLineToBR(hcrule.TechnicalExplanation));
						Add("</p>\r\n<strong>Advised solution:</strong><p class=\"text-justify\">");
						Add(NewLineToBR(hcrule.Solution));
						Add("</p>\r\n<strong>Points:</strong><p>");
						Add(NewLineToBR(hcrule.GetComputationModelString()));
						Add("</p>\r\n");
						if (!String.IsNullOrEmpty(hcrule.Documentation))
						{
							Add("<strong>Documentation:</strong><p>");
							Add(hcrule.Documentation);
							Add("</p>");
						}
					}
					if((rule.RuleDetails != null && rule.RuleDetails.Count > 0) || (hcrule != null && !String.IsNullOrEmpty(hcrule.ReportLocation)))
					{
						Add("<strong>Details:</strong>");
						if (!String.IsNullOrEmpty(hcrule.ReportLocation))
						{
							Add("<p>");
							Add(hcrule.ReportLocation);
							Add("</p>");
						}
						if (rule.RuleDetails != null && rule.RuleDetails.Count > 0)
						{
							foreach(var detail in rule.RuleDetails)
                            {
								if(detail.Type == CustomDetailsType.Table)
                                {
									var tableLines = CustomTable.GetTable(CustomData.CustomDelimiter, detail.FilePath);
									if (tableLines == null)
										continue;
									var firstLineParts = tableLines[0].Split(' ');
									if (firstLineParts.Length > 1 && firstLineParts[0].EndsWith(":"))
									{
										var tokens = new List<string>();
										for (int i = 0; i < firstLineParts.Length; i++)
										{
											if (!string.IsNullOrEmpty(firstLineParts[i]) && firstLineParts[i].EndsWith(":"))
											{
												tokens.Add(firstLineParts[i]);
											}
										}
										Add(@"<div class=""row"">
			<div class=""col-md-12 table-responsive"">
				<table class=""table table-striped table-bordered"">
					<thead><tr>");
										foreach (var token in tokens)
										{
											Add("<th>");
											string parsedToken = token.Replace("#$%%$#", " ").Replace("#$%:%$#", ": ");
											AddEncoded(parsedToken.Substring(0, parsedToken.Length - 1));
											Add("</th>");
										}
										Add("</tr></thead><tbody>");
										foreach (var d in tableLines)
										{
											if (string.IsNullOrEmpty(d))
												continue;
											Add("<tr>");
											var t = d.Split(' ');
											for (int i = 0, j = 0; i < t.Length && j <= tokens.Count; i++)
											{
												if (j < tokens.Count && t[i] == tokens[j])
												{
													if (j != 0)
														Add("</td>");
													j++;
													Add("<td>");
												}
												else
												{
													Add(t[i].Replace("#$%:%$#", ": "));
													Add(" ");
												}
											}
											Add("</td>");
											Add("</tr>");
										}
										Add("</tbody></table></div></div>");

									}
									else
									{
										Add("<p>");
										Add(String.Join("<br>\r\n", tableLines.ToArray()));
										Add("</p>");
									}
								}
								else if(detail.Type == CustomDetailsType.Chart)
                                {
									CustomData.GetChart(detail.Id, out var chart);
									Add(CustomChart.GetChart(detail.FilePath, chart));
                                }
                            }							
						}
					}
				});
		}
		#endregion indicators
	}
}
