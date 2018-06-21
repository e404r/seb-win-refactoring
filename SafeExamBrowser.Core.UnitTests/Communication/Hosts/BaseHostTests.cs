﻿/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.UnitTests.Communication.Hosts
{
	[TestClass]
	public class BaseHostTests
	{
		private Mock<IHostObject> hostObject;
		private Mock<IHostObjectFactory> hostObjectFactory;
		private Mock<ILogger> logger;
		private BaseHostImpl sut;

		[TestInitialize]
		public void Initialize()
		{
			hostObject = new Mock<IHostObject>();
			hostObjectFactory = new Mock<IHostObjectFactory>();
			logger = new Mock<ILogger>();

			hostObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>(), It.IsAny<ICommunication>())).Returns(hostObject.Object);

			sut = new BaseHostImpl("net.pipe://some/address/here", hostObjectFactory.Object, logger.Object);
		}

		[TestMethod]
		public void MustCorrectlyStartHost()
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;

			hostObject.Setup(h => h.Open()).Callback(() => threadId = Thread.CurrentThread.ManagedThreadId);

			sut.Start();

			hostObjectFactory.Verify(f => f.CreateObject(It.IsAny<string>(), sut), Times.Once);

			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		[TestMethod]
		[ExpectedException(typeof(CommunicationException))]
		public void MustCorrectlyHandleStartupException()
		{
			// TODO: Takes waaay too long, extract timeout (move to constructor like in ClientOperation)!
			hostObject.Setup(h => h.Open()).Throws<Exception>();

			sut.Start();
		}

		[TestMethod]
		public void MustCorrectlyStopHost()
		{
			sut.Start();
			sut.Stop();

			hostObject.Verify(h => h.Close(), Times.Once);
		}

		[TestMethod]
		[ExpectedException(typeof(CommunicationException))]
		public void MustCorrectlyHandleShutdownException()
		{
			hostObject.Setup(h => h.Close()).Throws<Exception>();

			sut.Start();
			sut.Stop();
		}

		[TestMethod]
		public void MustNotFailToStopIfNotRunning()
		{
			sut.Stop();
			sut.Stop();
			sut.Stop();
		}

		[TestMethod]
		public void MustNotFailToEvaluateIsRunningIfNotRunning()
		{
			var running = sut.IsRunning;

			Assert.IsFalse(running);
		}

		[TestMethod]
		public void MustCorrectlyIndicateWhetherHostIsRunning()
		{
			hostObject.SetupGet(h => h.State).Returns(CommunicationState.Faulted);

			sut.Start();

			Assert.IsFalse(sut.IsRunning);

			hostObject.SetupGet(h => h.State).Returns(CommunicationState.Opened);

			Assert.IsTrue(sut.IsRunning);
		}

		[TestMethod]
		public void MustCorrectlyHandleConnectionRequest()
		{
			var token = Guid.NewGuid();
			var receivedToken = default(Guid?);

			sut.OnConnectStub = (t) =>
			{
				receivedToken = t;

				return true;
			};

			var response = sut.Connect(token);

			Assert.IsTrue(response.ConnectionEstablished);
			Assert.AreEqual(token, receivedToken);
			Assert.AreEqual(sut.GetCommunicationToken(), response.CommunicationToken);
		}

		[TestMethod]
		public void MustCorrectlyHandleDeniedConnectionRequest()
		{
			var token = Guid.NewGuid();
			var receivedToken = default(Guid?);

			sut.OnConnectStub = (t) =>
			{
				receivedToken = t;

				return false;
			};

			var response = sut.Connect(token);

			Assert.IsFalse(response.ConnectionEstablished);
			Assert.AreEqual(token, receivedToken);
			Assert.IsNull(sut.GetCommunicationToken());
			Assert.IsNull(response.CommunicationToken);
		}

		[TestMethod]
		public void MustCorrectlyHandleDisconnectionRequest()
		{
			var message = new DisconnectionMessage();
			var disconnected = false;

			sut.OnConnectStub = (t) => { return true; };
			sut.OnDisconnectStub = () => disconnected = true;
			sut.Connect();

			message.CommunicationToken = sut.GetCommunicationToken().Value;

			var response = sut.Disconnect(message);

			Assert.IsTrue(disconnected);
			Assert.IsTrue(response.ConnectionTerminated);
			Assert.IsNull(sut.GetCommunicationToken());
		}

		[TestMethod]
		public void MustCorrectlyHandleUnauthorizedDisconnectionRequest()
		{
			var disconnected = false;

			sut.OnConnectStub = (t) => { return true; };
			sut.OnDisconnectStub = () => disconnected = true;
			sut.Connect();

			var response = sut.Disconnect(new DisconnectionMessage());

			Assert.IsFalse(disconnected);
			Assert.IsFalse(response.ConnectionTerminated);
			Assert.IsNotNull(sut.GetCommunicationToken());
		}

		[TestMethod]
		public void MustCorrectlyHandleUnauthorizedTransmission()
		{
			var received = false;
			var simpleReceived = false;

			sut.OnReceiveStub = (m) => { received = true; return null; };
			sut.OnReceiveSimpleMessageStub = (m) => { simpleReceived = true; return null; };

			var response = sut.Send(new DisconnectionMessage());

			Assert.IsFalse(received);
			Assert.IsFalse(simpleReceived);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Unauthorized, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustCorrectlyHandlePingMessage()
		{
			var received = false;
			var simpleReceived = false;
			var message = new SimpleMessage(SimpleMessagePurport.Ping);

			sut.OnReceiveStub = (m) => { received = true; return null; };
			sut.OnReceiveSimpleMessageStub = (m) => { simpleReceived = true; return null; };
			sut.OnConnectStub = (t) => { return true; };
			sut.Connect();

			message.CommunicationToken = sut.GetCommunicationToken().Value;

			var response = sut.Send(message);

			Assert.IsFalse(received);
			Assert.IsFalse(simpleReceived);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustCorrectlyReceiveSimpleMessage()
		{
			var received = false;
			var simpleReceived = false;
			var purport = default(SimpleMessagePurport);
			var message = new SimpleMessage(SimpleMessagePurport.ConfigurationNeeded);
			var simpleResponse = new SimpleResponse(SimpleResponsePurport.UnknownMessage);

			sut.OnReceiveStub = (m) => { received = true; return null; };
			sut.OnReceiveSimpleMessageStub = (m) => { simpleReceived = true; purport = m; return simpleResponse; };
			sut.OnConnectStub = (t) => { return true; };
			sut.Connect();

			message.CommunicationToken = sut.GetCommunicationToken().Value;

			var response = sut.Send(message);

			Assert.IsFalse(received);
			Assert.IsTrue(simpleReceived);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleMessagePurport.ConfigurationNeeded, purport);
			Assert.AreSame(simpleResponse, response);
		}

		[TestMethod]
		public void MustCorrectlyReceiveMessage()
		{
			var received = false;
			var simpleReceived = false;
			var message = new ReconfigurationMessage(null);
			var reconfigurationResponse = new ReconfigurationResponse();

			sut.OnReceiveStub = (m) => { received = true; return reconfigurationResponse; };
			sut.OnReceiveSimpleMessageStub = (m) => { simpleReceived = true; return null; };
			sut.OnConnectStub = (t) => { return true; };
			sut.Connect();

			message.CommunicationToken = sut.GetCommunicationToken().Value;

			var response = sut.Send(message);

			Assert.IsTrue(received);
			Assert.IsFalse(simpleReceived);
			Assert.IsInstanceOfType(response, typeof(ReconfigurationResponse));
			Assert.AreSame(reconfigurationResponse, response);
		}
	}
}