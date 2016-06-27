# STCustomDataImporter

ShoreTelCustomDataImporter enables you to insert data in to the
Account Code information fields in the ShoreTel CDR database.

The ShoreTel CDR database contains two fields for every call record:
BillingCode and FriendlyBillingCode.  They correspond to Account Code
and Account Code Name in reports based on this data.  ShoreTel only
populates this information for outbound calls when the caller has
account codes enabled and enters the appropriate account code.  This
application will go through the CDR database and populate the
BillingCode and FriendlyBillingCode fields of *Inbound* (and Tandem)
calls based on looking up the CallerId and matching it to customer
information.

Only a CSV lookup is included.  A CSV file must be supplied that
contains three columns: CallerId, CustomerId, and CustomerName.  This
file may be produced by, for example, an export from a CRM
application.

An example of such a file would be:

```
CallerId,CustomerId,CustomerName
+17075551234,Brightmetrics,Brightmetrics Inc.
+14155558888,SymmCW,Symmetry Codeworks
```

Note that in this case the CustomerId field is a short name and
CustomerName is a longer description.  But the fields can be used
however you like.  CustomerId could be a numeric code while
CustomerName is a text description, or CustomerId could be the name of
the company and CustomerName could be the name of an individual at
that company, if, for example, you have CallerId numbers for different
individuals at the same company.  The only requirements are those
imposed by the ShoreTel database, which is that the CustomerId field
can only be 32 characters, and CustomerName can only be 50 characters.

## Requirements

This application requires .Net Framework 4 or better.

The computer from which this application is run must be able to
connect to the ShoreWare configuration MySQL database running on TCP
port 4308 on the ShoreTel HQ server.

## Obtaining and installing

The latest release can be downloaded from the
[Releases](https://github.com/brightmetrics/STCustomDataImporter/releases)
section of this repository.  As of this writing, the latest release is
[STCustomDataImporter-v1.0](https://github.com/brightmetrics/STCustomDataImporter/releases/download/v1.0/STCustomDataImporter-v1.0.zip).
No installer is provided.  Download the zip file and extract its
contents to a folder, and run either ImportDataGUI for the GUI
version, or run ImportData from the command shell for the CLI version.

## Usage

This application contains two executables, a GUI version and a CLI
version.  The GUI version lets you browse to select a CSV file, enter
the address of your ShoreTel HQ server, and choose a time range over
which to update the CDR data.  You can choose whether or not to update
calls that already have account code information supplied.  By default
it is disabled, but if you are correcting a past import, for example,
you can check the box to have it ignore any existing information.  In
any case, no changes will be made if there is no CallerId match for a
given call.

The CLI version is suitable for scripting via a scheduled task or
other batch operation.  Usage will be printed if no command line
options are provided, which is:

```
Usage: ImportData [options] inputData

-startDate <date|datetime>   The start datetime from which to process calls.

-endDate <date|datetime>     The end datetime (exclusive) to which to process
                             calls (optional).

-updateExisting              Process calls that already have account code
                             information populated.

-stserver <host>             The address of the ShoreTel HQ server.  Will
                             assume localhost if not specified.

-verbose                     Report each call as it is updated.

-h                           This help text.

-lookupProvider <TypeName>   Provide the assembly-qualified name of a .Net Type
                             to use an alternate lookup provider.  The
                             default is the built-in CSV file lookup provider.
                             Any additional command line arguments will be
                             passed to the lookup provider constructor.  For
                             the CSV lookup provider, only a single argument
                             naming the CSV file can be provided.  The file
                             should contain three columns: CallerID,
                             Customer ID, and Customer Name.
```

An example might be:

    ImportData -stserver 192.168.221.10 -startDate 2016-01-01 customerData.csv

### Advanced usage note:

The CLI version supports extension by providing an alternate lookup
provider that implements the ICustomerLookup interface.  Instructions
for doing so are outside the scope of this documentation.  Any command
line options provided other than those listed above will be passed to
the lookup provider.  For example if it were a dynamic lookup that
connected directly to a CRM application you might provide the
connection information for the CRM application.

## License and Copyright

The MIT License (MIT)

Copyright (c) 2016 Brightmetrics, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
