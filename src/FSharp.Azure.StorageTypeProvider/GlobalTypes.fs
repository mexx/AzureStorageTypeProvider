﻿namespace FSharp.Azure.StorageTypeProvider.Table

open System

/// The different types of insertion mechanism to use.
type TableInsertMode = 
    /// Insert if the entity does not already exist.
    | Insert = 0
    /// Insert if the entity does not already exist; otherwise overwrite the entity.
    | Upsert = 1

/// The name of the partition.
type Partition = | Partition of string
/// The row key.
type Row = | Row of string
/// Represents a Partition and Row combined to key a single entity.
type EntityId = Partition * Row

/// Different responses from a table operation.
type TableResponse =
    /// The operation was successful.
    | SuccessfulResponse of EntityId * HttpCode : int
    /// The operation for this specific entity failed.
    | EntityError of EntityId * HttpCode : int * ErrorCode : string
    /// The operation for this specific entity was not carried out because an operation for another entity in the same batch failed.
    | BatchOperationFailedError of EntityId
    /// An unknown error occurred in this batch.
    | BatchError of EntityId * HttpCode : int * ErrorCode : string

/// Represents a single table entity.
type LightweightTableEntity
    internal (partitionKey:Partition, rowKey:Row, timestamp:DateTimeOffset, values:Map<string,obj>) =

    let (Partition pkey) = partitionKey
    let (Row rkey) = rowKey

    member x.PartitionKey with get () = pkey
    member x.RowKey with get () = rkey
    member x.Timestamp with get () = timestamp
    member x.Values with get () = values

/// The type of enumeration to return from folder child list operations.
type BlobFolderSearch =
| TopLevel
| Recursive
