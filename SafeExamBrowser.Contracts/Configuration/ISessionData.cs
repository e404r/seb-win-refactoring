﻿/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Defines all session-related (configuration) data.
	/// </summary>
	public interface ISessionData
	{
		/// <summary>
		/// The communication proxy for the client instance associated to this session.
		/// </summary>
		IClientProxy ClientProxy { get; set; }

		/// <summary>
		/// The process information of the client instance associated to this session.
		/// </summary>
		IProcess ClientProcess { get; set; }

		/// <summary>
		/// The unique session identifier.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The startup token used by the client and runtime components for initial authentication.
		/// </summary>
		Guid StartupToken { get; }
	}
}