using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace TimetrackerReportingClient
{
    public class CommandLineOptions
    {
        [Value( 0, Required = true, HelpText = "Service URL for Timetracker OData endpoint (without ?api-version)" )]
        public Uri ServiceUri { get; set; }

        [Option( 'x', HelpText = "Export format (if required). Possible values: xml, json. Provide empty string if no export required" )]
        public string Format { get; set; }

        [Option( 'w', Default = false, HelpText = "On-premise usage (NTLM auth)" )]
        public bool IsWindowsAuth { get; set; }

        [Option( 't', HelpText = "Token for Timetracker API (VSTS usage)" )]
        public string Token { get; set; }

        [Option( 'c', Default = null, HelpText = "Comma separated list of work item's custom fields, e.g. System.RemoteLinkCount,System.CommentCount,System.IterationLevel1 ")]
        public IEnumerable<string> CustomFields { get; set; }
    }
}