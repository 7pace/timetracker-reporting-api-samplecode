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
using TimetrackerOnline.BusinessLayer.Models;
using Formatting = Newtonsoft.Json.Formatting;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace TimetrackerOdataClient
{
    internal class Program
    {
        private const string DateParametersFormat = @"yyyy-MM-dd";

        private static void Main( string[] args )
        {
            bool parsed = false;
            CommandLineOptions cmd = null;
            // Get parameters
            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>( args ).WithParsed( x =>
                                                                                              {
                                                                                                  parsed = true;
                                                                                                  cmd = x;
                                                                                              } )
                       .WithNotParsed( x => { Console.WriteLine( "Check https://github.com/7pace/timetracker-api-samplecode to get samples of usage" ); } );

            if ( !parsed )
            {
                Console.ReadLine();
                return;
            }

            // Create OData service context
            var context = cmd.IsWindowsAuth
                ? new TimetrackerOdataContext( cmd.ServiceUri )
                : new TimetrackerOdataContext( cmd.ServiceUri, cmd.Token );

            //TODO: DEFINE DATE PERIOD HERE
            // Perform query for 3 last years
            var startDate = DateTime.Today.AddYears( -3 ).ToString( DateParametersFormat );
            var endDate = DateTime.Today.ToString( DateParametersFormat );

            var timeExport = context.Container.TimeExport( startDate, endDate, null, null, null );
            timeExport = timeExport.AddQueryOption( "api-version", "2.1" );

            ExportItemViewModelApi[] timeExportResult = timeExport.ToArray();
            var rows = ExtendWithAdditionalFields( cmd, timeExportResult );
            // Print out the result
            foreach ( var row in rows )
            {
                Console.WriteLine( "{0:g} {1} {2}", row.TimetrackerRow.RecordDate, row.TimetrackerRow.TeamMember, row.TimetrackerRow.DurationInSeconds );
            }

            Export( cmd.Format, rows );
        }

        public static List<ExtendedTimetrackerRow> ExtendWithAdditionalFields( CommandLineOptions options, ExportItemViewModelApi[] timeExportResult )
        {
            var extender = new TFSExtender( options.TfsUrl, options.VstsToken );

            var extendedData = new List<ExtendedTimetrackerRow>();

            string[] tfsFields = new string[0];
            if ( options.TfsFields != null )
            {
                tfsFields = options.TfsFields.ToArray();
            }

            foreach ( var row in timeExportResult )
            {
                var extendedRow = new ExtendedTimetrackerRow
                {
                    TimetrackerRow = row
                };

                extendedData.Add( extendedRow );

                //non tfs
                if ( row.TFSID == null )
                {
                    continue;
                }

                extendedRow.TfsData = extender.GetTfsItemData( row.TFSID.Value, tfsFields );
            }

            return extendedData;
        }

        public static void Export( string format, List<ExtendedTimetrackerRow> extendedData )
        {
            if ( string.IsNullOrEmpty( format ) )
            {
                return;
            }

            //save here
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //once you have the path you get the directory with:
            var directory = System.IO.Path.GetDirectoryName( location );

            if ( format == "xml" )
            {
                var serializer = new ConfigurationContainer()

                    // Configure...
                    .Create();

                var exportPath = directory + "/export.xml";

                var file = File.OpenWrite( exportPath );
                var settings = new XmlWriterSettings { Indent = true };

                var xmlTextWriter = new XmlTextWriter( file, Encoding.UTF8 );
                xmlTextWriter.Formatting = System.Xml.Formatting.Indented;

                xmlTextWriter.Indentation = 4;

                serializer.Serialize( xmlTextWriter, extendedData );
                xmlTextWriter.Close();
                xmlTextWriter.Dispose();
                file.Close();
                file.Dispose();
            }
            else if ( format == "json" )
            {
                var json = JsonConvert.SerializeObject( extendedData, Formatting.Indented );
                var exportPath = directory + "/export.json";
                File.WriteAllText( exportPath, json );
            }
            else
            {
                throw new NotSupportedException( "Provided format is not supported: " + format );
            }
        }
    }

    [Serializable]
    public class ExtendedTimetrackerRow
    {
        public ExportItemViewModelApi TimetrackerRow { get; set; }
        public Dictionary<string, string> TfsData { get; set; }
    }
}