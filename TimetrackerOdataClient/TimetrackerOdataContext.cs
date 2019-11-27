using System;
using Microsoft.OData.Client;

namespace TimetrackerOdataClient
{
    /// <summary>
    /// oData client proxy
    /// Implements Timetracker authentication
    /// </summary>
    public class TimetrackerOdataContext
    {
        private readonly Uri _serviceUri;
        private readonly string _token;

        /// <summary>
        /// On-premise context
        /// </summary>
        /// <param name="serviceUri"></param>
        public TimetrackerOdataContext( Uri serviceUri )
        {
            _serviceUri = serviceUri;

            Container = new Default.Container( serviceUri );
            Container.Credentials = System.Net.CredentialCache.DefaultCredentials;
        }

        /// <summary>
        /// VSTS context
        /// </summary>
        /// <param name="serviceUri"></param>
        /// <param name="token"></param>
        public TimetrackerOdataContext( Uri serviceUri, string token )
        {
            _serviceUri = serviceUri;
            _token = token;

            Container = new Default.Container( serviceUri );

            Container.SendingRequest2 += SendHeaderAuth;
        }

        private void SendHeaderAuth( object sender, SendingRequest2EventArgs e )
        {
            e.RequestMessage.SetHeader( "Authorization", "Bearer " + _token );
        }

        /// <summary>
        /// oData client container
        /// </summary>
        public Default.Container Container { get; private set; }
    }
}