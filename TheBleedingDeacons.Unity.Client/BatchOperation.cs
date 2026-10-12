// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// One queued request in an <see cref="IntegrityBatch"/>.
/// </summary>
/// <param name="Path">The route under <c>integrity/v1</c>, e.g. <c>/members/5/update</c>.</param>
/// <param name="Payload">The request body, serialized with the client's JSON options.</param>
/// <param name="DataType">The type the response's <c>data</c> deserializes to.</param>
internal sealed record BatchOperation(string Path, object Payload, Type DataType);
