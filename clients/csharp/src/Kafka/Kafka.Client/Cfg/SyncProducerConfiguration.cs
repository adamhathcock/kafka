﻿/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Kafka.Client.Cfg
{
    using Kafka.Client.Utils;

    public class SyncProducerConfiguration : ISyncProducerConfigShared
    {
        public const int DefaultBufferSize = 100 * 1024;

        public const int DefaultConnectTimeout = 5 * 1000;

        public const int DefaultSocketTimeout = 30 * 1000;

        public const int DefaultReconnectInterval = 30 * 1000;

        public const int DefaultMaxMessageSize = 1000 * 1000;

        public const int DefaultReconnectTimeInterval = 1000 * 1000 * 10;

        public SyncProducerConfiguration()
        {
            this.BufferSize = DefaultBufferSize;
            this.ConnectTimeout = DefaultConnectTimeout;
            this.SocketTimeout = DefaultSocketTimeout;
            this.MaxMessageSize = DefaultMaxMessageSize;
            this.ReconnectInterval = DefaultReconnectInterval;
            this.ReconnectTimeInterval = DefaultReconnectTimeInterval;
        }

        public SyncProducerConfiguration(ProducerConfiguration config, int id, string host, int port) 
        {
            Guard.NotNull(config, "config");

            this.Host = host;
            this.Port = port;
            this.BrokerId = id;
            this.BufferSize = config.BufferSize;
            this.ConnectTimeout = config.ConnectTimeout;
            this.SocketTimeout = config.SocketTimeout;
            this.MaxMessageSize = config.MaxMessageSize;
            this.ReconnectInterval = config.ReconnectInterval;
            this.ReconnectTimeInterval = config.ReconnectTimeInterval;
        }

        public int BufferSize { get; set; }

        public int ConnectTimeout { get; set; }

        public int SocketTimeout { get; set; }

        public int MaxMessageSize { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public int BrokerId { get; set; }

        public int ReconnectInterval { get; set; }

        public int ReconnectTimeInterval { get; set; }
    }
}
