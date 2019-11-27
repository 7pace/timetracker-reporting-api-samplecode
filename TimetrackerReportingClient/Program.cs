using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CommandLine;
using ExtendedXmlSerializer.Configuration;
using Newtonsoft.Json;
using RestSharp.Serializers;
using TimetrackerOnline.Reporting.Models;
using Formatting = Newtonsoft.Json.Formatting;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace TimetrackerReportingClient
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            bool parsed = false;
            CommandLineOptions cmd = null;
            // Get parameters
            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(x =>
            {
                parsed = true;
                cmd = x;
            })
                       .WithNotParsed(x => { Console.WriteLine("Check https://github.com/7pace/timetracker-reporting-api-samplecode to get samples of usage"); });

            if (!parsed)
            {
                Console.ReadLine();
                return;
            }

            // Create OData service context
            var context = cmd.IsWindowsAuth
                ? new TimetrackerOdataContext(cmd.ServiceUri)
                : new TimetrackerOdataContext(cmd.ServiceUri, cmd.Token);

            // request for work items with worklogs
            var workLogsWorkItemsExport = context.Container.workLogsWorkItems;
            //fills custom fields values if provided. Check https://support.7pace.com/hc/en-us/articles/360035502332-Reporting-API-Overview#user-content-customfields to get more information
            if (cmd.CustomFields != null && cmd.CustomFields.Any())
            {
                workLogsWorkItemsExport = workLogsWorkItemsExport.AddQueryOption("customFields", string.Join(",", cmd.CustomFields));
            }
            var workLogsWorkItemsExportResult = workLogsWorkItemsExport
                // Perform query for 3 last months
                .Where(s => s.Timestamp > DateTime.Today.AddMonths(-3) && s.Timestamp < DateTime.Today)
                // orfer items by worklog date
                .OrderByDescending(g => g.WorklogDate.ShortDate).ToArray();
            // Print out the result
            foreach (var row in workLogsWorkItemsExportResult)
            {
                Console.WriteLine("{0:g} {1} {2}", row.WorklogDate.ShortDate, row.User.Name, row.PeriodLength);
            }
            Export(cmd.Format, workLogsWorkItemsExportResult, "workLogsWorkItemsExport");


            // request for work items with its hierarchy
            var workItemsHierarchyExport = context.Container.workItemsHierarchy;
            // fills rollup field with the sum of specified numeric field of work item and its children. Check https://support.7pace.com/hc/en-us/articles/360035502332-Reporting-API-Overview#rollupFields to get more information
            workItemsHierarchyExport = workItemsHierarchyExport.AddQueryOption("rollupFields", "Microsoft.VSTS.Scheduling.CompletedWork");
            var workItemsHierarchyExportResult = workItemsHierarchyExport
                // Perform query for 3 last months
                .Where(s => s.System_CreatedDate > DateTime.Today.AddMonths(-3) && s.System_CreatedDate < DateTime.Today).ToArray();
            Export(cmd.Format, workItemsHierarchyExportResult, "workItemsHierarchyExport");
        }

        public static void Export(string format, object extendedData, string fileName)
        {
            if (string.IsNullOrEmpty(format))
            {
                return;
            }

            //save here
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //once you have the path you get the directory with:
            var directory = System.IO.Path.GetDirectoryName(location);

            if (format == "xml")
            {
                var serializer = new ConfigurationContainer()

                    // Configure...
                    .Create();

                var exportPath = directory + $"/{fileName}.xml";

                var file = File.OpenWrite(exportPath);
                var settings = new XmlWriterSettings { Indent = true };

                var xmlTextWriter = new XmlTextWriter(file, Encoding.UTF8);
                xmlTextWriter.Formatting = System.Xml.Formatting.Indented;

                xmlTextWriter.Indentation = 4;

                serializer.Serialize(xmlTextWriter, extendedData);
                xmlTextWriter.Close();
                xmlTextWriter.Dispose();
                file.Close();
                file.Dispose();
            }
            else if (format == "json")
            {
                var json = JsonConvert.SerializeObject(extendedData, Formatting.Indented);
                var exportPath = directory + $"/{fileName}.json";
                File.WriteAllText(exportPath, json);
            }
            else
            {
                throw new NotSupportedException("Provided format is not supported: " + format);
            }
        }
    }
}
