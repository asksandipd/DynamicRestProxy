﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Diagnostics;

using RestSharp;

using DynamicRestProxy.RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.RestSharp.UnitTests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        public async Task ExplicitGetInvoke()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;

            client.AddDefaultHeader("X-APIKEY", key);

            dynamic proxy = new RestSharpProxy(client);

            dynamic result = await proxy.metadata.mn.get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Minnesota");
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        public async Task GetMethodSegmentWithArgs()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
            client.AddDefaultHeader("X-APIKEY", key);

            dynamic proxy = new RestSharpProxy(client);

            var result = await proxy.bills.mn("2013s1")("SF 1").get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.id == "MNB00017167");
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        public async Task GetMethod2PathAsProperty2Params()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
            client.AddDefaultHeader("X-APIKEY", key);

            dynamic proxy = new RestSharpProxy(client);
            var parameters = new Dictionary<string, object>()
            {
                { "lat", 44.926868 },
                { "long", -93.214049 } // since long is a keyword we need to pass arguments in a Dictionary
            };
            var result = await proxy.legislators.geo.get(paramList: parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        public async Task GetMethod1PathArg1Param()
        {
            var client = new RestClient("http://openstates.org/api/v1");
            string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
            client.AddDefaultHeader("X-APIKEY", key);

            dynamic proxy = new RestSharpProxy(client);

            var result = await proxy.bills.get(state: "mn", chamber: "upper", status: "passed_upper");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Count > 0);
            Assert.IsTrue(result[0].chamber == "upper");
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        public async Task EscapeParameterName()
        {
            var client = new RestClient("http://congress.api.sunlightfoundation.com");
            string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
            client.AddDefaultHeader("X-APIKEY", key);

            dynamic proxy = new RestSharpProxy(client);

            // this is the mechanism by which parameter names that are not valid c# property names can be used
            var parameters = new Dictionary<string, object>()
            {
                { "chamber", "senate" },
                { "history.house_passage_result", "pass" }
            };

            dynamic result = await proxy.bills.get(paramList: parameters);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.results != null);
            Assert.IsTrue(result.results.Count > 0);

            foreach (dynamic bill in result.results)
            {
                Assert.AreEqual("senate", (string)bill.chamber);
                Assert.AreEqual("pass", (string)bill.history.house_passage_result);
            }
        }
    }
}
