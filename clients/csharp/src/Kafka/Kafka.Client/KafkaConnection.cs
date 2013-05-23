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

namespace Kafka.Client
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;

    using Kafka.Client.Exceptions;
    using Kafka.Client.Producers.Async;
    using Kafka.Client.Requests;
    using Kafka.Client.Serialization;
    using Kafka.Client.Utils;

    /// <summary>
    /// Manages connections to the Kafka.
    /// </summary>
    public class KafkaConnection : IDisposable
    {
        private readonly int bufferSize;

        private readonly int socketTimeout;

        private readonly TcpClient client;

        private volatile bool disposed;

        /// <summary>
        /// Initializes a new instance of the KafkaConnection class.
        /// </summary>
        /// <param name="server">The server to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        public KafkaConnection(string server, int port, int bufferSize, int socketTimeout)
        {
            this.bufferSize = bufferSize;
            this.socketTimeout = socketTimeout;

            try
            {
                // connection opened
                this.client = new TcpClient(server, port)
                    {
                        ReceiveTimeout = socketTimeout,
                        SendTimeout = socketTimeout,
                        ReceiveBufferSize = bufferSize,
                        SendBufferSize = bufferSize
                    };
                var stream = this.client.GetStream();
                this.Reader = new KafkaBinaryReader(stream);
            }
            catch (Exception e)
            {
                Dispose();
                throw new KafkaConnectionException(e);
            }
        }

        public KafkaBinaryReader Reader { get; private set; }

        /// <summary>
        /// Writes a producer request to the server asynchronously.
        /// </summary>
        /// <param name="request">The request to make.</param>
        public void BeginWrite(AbstractRequest request)
        {
            this.EnsuresNotDisposed();
            Guard.NotNull(request, "request");

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] data = request.RequestBuffer.GetBuffer();
                stream.BeginWrite(data, 0, data.Length, asyncResult => ((NetworkStream)asyncResult.AsyncState).EndWrite(asyncResult), stream);
            }
            catch (InvalidOperationException e)
            {
                throw new KafkaConnectionException(e);
            }
            catch (IOException e)
            {
                throw new KafkaConnectionException(e);
            }
        }

        /// <summary>
        /// Writes a producer request to the server asynchronously.
        /// </summary>
        /// <param name="request">The request to make.</param>
        /// <param name="callback">The code to execute once the message is completely sent.</param>
        /// <remarks>
        /// Do not dispose connection till callback is invoked, 
        /// otherwise underlying network stream will be closed.
        /// </remarks>
        public void BeginWrite(ProducerRequest request, MessageSent<ProducerRequest> callback)
        {
            this.EnsuresNotDisposed();
            Guard.NotNull(request, "request");
            if (callback == null)
            {
                this.BeginWrite(request);
                return;
            }

            try
            {
                NetworkStream stream = client.GetStream();
                var ctx = new RequestContext<ProducerRequest>(stream, request);

                byte[] data = request.RequestBuffer.GetBuffer();
                stream.BeginWrite(
                    data,
                    0,
                    data.Length,
                    delegate(IAsyncResult asyncResult)
                    {
                        var context = (RequestContext<ProducerRequest>)asyncResult.AsyncState;
                        callback(context);
                        context.NetworkStream.EndWrite(asyncResult);
                    },
                    ctx);
            }
            catch (InvalidOperationException e)
            {
                throw new KafkaConnectionException(e);
            }
            catch (IOException e)
            {
                throw new KafkaConnectionException(e);
            }
        }

        /// <summary>
        /// Writes a producer request to the server.
        /// </summary>
        /// <remarks>
        /// Write timeout is defaulted to infitite.
        /// </remarks>
        /// <param name="request">The <see cref="ProducerRequest"/> to send to the server.</param>
        public void Write(AbstractRequest request)
        {
            this.EnsuresNotDisposed();
            Guard.NotNull(request, "request");
            this.Write(request.RequestBuffer.GetBuffer());
        }

        /// <summary>
        /// Writes data to the server.
        /// </summary>
        /// <param name="data">The data to write to the server.</param>
        private void Write(byte[] data)
        {
            try
            {
                NetworkStream stream = this.client.GetStream();
                //// Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);
            }
            catch (InvalidOperationException e)
            {
                throw new KafkaConnectionException(e);
            }
            catch (IOException e)
            {
                throw new KafkaConnectionException(e);
            }
        }

        /// <summary>
        /// Close the connection to the server.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            if (this.client != null)
            {
                this.client.Close();
            }
        }

        /// <summary>
        /// Ensures that object was not disposed
        /// </summary>
        private void EnsuresNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}
