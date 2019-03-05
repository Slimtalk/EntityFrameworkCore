// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class LikeTranslator : IMethodCallTranslator
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        private static readonly MethodInfo _methodInfo
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _methodInfoWithEscape
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        public LikeTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (Equals(method, _methodInfo))
            {
                var typeMapping = ExpressionExtensions.InferTypeMapping(arguments[1], arguments[2]);

                return new LikeExpression(
                    _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[1], typeMapping),
                    _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[2], typeMapping),
                    null,
                    _typeMappingSource.FindMapping(typeof(bool)));
            }

            if (Equals(method, _methodInfoWithEscape))
            {
                var typeMapping = ExpressionExtensions.InferTypeMapping(arguments[1], arguments[2], arguments[3]);

                return new LikeExpression(
                    _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[1], typeMapping),
                    _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[2], typeMapping),
                    _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[3], typeMapping),
                    _typeMappingSource.FindMapping(typeof(bool)));
            }

            return null;
        }
    }
}
