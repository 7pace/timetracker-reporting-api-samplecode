using System;
using System.Collections.Generic;
using RestSharp;

namespace TimetrackerOdataClient
{
    public class TFSExtender
    {
        private RestClient _client;
        private Dictionary<int, Dictionary<string, string>> _cache = new Dictionary<int, Dictionary<string, string>>();
        private string vstsToken;

        public TFSExtender( string tfsUrl )
        {
            _client = new RestClient( tfsUrl );
            _client.AddDefaultHeader( "Accept", "application/json" );
        }

        public TFSExtender( string tfsUrl, string vstsToken ) : this( tfsUrl )
        {
            this.vstsToken = vstsToken;
        }

        public Dictionary<string, string> GetTfsItemData( int id, string[] fields )
        {
            if ( _cache.ContainsKey( id ) )
            {
                return _cache[id];
            }

            var request = new RestRequest( "/_apis/wit/workItems/" + id );
            if ( !string.IsNullOrEmpty( vstsToken ) )
            {
                var auth = Convert.ToBase64String( System.Text.Encoding.ASCII.GetBytes( $"{""}:{vstsToken}" ) );

                request.AddHeader( "Authorization", "Basic" + auth );
            }
            else
            {
                request.UseDefaultCredentials = true;
            }
            var fieldValues = new Dictionary<string, string>();

            try
            {
                var objTemplate = new { fields = new Dictionary<string, string>() };

                var response = _client.Get( request );
                var obj = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType( response.Content, objTemplate );

                if ( obj == null || obj.fields == null )
                {
                    return fieldValues;
                }

                //get fields
                foreach ( var name in fields )
                {
                    if ( obj.fields.ContainsKey( name ) )
                    {
                        fieldValues[name] = obj.fields[name];
                    }
                }

                _cache[id] = fieldValues;
            }
            catch ( Exception e )
            {
                _cache[id] = fieldValues;
                //handle errors here
                Console.WriteLine( "failed getting info for tfs#" + id );
            }

            return fieldValues;
        }
    }
}