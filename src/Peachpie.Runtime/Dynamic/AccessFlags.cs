﻿using System;

namespace Pchp.Core.Dynamic
{
    [Flags]
    public enum AccessMask
    {
        /// <summary>
        /// Serves for case when Expression is body of a ExpressionStmt.
        /// It is useless to push its value on the stack in that case.
        /// </summary>
        /// <remarks>Values must match the ones in <c>CodeAnalysis</c>, AccessMask.</remarks>
        None = 0,

        /// <summary>
        /// The result value will be read first.
        /// </summary>
        Read = 1,

        /// <summary>
        /// A value will be written to the place.
        /// Only available for VariableUse (variables, fields, properties, array items, references).
        /// </summary>
        Write = 2,

        /// <summary>
        /// The expression will be aliased and the alias will be read.
        /// </summary>
        ReadRef = 4 | Read,

        /// <summary>
        /// The expression will be read by value and copied.
        /// </summary>
        ReadCopy = 8 | Read,

        /// <summary>
        /// An aliased value will be written to the place.
        /// Only available for VariableUse (variables, fields, properties, array items, references).
        /// </summary>
        WriteRef = 16 | Write,

        /// <summary>
        /// The expression is accessed as a part of chain,
        /// its member field will be written to.
        /// E.g. (EnsureObject)->Field = Value
        /// </summary>
        EnsureObject = 32 | Read,

        /// <summary>
        /// The expression is accessed as a part of chain,
        /// its item entry will be written to.
        /// E.g. (EnsureArray)[] = Value
        /// </summary>
        EnsureArray = 64 | Read,

        /// <summary>
        /// Read is check only and won't result in an exception in case the variable does not exist.
        /// </summary>
        ReadQuiet = 128,

        /// <summary>
        /// The variable will be unset. Combined with <c>quiet</c> flag, valid for variables, array entries and fields.
        /// </summary>
        Unset = 256,

        // NOTE: WriteAndReadRef has to be constructed by semantic binder as bound expression with Write and another bound expression with ReadRef
        // NOTE: ReadAndWriteAndReadRef has to be constructed by semantic binder as bound expression with Read|Write and another bound expression with ReadRef

        //
        ReadMask = EnsureObject | EnsureArray | ReadRef | ReadCopy | ReadQuiet,
        WriteMask = Write | WriteRef| Unset,
    }

    internal static class AccessMaskExtensions
    {
        public static bool EnsureObject(this AccessMask flags) => (flags & AccessMask.EnsureObject) == AccessMask.EnsureObject;
        public static bool EnsureArray(this AccessMask flags) => (flags & AccessMask.EnsureArray) == AccessMask.EnsureArray;
        public static bool EnsureAlias(this AccessMask flags) => (flags & AccessMask.ReadRef) == AccessMask.ReadRef;
        public static bool Quiet(this AccessMask flags) => (flags & AccessMask.ReadQuiet) != 0;
        public static bool Read(this AccessMask flags) => (flags & AccessMask.Read) != 0;
        public static bool WriteAlias(this AccessMask flags) => (flags & AccessMask.WriteRef) == AccessMask.WriteRef;
        public static bool Write(this AccessMask flags) => (flags & AccessMask.Write) != 0;
        public static bool Unset(this AccessMask flags) => (flags & AccessMask.Unset) == AccessMask.Unset;
        public static bool Isset(this AccessMask flags) => Quiet(flags) && Read(flags);
    }
}
