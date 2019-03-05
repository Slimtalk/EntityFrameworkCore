// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class IsNullOrEmptyTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) });
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public IsNullOrEmptyTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (Equals(method, _methodInfo))
            {
                var argument = arguments[0];
                Debug.Assert(argument.TypeMapping != null, "Must have typeMapping.");
                var boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));

                return new SqlBinaryExpression(
                    ExpressionType.OrElse,
                    new SqlNullExpression(argument, false, boolTypeMapping),
                    new SqlBinaryExpression(
                        ExpressionType.Equal,
                        argument,
                        new SqlConstantExpression(Expression.Constant(""), argument.TypeMapping),
                        typeof(bool),
                        boolTypeMapping),
                    typeof(bool),
                    boolTypeMapping);
            }

            return null;
        }
    }
}
