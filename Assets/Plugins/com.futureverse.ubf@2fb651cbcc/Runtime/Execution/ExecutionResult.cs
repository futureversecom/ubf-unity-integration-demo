// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;

namespace Futureverse.UBF.Runtime.Execution
{
	/// <summary>
	/// Contains details about the set of Blueprints that have finished executing.
	/// </summary>
	public class ExecutionResult
	{
		/// <summary>
		/// Whether the Blueprints finished running successfully.
		/// </summary>
		public bool Success { get; }
		/// <summary>
		/// Instance Id of the root Blueprint that was executed.
		/// </summary>
		public string InstanceId => _executionContext.InstanceId;

		/// <summary>
		/// The outputs of the root Blueprint that was executed.
		/// </summary>
		public Dictionary<string, object> BlueprintOutputs
		{
			get
			{
				_blueprintOutputs ??= DynamicUtils.ToObjectDictionary(
					_executionContext?.GetBlueprintOutputs() ?? new Dictionary<string, Dynamic>()
				);
				return _blueprintOutputs;
			}
		}

		private readonly ExecutionContext _executionContext;
		private Dictionary<string, object> _blueprintOutputs;
		
		internal ExecutionResult(bool success, ExecutionContext context)
		{
			Success = success;
			_executionContext = context;
		}
	}
}