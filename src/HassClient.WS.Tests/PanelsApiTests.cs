﻿using HassClient.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HassClient.WS.Tests
{
    public class PanelsApiTests : BaseHassWSApiTest
    {
        private IEnumerable<PanelInfo> panels;

        [OneTimeSetUp]
        [Test]
        public async Task GetPanels()
        {
            if (this.panels != null)
            {
                return;
            }

            this.panels = await this.hassWSApi.GetPanelsAsync();

            Assert.IsNotNull(this.panels);
            Assert.IsNotEmpty(this.panels);
        }

        [Test]
        public async Task GetPanel()
        {
            if (this.panels != null)
            {
                return;
            }

            var firstPanel = this.panels?.FirstOrDefault();
            Assert.NotNull(firstPanel, "SetUp failed");

            var result = await this.hassWSApi.GetPanelAsync(firstPanel.UrlPath);

            Assert.IsNotNull(result);
            Assert.AreEqual(firstPanel, result);
        }

        [Test]
        public void GetPanelWithNullUrlPathThrows()
        {
            Assert.ThrowsAsync<ArgumentException>(() => this.hassWSApi.GetPanelAsync(null));
        }

        [Test]
        public void GetPanelsHasComponentName()
        {
            Assert.IsTrue(this.panels.All(x => !string.IsNullOrEmpty(x.ComponentName)));
        }

        [Test]
        public void GetPanelsHasConfiguration()
        {
            Assert.IsTrue(this.panels.All(x => x.Configuration != null));
        }

        [Test]
        public void GetPanelsHasIcon()
        {
            Assert.IsTrue(this.panels.Any(x =>!string.IsNullOrEmpty(x.Icon)));
        }

        [Test]
        public void GetPanelsHasRequireAdmin()
        {
            Assert.IsTrue(this.panels.Any(x => x.RequireAdmin == true));
        }

        [Test]
        public void GetPanelsHasTitle()
        {
            Assert.IsTrue(this.panels.Any(x => !string.IsNullOrEmpty(x.Title)));
        }

        [Test]
        public void GetPanelsHasUrlPath()
        {
            Assert.IsTrue(this.panels.All(x => !string.IsNullOrEmpty(x.UrlPath)));
        }
    }
}
