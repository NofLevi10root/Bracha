﻿using PingCastle.Addition;
using PingCastle.Addition.LogicEnteties;
using PingCastle.Data;

namespace PingCastle.Report
{
    public delegate string GetUrlDelegate(DomainKey domainKey, string displayName);
    public delegate string GetAdditionInfoDelegate(DomainKey domainKey);

	public interface IPingCastleReportUser<T> where T : IPingCastleReport
	{
		string GenerateReportFile(T report, ADHealthCheckingLicense license, string filename);
		string GenerateRawContent(T report);
		void SetUrlDisplayDelegate(GetUrlDelegate uRLDelegate);
		void SetCustomData(CustomHealthCheckData customData);
	}
}
