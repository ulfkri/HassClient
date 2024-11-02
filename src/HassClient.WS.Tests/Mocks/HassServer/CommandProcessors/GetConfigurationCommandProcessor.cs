﻿using HassClient.Serialization;
using HassClient.WS.Messages;
using Newtonsoft.Json.Linq;

namespace HassClient.WS.Tests.Mocks.HassServer
{
    public class GetConfigurationCommandProcessor : BaseCommandProcessor
    {
        public override bool CanProcess(BaseIdentifiableMessage receivedCommand) => receivedCommand is GetConfigMessage;

        public override BaseIdentifiableMessage ProcessCommand(MockHassServerRequestContext context, BaseIdentifiableMessage receivedCommand)
        {
            var configuration = MockHassModelFactory.ConfigurationFaker.Generate();
            var resultObject = new JRaw(HassSerializer.SerializeObject(configuration));
            return this.CreateResultMessageWithResult(resultObject);
        }
    }
}
