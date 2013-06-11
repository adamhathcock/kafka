using System.Linq;

namespace Kafka.Client.Tests.Request
{
    using System;
    using System.Collections.Generic;

    using Kafka.Client.Requests;

    using NUnit.Framework;

    [TestFixture]
    public class TopicMetadataRequestTests
    {
        [Test]
        public void TopicMetadataRequestWithNoSegmentMetadataCreation()
        {
            var topics = new List<string> { "topic1", "topic2" };

            var request = TopicMetadataRequest.Create(topics, 1, 1, "test");

            Assert.IsNotNull(request);
            Assert.AreEqual(topics.Count, request.Topics.Count());

            for (int i = 0; i < topics.Count; i++)
            {
                var expectedTopic = topics[i];
                var actualTopic = request.Topics.ElementAt(i);

                Assert.AreEqual(expectedTopic, actualTopic);
            }

            Assert.AreEqual(DetailedMetadataRequest.NoSegmentMetadata, request.DetailedMetadata);
            Assert.AreEqual(0, request.Timestamp);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TopicMetadataRequestShouldThrowExceptionWhenListOfTopicsIsNull()
        {
            IList<string> topics = null;

            TopicMetadataRequest.Create(topics, 1, 1, "test");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TopicMetadataRequestShouldThrowExceptionWhenListOfTopicsIsEmpty()
        {
            IList<string> topics = new List<string>();

            TopicMetadataRequest.Create(topics, 1, 1, "test");
        }
    }
}