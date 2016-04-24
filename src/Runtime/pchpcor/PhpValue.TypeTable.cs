﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Pchp.Core
{
    partial struct PhpValue
    {
        /// <summary>
        /// Methods table for <see cref="PhpValue"/> instance.
        /// </summary>
        abstract class TypeTable
        {
            #region Singletons

            public static readonly NullTable NullTable = new NullTable();
            public static readonly LongTable LongTable = new LongTable();
            public static readonly DoubleTable DoubleTable = new DoubleTable();
            public static readonly BoolTable BoolTable = new BoolTable();
            public static readonly StringTable StringTable = new StringTable();
            public static readonly TypeTable WritableStringTable = new WritableStringTable();
            public static readonly ClassTable ClassTable = new ClassTable();
            public static readonly ArrayTable ArrayTable = new ArrayTable();
            public static readonly AliasTable AliasTable = new AliasTable();

            #endregion

            public abstract PhpTypeCode Type { get; }
            public abstract bool IsNull { get; }
            public abstract object ToClass(ref PhpValue me, Context ctx);
            public abstract string ToString(ref PhpValue me, Context ctx);
            public abstract string ToStringOrThrow(ref PhpValue me, Context ctx);
            public abstract long ToLong(ref PhpValue me);
            public abstract double ToDouble(ref PhpValue me);
            public abstract bool ToBoolean(ref PhpValue me);
            public abstract Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number);

            /// <summary>
            /// Ensures the value is a class object.
            /// In case it isn't, creates stdClass according to PHP semantics.
            /// In case current value is empty, replaces current value with newly created stdClass.
            /// </summary>
            /// <returns>Non-null object.</returns>
            public abstract object EnsureObject(ref PhpValue me, Context ctx);

            /// <summary>
            /// Ensures the value is a PHP array.
            /// In case it isn't, creates PhpArray according to PHP semantics.
            /// In case current value is empty, replaces current value with newly created array.
            /// </summary>
            /// <returns>Non-null object.</returns>
            public abstract PhpArray EnsureArray(ref PhpValue me);

            /// <summary>
            /// Ensures the value as an alias.
            /// In case it isn't, the value is aliased.
            /// </summary>
            /// <returns>Non-null alias of the value.</returns>
            public virtual PhpAlias EnsureAlias(ref PhpValue me)
            {
                Debug.Assert(Type != PhpTypeCode.Alias, "To be overriden!");
                var alias = new PhpAlias(me, 1);
                me = Create(alias);
                return alias;
            }

            /// <summary>
            /// Creates a deep copy of PHP variable.
            /// </summary>
            /// <returns>A deep copy of the value.</returns>
            public virtual PhpValue DeepCopy(ref PhpValue me) => me;

            /// <summary>
            /// Debug textual representation of the value.
            /// </summary>
            public abstract string DisplayString(ref PhpValue me);
        }

        class NullTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.Object;
            public override bool IsNull => true;
            public override object ToClass(ref PhpValue me, Context ctx) => new stdClass();
            public override string ToString(ref PhpValue me, Context ctx) => string.Empty;
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => string.Empty;
            public override long ToLong(ref PhpValue me) => 0;
            public override double ToDouble(ref PhpValue me) => 0.0;
            public override bool ToBoolean(ref PhpValue me) => false;
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number)
            {
                number = PhpNumber.Create(0L);
                return Convert.NumberInfo.Unconvertible;
            }
            public override object EnsureObject(ref PhpValue me, Context ctx)
            {
                var obj = ToClass(ref me, ctx);
                me = PhpValue.FromClass(obj);
                return obj;
            }
            public override PhpArray EnsureArray(ref PhpValue me)
            {
                var arr = new PhpArray();
                me = PhpValue.Create(arr);
                return arr;
            }
            public override string DisplayString(ref PhpValue me) => "NULL";
        }

        sealed class VoidTable : NullTable
        {
            public override PhpTypeCode Type => PhpTypeCode.Undefined;
            public override string DisplayString(ref PhpValue me) => "void";
        }

        sealed class LongTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.Long;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) { throw new NotImplementedException(); }	// new stdClass(){ $scalar = VALUE }
            public override string ToString(ref PhpValue me, Context ctx) => me.Long.ToString();
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => me.Long.ToString();
            public override long ToLong(ref PhpValue me) => me.Long;
            public override double ToDouble(ref PhpValue me) => (double)me.Long;
            public override bool ToBoolean(ref PhpValue me) => me.Long != 0;
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number)
            {
                number = PhpNumber.Create(me.Long);
                return Convert.NumberInfo.IsNumber | Convert.NumberInfo.LongInteger;
            }
            public override object EnsureObject(ref PhpValue me, Context ctx) => PhpValue.FromClass(ToClass(ref me, ctx)); // me is not changed
            public override PhpArray EnsureArray(ref PhpValue me) => new PhpArray(); // me is not changed
            public override string DisplayString(ref PhpValue me) => me.Long.ToString();
        }

        sealed class DoubleTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.Double;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) { throw new NotImplementedException(); }	// new stdClass(){ $scalar = VALUE }
            public override string ToString(ref PhpValue me, Context ctx) => Convert.ToString(me.Double, ctx);
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => Convert.ToString(me.Double, ctx);
            public override long ToLong(ref PhpValue me) => (long)me.Double;
            public override double ToDouble(ref PhpValue me) => me.Double;
            public override bool ToBoolean(ref PhpValue me) => me.Double != 0;
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number)
            {
                number = PhpNumber.Create(me.Double);
                return Convert.NumberInfo.IsNumber | Convert.NumberInfo.Double;
            }
            public override object EnsureObject(ref PhpValue me, Context ctx) => PhpValue.FromClass(ToClass(ref me, ctx)); // me is not changed
            public override PhpArray EnsureArray(ref PhpValue me) => new PhpArray(); // me is not changed
            public override string DisplayString(ref PhpValue me) => me.Double.ToString();
        }

        sealed class BoolTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.Boolean;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) { throw new NotImplementedException(); }	// new stdClass(){ $scalar = VALUE }
            public override string ToString(ref PhpValue me, Context ctx) => Convert.ToString(me.Boolean);
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => Convert.ToString(me.Boolean);
            public override long ToLong(ref PhpValue me) => me.Boolean ? 1L : 0L;
            public override double ToDouble(ref PhpValue me) => me.Boolean ? 1.0 : 0.0;
            public override bool ToBoolean(ref PhpValue me) => me.Boolean;
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number)
            {
                number = PhpNumber.Create(me.Boolean ? 1L : 0L);
                return Convert.NumberInfo.IsNumber | Convert.NumberInfo.LongInteger;
            }
            public override object EnsureObject(ref PhpValue me, Context ctx)
            {
                var obj = ToClass(ref me, ctx);
                
                // me is changed if me.Boolean == FALSE
                if (me.Boolean == false)
                    me = PhpValue.FromClass(obj);
                
                return obj;
            }
            public override PhpArray EnsureArray(ref PhpValue me)
            {
                var arr = new PhpArray();

                // me is changed if me.Boolean == FALSE
                if (me.Boolean == false)
                    me = PhpValue.Create(arr);

                return arr;
            }
            public override string DisplayString(ref PhpValue me) => me.Boolean ? "TRUE" : "FALSE";
        }

        sealed class StringTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.String;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) { throw new NotImplementedException(); }	// new stdClass(){ $scalar = VALUE }
            public override string ToString(ref PhpValue me, Context ctx) => me.String;
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => me.String;
            public override long ToLong(ref PhpValue me) => Convert.StringToLongInteger(me.String);
            public override double ToDouble(ref PhpValue me) => Convert.StringToDouble(me.String);
            public override bool ToBoolean(ref PhpValue me) => Convert.ToBoolean(me.String);
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number) => Convert.ToNumber(me.String, out number);
            public override object EnsureObject(ref PhpValue me, Context ctx)
            {
                var obj = ToClass(ref me, ctx);

                // me is changed if value is empty
                if (string.IsNullOrEmpty(me.String))
                    me = PhpValue.FromClass(obj);

                return obj;
            }
            public override PhpArray EnsureArray(ref PhpValue me)
            {
                var arr = new PhpArray();

                // me is changed if value is empty
                if (string.IsNullOrEmpty(me.String))
                    me = PhpValue.Create(arr);

                return arr;
            }
            public override string DisplayString(ref PhpValue me) => $"'{me.String}'";
        }

        sealed class WritableStringTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.WritableString;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) { throw new NotImplementedException(); }	// new stdClass(){ $scalar = VALUE }
            public override string ToString(ref PhpValue me, Context ctx) => me.WritableString.ToString(ctx);
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => me.WritableString.ToStringOrThrow(ctx);
            public override long ToLong(ref PhpValue me) => me.WritableString.ToLong();
            public override double ToDouble(ref PhpValue me) => me.WritableString.ToDouble();
            public override bool ToBoolean(ref PhpValue me) => me.WritableString.ToBoolean();
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number) => me.WritableString.ToNumber(out number);
            public override object EnsureObject(ref PhpValue me, Context ctx)
            {
                //var obj = PhpValue.Create(new stdClass(ctx));
                //if (me.WritableString.IsEmpty)
                //{
                //    // me is changed if value is empty
                //    me = obj;
                //}
                //return obj;
                throw new NotImplementedException();
            }
            public override PhpArray EnsureArray(ref PhpValue me)
            {
                //var arr = new PhpArray();

                //// me is changed if value is empty
                //if (me.WritableString.IsEmpty)
                //    me = PhpValue.Create(arr);

                //return arr;
                throw new NotImplementedException();
            }
            public override PhpValue DeepCopy(ref PhpValue me)
            {
                //me.WritableString.DeepCopy()
                throw new NotImplementedException();
            }
            public override string DisplayString(ref PhpValue me) => $"'{me.WritableString.ToString()}'";
        }

        sealed class ClassTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.Object;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) => me.Object;
            public override string ToString(ref PhpValue me, Context ctx)
            {
                if (me.Object is IPhpConvertible) return ((IPhpConvertible)me.Object).ToString(ctx);
                throw new NotImplementedException();
            }
            public override string ToStringOrThrow(ref PhpValue me, Context ctx)
            {
                if (me.Object is IPhpConvertible) return ((IPhpConvertible)me.Object).ToStringOrThrow(ctx);
                throw new NotImplementedException();
            }
            public override long ToLong(ref PhpValue me)
            {
                if (me.Object is IPhpConvertible) return ((IPhpConvertible)me.Object).ToLong();
                throw new NotImplementedException();
            }
            public override double ToDouble(ref PhpValue me)
            {
                if (me.Object is IPhpConvertible) return ((IPhpConvertible)me.Object).ToDouble();
                throw new NotImplementedException();
            }
            public override bool ToBoolean(ref PhpValue me)
            {
                if (me.Object is IPhpConvertible) return ((IPhpConvertible)me.Object).ToBoolean();
                throw new NotImplementedException();
            }
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number)
            {
                if (me.Object is IPhpConvertible) return ((IPhpConvertible)me.Object).ToNumber(out number);
                throw new NotImplementedException();
            }
            public override object EnsureObject(ref PhpValue me, Context ctx) => me.Object;
            public override PhpArray EnsureArray(ref PhpValue me)
            {
                throw new InvalidOperationException();  // Fatal Error: Cannot use object of type stdClass as array
            }
            public override string DisplayString(ref PhpValue me) => me.Object.GetType().FullName.Replace('.', '\\') + "#" + me.Object.GetHashCode().ToString("X");
        }

        sealed class ArrayTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.PhpArray;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) => me.Array.ToClass(ctx);
            public override string ToString(ref PhpValue me, Context ctx) => me.Array.ToString(ctx);
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => me.Array.ToStringOrThrow(ctx);
            public override long ToLong(ref PhpValue me) => me.Array.ToLong();
            public override double ToDouble(ref PhpValue me) => me.Array.ToDouble();
            public override bool ToBoolean(ref PhpValue me) => me.Array.ToBoolean();
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number) => me.Array.ToNumber(out number);
            public override object EnsureObject(ref PhpValue me, Context ctx) => ToClass(ref me, ctx);    // me is not modified
            public override PhpArray EnsureArray(ref PhpValue me) => me.Array;
            public override PhpValue DeepCopy(ref PhpValue me) => PhpValue.Create(me.Array.DeepCopy());
            public override string DisplayString(ref PhpValue me) => $"array(length = {me.Array.Count})";
        }

        sealed class AliasTable : TypeTable
        {
            public override PhpTypeCode Type => PhpTypeCode.Alias;
            public override bool IsNull => false;
            public override object ToClass(ref PhpValue me, Context ctx) => me.Alias.ToClass(ctx);
            public override string ToString(ref PhpValue me, Context ctx) => me.Alias.ToString(ctx);
            public override string ToStringOrThrow(ref PhpValue me, Context ctx) => me.Alias.ToStringOrThrow(ctx);
            public override long ToLong(ref PhpValue me) => me.Alias.ToLong();
            public override double ToDouble(ref PhpValue me) => me.Alias.ToDouble();
            public override bool ToBoolean(ref PhpValue me) => me.Alias.ToBoolean();
            public override Convert.NumberInfo ToNumber(ref PhpValue me, out PhpNumber number) => me.Alias.ToNumber(out number);
            public override object EnsureObject(ref PhpValue me, Context ctx) => me.Alias.Value.EnsureObject(ctx);
            public override PhpArray EnsureArray(ref PhpValue me) => me.Alias.Value.EnsureArray();
            public override PhpAlias EnsureAlias(ref PhpValue me) => me.Alias;
            public override string DisplayString(ref PhpValue me) => "&" + me.Alias.Value.DisplayString;
        }
    }
}