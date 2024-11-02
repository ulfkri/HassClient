﻿using HassClient.Models;
using NUnit.Framework;
using System.Threading.Tasks;

namespace HassClient.WS.Tests
{
    public class ServiceApiTests : BaseHassWSApiTest
    {
        [Test]
        public async Task GetServices()
        {
            var services = await this.hassWSApi.GetServicesAsync();

            Assert.NotNull(services);
            Assert.IsNotEmpty(services);
        }

        [Test]
        public async Task CallService()
        {
            var result = await this.hassWSApi.CallServiceAsync("homeassistant", "check_config");

            Assert.NotNull(result);
        }

        [Test]
        public async Task CallServiceForEntities()
        {
            var result = await this.hassWSApi.CallServiceForEntitiesAsync("homeassistant", "update_entity", "sun.sun");

            Assert.NotNull(result);
        }

        [Test]
        public async Task CallServiceWithKnownDomain()
        {
            var result = await this.hassWSApi.CallServiceAsync(KnownDomains.Homeassistant, KnownServices.CheckConfig);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task CallServiceForEntitiesWithKnownDomain()
        {
            var result = await this.hassWSApi.CallServiceForEntitiesAsync(KnownDomains.Homeassistant, KnownServices.UpdateEntity, "sun.sun");

            Assert.IsTrue(result);
        }
    }
}
