/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

03/11/2023	1.0.0.1		MPL, Skyline	Initial version
****************************************************************************
*/

namespace SLC_GQIDS_ResourceProperties_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	[GQIMetaData(Name = "Resource Properties")]
	public class MyDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        private readonly GQIStringArgument _resourceIdArg = new GQIStringArgument("Resource ID") { IsRequired = true, DefaultValue = String.Empty };

        private GQIDMS _dms;
        private string _resourceId;
        private List<GQIColumn> _columns;

        public GQIColumn[] GetColumns()
        {
            return _columns.ToArray();
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var resourceIdFilter = ResourceExposers.ID.Equal(Guid.Parse(_resourceId));
            FilterElement<Resource> filter = new ANDFilterElement<Resource>(resourceIdFilter);

            ResourceResponseMessage resourceResponse = GetResources(filter);
            var rows = GenerateRows(resourceResponse);

            var page = new GQIPage(rows.ToArray())
            {
                HasNextPage = false,
            };

            return page;
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { _resourceIdArg };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            _columns = new List<GQIColumn>
        {
         new GQIStringColumn("Name"),
         new GQIStringColumn("Value"),
        };

            _resourceId = args.GetArgumentValue(_resourceIdArg);
            return new OnArgumentsProcessedOutputArgs();
        }

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            return new OnInitOutputArgs();
        }

        private List<GQIRow> GenerateRows(ResourceResponseMessage resourceResponse)
        {
            List<GQIRow> rows = new List<GQIRow>();
            var resource = resourceResponse.ResourceManagerObjects.FirstOrDefault();

            if (resource == default)
            {
                return rows;
            }

            foreach (var property in resource.Properties)
            {
                List<GQICell> cells = new List<GQICell>();

                foreach (var column in _columns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            {
                                cells.Add(new GQICell { Value = property.Name });
                                break;
                            }

                        case "Value":
                            {
                                cells.Add(new GQICell { Value = property.Value });
                                break;
                            }

                        default:
                            {
                                throw new NotSupportedException("Unknown Column Definition!");
                            }
                    }
                }

                rows.Add(new GQIRow(cells.ToArray()));
            }

            return rows;
        }

        private ResourceResponseMessage GetResources(FilterElement<Resource> filter)
        {
            ResourceResponseMessage resourceResponse;
            resourceResponse = (ResourceResponseMessage)_dms.SendMessage(new GetResourceMessage(filter));
            return resourceResponse;
        }
    }
}