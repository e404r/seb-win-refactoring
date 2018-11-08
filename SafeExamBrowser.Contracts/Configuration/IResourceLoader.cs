﻿/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Loads binary data from a particular resource.
	/// </summary>
	public interface IResourceLoader
	{
		/// <summary>
		/// Indicates whether the resource loader is able to load data from the specified resource.
		/// </summary>
		bool CanLoad(Uri resource);

		/// <summary>
		/// Loads the binary data from the specified resource.
		/// </summary>
		byte[] Load(Uri resource);
	}
}
