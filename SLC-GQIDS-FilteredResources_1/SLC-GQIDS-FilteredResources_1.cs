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

namespace SLC_GQIDS_FilteredResources_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	[GQIMetaData(Name = "Filtered Resources")]
	public class MyDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        private readonly GQIStringArgument _resourcePoolArg = new GQIStringArgument("Resource Pool") { IsRequired = true, DefaultValue = String.Empty };

        private GQIDMS _dms;
        private string _resourcePool;
        private List<GQIColumn> _columns;

        public GQIColumn[] GetColumns()
        {
            return _columns.ToArray();
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { _resourcePoolArg };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            FilterElement<Resource> filter;
            var resourcePoolFilter = ResourceExposers.PoolGUIDs.Contains(new Guid(_resourcePool));
            filter = new ANDFilterElement<Resource>(resourcePoolFilter);

            ResourceResponseMessage resources = GetResources(filter);
            var rows = GenerateRows(resources);

            var page = new GQIPage(rows.ToArray())
            {
                HasNextPage = false,
            };

            return page;
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            _columns = new List<GQIColumn>
        {
         new GQIStringColumn("ID"),
         new GQIStringColumn("Name"),
         new GQIStringColumn("Status"),
         new GQIIntColumn("Concurrency"),
         new GQIStringColumn("Type"),
        };

            _resourcePool = args.GetArgumentValue(_resourcePoolArg);
            return new OnArgumentsProcessedOutputArgs();
        }

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            return new OnInitOutputArgs();
        }

        private string SetCapabilityValue(string v, Resource resource)
        {
            // Obtaining all Capabilities GUIDs for this resource:
            var guids = resource.Capabilities.Select(obj => obj.CapabilityProfileID);

            // Requesting Profile Parameters for this GUIDs:
            var profileParameterResponse = _dms.SendMessage(new GetProfileManagerParameterMessage(guids)) as ProfileManagerResponseMessage;

            // Finding Profile Parameter with Name field set to value of argument 'v':
            foreach (var tuple in profileParameterResponse.ManagerObjects)
            {
                if (tuple.Item2 is Skyline.DataMiner.Net.Profiles.Parameter parameter && parameter.Name.Equals(v))
                {
                    var stringList = resource.Capabilities.FirstOrDefault(obj => obj.CapabilityProfileID.Equals(parameter.ID))?.Value.Discreets;
                    return stringList.IsNotNullOrEmpty() ? String.Join(";", stringList) : $"N/A";
                }
            }

            return $"N/A";
        }

        private List<GQIRow> GenerateRows(ResourceResponseMessage resources)
        {
            List<GQIRow> rows = new List<GQIRow>();
            foreach (var resource in resources.ResourceManagerObjects)
            {
                List<GQICell> cells = new List<GQICell>();

                foreach (var column in _columns)
                {
                    switch (column.Name)
                    {
                        case "ID":
                            {
                                cells.Add(new GQICell { Value = resource.ID.ToString() });
                                break;
                            }

                        case "Name":
                            {
                                cells.Add(new GQICell { Value = resource.Name });
                                break;
                            }

                        case "Status":
                            {
                                cells.Add(new GQICell { Value = resource.Mode.ToString() });
                                break;
                            }

                        case "Concurrency":
                            {
                                cells.Add(new GQICell { Value = resource.MaxConcurrency });
                                break;
                            }

                        case "Type":
                            {
                                cells.Add(new GQICell { Value = SetCapabilityValue("Type", resource) });
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