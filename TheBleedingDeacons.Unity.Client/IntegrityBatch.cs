// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using TheBleedingDeacons.Unity.Models;

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// A set of write requests sent to the Integrity API in one HTTP call, through
/// WordPress core's <c>/wp-json/batch/v1</c>. Create one with
/// <see cref="UnityRestSharp.CreateBatch"/>.
/// </summary>
/// <remarks>
/// Only the write endpoints can be batched: WordPress core accepts POST, PUT,
/// PATCH and DELETE in a batch, never GET. The API key is sent once, with the
/// batch, and the server validates it once; each request is still checked
/// against its own permission and counts once against the key's rate limit.
/// </remarks>
/// <example>
/// <code>
/// var result = await client.CreateBatch()
///     .UpdateMember(5, new UpdateMemberRequest { MobileNumber = "07700 900123" })
///     .RegisterGroup(7, groupId: 12, memberId: 0, gsrName: "John D.")
///     .SendAsync(cancellationToken);
/// </code>
/// </example>
public sealed class IntegrityBatch
{
	/// <summary>
	/// WordPress core's default batch limit (filter: <c>rest_get_max_batch_size</c>).
	/// </summary>
	public const int DefaultMaxSize = 25;

	private readonly UnityRestSharp _client;
	private readonly bool _requireAllValid;
	private readonly int _maxSize;
	private readonly List<BatchOperation> _operations = [];

	internal IntegrityBatch(UnityRestSharp client, bool requireAllValid, int maxSize)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(maxSize, 1);

		_client = client;
		_requireAllValid = requireAllValid;
		_maxSize = maxSize;
	}

	/// <summary>
	/// Gets the number of requests queued.
	/// </summary>
	public int Count => _operations.Count;

	/// <summary>
	/// Queues a partial member update. See <see cref="UnityRestSharp.UpdateMemberAsync"/>.
	/// </summary>
	/// <returns>This batch, for chaining.</returns>
	public IntegrityBatch UpdateMember(int id, UpdateMemberRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		return Add($"/members/{id}/update", request, typeof(Member));
	}

	/// <summary>
	/// Queues a member creation. See <see cref="UnityRestSharp.CreateMemberAsync"/>.
	/// </summary>
	/// <returns>This batch, for chaining.</returns>
	public IntegrityBatch CreateMember(CreateMemberRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		return Add("/members/create", request, typeof(Member));
	}

	/// <summary>
	/// Queues a GDPR compliance record. See <see cref="UnityRestSharp.RecordComplianceAsync"/>.
	/// </summary>
	/// <returns>This batch, for chaining.</returns>
	public IntegrityBatch RecordCompliance(int id, RecordComplianceRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		return Add($"/members/{id}/compliance", request, typeof(Member));
	}

	/// <summary>
	/// Queues a group registration. See <see cref="UnityRestSharp.RegisterGroupAsync"/>.
	/// </summary>
	/// <returns>This batch, for chaining.</returns>
	public IntegrityBatch RegisterGroup(
		int intergroupMeetingId,
		int groupId,
		int memberId,
		string gsrName,
		bool gsrProxy = false,
		string? gsrProxyName = null)
	{
		var payload = new
		{
			group_id = groupId,
			member_id = memberId,
			gsr_name = gsrName,
			gsr_proxy = gsrProxy,
			gsr_proxy_name = gsrProxyName ?? string.Empty,
		};
		return Add($"/intergroup-meetings/{intergroupMeetingId}/register-group", payload, typeof(IntergroupMeetingGroupRegistration));
	}

	/// <summary>
	/// Queues a group unregistration. See <see cref="UnityRestSharp.UnregisterGroupAsync"/>.
	/// </summary>
	/// <returns>This batch, for chaining.</returns>
	public IntegrityBatch UnregisterGroup(int intergroupMeetingId, int groupId)
	{
		return Add($"/intergroup-meetings/{intergroupMeetingId}/unregister-group", new { group_id = groupId }, typeof(IntergroupMeetingGroupRegistration));
	}

	/// <summary>
	/// Queues an officer registration. See <see cref="UnityRestSharp.RegisterOfficerAsync"/>.
	/// </summary>
	/// <returns>This batch, for chaining.</returns>
	public IntegrityBatch RegisterOfficer(int intergroupMeetingId, int officerId, string positionName, string officerName)
	{
		var payload = new
		{
			officer_id = officerId,
			position_name = positionName,
			officer_name = officerName,
		};
		return Add($"/intergroup-meetings/{intergroupMeetingId}/register-officer", payload, typeof(IntergroupMeetingOfficerRegistration));
	}

	/// <summary>
	/// Queues an officer unregistration. See <see cref="UnityRestSharp.UnregisterOfficerAsync"/>.
	/// </summary>
	/// <returns>This batch, for chaining.</returns>
	public IntegrityBatch UnregisterOfficer(int intergroupMeetingId, int officerId)
	{
		return Add($"/intergroup-meetings/{intergroupMeetingId}/unregister-officer", new { officer_id = officerId }, typeof(IntergroupMeetingOfficerRegistration));
	}

	/// <summary>
	/// Sends every queued request in one call.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The per-request results.</returns>
	public Task<BatchResponse> SendAsync(CancellationToken cancellationToken = default)
	{
		return _client.SendBatchAsync(_operations, _requireAllValid, cancellationToken);
	}

	private IntegrityBatch Add(string path, object payload, Type dataType)
	{
		if (_operations.Count >= _maxSize)
		{
			throw new InvalidOperationException(
				$"A batch holds at most {_maxSize} requests; send this one and start another.");
		}

		_operations.Add(new BatchOperation(path, payload, dataType));
		return this;
	}
}
