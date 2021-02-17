using PingCastle.Addition;
using PingCastle.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Report
{
	public delegate string GetUrlDelegate(DomainKey domainKey, string displayName);

	public interface IPingCastleReportUser<T> where T : IPingCastleReport
	{
		string GenerateReportFile(T report, ADHealthCheckingLicense license, string filename);
		string GenerateReportFile(T report, ADHealthCheckingLicense license, string filename, CustomHealthCheckData data);
		string GenerateRawContent(T report);
		void SetUrlDisplayDelegate(GetUrlDelegate uRLDelegate);
	}
}
