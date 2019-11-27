Timetracker REST API Sample Code
===================

This repo contains code samples for using the REST API's in Timetracker. The code is optimized to be simple and easy to understand.

The OData Connected Service Extension (http://odata.github.io/odata.net/#OData-Client-Code-Generation-Tool) has been used to build this code sample.
The T4 templates (ServiceReferences\Timetracker\TimetrackerProxy.tt, ServiceReferences\Timetracker\TimetrackerProxy.ttinclude) were modified since the original OData Connected Service Extension does not support spaces in the naming of entity members.

## Exporting Timetracker data into hierarchical Excel
There is implementation of exporting feature for Timetracker into Excel file with hierarchy of Work Items based on this sample code:
https://github.com/laugel/timetracker-excel-exporter


## TimetrackerOdataClient usage

This is a sample of the console application which connects to VSTS or On-premise version of Timetracker and returns all time records for last 3 months.
Command line parameters:

VSTS usage (token auth): 
```
TimetrackerOdataClient.exe ServiceURI -t Token -f VSTS_ACCOUNT_URL -v VSTS_TOKEN -a System.Tags,System.Title -x json
```

On-premise usage (NTLM auth):
```
TimetrackerOdataClient.exe ServiceURI -w -f TFS_URL_WITH_COLLECTION -a System.Tags,System.Title -x json
```
## Parameters

|   | TFS  | VSTS  |
|---|---|---|
| ServiceUri  | [timetrackerServiceUrl:Port]/api/[CollectionName]/odata  |  https://[accountName].timehub.7pace.com/api/odata |
|-f| TFS URL (like http://tfs:8080/tfs)|VSTS Account URL (https://[accountName].visualstudio.com)|
| -t  | -  | API Token  |
| -v  | -  | (Optional) VSTS Personal token. Used only when fetching fields from VSTS API that do not included in default list of fields when exporting. |
| -w  | no value, tells application to use Windows Credentials  | -  |
|-x|(Optional) Export format. Possible values: xml, json. Provide empty string if no export required|(Optional) Export format. Possible values: xml, json. Provide empty string if no export required|
|-a| (Optional) Comma separated list of TFS fields, e.g. System.Tags,System.Title|(Optional) Comma separated list of TFS fields, e.g. System.Tags,System.Title|
